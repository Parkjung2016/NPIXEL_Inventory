using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIEventHandler : MonoBehaviour, IPointerClickHandler
{
    public Action<PointerEventData> OnClickHandler = null;
    public Action<PointerEventData> OnDragHandler = null;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (OnClickHandler != null)
            OnClickHandler.Invoke(eventData);
    }
    // 문제 있음(Scroll Rect 작동 안함)
    // public void OnDrag(PointerEventData eventData)
    // {
    //     if (OnDragHandler != null)
    //         OnDragHandler.Invoke(eventData);
    //     ExecuteEvents.ExecuteHierarchy(transform.parent.parent.parent.parent.parent.parent.gameObject, eventData,
    //         ExecuteEvents.dragHandler); // InventoryUI
    // }
}