using System.Collections;
using System.Collections.Generic;
using PJH.Utility.CoroutineHelpers;
using Reflex.Attributes;
using UnityEngine;
using ZLinq;

public class TestInventory : MonoBehaviour
{
    public BaseItemDataSO test;
    public InventoryUI inventoryUI;
    [Inject] private ItemDataListSO _itemDataListSo;
    [Inject] private InventoryListSO _inventoryListSO;
    [Inject] private SaveManagerSO _saveManagerSO;
    private string _inputCountToAddItem = "1";
    private string _inputCountToAddInventorySlotCapacity = "100";

    private IEnumerator Start()
    {
        yield return YieldCache.GetWaitForSeconds(.5f);
        ItemDataBase itemData = test.GetItemData();
        _inventoryListSO.AddItem(itemData);
    }

    private void AddRandomItem()
    {
        int addCount = int.Parse(_inputCountToAddItem);
        List<ItemDataBase> itemDataList = new List<ItemDataBase>();
        for (int i = 0; i < addCount; i++)
        {
            ItemDataBase itemData = _itemDataListSo.GetRandomItemData(inventoryUI.InventoryType);
            itemDataList.Add(itemData);
        }

        _inventoryListSO.AddItems(itemDataList);
    }

    private void RemoveItem_End()
    {
        InventorySO inventorySO = _inventoryListSO[inventoryUI.InventoryType];
        ItemDataBase itemData = inventorySO.inventoryData.currentInventoryDataList
            .AsValueEnumerable()
            .Where(item => item != null).LastOrDefault();
        inventorySO.inventoryData.RemoveItem(itemData);
    }

    private void AddInventorySlotCapacity()
    {
        int countToAdd = int.Parse(_inputCountToAddInventorySlotCapacity);
        _inventoryListSO.AddInventorySlotCapacity(inventoryUI.InventoryType, countToAdd);
    }

    private void OnGUI()
    {
        float buttonWidth = Screen.width * 0.2f;
        float buttonHeight = Screen.height * 0.08f;
        float textFieldWidth = buttonWidth;
        float textFieldHeight = buttonHeight * .5f;
        float startX = 10;
        float startY = 10;
        float space = 10;

        if (GUI.Button(new Rect(startX, startY, buttonWidth, buttonHeight), $"AddItem_{_inputCountToAddItem}"))
            AddRandomItem();
        startY += buttonHeight + space;
        _inputCountToAddItem =
            GUI.TextField(new Rect(startX, startY, textFieldWidth, textFieldHeight), _inputCountToAddItem);
        startY += buttonHeight + space;


        if (GUI.Button(new Rect(startX, startY, buttonWidth, buttonHeight), "RemoveItem_End"))
            RemoveItem_End();
        startY += buttonHeight + space;

        if (GUI.Button(new Rect(startX, startY, buttonWidth, buttonHeight),
                $"AddInventorySlotCapacity_{_inputCountToAddInventorySlotCapacity}"))
            AddInventorySlotCapacity();
        startY += buttonHeight + space;
        _inputCountToAddInventorySlotCapacity =
            GUI.TextField(new Rect(startX, startY, textFieldWidth, textFieldHeight),
                _inputCountToAddInventorySlotCapacity);
        startY += buttonHeight + space;

        if (GUI.Button(new Rect(startX, startY, buttonWidth, buttonHeight), "Save"))
            _saveManagerSO.Save();
        startY += buttonHeight + space;

        if (GUI.Button(new Rect(startX, startY, buttonWidth, buttonHeight), "Load"))
            _saveManagerSO.Load();
    }
}