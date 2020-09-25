using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Newtonsoft.Json;
namespace Client.Core
{
    public static class ListExtend{

        public static string GetJsonText<T>(this List<T> list)
        {
            return JsonConvert.SerializeObject(list);
        }

        public static void SetJsonText<T>(this List<T> list,string jsonText)
        {
            list = JsonConvert.DeserializeObject<List<T>>(jsonText);
        }

    }

    public class DataObject
    {
        /// <summary>
        /// json序列化与反序列化
        /// </summary>
        [JsonIgnore]
        public string JsonText
        {
            get
            {
                return JsonConvert.SerializeObject(this);
            }
            set
            {
                var jObject = JsonConvert.DeserializeObject(value, GetType());
                foreach (var item in GetType().GetProperties())
                {
                    bool flag = true;
                    var attrs = item.GetCustomAttributes(true);
                    for (int i = 0; i < attrs.Length; i++)
                        if(attrs[i] as JsonIgnoreAttribute != null)
                            flag = false;
                    if (flag)
                    {
                        var itemValue = item.GetValue(jObject, null);
                        if (itemValue != null)
                            item.SetValue(this, itemValue, null);
                    }
                    
                }
            }
        }

    }
}
