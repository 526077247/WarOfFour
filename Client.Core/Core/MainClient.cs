using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;

namespace Client.Core
{
    public class MainClient
    {
        private static MainClient instance;
        Socket clientSocket;
        Thread recvThread;
        List<byte> msg;
        object o;
        Queue<SocketDataObject> commands;
        private MainClient()
        {
            o = new object();
            msg = new List<byte>();
            commands = new Queue<SocketDataObject>();
            ThreadPool.SetMaxThreads(100, 100);
        }
        public void Start(string ip, int port)
        {
            clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            clientSocket.Connect(new IPEndPoint(IPAddress.Parse(ip), port));
            ThreadPool.QueueUserWorkItem(ReciveMsgThread);

        }

        public static MainClient Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new MainClient();
                }
                return instance;
            }
        }
        /// <summary>
        /// 取指令
        /// </summary>
        /// <returns></returns>
        public SocketDataObject GetCommand()
        {
            if (commands.Count > 0)
            {
                lock (o)
                {
                    return commands.Dequeue();
                }
            }
            return null;
        }
        public void InvokeMsg(SocketDataObject data)
        {
            Type intf = ServiceManager.Instance.GetTypeDefine(data.ServiceName);
            if (intf != null)
            {
                GetServiceResult(data, intf, data.MethodName);
            }
            else
            {
                Console.WriteLine("接口未定义");
            }
        }

        #region 私有方法
        internal void SendMsgToServer(SocketDataObject data)
        {
            ThreadPool.QueueUserWorkItem((obj) =>
            {
                byte[] bs = DataUtils.ObjectToBytes(data);
                clientSocket.Send(StickyPackageHelper.encode(bs));
            });
        }
        private void ReciveMsgThread(object o)
        {
            while (clientSocket != null)
            {
                Thread.Sleep(1);
                byte[] bs = new byte[5120];
                int count;
                try
                {
                    count = clientSocket.Receive(bs);
                }
                catch (Exception ex)
                {
                    Close();
                    return;
                }
                if (count == 0) break;
                else
                {
                    for (int i = 0; i < count; i++)
                    {
                        msg.Add(bs[i]);
                    }
                    byte[] real = StickyPackageHelper.decode(msg);
                    if (real != null)
                    {
                        SocketDataObject obj = DataUtils.BytesToObject<SocketDataObject>(real);
                        //if (obj != null)
                        //{
                        //    lock (o)
                        //    {
                        commands.Enqueue(obj);
                        //    }
                        //}
                    }
                }
            }

        }

        public void Close()
        {
            if (clientSocket != null)
            {
                clientSocket.Close();
            }
            if (recvThread != null)
            {
                recvThread.Abort();
            }
        }

        
        /// <summary>
        /// 反射调用方法获取执行结果
        /// </summary>
        /// <param name="data"></param>
        /// <param name="intf"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        private object GetServiceResult(SocketDataObject data, Type intf, string method)
        {
            Dictionary<string, string> map = GetQueryString(data.Paras);
            if (map.ContainsKey("actionTime"))
                map["actionTime"] = data.Time;
            else
                map.Add("actionTime", data.Time);
            object obj = ServiceManager.Instance.GetService(data.ServiceName, intf);

            if (obj == null)
            {
                throw new Exception( "服务未定义" + data.ServiceName);
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
                throw new Exception("未找到方法" + method);
            else
            {
                bool flag = false;
                var attrs = realmethod.GetCustomAttributes(true);
                for (int i = 0; i < attrs.Length; i++)
                {
                    if(attrs[i] as PublishMethodAttribute != null)
                    {
                        flag = true;
                    }
                }
                if (!flag)
                {
                    throw new Exception( "服务未发布");
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

                        else if (infos[i].DefaultValue!=null)
                        {
                            objs[i] = infos[i].DefaultValue;

                        }
                        else
                        {
                            throw new Exception($"参数{infos[i]}未找到");
                        }
                    }
                    try
                    {
                        object res = realmethod.Invoke(obj, objs);
                        return res;
                    }
                    catch (Exception ex)
                    {
                        
                        throw ex;
                    }
                }
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

