using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Client.Core
{
    public class DataUtils
    {
        private static char[] constant =
    {
        '0','1','2','3','4','5','6','7','8','9',
        'a','b','c','d','e','f','g','h','i','j','k','l','m','n','o','p','q','r','s','t','u','v','w','x','y','z',
        'A','B','C','D','E','F','G','H','I','J','K','L','M','N','O','P','Q','R','S','T','U','V','W','X','Y','Z'
      };
        public static string GenerateRandomNumber(int Length)
        {
            StringBuilder newRandom = new StringBuilder(62);
            Random rd = new Random(DateTime.Now.Millisecond);
            for (int i = 0; i < Length; i++)
            {
                newRandom.Append(constant[rd.Next(62)]);
            }
            return newRandom.ToString();
        }
        public static byte[] ObjectToBytes<T>(T instance)
        {
            try
            {
                byte[] array;
                if (instance == null)
                {
                    array = new byte[0];
                }
                else
                {
                    MemoryStream memoryStream = new MemoryStream();
                    Serializer.Serialize(memoryStream, instance);
                    array = new byte[memoryStream.Length];
                    memoryStream.Position = 0L;
                    memoryStream.Read(array, 0, array.Length);
                    memoryStream.Dispose();
                }

                return array;

            }
            catch (Exception ex)
            {

                return new byte[0];
            }
        }

        public static T BytesToObject<T>(byte[] bytesData)
        {
            if (bytesData.Length == 0)
            {
                return default(T);
            }
            try
            {
                MemoryStream memoryStream = new MemoryStream();
                memoryStream.Write(bytesData, 0, bytesData.Length);
                memoryStream.Position = 0L;
                T result = Serializer.Deserialize<T>(memoryStream);
                memoryStream.Dispose();
                return result;
            }
            catch (Exception ex)
            {
                return default(T);
            }
        }
    }
    public static class StaticClass
    {
        public static string ToFormatString(this DateTime time)
        {
            return time.ToString("yyyy-MM-dd HH:mm:ss");
        }

    }
}
