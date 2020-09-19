using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Service.SocketCore
{
    public class MainServer
    {
        private static ILogger logger = LogManager.GetLog("System");
        private static MainServer instance;
        private string ipStr;
        private int port;

        Socket acceptSocket;
        public Action<string> LoginOutEvt;
        Dictionary<string, ClientUser> cSockets;

        Queue<SocketDataObject> handleEvts;
        private MainServer()
        {
            cSockets = new Dictionary<string, ClientUser>();
            handleEvts = new Queue<SocketDataObject>();
            ThreadPool.SetMaxThreads(2000, 2000);
        }
        public static MainServer Instance
        {
            get
            {
                return instance;
            }
        }
        public static MainServer Build(string ip, int port)
        {
            if (instance == null)
            {
                instance = new MainServer
                {
                    ipStr = ip,
                    port = port
                };
            }
            return instance;
           
        }
        /// <summary>
        /// 启动
        /// </summary>
        /// <returns></returns>
        public void Start(int backlog = 1000)
        {
            acceptSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint point = new IPEndPoint(IPAddress.Parse(ipStr), port);
            acceptSocket.Bind(point);

            acceptSocket.Listen(backlog);
            Console.WriteLine("Socket server start,listening "+ ipStr+":"+ port);

            ThreadPool.QueueUserWorkItem(StartListen, acceptSocket);
            ThreadPool.QueueUserWorkItem(InvokeThread);

        }

        /// <summary>
        /// 发送信息到客户端
        /// </summary>
        /// <param name="tokens"></param>
        /// <param name="obj"></param>
        public void SendMsgToClient(List<string> tokens,SocketDataObject obj)
        {
            if (tokens.Count > 0)
            {
                byte[] buffer = DataUtils.ObjectToBytes(obj);
                byte[] bs = StickyPackageHelper.encode(buffer);
                Parallel.ForEach(tokens, item =>
                {
                    try
                    {
                        cSockets[item].SendMsg(bs);
                    }
                    catch
                    {
                        cSockets.Remove(item);
                        LoginOutEvt?.Invoke(item);
                    }
                });
            }
        }

        /// <summary>
        /// 关闭客户端连接
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public bool CloseConnect(string token)
        {
            if (cSockets.ContainsKey(token))
            {
                cSockets[token].Close();
            }
            return true;
        }

        #region 私有方法
        /// <summary>
        /// 移除客户端
        /// </summary>
        /// <param name="id"></param>
        internal void CloseLink(string token)
        {
            cSockets.Remove(token);
            LoginOutEvt?.Invoke(token);
        }

        /// <summary>
        /// 添加待处理请求
        /// </summary>
        /// <param name="obj"></param>
        internal void AddHandleEvt(SocketDataObject obj)
        {
            handleEvts.Enqueue(obj);
        }

        /// <summary>
        /// 开始监听
        /// </summary>
        /// <param name="o"></param>
        private void StartListen(object o)
        {
            Socket serverSocket = o as Socket;

            while (true)
            {
                try
                {
                    Socket clientSocket = serverSocket.Accept();
                    ClientUser linkSocket = new ClientUser(clientSocket);
                    cSockets.Add(linkSocket.ClientId, linkSocket);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }

        private void CheckBeats(object o)
        {
            while (true)
            {
                try
                {
                    var vs = cSockets.Keys.ToList();
                    for (int i = 0; i < vs.Count; i++)
                    {
                        SendMsgToClient(new List<string> { vs[i] }, new SocketDataObject());
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
                Thread.Sleep(10000);
            }
        }
        /// <summary>
        /// 处理请求线程
        /// </summary>
        /// <param name="o"></param>
        private void InvokeThread(object o)
        {
            while (true)
            {
                try
                {
                    if (handleEvts.Count > 0)
                    {
                        ThreadPool.QueueUserWorkItem(InvokeHandle, handleEvts.Dequeue());
                    }
                    else
                    {
                        Thread.Sleep(10);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    logger.Error(ex.ToString());
                }
            }
        }

        /// <summary>
        /// 处理请求
        /// </summary>
        /// <param name="o"></param>
        private void InvokeHandle(object o)
        {
            SocketDataObject data = o as SocketDataObject;
            var path = Directory.GetParent("wwwroot/1").ToString()+"/"+ data.ServiceName + ".json";
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
                    GetServiceResult(data, intf, serviceDefine, data.MethodName);
                }
                else
                {
                    Console.WriteLine("接口未定义");
                }
            }
        }



        /// <summary>
        /// 将查询字符串解析转换为名值集合.
        /// </summary>
        /// <param name="queryString"></param>
        /// <returns></returns>
        private static Dictionary<string,string> GetQueryString(string queryString)
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
            if(!map.TryAdd("token", data.ClientId))
            {
                map["token"] = data.ClientId;
            }
            if (!map.TryAdd("actionTime", data.Time))
            {
                map["actionTime"] = data.Time;
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

                        else if(infos[i].HasDefaultValue)
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
                            logger.Info($"Source:{cSockets[data.ClientId].Endpoint},Path:{data.ServiceName+"."+data.MethodName},Para:{JsonConvert.SerializeObject(objs)}");
                        }
                        catch{}
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

        #endregion

    }
}
