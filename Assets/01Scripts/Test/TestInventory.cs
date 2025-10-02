using System.Collections;
using PJH.Utility.CoroutineHelpers;
using Reflex.Attributes;
using UnityEngine;

public class TestInventory : MonoBehaviour
{
    [Inject] private InventorySO _inventorySO;
    [Inject] private InventoryDataListSO _inventoryDataListSO;
    [Inject] private SaveManagerSO _saveManagerSO;
    public InventoryUI inventoryUI;
    public BaseItemDataSO test;

    private IEnumerator Start()
    {
        yield return YieldCache.GetWaitForSeconds(.5f);
        _inventorySO.AddItem(test.GetItemData());
    }

    private void AddRandomItem()
    {
        ItemDataBase inventoryData = _inventoryDataListSO.GetRandomInventoryData();
        _inventorySO.AddItem(inventoryData);
    }

    private void RemoveItem_End()
    {
        if (_inventorySO.inventoryData.currentInventoryDataList.Count > 0)
        {
            ItemDataBase inventoryData = _inventorySO.inventoryData.currentInventoryDataList[^1];
            _inventorySO.RemoveItem(inventoryData.itemID);
        }
    }

    private void AddMaxInventoryDataLength_100()
    {
        inventoryUI.ClonedInventoryScrollRectDataSourceSO.dataSource.AddDataLength(100);
    }

    private void OnGUI()
    {
        float buttonWidth = Screen.width * 0.2f;
        float buttonHeight = Screen.height * 0.08f;
        float startX = 10;
        float startY = 10;
        float space = 10;

        if (GUI.Button(new Rect(startX, startY, buttonWidth, buttonHeight), "AddItem"))
            AddRandomItem();
        startY += buttonHeight + space;

        if (GUI.Button(new Rect(startX, startY, buttonWidth, buttonHeight), "RemoveItem_End"))
            RemoveItem_End();
        startY += buttonHeight + space;

        if (GUI.Button(new Rect(startX, startY, buttonWidth, buttonHeight), "AddMaxInventoryDataLength_100"))
            AddMaxInventoryDataLength_100();
        startY += buttonHeight + space;

        if (GUI.Button(new Rect(startX, startY, buttonWidth, buttonHeight), "Save"))
            _saveManagerSO.Save();
        startY += buttonHeight + space;

        if (GUI.Button(new Rect(startX, startY, buttonWidth, buttonHeight), "Load"))
            _saveManagerSO.Load();
    }
}