using System;
using System.Collections.Generic;
using Reflex.Attributes;
using TMPro;
using UnityEngine;
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

    [field: SerializeField] public InventoryScrollRectDataSourceSO ScrollRectDataSourceSO { get; private set; }
    [Inject] private ItemManagerSO _itemManagerSO;
    [Inject] private InventorySO _inventorySO;
    private OptimizeScrollRect _optimizeScrollRect;


    public override void Init()
    {
        _optimizeScrollRect = GetComponent<OptimizeScrollRect>();
        _optimizeScrollRect.SetDataSource(ScrollRectDataSourceSO);
        ScrollRectDataSourceSO.ClearData();

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
    }

    private void UpdateSortTypeText()
    {
        string sortTypeName = Enum.GetName(typeof(InventorySortType), ScrollRectDataSourceSO.sortType);
        GetText((byte)Texts.Text_SortType).text = $"Sort Type: {sortTypeName}";
    }

    private void HandleClickSortButton()
    {
        int current = (int)ScrollRectDataSourceSO.sortType;

        current = (current + 1) % Enum.GetValues(typeof(InventorySortType)).Length;

        ScrollRectDataSourceSO.sortType = (InventorySortType)current;
        ScrollRectDataSourceSO.SortData();
        UpdateSortTypeText();
        _optimizeScrollRect.ReloadData();
    }

    private void HandleUsedItemWithStackable(ItemDataBase itemData)
    {
        _optimizeScrollRect.ReloadData();
    }

    private void HandleAddedInventoryData(ItemDataBase addedInventoryData)
    {
        ScrollRectDataSourceSO.AddData(addedInventoryData);
    }

    private void HandleRemovedInventoryData(ItemDataBase removedInventoryData)
    {
        ScrollRectDataSourceSO.RemoveData(removedInventoryData);
    }

    private void HandleForceChangedInventoryData(List<ItemDataBase> inventoryDataList)
    {
        ScrollRectDataSourceSO.ClearData();
        foreach (var data in inventoryDataList)
        {
            ScrollRectDataSourceSO.AddData(data);
        }

        _optimizeScrollRect.ReloadData();
    }

    private void HandleChangedInventoryData(List<ItemDataBase> inventoryDataList)
    {
        var existingIDs = new HashSet<Guid>();
        for (int i = 0; i < ScrollRectDataSourceSO.ItemDataList.Count; i++)
        {
            ItemDataBase itemData = ScrollRectDataSourceSO.ItemDataList[i];
            if (itemData != null)
                existingIDs.Add(ScrollRectDataSourceSO.ItemDataList[i].uniqueID);
            else
                existingIDs.Add(Guid.Empty);
        }

        foreach (var data in inventoryDataList)
        {
            if (!existingIDs.Contains(data.uniqueID))
            {
                ScrollRectDataSourceSO.AddData(data);
            }
        }

        for (int i = ScrollRectDataSourceSO.ItemDataList.Count - 1; i >= 0; i--)
        {
            var existing = ScrollRectDataSourceSO.ItemDataList[i];
            if (existing == null) continue;
            if (!inventoryDataList.Exists(itemData => itemData?.uniqueID == existing?.uniqueID))
            {
                ScrollRectDataSourceSO.RemoveData(existing);
                _inventorySO.RemoveItem(existing);
            }
        }

        _optimizeScrollRect.ReloadData();
    }
}