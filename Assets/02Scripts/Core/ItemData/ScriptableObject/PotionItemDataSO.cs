using System.Collections.Generic;

public class PotionItemDataSO : BaseItemDataSO
{
    public PotionItemData itemData = new();
    public List<UsableItemEffectSO> usableItemEffectSOList;

    public override ItemDataBase GetItemData()
    {
        return itemData;
    }

#if UNITY_EDITOR
    public override void OnValidate()
    {
        base.OnValidate();
        itemData.UsableItemEffects = new();
        foreach (var effect in usableItemEffectSOList)
        {
            if (effect != null)
                itemData.UsableItemEffects.Add(effect.GetUsableItemEffect());
        }
    }
#endif
}