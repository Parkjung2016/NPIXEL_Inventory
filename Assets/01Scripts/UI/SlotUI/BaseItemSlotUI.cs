using PJH.Utility.Managers;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public abstract class BaseItemSlotUI : UIBase, IItemSlotUI
{
    protected enum GameObjects
    {
        EmptyItemData,
        ExistingItemData
    }

    protected enum Images
    {
        Image_NoItemData,
        Image_Background,
        Image_Outline,
        Image_Icon
    }

    public abstract int CellIndex { get; protected set; }
    public ItemDataBase CurrentItemData => _slotTooltipHandler.CurrentItemData;

    [Inject] protected ItemRankColorMappingSO _rankColorMappingSO;
    [Inject] protected ItemManagerSO _itemManagerSO;
    [Inject] protected PlayerStatus _playerStatus;

    protected GameEventChannelSO _uiEventChannelSO;
    protected ItemSlotTooltipHandler _slotTooltipHandler;
    protected RectTransform _rectTransform;

    public override void Init()
    {
        _rectTransform = transform as RectTransform;
        _uiEventChannelSO = AddressableManager.Load<GameEventChannelSO>("UIEventChannelSO");

        Bind<Image>(typeof(Images));
        Bind<GameObject>(typeof(GameObjects));

        _slotTooltipHandler = GetImage((byte)Images.Image_Background).GetComponent<ItemSlotTooltipHandler>();

        BindCommonEvents();
        SetItemData(null); // 기본 초기화
    }

    protected virtual void BindCommonEvents()
    {
        var bg = GetImage((byte)Images.Image_Background).gameObject;
        var noData = GetImage((byte)Images.Image_NoItemData).gameObject;

        BindEvent(bg, HandleSlotClick, Define.UIEvent.Click);
        BindEvent(noData, HandleNoItemSlotClick, Define.UIEvent.Click);

        BindEvent(bg, HandleSlotBeginDrag, Define.UIEvent.BeginDrag);
        BindEvent(bg, HandleSlotDrag, Define.UIEvent.Drag);
        BindEvent(bg, HandleSlotEndDrag, Define.UIEvent.EndDrag);
        BindEvent(bg, HandleSlotDrop, Define.UIEvent.Drop);
        BindEvent(noData, HandleSlotDrop, Define.UIEvent.Drop);
    }

    protected abstract void HandleSlotClick(PointerEventData pointerEvent);

    protected virtual void HandleSlotBeginDrag(PointerEventData pointerEvent)
    {
        if (pointerEvent.button != PointerEventData.InputButton.Left) return;
        var itemSlotBeginDragEvt = UIEvents.ItemSlotDragAction;
        itemSlotBeginDragEvt.itemSlot = this;
        itemSlotBeginDragEvt.startPosition = pointerEvent.position;
        itemSlotBeginDragEvt.slotSize = _rectTransform.sizeDelta;
        _uiEventChannelSO.RaiseEvent(itemSlotBeginDragEvt);
        ResetClickEvent();
        var showItemSlotTooltipEvt = UIEvents.ShowItemSlotTooltip;
        showItemSlotTooltipEvt.show = false;
        showItemSlotTooltipEvt.itemData = null;
        _uiEventChannelSO.RaiseEvent(showItemSlotTooltipEvt);
    }

    protected virtual void HandleSlotDrag(PointerEventData pointerEvent)
    {
        if (pointerEvent.button != PointerEventData.InputButton.Left) return;
        var evt = UIEvents.ItemSlotDrag;
        evt.currentPosition = pointerEvent.position;
        _uiEventChannelSO.RaiseEvent(evt);
    }

    protected virtual void HandleSlotEndDrag(PointerEventData pointerEvent)
    {
        if (pointerEvent.button != PointerEventData.InputButton.Left) return;
        ResetDragEvent();
    }

    protected abstract void HandleSlotDrop(PointerEventData pointerEvent);

    protected virtual void HandleNoItemSlotClick(PointerEventData pointerEvent)
    {
        if (pointerEvent.button != PointerEventData.InputButton.Left) return;
        var evt = UIEvents.ClickItemSlot;
        if (!evt.isClicked) return;
        evt.itemSlot = null;
        evt.isClicked = false;
        _uiEventChannelSO.RaiseEvent(evt);
    }

    public virtual void SetItemData(ItemDataBase itemData)
    {
        _slotTooltipHandler.SetItemData(itemData);
        bool isEmpty = itemData == null;
        GetObject((byte)GameObjects.EmptyItemData).SetActive(isEmpty);
        GetObject((byte)GameObjects.ExistingItemData).SetActive(!isEmpty);

        if (isEmpty) return;

        GetImage((byte)Images.Image_Icon).sprite = itemData.GetIcon();
        GetImage((byte)Images.Image_Outline).color = _rankColorMappingSO.GetOutlineColor(itemData.rank);
    }

    protected void ResetDragEvent()
    {
        var evt = UIEvents.ItemSlotDragAction;
        evt.itemSlot = null;
        _uiEventChannelSO.RaiseEvent(evt);
    }

    protected void ResetClickEvent()
    {
        var evt = UIEvents.ClickItemSlot;
        evt.isClicked = false;
        evt.itemSlot = null;
        _uiEventChannelSO.RaiseEvent(evt);
    }

    protected void TryEquipOrUnEquip(ItemDataBase itemData)
    {
        if (itemData is IEquipable equipable)
        {
            if (equipable.IsEquipped)
                _itemManagerSO.UnEquipItem(itemData);
            else
            {
                ItemDataBase prevCurrentItemData = CurrentItemData;
                if (_playerStatus.playerStatusData.equippedItems.TryGetValue(CurrentItemData.detailType,
                        out ItemDataBase equippedItem))
                {
                    if (equippedItem != null)
                    {
                        int prevIndex = _itemManagerSO.UnEquipItem(equippedItem);
                        int newIndex = CellIndex;
                        _itemManagerSO.ChangeItemDataIndex(equippedItem, prevIndex, newIndex);
                    }
                }

                _itemManagerSO.EquipItem(prevCurrentItemData);
            }

            ResetClickEvent();
        }
    }
}