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
    public ItemData test;

    private IEnumerator Start()
    {
        yield return YieldCache.GetWaitForSeconds(.5f);
        _inventorySO.AddItem(test);
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
        ItemData inventoryData = _inventoryDataListSO.GetRandomInventoryData();
        _inventorySO.AddItem(inventoryData);
    }

    private void OnGUI()
    {
        EditorGUILayout.BeginVertical();
        {
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("AddItem", GUILayout.Height(50)))
                AddRandomItem();

            GUILayout.Space(10);

            if (GUILayout.Button("Save", GUILayout.Height(50)))
                _saveManagerSO.Save();

            GUILayout.Space(10);

            if (GUILayout.Button("Load", GUILayout.Height(50)))
                _saveManagerSO.Load();

            GUILayout.FlexibleSpace();
        }
        EditorGUILayout.EndVertical();
    }
}