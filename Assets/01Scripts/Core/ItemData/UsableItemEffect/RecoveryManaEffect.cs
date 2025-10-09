using System;
using System.Collections.Generic;
using MemoryPack;
using PJH.Utility;
using UnityEngine;

[Serializable]
[MemoryPackable]
public partial class RecoveryManaEffect : UsableItemEffect
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

        SoundManager.CreateSoundBuilder().Play(GetUseSound());
        PJHDebug.LogColorPart($"Mana Recovered: {recoveryAmount}", color: Color.green, tag: "RecoveryManaEffect");
    }
}