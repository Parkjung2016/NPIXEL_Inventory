using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public abstract class UsableItemEffectSO : ScriptableObject
{
    public SoundDataSO useSound;
    public List<ItemAttributeSO> requiredAttributes = new();
    public abstract UsableItemEffect GetUsableItemEffect();

#if UNITY_EDITOR
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
#endif
}