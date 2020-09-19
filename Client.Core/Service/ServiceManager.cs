using Castle.MicroKernel.Registration;
using Castle.Windsor;
using System;
using System.Collections.Generic;

namespace Client.Core
{
    public class ServiceManager
    {
        private static ServiceManager instance;
        private Dictionary<string, Type> serviceDefine;
        private IWindsorContainer container ;
        private ServiceManager()
        {
            container = new WindsorContainer();
            serviceDefine = new Dictionary<string, Type>();
        }
        public static ServiceManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ServiceManager();
                }
                return instance;
            }
        }

        /// <summary>
        /// 取Service实例
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <returns></returns>
        public TService GetService<TService>() where TService : class
        {
            return container.Resolve<TService>();
        }
        /// <summary>
        /// 取Service实例
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <param name="SvrID"></param>
        /// <returns></returns>
        public TService GetService<TService>(string SvrID) where TService : class
        {
            return container.Resolve<TService>(SvrID);
        }
        /// <summary>
        /// 取Service实例
        /// </summary>
        /// <param name="SvrID"></param>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        public object GetService(string SvrID, Type serviceType)
        {
            return container.Resolve(SvrID, serviceType);
        }
        /// <summary>
        /// 取Service实例
        /// </summary>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        public object GetService(Type serviceType)
        {
            return container.Resolve(serviceType);
        }
        /// <summary>
        /// 根据类名取类
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public Type GetTypeDefine(string typeName)
        {
            return serviceDefine[typeName];
        }
        /// <summary>
        /// 注册服务
        /// </summary>
        /// <param name="type"></param>
        /// <param name="instance"></param>
        /// <param name="name"></param>
        public void AddService(Type type,object instance,string name)
        {
            if (serviceDefine.ContainsKey(name))
            {
                throw new Exception("name重复");
            }
            serviceDefine.Add(name, type);
            container.Register(
                  Component.For(type)
                  .Instance(instance)
                  .Named(name)
                  .LifeStyle.Singleton
               );
            return ;
        }
        /// <summary>
        /// 注册服务
        /// </summary>
        /// <param name="interfaceType"></param>
        /// <param name="classType"></param>
        /// <param name="name"></param>
        public void AddService(Type interfaceType, Type classType, string name)
        {
            if (serviceDefine.ContainsKey(name))
            {
                throw new Exception("name重复");
            }
            serviceDefine.Add(name, interfaceType);
            container.Register(
                  Component.For(interfaceType)
                  .ImplementedBy(classType)
                  .Named(name)
                  .LifeStyle.Singleton
               );
            return;
        }
        /// <summary>
        /// 注册服务
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance"></param>
        /// <param name="name"></param>
        public void AddService<T>(T instance, string name) where T : class
        {
            if (serviceDefine.ContainsKey(name))
            {
                throw new Exception("name重复");
            }
            serviceDefine.Add(name, typeof(T));
            container.Register(
                  Component.For<T>()
                  .Instance(instance)
                  .Named(name)
                  .LifeStyle.Singleton
               );
            return;
        }
    }

}
