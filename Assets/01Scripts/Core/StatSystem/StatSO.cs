using System;
using System.Collections.Generic;
using MemoryPack;
using PJH.Utility.Extensions;
using PJH.Utility.Managers;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

[Serializable]
[MemoryPackable]
public partial class StatData
{
    public delegate void ValueChangeHandler(StatData stat, float current, float prev);

    public event ValueChangeHandler OnValueChanged;


    [Delayed] public string statName;

    [SerializeField] private string _displayName;

    public string DisplayName => _displayName;


    [SerializeField] private float _baseValue;

    [SerializeField] private float _minValue, _maxValue;

    [MemoryPackIgnore]
    private Dictionary<object, Stack<float>> _modifyValueByKeys = new Dictionary<object, Stack<float>>();

    [MemoryPackIgnore] private Dictionary<object, float> _modifyValuePercentByKeys = new();

    [MemoryPackIgnore] private float _modifiedValue = 0;
    [MemoryPackIgnore] private float _modifiedValuePercent = 0f;

    public float MaxValue
    {
        get => _maxValue;
        set => _maxValue = value;
    }

    public float MinValue
    {
        get => _minValue;
        set => _minValue = value;
    }

    public float Value
    {
        get
        {
            float value = Mathf.Clamp(_baseValue + _modifiedValue, MinValue, MaxValue);
            if (_modifiedValuePercent != 0)
            {
                value *= (1 + _modifiedValuePercent * .01f);
            }

            float roundedValue = MathF.Round(value, 1);
            return roundedValue;
        }
    }

    public bool IsMax => Mathf.Approximately(Value, MaxValue);
    public bool IsMin => Mathf.Approximately(Value, MinValue);

    public float BaseValue
    {
        get
        {
            if (_baseValue == 0)
                return 0;
            float roundedValue = (float)Math.Round(_baseValue, 1);
            return roundedValue;
        }
        set
        {
            float prevValue = Value;
            _baseValue = Mathf.Clamp(value, MinValue, MaxValue);
            TryInvokeValueChangeEvent(Value, prevValue);
        }
    }

    public bool HasModifier() => _modifyValueByKeys.Count > 0 || _modifyValuePercentByKeys.Count > 0;

    public float GetTotalModifyValue()
    {
        float modifyValue = _modifiedValue;
        modifyValue = MathF.Round(modifyValue, 1);
        return modifyValue;
    }

    public float GetTotalModifyValuePercent()
    {
        float modifyValuePercent = _modifiedValuePercent;
        modifyValuePercent = MathF.Round(modifyValuePercent, 1);
        return modifyValuePercent;
    }

    public void AddModifyValue(object key, float value)
    {
        float prevValue = Value;
        _modifiedValue += value;

        if (!_modifyValueByKeys.ContainsKey(key))
            _modifyValueByKeys.Add(key, new Stack<float>(new[] { value }));
        else
            _modifyValueByKeys[key].Push(value);

        TryInvokeValueChangeEvent(Value, prevValue);
    }

    public void RemoveModifyValue(object key)
    {
        if (_modifyValueByKeys.TryGetValue(key, out Stack<float> value))
        {
            if (value.Count <= 0) return;

            float prevValue = Value;
            _modifiedValue -= value.Pop();
            if (value.Count <= 0)
                _modifyValueByKeys.Remove(key);
            TryInvokeValueChangeEvent(Value, prevValue);
        }
    }


    public void AddModifyValuePercent(object key, float value)
    {
        if (_modifyValuePercentByKeys.ContainsKey(key)) return;
        float prevValue = Value;
        _modifiedValuePercent += value;

        _modifyValuePercentByKeys.Add(key, value);

        TryInvokeValueChangeEvent(Value, prevValue);
    }

    public void RemoveModifyValuePercent(object key)
    {
        if (_modifyValuePercentByKeys.Remove(key, out float value))
        {
            float prevValue = Value;
            _modifiedValuePercent -= value;

            TryInvokeValueChangeEvent(Value, prevValue);
        }
    }

    public void ClearModifier()
    {
        ClearModifyValue();
        ClearModifyValuePercent();
    }

    public void ClearModifyValue()
    {
        float prevValue = Value;
        _modifyValueByKeys.Clear();
        _modifiedValue = 0;
        TryInvokeValueChangeEvent(Value, prevValue);
    }

    public void ClearModifyValuePercent()
    {
        float prevValue = Value;
        _modifyValuePercentByKeys.Clear();
        _modifiedValuePercent = 0;
        TryInvokeValueChangeEvent(Value, prevValue);
    }

    private void TryInvokeValueChangeEvent(float value, float prevValue)
    {
        if (!Mathf.Approximately(value, prevValue))
            OnValueChanged?.Invoke(this, value, prevValue);
    }
}

[CreateAssetMenu(fileName = "StatSO", menuName = "SO/StatSystem/Stat")]
public class StatSO : ScriptableObject, ICloneable
{
    public StatData statData;
#if UNITY_EDITOR
    private void OnValidate()
    {
        string assetName = $"{statData.statName}Stat";
        AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(this), assetName);
    }
#endif
    public object Clone()
    {
        StatSO stat = Instantiate(this);
        stat.statData = stat.statData.DeepCopy();
        return stat;
    }
}