using System;
using System.Collections.Generic;
using Reflex.Attributes;
using Reflex.Extensions;
using Reflex.Injectors;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InventoryUI : UIBase
{
    enum Buttons
    {
        Button_ChangeSortType
    }

    enum Texts
    {
        Text_SortType
    }

    [SerializeField] private InventoryScrollRectDataSourceSO _scrollRectDataSourceSO;
    [Inject] private ItemManagerSO _itemManagerSO;
    [Inject] private InventorySO _inventorySO;
    private OptimizeScrollRect _optimizeScrollRect;
    public InventoryScrollRectDataSourceSO ClonedInventoryScrollRectDataSourceSO { get; private set; }

    public override void Init()
    {
        _optimizeScrollRect = GetComponent<OptimizeScrollRect>();
        ClonedInventoryScrollRectDataSourceSO = _scrollRectDataSourceSO.Clone();
        ClonedInventoryScrollRectDataSourceSO.OnLoadedInventoryData += HandleLoadedInventoryData;
        AttributeInjector.Inject(ClonedInventoryScrollRectDataSourceSO,
            SceneManager.GetActiveScene().GetSceneContainer());
        ClonedInventoryScrollRectDataSourceSO.Init();
        _optimizeScrollRect.SetDataSource(ClonedInventoryScrollRectDataSourceSO.dataSource);
        ClonedInventoryScrollRectDataSourceSO.dataSource.ClearData();

        Bind<Button>(typeof(Buttons));
        Bind<TMP_Text>(typeof(Texts));
        GetButton((byte)Buttons.Button_ChangeSortType).onClick.AddListener(HandleClickSortButton);
    }

    protected override void Start()
    {
        if (!Application.isPlaying) return;
        if (_inventorySO != null)
        {
            _inventorySO.OnChangedInventoryData += HandleChangedInventoryData;
            _inventorySO.OnForceChangedInventoryData += HandleForceChangedInventoryData;
            _inventorySO.OnAddedInventoryData += HandleAddedInventoryData;
            _inventorySO.OnRemovedInventoryData += HandleRemovedInventoryData;
        }

        if (_itemManagerSO != null)
        {
            _itemManagerSO.OnUsedItemWithStackable += HandleUsedItemWithStackable;
        }

        UpdateSortTypeText();
    }

    protected override void OnDestroy()
    {
        if (_inventorySO != null)
        {
            _inventorySO.OnChangedInventoryData -= HandleChangedInventoryData;
            _inventorySO.OnForceChangedInventoryData -= HandleForceChangedInventoryData;
            _inventorySO.OnAddedInventoryData -= HandleAddedInventoryData;
            _inventorySO.OnRemovedInventoryData -= HandleRemovedInventoryData;
        }

        if (_itemManagerSO != null)
        {
            _itemManagerSO.OnUsedItemWithStackable -= HandleUsedItemWithStackable;
        }

        if (ClonedInventoryScrollRectDataSourceSO != null)
            ClonedInventoryScrollRectDataSourceSO.OnLoadedInventoryData -= HandleLoadedInventoryData;
    }

    private void UpdateSortTypeText()
    {
        string sortTypeName = Enum.GetName(typeof(InventorySortType),
            ClonedInventoryScrollRectDataSourceSO.dataSource.sortType);
        GetText((byte)Texts.Text_SortType).text = $"Sort Type: {sortTypeName}";
    }

    private void HandleLoadedInventoryData(InventoryScrollRectDataSource dataSource)
    {
        _optimizeScrollRect.SetDataSource(dataSource);
    }

    private void HandleClickSortButton()
    {
        int current = (int)ClonedInventoryScrollRectDataSourceSO.dataSource.sortType;

        current = (current + 1) % Enum.GetValues(typeof(InventorySortType)).Length;

        ClonedInventoryScrollRectDataSourceSO.dataSource.sortType = (InventorySortType)current;
        ClonedInventoryScrollRectDataSourceSO.dataSource.SortData();
        UpdateSortTypeText();
        _optimizeScrollRect.ReloadData();
    }

    private void HandleUsedItemWithStackable(ItemDataBase itemData)
    {
        _optimizeScrollRect.ReloadData();
    }

    private void HandleAddedInventoryData(ItemDataBase addedInventoryData)
    {
        ClonedInventoryScrollRectDataSourceSO.dataSource.AddData(addedInventoryData);
    }

    private void HandleRemovedInventoryData(ItemDataBase removedInventoryData)
    {
        ClonedInventoryScrollRectDataSourceSO.dataSource.RemoveData(removedInventoryData);
    }

    private void HandleForceChangedInventoryData(List<ItemDataBase> inventoryDataList)
    {
        ClonedInventoryScrollRectDataSourceSO.dataSource.ClearData();
        foreach (var data in inventoryDataList)
        {
            ClonedInventoryScrollRectDataSourceSO.dataSource.AddData(data);
        }

        _optimizeScrollRect.ReloadData();
    }

    private void HandleChangedInventoryData(List<ItemDataBase> inventoryDataList)
    {
        var existingIDs = new HashSet<Guid>();
        for (int i = 0; i < ClonedInventoryScrollRectDataSourceSO.dataSource.itemDataList.Count; i++)
        {
            ItemDataBase itemData = ClonedInventoryScrollRectDataSourceSO.dataSource.itemDataList[i];
            if (itemData != null)
                existingIDs.Add(ClonedInventoryScrollRectDataSourceSO.dataSource.itemDataList[i].uniqueID);
            else
                existingIDs.Add(Guid.Empty);
        }

        foreach (var data in inventoryDataList)
        {
            if (!existingIDs.Contains(data.uniqueID))
            {
                ClonedInventoryScrollRectDataSourceSO.dataSource.AddData(data);
            }
        }

        for (int i = ClonedInventoryScrollRectDataSourceSO.dataSource.itemDataList.Count - 1; i >= 0; i--)
        {
            var existing = ClonedInventoryScrollRectDataSourceSO.dataSource.itemDataList[i];
            if (existing == null) continue;
            if (!inventoryDataList.Exists(itemData => itemData?.uniqueID == existing?.uniqueID))
            {
                ClonedInventoryScrollRectDataSourceSO.dataSource.RemoveData(existing);
                _inventorySO.RemoveItem(existing);
            }
        }

        _optimizeScrollRect.ReloadData();
    }
}