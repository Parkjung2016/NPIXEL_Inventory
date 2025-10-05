using UnityEngine.EventSystems;

public class UIEventHandler_EndDrag : UIEventHandlerTypeBase, IEndDragHandler
{
    public void OnEndDrag(PointerEventData eventData)
    {
        InvokeEventHandler(eventData);
    }
}