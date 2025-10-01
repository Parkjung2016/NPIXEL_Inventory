using System;
using System.Text;
using Cysharp.Threading.Tasks;
using PJH.Utility;
using PJH.Utility.Managers;
using Reflex.Attributes;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ItemSlotTooltipUI : UIBase
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
        Text_AdditionalInfo
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
        Button_Cancel,
    }

    [Inject] private GameEventChannelSO _uiEventChannelSO;
    [Inject] private ItemRankColorMappingSO _itemRankColorMappingSO;
    [Inject] private ItemManagerSO _itemManagerSO;
    private RectTransform _rectTrm;
    private Vector2 _originPivot;
    private ItemData _currentItemData;

    private bool _lockedUpdatePosition;

    public override void Init()
    {
        _rectTrm = transform as RectTransform;
        _uiEventChannelSO.AddListener<ShowItemSlotTooltipUIEvent>(HandleShowItemSlotTooltipUI);
        _uiEventChannelSO.AddListener<ClickItemSlotEvent>(HandleClickItemSlot);
        gameObject.SetActive(false);
        _originPivot = _rectTrm.pivot;

        Bind<Image>(typeof(Images));
        Bind<TMP_Text>(typeof(Texts));
        Bind<GameObject>(typeof(Objects));
        Bind<Button>(typeof(Buttons));
        GetButton((byte)Buttons.Button_Use).onClick.AddListener(HandleClickUseButton);
        GetButton((byte)Buttons.Button_Cancel).onClick.AddListener(HandleClickCancelButton);
        GetButton((byte)Buttons.Button_Split).onClick.AddListener(HandleClickSplitButton);
    }

    protected override void OnDestroy()
    {
        _uiEventChannelSO.RemoveListener<ShowItemSlotTooltipUIEvent>(HandleShowItemSlotTooltipUI);
        _uiEventChannelSO.RemoveListener<ClickItemSlotEvent>(HandleClickItemSlot);
    }

    private void HandleClickSplitButton()
    {
        Transform itemSplitPopupUIParentTransform = GetObject((byte)Objects.AdditionalInteractInfo).transform;
        Managers.UI.ShowPopup<ItemSplitPopupUI>("ItemSplitPopupUI", itemSplitPopupUIParentTransform);
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
            bool usableItem = evt.itemSlot.CurrentItemData.Usable();
            GetObject((byte)Objects.InteractGroup).SetActive(true);
            GetButton((byte)Buttons.Button_Use).gameObject.SetActive(usableItem);
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
            gameObject.SetActive(false);
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

    private void ShowUIInfo(ItemData itemData)
    {
        _currentItemData = itemData;
        gameObject.SetActive(true);
        GetImage((byte)Images.Image_Icon).sprite = itemData.GetIcon();
        GetImage((byte)Images.Image_IconBackground).color = _itemRankColorMappingSO[itemData.rank];
        GetText((byte)Texts.Text_DisplayName).SetText(itemData.GetItemDisplayName());
        GetText((byte)Texts.Text_Description).SetText(itemData.description);
        GetText((byte)Texts.Text_Type).SetText(itemData.GetItemTypeDisplayName());
        StringBuilder baseInfo = itemData.GetBaseInfo();
        StringBuilder detailInfo = itemData.GetDetailInfo();
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

        bool hasAdditionalInfo = itemData.HasAdditionalInfo();
        GetObject((byte)Objects.AdditionalInfoGroup).SetActive(hasAdditionalInfo);
        if (hasAdditionalInfo)
        {
            GetText((byte)Texts.Text_AdditionalInfo).SetText(itemData.GetAdditionalAttributeInfo());
        }
    }
}