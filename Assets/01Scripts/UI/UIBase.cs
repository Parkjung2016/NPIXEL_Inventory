using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public abstract class UIBase : UIBehaviour
{
    protected Dictionary<Type, UnityEngine.Object[]> _objects = new Dictionary<Type, UnityEngine.Object[]>();

    public abstract void Init();

    protected override void Awake()
    {
        Init();
    }

    protected void Bind<T>(Type type) where T : UnityEngine.Object
    {
        string[] names = Enum.GetNames(type);
        UnityEngine.Object[] objects = new UnityEngine.Object[names.Length];
        _objects.Add(typeof(T), objects);

        for (byte i = 0; i < names.Length; i++)
        {
            if (typeof(T) == typeof(GameObject))
                objects[i] = Util.FindChild(gameObject, names[i], true);
            else
                objects[i] = Util.FindChild<T>(gameObject, names[i], true);

            if (objects[i] == null)
                Debug.Log($"Failed to bind({names[i]})");
        }
    }

    public static void BindEvent(GameObject gameObject, Action<PointerEventData> action,
        Define.UIEvent type = Define.UIEvent.Click)
    {
        UIEventHandler evt = Util.GetOrAddComponent<UIEventHandler>(gameObject);

        switch (type)
        {
            case Define.UIEvent.Click:
                evt.OnClickHandler -= action;
                evt.OnClickHandler += action;
                break;
        }
    }

    protected T Get<T>(byte idx) where T : UnityEngine.Object
    {
        UnityEngine.Object[] objects = null;
        if (_objects.TryGetValue(typeof(T), out objects) == false)
            return null;

        return objects[idx] as T;
    }

    protected GameObject GetObject(byte idx)
    {
        return Get<GameObject>(idx);
    }

    protected TMP_Text GetText(byte idx)
    {
        return Get<TMP_Text>(idx);
    }

    protected Button GetButton(byte idx)
    {
        return Get<Button>(idx);
    }

    protected Slider GetSlider(byte idx)
    {
        return Get<Slider>(idx);
    }

    protected Image GetImage(byte idx)
    {
        return Get<Image>(idx);
    }
}