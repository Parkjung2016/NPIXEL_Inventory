using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using MemoryPack;
using Reflex.Attributes;
using Reflex.Extensions;
using Reflex.Injectors;
using UnityEngine;
using UnityEngine.SceneManagement;
using ZLinq;

public delegate void ChangedInventoryDataEvent(List<ItemDataBase> inventoryDataList);

public delegate void ForceChangedInventoryDataEvent(List<ItemDataBase> inventoryDataList);

public delegate void AddedInventoryDataEvent(ItemDataBase addedInventoryData);

public delegate void RemovedInventoryDataEvent(ItemDataBase removedInventoryData);

[MemoryPackable]
[Serializable]
public partial class InventoryData
{
    public List<ItemDataBase> currentInventoryDataList = new();
}

[CreateAssetMenu]
public class InventorySO : ScriptableObject, ISaveable
{
    public ChangedInventoryDataEvent OnChangedInventoryData;
    public ForceChangedInventoryDataEvent OnForceChangedInventoryData;
    public AddedInventoryDataEvent OnAddedInventoryData;
    public RemovedInventoryDataEvent OnRemovedInventoryData;
    [field: SerializeField] public int SaveID { get; private set; }
    [Inject] public SaveManagerSO SaveManagerSO { get; private set; }

    public InventoryData inventoryData = new();

    public void Init()
    {
        ClearData();
        SaveManagerSO.RegisterSaveable(this);
    }

    [ContextMenu("Clear Data")]
    private void ClearData()
    {
        inventoryData.currentInventoryDataList.Clear();
    }

    public void AddItem(ItemDataBase itemData)
    {
        IStackable stackable = itemData as IStackable;
        if (inventoryData.currentInventoryDataList.Count > 0 && stackable != null)
        {
            List<ItemDataBase> existingItems =
                inventoryData.currentInventoryDataList.FindAll(item => item.itemID == itemData.itemID);
            if (existingItems != null)
            {
                stackable = existingItems
                    .AsValueEnumerable().OfType<IStackable>()
                    .FirstOrDefault(s => s.StackCount < s.MaxStackCount);
                if (stackable != null)
                {
                    stackable.StackCount++;
                    OnChangedInventoryData?.Invoke(inventoryData.currentInventoryDataList);
                    return;
                }
            }
        }

        ItemDataBase dataInstance = itemData.DeepCopy();
        AttributeInjector.Inject(dataInstance, SceneManager.GetActiveScene().GetSceneContainer());
        stackable = dataInstance as IStackable;
        if (stackable != null)
        {
            stackable.StackCount = 1;
        }

        dataInstance.uniqueID = Guid.NewGuid();
        inventoryData.currentInventoryDataList.Add(dataInstance);
        OnAddedInventoryData?.Invoke(dataInstance);
        OnChangedInventoryData?.Invoke(inventoryData.currentInventoryDataList);
    }

    public void Split(ItemDataBase itemData, int splitCount)
    {
        if (itemData is IStackable stackable)
        {
            if (splitCount <= 0 || splitCount >= stackable.StackCount)
            {
                Debug.LogWarning("Invalid split count");
                return;
            }

            ItemDataBase newItem = itemData.DeepCopy();
            AttributeInjector.Inject(newItem, SceneManager.GetActiveScene().GetSceneContainer());
            (newItem as IStackable).StackCount = splitCount;
            stackable.StackCount -= splitCount;
            inventoryData.currentInventoryDataList.Add(newItem);
            OnAddedInventoryData?.Invoke(newItem);
            OnChangedInventoryData?.Invoke(inventoryData.currentInventoryDataList);
        }
        else
        {
            Debug.LogWarning("Item is not stackable");
        }
    }

    public void RemoveItem(ItemDataBase dataToRemove)
    {
        if (inventoryData.currentInventoryDataList.Remove(dataToRemove))
        {
            OnRemovedInventoryData?.Invoke(dataToRemove);
            OnChangedInventoryData?.Invoke(inventoryData.currentInventoryDataList);
        }
        else
            Debug.LogWarning($"Item with ID {dataToRemove?.uniqueID} not found in inventory.");
    }

    public void RemoveItem(int index)
    {
        if (index >= 0 && index < inventoryData.currentInventoryDataList.Count)
        {
            ItemDataBase removedItem = inventoryData.currentInventoryDataList[index];
            inventoryData.currentInventoryDataList.RemoveAt(index);
            OnRemovedInventoryData?.Invoke(removedItem);
            OnChangedInventoryData?.Invoke(inventoryData.currentInventoryDataList);
        }
        else
        {
            Debug.LogWarning($"Index {index} is out of range. Cannot remove item.");
        }
    }

    #region saveable

    public async UniTask<byte[]> ParsingToBytes()
    {
        using var stream = new MemoryStream();
        await MemoryPackSerializer.SerializeAsync(stream, inventoryData);
        return stream.ToArray();
    }

    public async UniTask ParsingFromBytes(byte[] bytes)
    {
        using var stream = new MemoryStream(bytes);
        inventoryData = await MemoryPackSerializer.DeserializeAsync<InventoryData>(stream);
        for (int i = 0; i < inventoryData.currentInventoryDataList.Count; i++)
        {
            AttributeInjector.Inject(inventoryData.currentInventoryDataList[i],
                SceneManager.GetActiveScene().GetSceneContainer());
        }

        OnForceChangedInventoryData?.Invoke(inventoryData.currentInventoryDataList);
    }

    #endregion
}