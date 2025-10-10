using UnityEngine.EventSystems;

public class UIEventHandler_Click : UIEventHandlerTypeBase, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        InvokeEventHandler(eventData);
    }
}