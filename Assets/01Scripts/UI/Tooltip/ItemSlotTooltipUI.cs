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
        Text_EquipAndUnequip
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
        Button_EquipAndUnequip
    }

    public Transform ChildPopupUIParentTransform { get; private set; }
    public Stack<IPopupUI> ChildPopupUIStack { get; set; } = new Stack<IPopupUI>();
    [SerializeField] private SoundDataSO buttonClickSound;
    [SerializeField] private SoundDataSO splitSound;
    [SerializeField] private SoundDataSO deleteSound;
    [SerializeField] private SoundDataSO equipSound;
    [SerializeField] private SoundDataSO unequipSound;
    [Inject] private ItemRankColorMappingSO _itemRankColorMappingSO;
    [Inject] private ItemManagerSO _itemManagerSO;
    [Inject] private InventoryListSO _inventoryListSO;
    [Inject] private PlayerStatus _playerStatus;
    private GameEventChannelSO _uiEventChannelSO;
    private RectTransform _rectTrm;
    private Vector2 _originPivot;
    private ItemDataBase _currentItemData;

    private bool _lockedUpdatePosition;
    private Vector2 _tooltipSize;

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
        GetButton((byte)Buttons.Button_EquipAndUnequip).onClick.AddListener(HandleClickEquipAndUnequipButton);
        _uiEventChannelSO.AddListener<ShowItemSlotTooltipUIEvent>(HandleShowItemSlotTooltipUI);
        _uiEventChannelSO.AddListener<ClickItemSlotEvent>(HandleClickItemSlot);

        ChildPopupUIParentTransform = GetObject((byte)Objects.AdditionalInteractInfo).transform;
    }


    protected override void OnDestroy()
    {
        _uiEventChannelSO.RemoveListener<ShowItemSlotTooltipUIEvent>(HandleShowItemSlotTooltipUI);
        _uiEventChannelSO.RemoveListener<ClickItemSlotEvent>(HandleClickItemSlot);
    }

    private void HandleClickEquipAndUnequipButton()
    {
        if (_currentItemData is IEquipable equipable)
        {
            var evt = UIEvents.ClickItemSlot;
            if (equipable.IsEquipped)
            {
                SoundManager.CreateSoundBuilder().Play(unequipSound);
                _itemManagerSO.UnequipItem(_currentItemData);
            }
            else
            {
                if (_playerStatus.playerStatusData.equippedItems.TryGetValue(_currentItemData.detailType,
                        out ItemDataBase equippedItem))
                {
                    if (equippedItem != null)
                    {
                        int prevIndex = _itemManagerSO.UnequipItem(equippedItem);
                        int newIndex = evt.itemSlot.CellIndex;

                        _itemManagerSO.ChangeItemDataIndex(equippedItem, prevIndex, newIndex);
                    }
                }

                SoundManager.CreateSoundBuilder().Play(equipSound);
                _itemManagerSO.EquipItem(_currentItemData);
            }

            ResetClickItemSlotEvent();
        }
    }

    private void HandleClickDeleteButton()
    {
        SoundManager.CreateSoundBuilder().Play(deleteSound);
        _itemManagerSO.DeleteItem(_currentItemData);
        ResetClickItemSlotEvent();
    }

    private void HandleClickSplitButton()
    {
        SoundManager.CreateSoundBuilder().Play(buttonClickSound);
        if (_currentItemData is IStackable stackable)
        {
            if (stackable.StackCount == 2)
            {
                SoundManager.CreateSoundBuilder().Play(splitSound);
                _inventoryListSO.SplitItem(_currentItemData, 1);
                ResetClickItemSlotEvent();
                return;
            }
        }

        ItemSplitPopupUI itemSplitPopupUI =
            Managers.UI.ShowPopup<ItemSplitPopupUI>("ItemSplitPopupUI", this);
        if (itemSplitPopupUI != null)
        {
            itemSplitPopupUI.SetItemData(_currentItemData);
            itemSplitPopupUI.OnSplited += ResetClickItemSlotEvent;
        }
    }

    private void HandleClickCancelButton()
    {
        SoundManager.CreateSoundBuilder().Play(buttonClickSound);
        ResetClickItemSlotEvent();
    }

    private void HandleClickUseButton()
    {
        SoundManager.CreateSoundBuilder().Play(buttonClickSound);
        _itemManagerSO.UseItem(_currentItemData);
        if (_currentItemData is IStackable { StackCount: > 0 })
        {
            ShowUIInfo(_currentItemData);
            return;
        }

        ResetClickItemSlotEvent();
    }

    private void ResetClickItemSlotEvent()
    {
        var evt = UIEvents.ClickItemSlot;
        evt.isClicked = false;
        evt.itemSlot = null;
        _uiEventChannelSO.RaiseEvent(evt);
    }

    private void Update()
    {
        UpdatePosition();
    }

    private Vector2 GetTooltipSize()
    {
        return new Vector2(_rectTrm.rect.width, _rectTrm.rect.height);
    }

    private void UpdatePosition()
    {
        if (_lockedUpdatePosition) return;

        Vector2 mousePosition = Mouse.current.position.value;

        _tooltipSize = GetTooltipSize();

        Vector2 offset = new Vector2(10, -10);
        Vector2 desiredPosition = mousePosition + offset;

        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        Vector2 newPivot = _originPivot;
        float newX = desiredPosition.x;
        float newY = desiredPosition.y;

        if (desiredPosition.x + _tooltipSize.x * (1f - _originPivot.x) > screenWidth)
        {
            newPivot.x = 1f;
            newX = mousePosition.x - offset.x;

            float minX = _tooltipSize.x * newPivot.x;
            float maxX = screenWidth - _tooltipSize.x * (1f - newPivot.x);

            newX = Mathf.Clamp(newX, minX, maxX);
        }
        else
        {
            newPivot.x = _originPivot.x;

            float minX = _tooltipSize.x * newPivot.x;
            float maxX = screenWidth - _tooltipSize.x * (1f - newPivot.x);

            newX = Mathf.Clamp(desiredPosition.x, minX, maxX);
        }


        if (desiredPosition.y + _tooltipSize.y * (1f - _originPivot.y) > screenHeight)
        {
            newPivot.y = 1f;
            newY = mousePosition.y - offset.y;

            float minY = _tooltipSize.y * newPivot.y;
            float maxY = screenHeight - _tooltipSize.y * (1f - newPivot.y);

            newY = Mathf.Clamp(newY, minY, maxY);
        }
        else if (desiredPosition.y - _tooltipSize.y * newPivot.y < 0)
        {
            newPivot.y = 0f;
            newY = mousePosition.y + offset.y;

            float minY = _tooltipSize.y * newPivot.y;
            float maxY = screenHeight - _tooltipSize.y * (1f - newPivot.y);

            newY = Mathf.Clamp(newY, minY, maxY);
        }
        else
        {
            newPivot.y = _originPivot.y;

            float minY = _tooltipSize.y * newPivot.y;
            float maxY = screenHeight - _tooltipSize.y * (1f - newPivot.y);

            newY = Mathf.Clamp(desiredPosition.y, minY, maxY);
        }

        _rectTrm.pivot = newPivot;
        _rectTrm.position = new Vector3(newX, newY, 0);
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
        if (itemData == null) return;
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
        GetButton((byte)Buttons.Button_EquipAndUnequip).gameObject.SetActive(equipableItem);
        if (equipableItem)
        {
            GetText((byte)Texts.Text_EquipAndUnequip).SetText(equipable.IsEquipped ? "Unequip" : "Equip");
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