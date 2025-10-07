using System;
using UnityEngine;

[Serializable]
public class StatOverride
{
    [SerializeField] private StatSO _stat;
    [SerializeField] private bool _isUseOverride;

    [SerializeField] private float _overrideValue;


    private float GetMinValue()
    {
        return _stat.statData.MinValue;
    }

    private float GetMaxValue()
    {
        return _stat.statData.MaxValue;
    }

    public StatOverride(StatSO stat) => _stat = stat;

    public StatData CreateStat()
    {
        StatData newStat = (_stat.Clone() as StatSO).statData;
        if (_isUseOverride)
            newStat.BaseValue = _overrideValue;
        return newStat;
    }
}