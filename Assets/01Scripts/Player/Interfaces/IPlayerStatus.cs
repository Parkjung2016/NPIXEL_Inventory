using System;

public interface IPlayerStatus
{
    PlayerStatusData PlayerStatusData { get; }
    event Action<PlayerStatusData> OnLoadedPlayerStatusData;
    bool TryGetStat(string statName, out StatData outStat);
    bool TryGetStat(StatData stat, out StatData outStat);
    void SetBaseValue(StatData stat, float value);
    void AddBaseValuePercent(StatData stat, float percent);
    float GetBaseValue(string statName);
    float GetBaseValue(StatData stat);
    bool HasStat(string statName);
    bool HasStat(StatData stat);
    float IncreaseBaseValue(string statName, float value);
    float IncreaseBaseValue(StatData stat, float value);
    float DecreaseBaseValue(string statName, float value);
    float DecreaseBaseValue(StatData stat, float value);
    void AddValueModifier(string statName, object key, float value);
    void AddValueModifier(StatData stat, object key, float value);
    void RemoveValueModifier(string statName, object key);
    void RemoveValueModifier(StatData stat, object key);
    void AddValuePercentModifier(string statName, object key, float value);
    void AddValuePercentModifier(StatData stat, object key, float value);
    void RemoveValuePercentModifier(string statName, object key);
    void RemoveValuePercentModifier(StatData stat, object key);
    void ClearAllStatModifier();
    void ClearAllStatValueModifier();
    void ClearAllStatValuePercentModifier();
}