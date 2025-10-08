using System;
using System.IO;
using Cysharp.Threading.Tasks;
using MemoryPack;
using Reflex.Attributes;
using Reflex.Extensions;
using Reflex.Injectors;
using UnityEngine;
using UnityEngine.SceneManagement;


[CreateAssetMenu(menuName = "SO/Inventory/InventorySO")]
public class InventorySO : ScriptableObject, ISaveable
{
    public event Action OnLoadedInventoryData;
    [field: SerializeField] public int SaveID { get; private set; }
    [Inject] public SaveManagerSO SaveManagerSO { get; private set; }

    public InventoryData inventoryData = new();

    public void Init()
    {
        ClearData();
        SaveManagerSO?.RegisterSaveable(this);
    }

    private void OnDisable()
    {
        SaveManagerSO?.UnregisterSaveable(this);
    }

    [ContextMenu("Clear Data")]
    public void ClearData()
    {
        inventoryData.ClearData();
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
            ItemDataBase itemData = inventoryData.currentInventoryDataList[i];
            if (itemData == null)
            {
                continue;
            }

            AttributeInjector.Inject(itemData, SceneManager.GetActiveScene().GetSceneContainer());
        }

        inventoryData.RefreshEmptySlotPriorityQueue();
    }

    public void AllLoaded()
    {
        OnLoadedInventoryData?.Invoke();
        inventoryData.OnUpdateItemCount?.Invoke();
    }

    #endregion
}