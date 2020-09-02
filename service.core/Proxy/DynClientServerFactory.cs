using Castle.DynamicProxy;
using System;
using System.Collections.Generic;
using System.Text;

namespace service.core
{
    public static class DynClientServerFactory
    {
        private static readonly ProxyGenerator Generator = new ProxyGenerator();
        /// <summary>
        /// 创建代理服务
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serviceName"></param>
        /// <returns></returns>
        public static T CreateServer<T>(string serviceName)
        {
            DynamicProxyClientSvrInvocation Interceptor = new DynamicProxyClientSvrInvocation(serviceName);   
            T p = (T)Generator.CreateInterfaceProxyWithoutTarget(typeof(T),Interceptor);
            return p;
        }
        /// <summary>
        /// 创建代理服务
        /// </summary>
        /// <param name="serviceName"></param>
        /// <param name="intftype"></param>
        /// <returns></returns>
        public static object CreateServer(string serviceName, Type intftype)
        {
            DynamicProxyClientSvrInvocation Interceptor = new DynamicProxyClientSvrInvocation(serviceName);
            object p = Generator.CreateInterfaceProxyWithoutTarget(intftype, Interceptor);
            return p;
        }
    }
}
