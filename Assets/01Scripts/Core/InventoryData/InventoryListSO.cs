using System.Collections.Generic;
using PJH.Utility.Utils;
using Reflex.Extensions;
using Reflex.Injectors;
using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(menuName = "SO/Inventory/InventoryListSO")]
public class InventoryListSO : ScriptableObject
{
    [SerializeField] private SerializableDictionary<Define.ItemType, InventorySO> _inventorySODictionary = new();

    private Dictionary<Define.ItemType, InventorySO> _inventorySOIstanceDictionary = new();

    public InventorySO this[Define.ItemType itemType] =>
        _inventorySOIstanceDictionary.GetValueOrDefault(itemType);

    public void Init()
    {
        _inventorySOIstanceDictionary.Clear();
        foreach (var pair in _inventorySODictionary)
        {
            InventorySO instance = Instantiate(pair.Value);
            AttributeInjector.Inject(instance, SceneManager.GetActiveScene().GetSceneContainer());
            instance.Init();
            _inventorySOIstanceDictionary.Add(pair.Key, instance);
        }
    }

    public bool TryGetItemType(InventorySO inventorySO, out Define.ItemType itemType)
    {
        foreach (var pair in _inventorySOIstanceDictionary)
        {
            if (pair.Value == inventorySO)
            {
                itemType = pair.Key;
                return true;
            }
        }

        itemType = Define.ItemType.Consumable;
        return false;
    }

    public int GetItemDataIndex(ItemDataBase itemData)
    {
        if (_inventorySOIstanceDictionary.TryGetValue(itemData.itemType, out InventorySO inventorySO))
        {
            return inventorySO.inventoryData.GetItemDataIndex(itemData);
        }

        return -1;
    }

    public ItemDataBase AddItem(ItemDataBase itemData)
    {
        if (_inventorySOIstanceDictionary.TryGetValue(itemData.itemType, out InventorySO inventorySO))
        {
            return inventorySO.inventoryData.AddItem(itemData);
        }

        return null;
    }

    public void AddItems(List<ItemDataBase> itemDataList)
    {
        Dictionary<Define.ItemType, List<ItemDataBase>> itemDataDictionary = new();
        foreach (var itemData in itemDataList)
        {
            if (itemDataDictionary.ContainsKey(itemData.itemType))
            {
                itemDataDictionary[itemData.itemType].Add(itemData);
            }
            else
            {
                itemDataDictionary.Add(itemData.itemType, new List<ItemDataBase> { itemData });
            }
        }

        foreach (var pair in itemDataDictionary)
        {
            if (_inventorySOIstanceDictionary.TryGetValue(pair.Key, out InventorySO inventorySO))
            {
                inventorySO.inventoryData.AddItems(pair.Value);
            }
        }
    }

    public bool RemoveItem(ItemDataBase itemData)
    {
        if (_inventorySOIstanceDictionary.TryGetValue(itemData.itemType, out InventorySO inventorySO))
        {
            return inventorySO.inventoryData.RemoveItem(itemData);
        }

        return false;
    }

    public void AddInventorySlotCapacity(Define.ItemType inventoryType, int countToAdd)
    {
        if (_inventorySOIstanceDictionary.TryGetValue(inventoryType, out InventorySO inventorySO))
        {
            inventorySO.inventoryData.AddInventorySlotCapacity(countToAdd);
        }
    }

    public void ChangeItemDataIndex(ItemDataBase itemData, int prevIndex, int newIndex)
    {
        if (_inventorySOIstanceDictionary.TryGetValue(itemData.itemType, out InventorySO inventorySO))
        {
            inventorySO.inventoryData.ChangeItemDataIndex(itemData, prevIndex, newIndex);
        }
    }

    public void SplitItem(ItemDataBase itemData, int splitCount)
    {
        if (_inventorySOIstanceDictionary.TryGetValue(itemData.itemType, out InventorySO inventorySO))
        {
            inventorySO.inventoryData.SplitItem(itemData, splitCount);
        }
    }
}