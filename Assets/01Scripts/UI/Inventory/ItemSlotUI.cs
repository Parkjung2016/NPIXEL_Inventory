using PJH.Utility.Managers;
using Reflex.Attributes;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemSlotUI : UIBase, ICell, IItemSlotUI
{
    enum GameObjects
    {
        EmptyItemData,
        ExistingItemData
    }

    enum Images
    {
        Image_NoItemData,
        Image_Background,
        Image_Outline,
        Image_Icon,
    }

    enum Texts
    {
        Text_StackCount
    }

    public int CellIndex { get; private set; }
    public ItemDataBase CurrentItemData => _slotTooltipHandler.CurrentItemData;

    [Inject] private ItemRankColorMappingSO _rankColorMappingSO;
    [Inject] private ItemManagerSO _itemManagerSO;
    [Inject] private InventoryListSO _inventoryListSO;
    private GameEventChannelSO _uiEventChannelSO;
    private ItemSlotTooltipHandler _slotTooltipHandler;
    private ScrollRect _scrollRect;
    private RectTransform _rectTransform;

    public override void Init()
    {
        _rectTransform = transform as RectTransform;
        _uiEventChannelSO = AddressableManager.Load<GameEventChannelSO>("UIEventChannelSO");
        Bind<Image>(typeof(Images));
        Bind<TMP_Text>(typeof(Texts));
        Bind<GameObject>(typeof(GameObjects));

        _slotTooltipHandler = GetImage((byte)Images.Image_Background).GetComponent<ItemSlotTooltipHandler>();
        _scrollRect = GetComponentInParent<ScrollRect>();
        BindEvent(GetImage((byte)Images.Image_Background).gameObject, HandleSlotClick,
            Define.UIEvent.Click);
        BindEvent(GetImage((byte)Images.Image_NoItemData).gameObject, HandleNoItemSlotClick,
            Define.UIEvent.Click);

        BindEvent(GetImage((byte)Images.Image_Background).gameObject, HandleSlotBeginDrag,
            Define.UIEvent.BeginDrag);
        BindEvent(GetImage((byte)Images.Image_Background).gameObject, HandleSlotDrag,
            Define.UIEvent.Drag);
        BindEvent(GetImage((byte)Images.Image_Background).gameObject, HandleSlotEndDrag,
            Define.UIEvent.EndDrag);
        BindEvent(GetImage((byte)Images.Image_Background).gameObject, HandleSlotDrop,
            Define.UIEvent.Drop);

        BindEvent(GetImage((byte)Images.Image_NoItemData).gameObject,
            pointerEvent => _scrollRect.OnBeginDrag(pointerEvent),
            Define.UIEvent.BeginDrag);
        BindEvent(GetImage((byte)Images.Image_NoItemData).gameObject, pointerEvent => _scrollRect.OnDrag(pointerEvent),
            Define.UIEvent.Drag);
        BindEvent(GetImage((byte)Images.Image_NoItemData).gameObject,
            pointerEvent => _scrollRect.OnEndDrag(pointerEvent),
            Define.UIEvent.EndDrag);
        BindEvent(GetImage((byte)Images.Image_NoItemData).gameObject, HandleSlotDrop,
            Define.UIEvent.Drop);
    }

    private void HandleSlotDrop(PointerEventData pointerEvent)
    {
        IItemSlotUI targetItemSlot = UIEvents.ItemSlotDragAction.itemSlot;
        if (targetItemSlot == null) return;
        ItemDataBase itemData = targetItemSlot.CurrentItemData;
        if (!CanDragAndDrop(itemData)) return;

        _itemManagerSO.ChangeItemDataIndex(itemData, targetItemSlot.CellIndex, CellIndex);
        var evt = UIEvents.ItemSlotDragAction;
        evt.itemSlot = null;
        _uiEventChannelSO.RaiseEvent(evt);
    }

    private bool CanDragAndDrop(ItemDataBase targetItemData = null)
    {
        ItemDataBase itemData = targetItemData ?? CurrentItemData;

        return itemData != null && !_inventoryListSO[itemData.itemType].inventoryData.canAutoSort;
    }

    private void HandleSlotBeginDrag(PointerEventData pointerEvent)
    {
        if (!CanDragAndDrop())
        {
            _scrollRect.OnBeginDrag(pointerEvent);
            return;
        }

        if (pointerEvent.button != PointerEventData.InputButton.Left) return;
        var itemSlotBeginDragEvt = UIEvents.ItemSlotDragAction;
        itemSlotBeginDragEvt.itemSlot = this;
        itemSlotBeginDragEvt.startPosition = pointerEvent.position;
        itemSlotBeginDragEvt.slotSize = _rectTransform.sizeDelta;
        _uiEventChannelSO.RaiseEvent(itemSlotBeginDragEvt);

        var clickItemSlotEvt = UIEvents.ClickItemSlot;
        clickItemSlotEvt.isClicked = false;
        clickItemSlotEvt.itemSlot = null;

        _uiEventChannelSO.RaiseEvent(clickItemSlotEvt);
        var showItemSlotTooltipEvt = UIEvents.ShowItemSlotTooltip;
        showItemSlotTooltipEvt.show = false;
        showItemSlotTooltipEvt.itemData = null;
        _uiEventChannelSO.RaiseEvent(showItemSlotTooltipEvt);
    }

    private void HandleSlotDrag(PointerEventData pointerEvent)
    {
        if (!CanDragAndDrop())
        {
            _scrollRect.OnDrag(pointerEvent);
            return;
        }

        if (pointerEvent.button != PointerEventData.InputButton.Left) return;
        var evt = UIEvents.ItemSlotDrag;
        evt.currentPosition = pointerEvent.position;
        _uiEventChannelSO.RaiseEvent(evt);
    }

    private void HandleSlotEndDrag(PointerEventData pointerEvent)
    {
        if (!CanDragAndDrop())
        {
            _scrollRect.OnEndDrag(pointerEvent);
            return;
        }

        if (pointerEvent.button != PointerEventData.InputButton.Left) return;
        var evt = UIEvents.ItemSlotDragAction;
        evt.itemSlot = null;
        _uiEventChannelSO.RaiseEvent(evt);
    }


    private void HandleSlotClick(PointerEventData pointerEvent)
    {
        if (pointerEvent.button != PointerEventData.InputButton.Left) return;
        var evt = UIEvents.ClickItemSlot;
        if (evt.isClicked && ReferenceEquals(evt.itemSlot, this)) return;
        evt.itemSlot = this;
        evt.isClicked = true;
        _uiEventChannelSO.RaiseEvent(evt);
    }

    private void HandleNoItemSlotClick(PointerEventData pointerEvent)
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
        bool isEmptyItemData = itemData == null;
        GetObject((byte)GameObjects.EmptyItemData).SetActive(isEmptyItemData);
        GetObject((byte)GameObjects.ExistingItemData).SetActive(!isEmptyItemData);
        if (isEmptyItemData)
        {
            return;
        }

        GetImage((byte)Images.Image_Icon).sprite = itemData.GetIcon();
        Color outlineColor = _rankColorMappingSO.GetOutlineColor(itemData.rank);
        GetImage((byte)Images.Image_Outline).color = outlineColor;

        if (itemData is IStackable stackable)
        {
            GetText((byte)Texts.Text_StackCount).gameObject.SetActive(true);
            GetText((byte)Texts.Text_StackCount).SetText(stackable.StackCount.ToString());
        }
        else
            GetText((byte)Texts.Text_StackCount).gameObject.SetActive(false);
    }


    public void ConfigureCell(ItemDataBase itemData, int slotIndex)
    {
        CellIndex = slotIndex;
        SetItemData(itemData);
    }
}