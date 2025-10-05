using UnityEngine.EventSystems;

public class UIEventHandler_Drop : UIEventHandlerTypeBase, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        InvokeEventHandler(eventData);
    }
}