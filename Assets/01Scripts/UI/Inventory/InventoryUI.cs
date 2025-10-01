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

    [SerializeField] private InventoryScrollRectDataSourceSO _scrollRectDataSourceSO;
    [Inject] private ItemManagerSO _itemManagerSO;
    [Inject] private InventorySO _inventorySO;
    private OptimizeScrollRect _optimizeScrollRect;


    public override void Init()
    {
        _optimizeScrollRect = GetComponent<OptimizeScrollRect>();
        _scrollRectDataSourceSO.ClearData();

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
        string sortTypeName = Enum.GetName(typeof(InventorySortType), _scrollRectDataSourceSO.sortType);
        GetText((byte)Texts.Text_SortType).text = $"Sort Type: {sortTypeName}";
    }

    private void HandleClickSortButton()
    {
        int current = (int)_scrollRectDataSourceSO.sortType;

        current = (current + 1) % Enum.GetValues(typeof(InventorySortType)).Length;

        _scrollRectDataSourceSO.sortType = (InventorySortType)current;
        UpdateSortTypeText();
        _optimizeScrollRect.ReloadData();
    }

    private void HandleUsedItemWithStackable(ItemData itemData)
    {
        _optimizeScrollRect.ReloadData();
    }

    private void HandleAddedInventoryData(ItemData addedInventoryData)
    {
        _scrollRectDataSourceSO.AddData(addedInventoryData);
    }

    private void HandleRemovedInventoryData(ItemData removedInventoryData)
    {
        _scrollRectDataSourceSO.RemoveData(removedInventoryData);
    }

    private void HandleForceChangedInventoryData(List<ItemData> inventoryDataList)
    {
        _scrollRectDataSourceSO.ClearData();
        foreach (var data in inventoryDataList)
        {
            _scrollRectDataSourceSO.AddData(data);
        }

        _optimizeScrollRect.ReloadData();
    }

    private void HandleChangedInventoryData(List<ItemData> inventoryDataList)
    {
        var existingIDs = new HashSet<Guid>();
        for (int i = 0; i < _scrollRectDataSourceSO.ItemDataList.Count; i++)
        {
            ItemData itemData = _scrollRectDataSourceSO.ItemDataList[i];
            if (itemData != null)
                existingIDs.Add(_scrollRectDataSourceSO.ItemDataList[i].uniqueID);
            else
                existingIDs.Add(Guid.Empty);
        }

        foreach (var data in inventoryDataList)
        {
            if (!existingIDs.Contains(data.uniqueID))
            {
                _scrollRectDataSourceSO.AddData(data);
            }
        }

        for (int i = _scrollRectDataSourceSO.ItemDataList.Count - 1; i >= 0; i--)
        {
            var existing = _scrollRectDataSourceSO.ItemDataList[i];
            if (existing == null) continue;
            if (!inventoryDataList.Exists(itemData => itemData?.uniqueID == existing?.uniqueID))
            {
                _scrollRectDataSourceSO.RemoveData(existing);
                _inventorySO.RemoveItem(existing);
            }
        }

        _optimizeScrollRect.ReloadData();
    }
}