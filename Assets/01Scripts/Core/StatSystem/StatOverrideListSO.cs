using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StatOverrideListSO", menuName = "SO/StatSystem/StatOverrideList")]
public class StatOverrideListSO : ScriptableObject
{
    public List<StatOverride> statOverrides = new();
}