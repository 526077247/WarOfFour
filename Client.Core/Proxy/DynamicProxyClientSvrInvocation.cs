using Castle.DynamicProxy;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
namespace Client.Core
{
    public static class StaticClass
    {
        public static string ToFormatString(this DateTime time)
        {
            return time.ToString("yyyy-MM-dd HH:mm:ss");
        }

    }
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
            socketDataObject.Paras = GetpstData(invocation);
            socketDataObject.Time = DateTime.Now.ToFormatString();
            MainClient.Instance.SendMsgToServer(socketDataObject);

            invocation.ReturnValue = null;
        }


        private string GetpstData(IInvocation invocation)
        {
            StringBuilder builder = new StringBuilder();
            var Parameters = invocation.Method.GetParameters();
            for (int i = 0; i < Parameters.Length; i++)
            {
                string jStr = JsonConvert.SerializeObject(invocation.Arguments[i]);
                if (jStr.StartsWith("{") || jStr.StartsWith("["))
                    builder.Append(Parameters[i].Name + "=" + jStr + "&");
                else
                    builder.Append(Parameters[i].Name + "=" + invocation.Arguments[i].ToString() + "&");
            }
            string result = builder.ToString();
            if (result.EndsWith("&"))
                result = result.Substring(0,result.Length-1);
            return result;
        }

    }
}
