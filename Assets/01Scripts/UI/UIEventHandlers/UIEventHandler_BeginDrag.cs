using UnityEngine.EventSystems;

public class UIEventHandler_BeginDrag : UIEventHandlerTypeBase, IBeginDragHandler
{
    public void OnBeginDrag(PointerEventData eventData)
    {
        InvokeEventHandler(eventData);
    }
}