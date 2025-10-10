using System;
using System.Collections.Generic;
using PJH.Utility.Managers;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryPresenter
{
    private readonly IInventoryView _view;

    [Inject] private readonly ItemManagerSO _itemManagerSO;
    [Inject] private readonly InventoryListSO _inventoryListSO;
    private readonly GameEventChannelSO _uiEventChannelSO;

    public Define.ItemType CurrentInventoryType { get; private set; }

    public InventorySO CurrentInventorySO => _inventoryListSO[CurrentInventoryType];

    public InventoryPresenter(IInventoryView view, Define.ItemType initialType)
    {
        _view = view;
        CurrentInventoryType = initialType;
        _uiEventChannelSO = AddressableManager.Load<GameEventChannelSO>("UIEventChannelSO");

        _view.OnChangeSortTypeClicked += HandleChangeSortTypeClicked;
        _view.OnSortClicked += SortData;
        _view.OnStackAllClicked += HandleStackAllClicked;
        _view.OnGoToTopClicked += HandleGoToTopClicked;
        _view.OnGoToBottomClicked += HandleGoToBottomClicked;
        _view.OnAutoSortToggled += HandleToggleAutoSort;
        _view.OnInventoryTypeChanged += ChangeInventoryType;
        _view.OnViewportDrop += HandleViewportDrop;
        _view.OnViewportClicked += HandleViewportClicked;
        _view.OnScrollValueChanged += HandleScrollValueChanged;
        _view.OnBlocked += HandleBlocked;
        _uiEventChannelSO.AddListener<ClickItemSlotEvent>(HandleClickItemSlotEvent);
    }


    public void Init()
    {
        SubscribeInventoryDataEvents();
        UpdateView(isInitialLoad: true);
        _view.ChangeInventoryType(CurrentInventorySO);
    }

    public void OnDestroy()
    {
        UnsubscribeInventoryDataEvents();
        _uiEventChannelSO.RemoveListener<ClickItemSlotEvent>(HandleClickItemSlotEvent);
        _itemManagerSO.OnUsedItemWithStackable -= HandleUsedItemWithStackable;
    }

    private void SubscribeInventoryDataEvents()
    {
        CurrentInventorySO.OnLoadedInventoryData += HandleLoadedInventoryData;
        CurrentInventorySO.inventoryData.OnChangedCurrentInventorySlotCount += UpdateCurrentItemCountText;
        CurrentInventorySO.inventoryData.OnUpdateItemCount += HandleUpdateItemCount;
        CurrentInventorySO.inventoryData.OnChangedInventoryData += HandleChangedInventoryData;

        _itemManagerSO.OnUsedItemWithStackable += HandleUsedItemWithStackable;
    }

    private void UnsubscribeInventoryDataEvents()
    {
        CurrentInventorySO.OnLoadedInventoryData -= HandleLoadedInventoryData;
        CurrentInventorySO.inventoryData.OnChangedCurrentInventorySlotCount -= UpdateCurrentItemCountText;
        CurrentInventorySO.inventoryData.OnUpdateItemCount -= HandleUpdateItemCount;
        CurrentInventorySO.inventoryData.OnChangedInventoryData -= HandleChangedInventoryData;
    }

    private void UpdateView(bool isInitialLoad = false)
    {
        UpdateCurrentItemCountText();
        UpdateSortTypeText();
        UpdateInventoryTypeButtons();
        _view.SetSortButtonActive(!CurrentInventorySO.inventoryData.canAutoSort);
        _view.SetAutoSortToggle(CurrentInventorySO.inventoryData.canAutoSort);
        _view.SetStackAllButtonActive(CurrentInventoryType != Define.ItemType.Equipment);
        _view.BlockInteraction(false);

        if (isInitialLoad)
        {
            _view.OnAutoSortToggled?.Invoke(CurrentInventorySO.inventoryData.canAutoSort);
        }
    }

    private void UpdateCurrentItemCountText()
    {
        _view.SetItemCountText(
            $"{CurrentInventorySO.inventoryData.currentInventorySlotCount}/{CurrentInventorySO.inventoryData.inventorySlotCapacity}");
    }

    private void UpdateSortTypeText()
    {
        string sortTypeName = Enum.GetName(typeof(Define.InventorySortType), CurrentInventorySO.inventoryData.sortType);
        _view.SetSortTypeText($"Sort Type: {sortTypeName}");
    }

    private void UpdateInventoryTypeButtons()
    {
        foreach (Define.ItemType itemType in Enum.GetValues(typeof(Define.ItemType)))
        {
            _view.SetInventoryTypeSelected(itemType, itemType == CurrentInventoryType);
        }
    }


    public void ChangeInventoryType(Define.ItemType newType)
    {
        if (newType == CurrentInventoryType) return;

        UnsubscribeInventoryDataEvents();

        CurrentInventoryType = newType;
        _view.ChangeInventoryType(CurrentInventorySO);
        SubscribeInventoryDataEvents();

        UpdateView();
        _view.ReloadScrollData();

        var evt = UIEvents.ClickItemSlot;
        if (evt.isClicked)
        {
            evt.itemSlot = null;
            evt.isClicked = false;
            _uiEventChannelSO.RaiseEvent(evt);
        }
    }

    private void SortData()
    {
        CurrentInventorySO.inventoryData.SortData();
        _view.ReloadScrollData(false);
    }

    private void HandleClickItemSlotEvent(ClickItemSlotEvent evt)
    {
        if (evt.isClicked && evt.itemSlot?.CurrentItemData != null)
            _view.StopMovement();
    }

    private void HandleStackAllClicked()
    {
        CurrentInventorySO.inventoryData.StackAll();
    }

    private void HandleBlocked(bool isBlocked)
    {
        CurrentInventorySO.inventoryData.canSettingData = !isBlocked;
    }

    private void HandleChangeSortTypeClicked()
    {
        int current = (int)CurrentInventorySO.inventoryData.sortType;
        current = (current + 1) % Enum.GetValues(typeof(Define.InventorySortType)).Length;

        CurrentInventorySO.inventoryData.sortType = (Define.InventorySortType)current; // Model 업데이트
        UpdateSortTypeText();

        if (CurrentInventorySO.inventoryData.canAutoSort)
        {
            SortData();
        }
    }

    private void HandleToggleAutoSort(bool isOn)
    {
        CurrentInventorySO.inventoryData.canAutoSort = isOn;

        _view.SetSortButtonActive(!isOn);

        if (isOn)
        {
            SortData();
        }
    }

    // Viewport Drop 로직
    private void HandleViewportDrop(PointerEventData pointerEvent)
    {
        IItemSlotUI targetItemSlot = UIEvents.ItemSlotDragAction.itemSlot;
        if (targetItemSlot == null) return;
        ItemDataBase itemData = targetItemSlot.CurrentItemData;
        if (targetItemSlot.CellIndex != -1 || itemData is IEquipable { IsEquipped: false }) return;

        _itemManagerSO.UnequipItem(targetItemSlot.CurrentItemData);

        var evt = UIEvents.ItemSlotDragAction;
        evt.itemSlot = null;
        _uiEventChannelSO.RaiseEvent(evt);
    }

    private void HandleGoToTopClicked() => _view.GoToTop();
    private void HandleGoToBottomClicked() => _view.GoToBottom();

    private void HandleViewportClicked()
    {
        var evt = UIEvents.ClickItemSlot;
        evt.itemSlot = null;
        evt.isClicked = false;
        _uiEventChannelSO.RaiseEvent(evt);
    }

    private void HandleScrollValueChanged(Vector2 value)
    {
        var evt = UIEvents.ClickItemSlot;
        if (evt.isClicked)
        {
            evt.itemSlot = null;
            evt.isClicked = false;
            _uiEventChannelSO.RaiseEvent(evt);
        }
    }

    private void HandleUpdateItemCount()
    {
        UpdateCurrentItemCountText();
        _view.ReloadScrollData();
    }

    private void HandleUsedItemWithStackable(ItemDataBase itemData)
    {
        _view.ReloadScrollData(false);
    }

    private void HandleLoadedInventoryData()
    {
        UnsubscribeInventoryDataEvents();
        SubscribeInventoryDataEvents();
        _view.ReloadScrollData();
    }

    private void HandleChangedInventoryData(List<ItemDataBase> inventoryDataList, bool resetData)
    {
        _view.ReloadScrollData(resetData);
    }
}