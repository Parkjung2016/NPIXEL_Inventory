using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace PJH.Utility.Extensions
{
    public static class ClassExtensions
    {
        public static T DeepCopy<T>(this T obj) where T : class
        {
            if (typeof(T).IsSerializable == false
                || typeof(ISerializable).IsAssignableFrom(typeof(T)))
            {
                return null;
            }

            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, obj);
                ms.Position = 0;

                return (T)formatter.Deserialize(ms);
            }
        }
    }
}