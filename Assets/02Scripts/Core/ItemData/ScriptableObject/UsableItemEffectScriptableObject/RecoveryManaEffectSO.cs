using UnityEngine;

[CreateAssetMenu(menuName = "SO/Item/UsableItemEffect/RecoveryManaEffectSO")]
public class RecoveryManaEffectSO : UsableItemEffectSO
{
    private readonly RecoveryManaEffect _recoveryManaEffect = new();


    public override UsableItemEffect GetUsableItemEffect()
    {
        return _recoveryManaEffect;
    }
}