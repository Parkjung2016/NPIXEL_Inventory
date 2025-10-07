using Reflex.Attributes;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ItemSlotUI : BaseItemSlotUI, ICell
{
    enum Texts
    {
        Text_StackCount
    }

    [Inject] private InventoryListSO _inventoryListSO;
    private ScrollRect _scrollRect;
    public override int CellIndex { get; protected set; }

    public override void Init()
    {
        Bind<TMP_Text>(typeof(Texts));
        base.Init();
        _scrollRect = GetComponentInParent<ScrollRect>();
    }

    protected override void HandleSlotClick(PointerEventData pointerEvent)
    {
        if (pointerEvent.button == PointerEventData.InputButton.Left)
        {
            if (Keyboard.current.shiftKey.isPressed && CurrentItemData is IEquipable)
            {
                TryEquipOrUnEquip(CurrentItemData);
                return;
            }

            var evt = UIEvents.ClickItemSlot;
            if (evt.isClicked && ReferenceEquals(evt.itemSlot, this)) ResetClickEvent();
            else
            {
                evt.itemSlot = this;
                evt.isClicked = true;
                _uiEventChannelSO.RaiseEvent(evt);
            }
        }
        else if (pointerEvent.button == PointerEventData.InputButton.Right)
        {
            if (CurrentItemData is IUsable)
            {
                _itemManagerSO.UseItem(CurrentItemData);
                ResetClickEvent();
            }
            else
            {
                TryEquipOrUnEquip(CurrentItemData);
            }
        }
    }

    protected override void HandleSlotBeginDrag(PointerEventData pointerEvent)
    {
        if (!CanDragAndDrop())
        {
            _scrollRect.OnBeginDrag(pointerEvent);
            return;
        }

        base.HandleSlotBeginDrag(pointerEvent);
    }

    protected override void HandleSlotDrag(PointerEventData pointerEvent)
    {
        if (!CanDragAndDrop())
        {
            _scrollRect.OnDrag(pointerEvent);
            return;
        }

        base.HandleSlotDrag(pointerEvent);
    }

    protected override void HandleSlotEndDrag(PointerEventData pointerEvent)
    {
        if (!CanDragAndDrop())
        {
            _scrollRect.OnEndDrag(pointerEvent);
            return;
        }

        base.HandleSlotEndDrag(pointerEvent);
    }

    protected override void HandleSlotDrop(PointerEventData pointerEvent)
    {
        var targetSlot = UIEvents.ItemSlotDragAction.itemSlot;
        if (targetSlot == null) return;
        var itemData = targetSlot.CurrentItemData;
        if (!CanDragAndDrop(itemData)) return;
        if (targetSlot.CellIndex == -1) _itemManagerSO.UnEquipItem(itemData);
        else _itemManagerSO.ChangeItemDataIndex(itemData, targetSlot.CellIndex, CellIndex);
        ResetDragEvent();
        ResetClickEvent();
    }

    private bool CanDragAndDrop(ItemDataBase targetItemData = null)
    {
        var itemData = targetItemData ?? CurrentItemData;
        if (itemData == null) return false;
        if (itemData is IEquipable { IsEquipped: true }) return true;
        return !_inventoryListSO[itemData.itemType].inventoryData.canAutoSort;
    }

    public override void SetItemData(ItemDataBase itemData)
    {
        base.SetItemData(itemData);
        if (itemData is IStackable stackable)
        {
            GetText((byte)Texts.Text_StackCount).gameObject.SetActive(true);
            GetText((byte)Texts.Text_StackCount).SetText(stackable.StackCount.ToString());
        }
        else GetText((byte)Texts.Text_StackCount).gameObject.SetActive(false);
    }

    public void ConfigureCell(ItemDataBase itemData, int slotIndex)
    {
        CellIndex = slotIndex;
        SetItemData(itemData);
    }
}