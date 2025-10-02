using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.EventSystems;

public static class Extension
{
    public static T GetOrAddComponent<T>(this GameObject gameObject) where T : UnityEngine.Component
    {
        return Util.GetOrAddComponent<T>(gameObject);
    }

    public static void BindEvent(this GameObject go, Action<PointerEventData> action,
        Define.UIEvent type = Define.UIEvent.Click)
    {
        UIBase.BindEvent(go, action, type);
    }


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