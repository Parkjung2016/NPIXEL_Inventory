using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class EquipSlotUI : BaseItemSlotUI
{
    [SerializeField] private Define.ItemDetailType _equipSlotType;
    public override int CellIndex { get; protected set; } = -1;

    public override void Init()
    {
        base.Init();
        _itemManagerSO.OnItemEquipped += HandleItemEquipped;
        _itemManagerSO.OnItemUnequipped += HandleItemUnequipped;
    }

    protected override void OnDestroy()
    {
        _itemManagerSO.OnItemEquipped -= HandleItemEquipped;
        _itemManagerSO.OnItemUnequipped -= HandleItemUnequipped;
    }

    protected override void HandleSlotClick(PointerEventData pointerEvent)
    {
        if (pointerEvent.button == PointerEventData.InputButton.Left)
        {
            if (Keyboard.current.shiftKey.isPressed)
            {
                TryEquipOrUnequip(CurrentItemData);
                return;
            }

            base.HandleSlotClick(pointerEvent);
        }
        else if (pointerEvent.button == PointerEventData.InputButton.Right)
        {
            TryEquipOrUnequip(CurrentItemData);
        }
    }

    protected override void HandleSlotBeginDrag(PointerEventData pointerEvent)
    {
        if (CurrentItemData == null)
        {
            return;
        }

        base.HandleSlotBeginDrag(pointerEvent);
    }

    protected override void HandleSlotDrag(PointerEventData pointerEvent)
    {
        if (CurrentItemData == null)
        {
            return;
        }

        base.HandleSlotDrag(pointerEvent);
    }

    protected override void HandleSlotEndDrag(PointerEventData pointerEvent)
    {
        if (CurrentItemData == null)
        {
            return;
        }

        base.HandleSlotEndDrag(pointerEvent);
    }

    protected override void HandleSlotDrop(PointerEventData pointerEvent)
    {
        var targetSlot = UIEvents.ItemSlotDragAction.itemSlot;
        if (targetSlot == null) return;
        var itemData = targetSlot.CurrentItemData;
        if (itemData == null || itemData is not IEquipable || itemData.detailType != _equipSlotType) return;
        if (CurrentItemData != null)
        {
            var prev = CurrentItemData;
            int prevIndex = _itemManagerSO.UnequipItem(prev);
            _itemManagerSO.ChangeItemDataIndex(prev, prevIndex, targetSlot.CellIndex);
        }

        SoundManager.CreateSoundBuilder().Play(equipSound);
        _itemManagerSO.EquipItem(itemData);
        base.HandleSlotDrop(pointerEvent);
    }


    private void HandleItemEquipped(ItemDataBase itemData)
    {
        if (itemData is IEquipable && itemData.detailType == _equipSlotType) SetItemData(itemData);
    }

    private void HandleItemUnequipped(ItemDataBase itemData)
    {
        if (itemData == CurrentItemData) SetItemData(null);
    }
}