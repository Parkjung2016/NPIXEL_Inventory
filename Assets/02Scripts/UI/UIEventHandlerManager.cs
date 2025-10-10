using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class UIEventHandlerManager : MonoBehaviour
{
    private Dictionary<Define.UIEvent, UIEventHandlerTypeBase> _eventHandlerTypes = new();

    public UIEventHandlerTypeBase GetOrAddEventHandlerType(Define.UIEvent type)
    {
        UIEventHandlerTypeBase handler = null;
        if (_eventHandlerTypes.TryGetValue(type, out handler))
            return handler;

        Type handlerType =
            Type.GetType($"UIEventHandler_{type}, Game.UI.UIEventHandlers");
        handler = gameObject.AddComponent(handlerType) as UIEventHandlerTypeBase;

        _eventHandlerTypes.Add(type, handler);
        return handler;
    }
}