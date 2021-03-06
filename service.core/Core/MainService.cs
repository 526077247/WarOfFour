﻿using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Service.SocketCore
{
    public class MainServer
    {
        private static ILogger logger = LogManager.GetLog("System");
        private static MainServer instance;
        private List<IPAddress> ips;
        private int port;

        Socket[] acceptSockets;
        public Action<string> LoginOutEvt;
        internal ConcurrentDictionary<string, ClientUser> cSockets;

        Queue<SocketDataObject> handleEvts;

        private MainServer()
        {
            cSockets = new ConcurrentDictionary<string, ClientUser>();
            handleEvts = new Queue<SocketDataObject>();
            ThreadPool.SetMaxThreads(2000, 2000);
        }
        public static MainServer Instance
        {
            get
            {
                if (instance == null) throw new Exception("未绑定端口");
                return instance;
            }
        }
        public static MainServer Build(int port, IPAddress[] ipAddress= null)
        {
            if (instance == null)
            {
                instance = new MainServer
                {
                    ips = new List<IPAddress>(),
                    port = port
                };
                if (ipAddress != null)
                    foreach (var item in ipAddress)
                    {
                        var ip = item.ToString();
                        if (ifip(ip))
                        {
                            instance.ips.Add(item);
                        }
                    }
                else
                    instance.ips.Add(IPAddress.Any);
            }
            return instance;

        }
        /// <summary>
        /// 启动
        /// </summary>
        /// <returns></returns>
        public void Start(int backlog = 1000)
        {
            try
            {
                acceptSockets = new Socket[ips.Count];
                for (int i = 0; i < ips.Count; i++)
                {
                    var acceptSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    IPEndPoint point = new IPEndPoint(ips[i], port);
                    acceptSocket.Bind(point);

                    acceptSocket.Listen(backlog);
                    Console.WriteLine("Socket server start,listening " + ips[i].ToString() + ":" + port);
                    acceptSockets[i] = acceptSocket;
                    StartListen(acceptSocket);
                }

                
            }
            catch(Exception ex)
            {
                logger.Error(ex);
            }
        }

        /// <summary>
        /// 发送信息到客户端
        /// </summary>
        /// <param name="cliendIds"></param>
        /// <param name="obj"></param>
        public void SendMsgToClient(List<string> cliendIds, SocketDataObject obj)
        {
            if (cliendIds.Count > 0)
            {
               
                byte[] buffer = DataUtils.ObjectToBytes(obj);
                byte[] bs = StickyPackageHelper.encode(buffer);
                Parallel.ForEach(cliendIds, item =>
                {
                    try
                    {
                        Console.WriteLine("SendMsgToClient:"+ item+" \n" + obj.ServiceName + "." + obj.MethodName + "?" + obj.Paras);
                        cSockets[item].SendMsg(bs);
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                        RemoveClient(item,out _);
                        logger.Error(ex);
                    }
                });
            }
        }

        /// <summary>
        /// 关闭连接
        /// </summary>
        /// <param name="cliendId"></param>
        /// <returns></returns>
        public bool CloseConnect(string cliendId)
        {
            RemoveClient(cliendId, out _);
            return true;
        }

        #region 私有方法
        /// <summary>
        /// 移除客户端
        /// </summary>
        /// <param name="id"></param>
        internal void RemoveClient(string cliendId,out ClientUser item)
        {
            if (cSockets.ContainsKey(cliendId))
            {
                cSockets[cliendId].Close();
                logger.Info("Close Socket:" + cliendId);
            }
            cSockets.Remove(cliendId,out item);
            LoginOutEvt?.Invoke(cliendId);
            logger.Info("RemoveClient cliendId:" + cliendId);
            logger.Info("cSockets count:" + cSockets.Count);
        }

        /// <summary>
        /// 添加待处理请求
        /// </summary>
        /// <param name="obj"></param>
        internal void AddHandleEvt(SocketDataObject obj)
        {
            handleEvts.Enqueue(obj);
        }
        private void StartListen(Socket acceptSocket)
        {
            acceptSocket.BeginAccept(ar =>
            {
                try
                {
                    Socket clientSocket = acceptSocket.EndAccept(ar);
                    ClientUser linkSocket = new ClientUser(clientSocket);
                    linkSocket.ReciveMsg();
                    cSockets.TryAdd(linkSocket.ClientId, linkSocket);
                    logger.Info("cSockets count:" + cSockets.Count);
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    logger.Error(ex.ToString());
                }
                //开始下一次监听
                StartListen(acceptSocket);
            }, null);
        }
        
        /// <summary>
        /// 处理请求
        /// </summary>
        /// <param name="o"></param>
        internal async Task InvokeHandle(object o)
        {
            try
            {
                SocketDataObject data = o as SocketDataObject;
                if (data == null) logger.Error(JsonConvert.SerializeObject(o));
                var path = Directory.GetCurrentDirectory() + "/wwwroot/" + data.ServiceName + ".json";
                if (File.Exists(path))
                {
                    string jstr = File.ReadAllText(path);
                    ServiceDefine serviceDefine = new ServiceDefine
                    {
                        JsonText = jstr
                    };
                    Type intf = ServiceManager.GetTypeFromAssembly(serviceDefine.IntfName, Assembly.Load(serviceDefine.IntfAssembly));
                    if (intf != null)
                    {
                        await Task.Run(() =>
                        {
                            GetServiceResult(data, intf, serviceDefine, data.MethodName);
                        });   
                    }
                    else
                    {
                        logger.Error("接口未定义");
                        throw new Exception("接口未定义");
                    }
                }
                else
                {
                    logger.Error("路径错误：" + path);
                }
            }
            catch(Exception ex)
            {
                logger.Error(ex);
            }
        }



        /// <summary>
        /// 将查询字符串解析转换为名值集合.
        /// </summary>
        /// <param name="queryString"></param>
        /// <returns></returns>
        private static Dictionary<string, string> GetQueryString(string queryString)
        {
            queryString = queryString.Replace("?", "");
            Dictionary<string, string> result = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(queryString))
            {
                int count = queryString.Length;
                for (int i = 0; i < count; i++)
                {
                    int startIndex = i;
                    int index = -1;
                    while (i < count)
                    {
                        char item = queryString[i];
                        if (item == '=')
                        {
                            if (index < 0)
                            {
                                index = i;
                            }
                        }
                        else if (item == '&')
                        {
                            break;
                        }
                        i++;
                    }
                    string key = null;
                    string value = null;
                    if (index >= 0)
                    {
                        key = queryString.Substring(startIndex, index - startIndex);
                        value = queryString.Substring(index + 1, (i - index) - 1);
                    }
                    else
                    {
                        key = queryString.Substring(startIndex, i - startIndex);
                    }
                    result[key] = value;

                    if ((i == (count - 1)) && (queryString[i] == '&'))
                    {
                        result[key] = string.Empty;
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 反射调用方法获取执行结果
        /// </summary>
        /// <param name="data"></param>
        /// <param name="intf"></param>
        /// <param name="serviceDefine"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        private object GetServiceResult(SocketDataObject data, Type intf, ServiceDefine serviceDefine, string method)
        {
            Dictionary<string, string> map = GetQueryString(data.Paras);
            if (!map.TryAdd("clientId", data.ClientId))
            {
                map["clientId"] = data.ClientId;
            }
            if (!map.TryAdd("actionTime", data.Time))
            {
                map["actionTime"] = data.Time;
            }
            if (!map.TryAdd("version", data.Time))
            {
                map["version"] = data.Time;
            }
            object obj = ServiceManager.GetService(serviceDefine.SvrID, intf);

            if (obj == null)
            {
                throw new ServiceException((int)TYPE_OF_RESULT_TYPE.failure, "服务未定义" + serviceDefine.SvrID);
            }
            MethodInfo realmethod = intf.GetMethod(method);
            if (realmethod == null)
            {
                foreach (Type type in intf.GetInterfaces())
                {
                    realmethod = type.GetMethod(method);
                    if (realmethod != null)
                        break;
                }
            }
            if (realmethod == null)
                throw new ServiceException((int)TYPE_OF_RESULT_TYPE.failure, "未找到方法" + method);
            else
            {
                if (realmethod.GetCustomAttribute(typeof(PublishMethodAttribute)) == null)
                {
                    throw new ServiceException((int)TYPE_OF_RESULT_TYPE.failure, "服务未发布");
                }
                else
                {
                    ParameterInfo[] infos = realmethod.GetParameters();
                    object[] objs = new object[infos.Length];
                    for (int i = 0; i < infos.Length; i++)
                    {
                        if (map != null && map.ContainsKey(infos[i].Name))
                        {
                            objs[i] = ChangeValueToType(map[infos[i].Name], infos[i].ParameterType);
                        }

                        else if (infos[i].HasDefaultValue)
                        {
                            objs[i] = infos[i].DefaultValue;

                        }
                        else
                        {
                            throw new ServiceException((int)TYPE_OF_RESULT_TYPE.failure, $"参数{infos[i]}未找到");
                        }
                    }
                    AutoLogAttribute logconfig = realmethod.GetCustomAttribute(typeof(AutoLogAttribute)) as AutoLogAttribute;
                    if (logconfig != null && (logconfig.Level == "INFO" || logconfig.Level == "ALL"))
                    {
                        try
                        {
                            logger.Info($"Source:{cSockets[data.ClientId].Endpoint},Path:{data.ServiceName + "." + data.MethodName},Para:{JsonConvert.SerializeObject(objs)}");
                        }
                        catch { }
                    }
                    try
                    {
                        object res = realmethod.Invoke(obj, objs);
                        return res;
                    }
                    catch (Exception ex)
                    {
                        if (logconfig != null && (logconfig.Level == "ERROR" || logconfig.Level == "ALL"))
                        {
                            logger.Error(ex);
                        }
                        throw ex;
                    }
                }
            }
        }
        /// <summary>
        /// 转换类型
        /// </summary>
        /// <param name="value"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private static object ChangeValueToType(string value, Type type)
        {
            object result = null;
            try
            {
                if (type == typeof(int))
                {
                    return Convert.ToInt32(value);
                }
                if (type == typeof(uint))
                {
                    return Convert.ToUInt32(value);
                }
                if (type == typeof(short))
                {
                    return Convert.ToInt16(value);
                }
                if (type == typeof(ushort))
                {
                    return Convert.ToUInt16(value);
                }
                if (type == typeof(long))
                {
                    return Convert.ToInt64(value);
                }
                if (type == typeof(ulong))
                {
                    return Convert.ToUInt64(value);
                }
                if (type == typeof(double))
                {
                    return Convert.ToDouble(value);
                }
                if (type == typeof(float))
                {
                    return Convert.ToSingle(value);
                }
                if (type == typeof(decimal))
                {
                    return Convert.ToDecimal(value);
                }
                if (type == typeof(Guid))
                {
                    return new Guid(value.ToString());
                }
                result = Convert.ChangeType(value, type);
            }
            catch { }
            if (result == null)
                result = JsonConvert.DeserializeObject(value, type);
            return result;

        }
        static bool ifip(string ip)
        {
            int num;
            //分割成四段
            string[] ip_4 = ip.Split('.');
            if (ip_4.Length != 4) return false;
            for (int i = 0; i < 4; i++)
            {
                if (!int.TryParse(ip_4[i], out num)) return false;
                if (num < 0 && num > 255) return false;
            }
            //全部检查完毕 无错误 
            return true;
        }
        static bool ifIp(string a)
        {
            return Regex.IsMatch(a, @"^((2[0-4]\d|25[0-5]|[01]?\d\d?)\.){3}(2[0-4]\d|25[0-5]|[01]?\d\d?)$");
        }
        #endregion

    }
}
