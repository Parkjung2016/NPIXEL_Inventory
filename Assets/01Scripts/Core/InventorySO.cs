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

public delegate void ChangedInventoryDataEvent(List<ItemData> inventoryDataList);

public delegate void ForceChangedInventoryDataEvent(List<ItemData> inventoryDataList);

public delegate void AddedInventoryDataEvent(ItemData addedInventoryData);

public delegate void RemovedInventoryDataEvent(ItemData removedInventoryData);

[MemoryPackable]
[Serializable]
public partial class InventoryData
{
    public List<ItemData> currentInventoryDataList = new();
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

    public void AddItem(ItemData itemData)
    {
        IStackable stackable = itemData as IStackable;
        if (inventoryData.currentInventoryDataList.Count > 0 && stackable != null)
        {
            List<ItemData> existingItems =
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

        ItemData dataInstance = itemData.DeepCopy();
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

    public void RemoveItem(ItemData dataToRemove)
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
            ItemData removedItem = inventoryData.currentInventoryDataList[index];
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
        OnForceChangedInventoryData?.Invoke(inventoryData.currentInventoryDataList);
    }

    #endregion
}