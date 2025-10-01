public static class UIEvents
{
    public static ShowItemSlotTooltipUIEvent ShowItemSlotTooltip = new ShowItemSlotTooltipUIEvent();
    public static ClickItemSlotEvent ClickItemSlot = new ClickItemSlotEvent();
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