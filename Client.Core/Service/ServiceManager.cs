
using System;
using System.Collections.Generic;
using System.Linq;

namespace Client.Core
{
    public class ServiceManager
    {
        private static ServiceManager instance;
        private Dictionary<string, Type> serviceDefine;
        private Dictionary<Type,Dictionary<string,object>> map ;
        private List<Dictionary<string, object>> containers;
        private ServiceManager()
        {
            map = new Dictionary<Type, Dictionary<string, object>>();
            containers = new List<Dictionary<string, object>>();
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
            Type type = typeof(TService);
            if (map.ContainsKey(type))
            {
                if (map[type].Count > 0)
                {
                    return map[type].Values.ToList()[0] as TService;
                }
            }
            return null;
        }
        /// <summary>
        /// 取Service实例
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <param name="SvrID"></param>
        /// <returns></returns>
        public TService GetService<TService>(string SvrID) where TService : class
        {
            Type type = typeof(TService);
            if (map.ContainsKey(type))
            {
                if (map[type].ContainsKey(SvrID))
                {
                    return map[type][SvrID] as TService;
                }
            }
            return null;
        }
        /// <summary>
        /// 取Service实例
        /// </summary>
        /// <param name="SvrID"></param>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        public object GetService(string SvrID, Type serviceType)
        {
            if (map.ContainsKey(serviceType))
            {
                if (map[serviceType].ContainsKey(SvrID))
                {
                    return map[serviceType][SvrID];
                }
            }
            return null;
        }
        /// <summary>
        /// 取Service实例
        /// </summary>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        public object GetService(Type serviceType)
        {
            if (map.ContainsKey(serviceType))
            {
                if (map[serviceType].Count > 0)
                {
                    return map[serviceType].Values.ToList()[0];
                }
            }
            return null;
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
            if (!map.ContainsKey(type))
            {
                var childmap = new Dictionary<string, object>();
                map.Add(type, childmap);
            }
            map[type].Add(name, instance);
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
            if (!map.ContainsKey(interfaceType))
            {
                var childmap = new Dictionary<string, object>();
                map.Add(interfaceType, childmap);
            }
            object obj = Activator.CreateInstance(classType);
            map[interfaceType].Add(name, obj);
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
            Type type = typeof(T);
            if (!map.ContainsKey(type))
            {
                var childmap = new Dictionary<string, object>();
                map.Add(type, childmap);
            }
            map[type].Add(name, instance);
            return;
        }
    }

}
