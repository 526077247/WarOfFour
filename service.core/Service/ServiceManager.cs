﻿using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Castle.Windsor.Configuration.Interpreters;
using IBatisNet.Common.Utilities.Objects.Members;


using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace Service.SocketCore
{
    public static class ServiceManager
    {
        private static IWindsorContainer container = null;
        static ServiceManager()
        {
            string path = ConfigurationManager.Configuration.GetSection("serviceCore:servicesFile").Value;
            if (!string.IsNullOrEmpty(path))
                container = new WindsorContainer(new XmlInterpreter(path));
        }

        private static IWindsorContainer Container
        {
            get
            {
                if (container == null)
                {
                    string path = ConfigurationManager.Configuration.GetSection("serviceCore:servicesFile").Value;
                    if (!string.IsNullOrEmpty(path))
                        container = new WindsorContainer(new XmlInterpreter(path));
                    else
                        throw new Exception("servicesFile未配置");
                }
                return container;
            }
        }

        /// <summary>
        /// 取Service实例
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <returns></returns>
        public static TService GetService<TService>() where TService : class
        {
            return Container.Resolve<TService>();
        }
        /// <summary>
        /// 取Service实例
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <param name="SvrID"></param>
        /// <returns></returns>
        public static TService GetService<TService>(string SvrID) where TService : class
        {
            
            try
            {
                IProxyService proxy = Container.Resolve<IProxyService>(SvrID);
                if (proxy != null)
                {
                    try
                    {
                        TService service = Container.Resolve<TService>(SvrID + "Proxy");
                        return service;
                    }
                    catch
                    {
                        TService svr = proxy.GetService<TService>();
                        Container.Register(
                           Component.For<TService>()
                           .Instance(svr)
                           .Named(SvrID + "Proxy")
                           .LifeStyle.Singleton
                        );
                        return svr;
                    }

                }
            }
            catch(Exception ex) { }
            return Container.Resolve<TService>(SvrID);
        }
        /// <summary>
        /// 取Service实例
        /// </summary>
        /// <param name="SvrID"></param>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        public static object GetService(string SvrID, Type serviceType)
        {
            try
            {
                IProxyService proxy = Container.Resolve<IProxyService>(SvrID);
                if (proxy != null)
                {
                    try
                    {
                        var service = Container.Resolve(SvrID + "Proxy", serviceType);
                        return service;
                    }
                    catch
                    {
                        var svr = proxy.GetService();
                        Container.Register(
                           Component.For(serviceType)
                           .Instance(svr)
                           .Named(SvrID + "Proxy")
                           .LifeStyle.Singleton
                        );
                        return svr;
                    }
                }
            }
            catch { }
            return Container.Resolve(SvrID, serviceType);
        }
        /// <summary>
        /// 取Service实例
        /// </summary>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        public static object GetService(Type serviceType)
        {
            return Container.Resolve(serviceType);
        }
        /// <summary>
        /// 根据类名取类
        /// </summary>
        /// <param name="typeName"></param>
        /// <param name="assembly"></param>
        /// <returns></returns>
        public static Type GetTypeFromAssembly(string typeName, Assembly assembly)
        {
            Type[] types = assembly.GetTypes();
            foreach (var t in types)
            {
                if (t.FullName == typeName)
                {
                    return t;
                }
            }
            return null;
        }

        
    }

}
