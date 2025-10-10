using System;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class UIEventHandlerTypeBase : MonoBehaviour
{
    public event Action<PointerEventData> OnEventHandler; 
    
    public void InvokeEventHandler(PointerEventData eventData)
    {
        OnEventHandler?.Invoke(eventData);
    }
}