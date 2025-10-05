using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using PJH.Utility.Managers;
using Reflex.Attributes;
using Reflex.Extensions;
using Reflex.Injectors;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InventoryUI : UIBase
{
    enum Buttons
    {
        Button_ChangeSortType,
        Button_Sort,
        Button_GoToTop,
        Button_GoToBottom
    }

    enum Texts
    {
        Text_SortType,
        Text_ItemCount
    }

    enum Toggles
    {
        Toggle_AutoSort
    }

    enum Objects
    {
        Viewport,
        TopGroups
    }

    enum CanvasGroups
    {
        BlockInteraction
    }

    private readonly Dictionary<ItemType, ChangeInventoryTypeButton> _changeInventoryTypeButtons =
        new Dictionary<ItemType, ChangeInventoryTypeButton>();

    public ItemType InventoryType => _inventoryType;

    [SerializeField] private ChangeInventoryTypeButton _changeInventoryTypeButtonPrefab;
    [SerializeField] private InventoryScrollRectDataSourceSO scrollRectDataSourceSO;
    [SerializeField] private ItemDragSlotUI itemDragSlotUIPrefab;
    [SerializeField] private ItemType _inventoryType;
    [Inject] private ItemManagerSO _itemManagerSO;
    [Inject] private InventoryListSO _inventoryListSO;
    private OptimizeScrollRect _optimizeScrollRect;
    private GameEventChannelSO _uiEventChannelSO;


    private InventorySO inventorySO => _inventoryListSO[_inventoryType];


    public override void Init()
    {
        _uiEventChannelSO = AddressableManager.Load<GameEventChannelSO>("UIEventChannelSO");
        Instantiate(itemDragSlotUIPrefab, transform);
        AttributeInjector.Inject(scrollRectDataSourceSO, SceneManager.GetActiveScene().GetSceneContainer());
        scrollRectDataSourceSO.SetInventorySO(inventorySO);
        _optimizeScrollRect = GetComponent<OptimizeScrollRect>();
        _optimizeScrollRect.SetDataSource(scrollRectDataSourceSO);

        Bind<GameObject>(typeof(Objects));
        if (_changeInventoryTypeButtonPrefab != null)
        {
            Transform topGroups = GetObject((byte)Objects.TopGroups).transform;
            foreach (ItemType itemType in Enum.GetValues(typeof(ItemType)))
            {
                ChangeInventoryTypeButton changeInventoryTypeButton =
                    Instantiate(_changeInventoryTypeButtonPrefab, topGroups);
                changeInventoryTypeButton.Init(this, itemType);
                _changeInventoryTypeButtons.Add(itemType, changeInventoryTypeButton);
            }
        }

        Bind<Button>(typeof(Buttons));
        Bind<TMP_Text>(typeof(Texts));
        Bind<Toggle>(typeof(Toggles));
        Bind<CanvasGroup>(typeof(CanvasGroups));

        BindEvent(GetObject((byte)Objects.Viewport),
            pointerEvent =>
            {
                if (pointerEvent.button != PointerEventData.InputButton.Left) return;
                var evt = UIEvents.ClickItemSlot;
                evt.itemSlot = null;
                evt.isClicked = false;
                _uiEventChannelSO.RaiseEvent(evt);
            });
        GetButton((byte)Buttons.Button_ChangeSortType).onClick.AddListener(HandleClickChangeSortButton);
        GetButton((byte)Buttons.Button_Sort).onClick.AddListener(HandleClickSortButton);
        GetButton((byte)Buttons.Button_GoToTop).onClick.AddListener(HandleClickGoToTopButton);
        GetButton((byte)Buttons.Button_GoToBottom).onClick.AddListener(HandleClickGoToBottomButton);
        Toggle autoSortToggle = GetToggle((byte)Toggles.Toggle_AutoSort);
        autoSortToggle.onValueChanged.AddListener(HandleToggleAutoSort);
        bool autoSort = inventorySO.inventoryData.canAutoSort;
        autoSortToggle.isOn = autoSort;
        GetButton((byte)Buttons.Button_Sort).gameObject.SetActive(!autoSort);

        DisableBlockInteraction();
        UpdateCurrentItemCountText();
        UpdateInventoryTypeButtons();
    }

    protected override void Start()
    {
        if (!Application.isPlaying) return;
        if (inventorySO != null)
        {
            inventorySO.OnLoadedInventoryData += HandleLoadedInventoryData;

            SubscribeInventoryDataEvents();
        }

        if (_itemManagerSO != null)
        {
            _itemManagerSO.OnUsedItemWithStackable += HandleUsedItemWithStackable;
        }

        UpdateSortTypeText();
    }

    protected override void OnDestroy()
    {
        if (inventorySO != null)
        {
            inventorySO.OnLoadedInventoryData -= HandleLoadedInventoryData;
            UnsubscribeInventoryDataEvents();
        }

        if (_itemManagerSO != null)
        {
            _itemManagerSO.OnUsedItemWithStackable -= HandleUsedItemWithStackable;
        }
    }

    private void SubscribeInventoryDataEvents()
    {
        inventorySO.inventoryData.OnChangedCurrentInventorySlotCount += UpdateCurrentItemCountText;
        inventorySO.inventoryData.OnUpdateItemCount += HandleUpdateItemCount;
        inventorySO.inventoryData.OnChangedInventoryData += HandleChangedInventoryData;
    }

    private void UnsubscribeInventoryDataEvents()
    {
        inventorySO.inventoryData.OnChangedCurrentInventorySlotCount -= UpdateCurrentItemCountText;
        inventorySO.inventoryData.OnUpdateItemCount -= HandleUpdateItemCount;
        inventorySO.inventoryData.OnChangedInventoryData -= HandleChangedInventoryData;
    }

    public void ChangeInventoryType(ItemType newType)
    {
        if (newType == _inventoryType) return;
        _inventoryType = newType;
        scrollRectDataSourceSO.SetInventorySO(inventorySO);
        UpdateSortTypeText();
        UpdateCurrentItemCountText();
        UnsubscribeInventoryDataEvents();
        SubscribeInventoryDataEvents();
        _optimizeScrollRect.ReloadData();
        UpdateInventoryTypeButtons();
    }

    private void UpdateInventoryTypeButtons()
    {
        foreach (var pair in _changeInventoryTypeButtons)
        {
            pair.Value.SetSelected(pair.Key == _inventoryType);
        }
    }

    private void UpdateCurrentItemCountText()
    {
        GetText((byte)Texts.Text_ItemCount).text =
            $"{inventorySO.inventoryData.currentInventorySlotCount}/{inventorySO.inventoryData.inventorySlotCapacity}";
    }

    private void HandleUpdateItemCount()
    {
        UpdateCurrentItemCountText();
        _optimizeScrollRect.ReloadData();
    }

    private void UpdateSortTypeText()
    {
        string sortTypeName = Enum.GetName(typeof(InventorySortType),
            inventorySO.inventoryData.sortType);
        GetText((byte)Texts.Text_SortType).text = $"Sort Type: {sortTypeName}";
    }

    private void HandleToggleAutoSort(bool isOn)
    {
        inventorySO.inventoryData.canAutoSort = isOn;
        GetButton((byte)Buttons.Button_Sort).gameObject.SetActive(!isOn);
        if (isOn)
        {
            SortData();
        }
    }

    private void HandleClickGoToTopButton()
    {
        BlockDuringAsync(_optimizeScrollRect.GoToTop());
    }

    private void HandleClickGoToBottomButton()
    {
        BlockDuringAsync(_optimizeScrollRect.GoToBottom());
    }

    private async void BlockDuringAsync(UniTask task)
    {
        EnableBlockInteraction();
        await task;
        DisableBlockInteraction();
    }

    private void EnableBlockInteraction()
    {
        CanvasGroup blockInteractionCanvasGroup = GetCanvasGroup((byte)CanvasGroups.BlockInteraction);
        blockInteractionCanvasGroup.alpha = 1;
        blockInteractionCanvasGroup.blocksRaycasts = true;
        inventorySO.inventoryData.canSettingData = false;
    }

    private void DisableBlockInteraction()
    {
        CanvasGroup blockInteractionCanvasGroup = GetCanvasGroup((byte)CanvasGroups.BlockInteraction);
        blockInteractionCanvasGroup.alpha = 0;
        blockInteractionCanvasGroup.blocksRaycasts = false;
        inventorySO.inventoryData.canSettingData = true;
    }

    private void SortData()
    {
        inventorySO.inventoryData.SortData();
        _optimizeScrollRect.ReloadData(false);
    }

    private void HandleClickSortButton()
    {
        SortData();
    }

    private void HandleClickChangeSortButton()
    {
        int current = (int)inventorySO.inventoryData.sortType;

        current = (current + 1) % Enum.GetValues(typeof(InventorySortType)).Length;

        inventorySO.inventoryData.sortType = (InventorySortType)current;
        UpdateSortTypeText();
        if (inventorySO.inventoryData.canAutoSort)
        {
            SortData();
        }
    }

    private void HandleUsedItemWithStackable(ItemDataBase itemData)
    {
        _optimizeScrollRect.ReloadData(false);
    }

    private void HandleLoadedInventoryData()
    {
        UnsubscribeInventoryDataEvents();
        SubscribeInventoryDataEvents();
        _optimizeScrollRect.ReloadData();
    }

    private void HandleChangedInventoryData(List<ItemDataBase> inventoryDataList, bool resetData)
    {
        _optimizeScrollRect.ReloadData(resetData);
    }
}