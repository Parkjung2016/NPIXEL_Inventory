using System.Collections;
using MemoryPack;
using PJH.Utility.CoroutineHelpers;
using Reflex.Attributes;
using UnityEditor;
using UnityEngine;

public class TestInventory : MonoBehaviour
{
    [Inject] private InventorySO _inventorySO;
    [Inject] private InventoryDataListSO _inventoryDataListSO;
    [Inject] private SaveManagerSO _saveManagerSO;
    public BaseItemDataSO test;

    private IEnumerator Start()
    {
        yield return YieldCache.GetWaitForSeconds(.5f);
        _inventorySO.AddItem(test.GetItemData());
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            _inventorySO.RemoveItem(1);
        }
    }

    private void AddRandomItem()
    {
        ItemDataBase inventoryData = _inventoryDataListSO.GetRandomInventoryData();
        _inventorySO.AddItem(inventoryData);
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
        if (GUI.Button(new Rect(startX, startY, buttonWidth, buttonHeight), "Save"))
            _saveManagerSO.Save();

        startY += buttonHeight + space;
        if (GUI.Button(new Rect(startX, startY, buttonWidth, buttonHeight), "Load"))
            _saveManagerSO.Load();
    }
}