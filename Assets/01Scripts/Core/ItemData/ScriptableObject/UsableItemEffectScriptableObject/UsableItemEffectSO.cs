using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public abstract class UsableItemEffectSO : ScriptableObject
{
    public SoundDataSO useSound;
    public List<ItemAttributeSO> requiredAttributes = new();
    public abstract UsableItemEffect GetUsableItemEffect();

    private void OnValidate()
    {
        if (GetUsableItemEffect() == null) return;
        GetUsableItemEffect().useSoundKey = useSound.name;
        GetUsableItemEffect().requiredAttributes = new();
        foreach (var attribute in requiredAttributes)
        {
            if (attribute != null)
            {
                GetUsableItemEffect().requiredAttributes.Add(attribute.attribute);
            }
        }

        EditorUtility.SetDirty(this);
    }
}