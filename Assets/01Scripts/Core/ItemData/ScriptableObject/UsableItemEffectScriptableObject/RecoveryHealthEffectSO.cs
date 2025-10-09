using UnityEngine;

[CreateAssetMenu(menuName = "SO/Item/UsableItemEffect/RecoveryHealthEffectSO")]
public class RecoveryHealthEffectSO : UsableItemEffectSO
{
    private readonly RecoveryHealthEffect _recoveryHealthEffect = new();

    public override UsableItemEffect GetUsableItemEffect()
    {
        return _recoveryHealthEffect;
    }
}