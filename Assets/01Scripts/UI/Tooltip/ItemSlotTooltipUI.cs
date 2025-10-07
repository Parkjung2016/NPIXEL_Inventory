using System;
using System.Collections.Generic;
using System.Text;
using Cysharp.Threading.Tasks;
using PJH.Utility;
using PJH.Utility.Managers;
using Reflex.Attributes;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ItemSlotTooltipUI : UIBase, IPopupParentable
{
    enum Images
    {
        Image_Icon,
        Image_IconBackground
    }

    enum Texts
    {
        Text_DisplayName,
        Text_Description,
        Text_Type,
        Text_BaseInfo,
        Text_DetailInfo,
        Text_AdditionalInfo,
        Text_EquipAndUnEquips
    }

    enum Objects
    {
        AdditionalInfoGroup,
        InteractGroup,
        InfoGroup,
        AdditionalInteractInfo,
    }

    enum Buttons
    {
        Button_Use,
        Button_Split,
        Button_Delete,
        Button_Cancel,
        Button_EquipAndUnEquip
    }

    public Transform ChildPopupUIParentTransform { get; private set; }
    public Stack<IPopupUI> ChildPopupUIStack { get; set; } = new Stack<IPopupUI>();

    private GameEventChannelSO _uiEventChannelSO;
    [Inject] private ItemRankColorMappingSO _itemRankColorMappingSO;
    [Inject] private ItemManagerSO _itemManagerSO;
    [Inject] private InventoryListSO _inventoryListSO;
    [Inject] private PlayerStatus _playerStatus;
    private RectTransform _rectTrm;
    private Vector2 _originPivot;
    private ItemDataBase _currentItemData;

    private bool _lockedUpdatePosition;

    public override void Init()
    {
        _uiEventChannelSO = AddressableManager.Load<GameEventChannelSO>("UIEventChannelSO");
        _rectTrm = transform as RectTransform;
        gameObject.SetActive(false);
        _originPivot = _rectTrm.pivot;

        Bind<Image>(typeof(Images));
        Bind<TMP_Text>(typeof(Texts));
        Bind<GameObject>(typeof(Objects));
        Bind<Button>(typeof(Buttons));
        GetButton((byte)Buttons.Button_Use).onClick.AddListener(HandleClickUseButton);
        GetButton((byte)Buttons.Button_Cancel).onClick.AddListener(HandleClickCancelButton);
        GetButton((byte)Buttons.Button_Split).onClick.AddListener(HandleClickSplitButton);
        GetButton((byte)Buttons.Button_Delete).onClick.AddListener(HandleClickDeleteButton);
        GetButton((byte)Buttons.Button_EquipAndUnEquip).onClick.AddListener(HandleClickEquipAndUnEquipButton);
        _uiEventChannelSO.AddListener<ShowItemSlotTooltipUIEvent>(HandleShowItemSlotTooltipUI);
        _uiEventChannelSO.AddListener<ClickItemSlotEvent>(HandleClickItemSlot);

        ChildPopupUIParentTransform = GetObject((byte)Objects.AdditionalInteractInfo).transform;
    }


    protected override void OnDestroy()
    {
        _uiEventChannelSO.RemoveListener<ShowItemSlotTooltipUIEvent>(HandleShowItemSlotTooltipUI);
        _uiEventChannelSO.RemoveListener<ClickItemSlotEvent>(HandleClickItemSlot);
    }

    private void HandleClickEquipAndUnEquipButton()
    {
        if (_currentItemData is IEquipable equipable)
        {
            var evt = UIEvents.ClickItemSlot;
            if (equipable.IsEquipped)
            {
                _itemManagerSO.UnEquipItem(_currentItemData);
            }
            else
            {
                if (_playerStatus.playerStatusData.equippedItems.TryGetValue(_currentItemData.detailType,
                        out ItemDataBase equippedItem))
                {
                    if (equippedItem != null)
                    {
                        int prevIndex = _itemManagerSO.UnEquipItem(equippedItem);
                        int newIndex = evt.itemSlot.CellIndex;

                        _itemManagerSO.ChangeItemDataIndex(equippedItem, prevIndex, newIndex);
                    }
                }

                _itemManagerSO.EquipItem(_currentItemData);
            }

            evt.isClicked = false;
            evt.itemSlot = null;
            _uiEventChannelSO.RaiseEvent(evt);
        }
    }

    private void HandleClickDeleteButton()
    {
        _itemManagerSO.DeleteItem(_currentItemData);
        var evt = UIEvents.ClickItemSlot;
        evt.isClicked = false;
        evt.itemSlot = null;
        _uiEventChannelSO.RaiseEvent(evt);
    }

    private void HandleClickSplitButton()
    {
        ItemSplitPopupUI itemSplitPopupUI =
            Managers.UI.ShowPopup<ItemSplitPopupUI>("ItemSplitPopupUI", this);
        if (itemSplitPopupUI != null)
        {
            itemSplitPopupUI.SetItemData(_currentItemData);
            itemSplitPopupUI.OnSplited += () =>
            {
                var evt = UIEvents.ClickItemSlot;
                evt.isClicked = false;
                evt.itemSlot = null;
                _uiEventChannelSO.RaiseEvent(evt);
            };
        }
    }

    private void HandleClickCancelButton()
    {
        var evt = UIEvents.ClickItemSlot;
        evt.isClicked = false;
        evt.itemSlot = null;
        _uiEventChannelSO.RaiseEvent(evt);
    }

    private void HandleClickUseButton()
    {
        _itemManagerSO.UseItem(_currentItemData);
        if (_currentItemData is IStackable stackable)
        {
            if (stackable.StackCount > 0)
            {
                ShowUIInfo(_currentItemData);
                return;
            }
        }

        var evt = UIEvents.ClickItemSlot;
        evt.isClicked = false;
        evt.itemSlot = null;
        _uiEventChannelSO.RaiseEvent(evt);
    }

    private void Update()
    {
        UpdatePosition();
    }

    private void UpdatePosition()
    {
        if (_lockedUpdatePosition) return;
        Vector3 mousePosition = Mouse.current.position.value;
        RectTransform canvasRectTrm = (RectTransform)transform.parent;

        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRectTrm,
            mousePosition,
            null,
            out localPoint
        );

        Vector2 sizeDelta = _rectTrm.sizeDelta;

        Vector2 halfCanvasSize = canvasRectTrm.sizeDelta * 0.5f;

        Vector2 targetPivot = _originPivot;
        if (localPoint.x - sizeDelta.x < -halfCanvasSize.x)
        {
            targetPivot.x = 0f;
        }

        if (localPoint.y - sizeDelta.y < -halfCanvasSize.y)
        {
            targetPivot.y = 0f;
        }

        _rectTrm.pivot = targetPivot;

        _rectTrm.anchoredPosition = localPoint;
    }

    private void HandleClickItemSlot(ClickItemSlotEvent evt)
    {
        if (evt.isClicked)
        {
            _lockedUpdatePosition = false;
            UpdatePosition();
            _lockedUpdatePosition = true;
            ShowUIInfo(evt.itemSlot.CurrentItemData);
        }
        else
        {
            _lockedUpdatePosition = false;
            var showItemSlotTooltipEvt = UIEvents.ShowItemSlotTooltip;
            showItemSlotTooltipEvt.itemData = null;
            showItemSlotTooltipEvt.show = false;
            _uiEventChannelSO.RaiseEvent(showItemSlotTooltipEvt);
        }
    }

    private async void HandleShowItemSlotTooltipUI(ShowItemSlotTooltipUIEvent evt)
    {
        if (UIEvents.ClickItemSlot.isClicked) return;
        if (!evt.show || evt.itemData == null)
        {
            HideTooltip();
            return;
        }

        UpdatePosition();
        try
        {
            await UniTask.Yield(gameObject.GetCancellationTokenOnDestroy());
            ShowUIInfo(evt.itemData);
            GetObject((byte)Objects.InteractGroup).SetActive(false);
        }
        catch (Exception e)
        {
            PJHDebug.LogError(e, tag: "ItemSlotTooltipUI");
        }
    }

    private void HideTooltip()
    {
        for (int i = 0; i < ChildPopupUIStack.Count; i++)
        {
            IPopupUI popupUI = ChildPopupUIStack.Pop();
            Managers.UI.ClosePopup(popupUI);
        }

        gameObject.SetActive(false);
    }

    private void ShowUIInfo(ItemDataBase itemData)
    {
        _currentItemData = itemData;
        gameObject.SetActive(true);

        bool usableItem = ItemTooltipFormatter.IsUsable(itemData);
        bool splitableItem = ItemTooltipFormatter.IsSplitable(itemData) &&
                             !_inventoryListSO[itemData.itemType].inventoryData.IsFull();
        IEquipable equipable = itemData as IEquipable;
        bool equipableItem = equipable != null;
        GetObject((byte)Objects.InteractGroup).SetActive(true);
        GetButton((byte)Buttons.Button_Use).gameObject.SetActive(usableItem);
        GetButton((byte)Buttons.Button_Split).gameObject.SetActive(splitableItem);
        GetButton((byte)Buttons.Button_EquipAndUnEquip).gameObject.SetActive(equipableItem);
        if (equipableItem)
        {
            GetText((byte)Texts.Text_EquipAndUnEquips).SetText(equipable.IsEquipped ? "Unequip" : "Equip");
        }

        GetImage((byte)Images.Image_Icon).sprite = itemData.GetIcon();
        GetImage((byte)Images.Image_IconBackground).color = _itemRankColorMappingSO[itemData.rank];
        GetText((byte)Texts.Text_DisplayName).SetText(ItemTooltipFormatter.GetItemDisplayName(itemData));
        GetText((byte)Texts.Text_Description).SetText(itemData.description);
        GetText((byte)Texts.Text_Type).SetText(ItemTooltipFormatter.GetItemTypeDisplayName(itemData));
        StringBuilder baseInfo = ItemTooltipFormatter.GetBaseInfo(itemData);
        StringBuilder detailInfo = ItemTooltipFormatter.GetDetailInfo(itemData);
        if (baseInfo.Length == 0 && detailInfo.Length == 0)
        {
            GetObject((byte)Objects.InfoGroup).SetActive(false);
        }
        else
        {
            GetObject((byte)Objects.InfoGroup).SetActive(true);
            GetText((byte)Texts.Text_BaseInfo).SetText(baseInfo);
            GetText((byte)Texts.Text_DetailInfo).SetText(detailInfo);
        }

        bool hasAdditionalInfo = ItemTooltipFormatter.HasAdditionalInfo(itemData);
        GetObject((byte)Objects.AdditionalInfoGroup).SetActive(hasAdditionalInfo);
        if (hasAdditionalInfo)
        {
            GetText((byte)Texts.Text_AdditionalInfo).SetText(ItemTooltipFormatter.GetAdditionalAttributeInfo(itemData));
        }
    }
}