using UnityEngine;

public static class UIEvents
{
    public static readonly ShowItemSlotTooltipUIEvent ShowItemSlotTooltip = new ShowItemSlotTooltipUIEvent();
    public static readonly ClickItemSlotEvent ClickItemSlot = new ClickItemSlotEvent();
    public static readonly ItemSlotDragActionEvent ItemSlotDragAction = new ItemSlotDragActionEvent();
    public static readonly ItemSlotDragEvent ItemSlotDrag = new ItemSlotDragEvent();
}

public class ItemSlotDragActionEvent : GameEvent
{
    public IItemSlotUI itemSlot;
    public Vector2 slotSize;
    public Vector3 startPosition;
}

public class ItemSlotDragEvent : GameEvent
{
    public Vector3 currentPosition;
}

public class ShowItemSlotTooltipUIEvent : GameEvent
{
    public bool show;
    public ItemDataBase itemData;
}

public class ClickItemSlotEvent : GameEvent
{
    public bool isClicked;
    public IItemSlotUI itemSlot;
}