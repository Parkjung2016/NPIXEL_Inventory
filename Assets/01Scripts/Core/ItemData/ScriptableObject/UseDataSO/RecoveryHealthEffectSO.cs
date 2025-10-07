using System;
using System.Collections.Generic;
using System.Drawing;
using MemoryPack;
using PJH.Utility;
using UnityEngine;
using Color = UnityEngine.Color;

[Serializable]
[MemoryPackable]
public partial class RecoveryHealthEffect : UsableItemEffect
{
    public override void UseItem(IList<ItemAttribute> attributes, IList<AdditionalItemAttribute> additionalAttributes)
    {
        float recoveryAmount = 0;
        foreach (var attribute in attributes)
        {
            if (requiredAttributes.Exists(x => x.attributeName == attribute.attributeName))
            {
                recoveryAmount += attribute.attributeValue;
            }
        }

        foreach (var additionalAttribute in additionalAttributes)
        {
            if (requiredAttributes.Exists(x =>
                    x.attributeName == additionalAttribute.additionalAttribute.attributeName))
            {
                recoveryAmount += additionalAttribute.value;
            }
        }

        PJHDebug.LogColorPart($"Health Recovered: {recoveryAmount}", color: Color.green, tag: "RecoveryHealthEffect");
    }
}

[CreateAssetMenu(menuName = "SO/Item/UsableItemEffect/RecoveryHealthEffectSO")]
public class RecoveryHealthEffectSO : UsableItemEffectSO
{
    private RecoveryHealthEffect _recoveryHealthEffect;

    private void OnEnable()
    {
        _recoveryHealthEffect = new();
    }

    public override UsableItemEffect GetUsableItemEffect()
    {
        return _recoveryHealthEffect;
    }
}