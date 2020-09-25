using Castle.DynamicProxy;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
namespace Service.SocketCore
{
    internal class DynamicProxyClientSvrInvocation : IInterceptor
    {
        private string serviceName;
        internal DynamicProxyClientSvrInvocation(string serviceName)
        {
            this.serviceName = serviceName;
        }
        /// <summary>
        /// 拦截器
        /// </summary>
        /// <param name="invocation"></param>
        public void Intercept(IInvocation invocation)
        {
            SocketDataObject socketDataObject = new SocketDataObject();
            socketDataObject.ServiceName = serviceName;
            socketDataObject.MethodName = invocation.Method.Name;
            socketDataObject.Paras = GetpstData(invocation,out List<string> tokens);
            socketDataObject.Time = DateTime.Now.ToFormatString();
            MainServer.Instance.SendMsgToClient(tokens, socketDataObject);

            invocation.ReturnValue = null;
        }


        private string GetpstData(IInvocation invocation,out List<string> tokens)
        {
            tokens = new List<string>();
            StringBuilder builder = new StringBuilder();
            var Parameters = invocation.Method.GetParameters();
            for (int i = 0; i < Parameters.Length; i++)
            {
                if (Parameters[i].Name == "clientIds")
                {
                    tokens = invocation.Arguments[i] as List<string>;
                    continue;
                }
                if (Parameters[i].Name == "clientId")
                {
                    tokens.Add(invocation.Arguments[i] as string);
                    continue;
                }
                string jStr = JsonConvert.SerializeObject(invocation.Arguments[i]);
                if (jStr.StartsWith("{")|| jStr.StartsWith("["))
                    builder.Append(Parameters[i].Name + "=" + jStr + "&");
                else
                    builder.Append(Parameters[i].Name + "=" + invocation.Arguments[i].ToString() + "&");
            }
            string result = builder.ToString();
            if (result.EndsWith("&"))
                result = result[0..^1];
            return result;
        }

    }
}
