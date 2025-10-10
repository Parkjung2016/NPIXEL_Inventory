using UnityEngine.EventSystems;

public class UIEventHandler_Drag : UIEventHandlerTypeBase, IDragHandler
{
    public void OnDrag(PointerEventData eventData)
    {
        InvokeEventHandler(eventData);
    }
}