using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using MemoryPack;
using PJH.Utility;
using Reflex.Attributes;
using Reflex.Extensions;
using Reflex.Injectors;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-999)]
public class PlayerStatus : MonoBehaviour, ISaveable, IPlayerStatus
{
    public event Action<PlayerStatusData> OnLoadedPlayerStatusData;
    [Inject] public SaveManagerSO SaveManagerSO { get; private set; }
    [Inject] private ItemManagerSO _itemManagerSO;
    [field: SerializeField] public int SaveID { get; private set; }
    [SerializeField] private StatOverrideListSO _statOverrideListSO;
    public PlayerStatusData PlayerStatusData { get; private set; } = new();

    private void Awake()
    {
        SaveManagerSO.RegisterSaveable(this);
        _itemManagerSO.OnItemEquipped += HandleItemEquipped;
        _itemManagerSO.OnItemUnequipped += HandleItemUnequipped;

        PlayerStatusData.statDatas = _statOverrideListSO.statOverrides.Select(x => x.CreateStat()).ToArray();
    }

    private void OnDestroy()
    {
        _itemManagerSO.OnItemEquipped -= HandleItemEquipped;
        _itemManagerSO.OnItemUnequipped -= HandleItemUnequipped;
    }

    private void HandleItemEquipped(ItemDataBase itemData)
    {
        if (!PlayerStatusData.equippedItems.ContainsValue(itemData))
            PlayerStatusData.equippedItems[itemData.detailType] = itemData;
    }

    private void HandleItemUnequipped(ItemDataBase itemData)
    {
        if (PlayerStatusData.equippedItems.ContainsValue(itemData))
            PlayerStatusData.equippedItems[itemData.detailType] = null;
    }

    public StatData GetStat(string statName)
    {
        return PlayerStatusData.statDatas.FirstOrDefault(x => x.statName == statName);
    }

    public StatData GetStat(StatData stat)
    {
        PJHDebug.Assert(stat != null, "Stats : GetStat - stat cannot be null");
        return PlayerStatusData.statDatas.FirstOrDefault(x => x.statName == stat.statName);
    }

    public bool TryGetStat(string statName, out StatData outStat)
    {
        outStat = PlayerStatusData.statDatas.FirstOrDefault(x => x.statName == statName);
        return outStat != null;
    }

    public bool TryGetStat(StatData stat, out StatData outStat)
    {
        Debug.Assert(stat != null, "Stats : GetStat - stat cannot be null");
        outStat = PlayerStatusData.statDatas.FirstOrDefault(x => x.statName == stat.statName);
        return outStat != null;
    }

    public void SetBaseValue(StatData stat, float value) => GetStat(stat).BaseValue = value;

    public void AddBaseValuePercent(StatData stat, float percent)
    {
        StatData agentStat = GetStat(stat);
        agentStat.BaseValue *= (1 + percent * .01f);
    }

    public float GetBaseValue(string statName) => GetStat(statName).BaseValue;
    public float GetBaseValue(StatData stat) => GetStat(stat).BaseValue;

    public bool HasStat(string statName) => PlayerStatusData.statDatas.Any(x => x.statName == statName);
    public bool HasStat(StatData stat) => PlayerStatusData.statDatas.Any(x => x.statName == stat.statName);

    public float IncreaseBaseValue(string statName, float value) => GetStat(statName).BaseValue += value;
    public float IncreaseBaseValue(StatData stat, float value) => GetStat(stat).BaseValue += value;

    public float DecreaseBaseValue(string statName, float value) => GetStat(statName).BaseValue -= value;
    public float DecreaseBaseValue(StatData stat, float value) => GetStat(stat).BaseValue -= value;

    public void AddValueModifier(string statName, object key, float value) =>
        GetStat(statName).AddModifyValue(key, value);

    public void AddValueModifier(StatData stat, object key, float value) => GetStat(stat).AddModifyValue(key, value);

    public void RemoveValueModifier(string statName, object key) => GetStat(statName).RemoveModifyValue(key);
    public void RemoveValueModifier(StatData stat, object key) => GetStat(stat).RemoveModifyValue(key);


    public void AddValuePercentModifier(string statName, object key, float value) =>
        GetStat(statName).AddModifyValuePercent(key, value);

    public void AddValuePercentModifier(StatData stat, object key, float value) =>
        GetStat(stat).AddModifyValuePercent(key, value);

    public void RemoveValuePercentModifier(string statName, object key) =>
        GetStat(statName).RemoveModifyValuePercent(key);

    public void RemoveValuePercentModifier(StatData stat, object key) =>
        GetStat(stat).RemoveModifyValuePercent(key);


    public void ClearAllStatModifier()
    {
        foreach (var stat in PlayerStatusData.statDatas)
        {
            stat.ClearModifier();
        }
    }

    public void ClearAllStatValueModifier()
    {
        foreach (var stat in PlayerStatusData.statDatas)
        {
            stat.ClearModifyValue();
        }
    }

    public void ClearAllStatValuePercentModifier()
    {
        foreach (var stat in PlayerStatusData.statDatas)
        {
            stat.ClearModifyValuePercent();
        }
    }


    #region saveable

    public async UniTask<byte[]> ParsingToBytes()
    {
        using var stream = new MemoryStream();
        await MemoryPackSerializer.SerializeAsync(stream, PlayerStatusData);
        return stream.ToArray();
    }

    public async UniTask ParsingFromBytes(byte[] bytes)
    {
        using var stream = new MemoryStream(bytes);
        PlayerStatusData = await MemoryPackSerializer.DeserializeAsync<PlayerStatusData>(stream);
        foreach (ItemDataBase itemData in PlayerStatusData.equippedItems.Values)
        {
            if (itemData == null) continue;
            AttributeInjector.Inject(itemData, SceneManager.GetActiveScene().GetSceneContainer());
        }
    }

    public void AllLoaded()
    {
        OnLoadedPlayerStatusData?.Invoke(PlayerStatusData);
    }

    #endregion
}