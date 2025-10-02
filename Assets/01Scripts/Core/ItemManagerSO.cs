using System;
using PJH.Utility;
using Reflex.Attributes;
using UnityEngine;

[CreateAssetMenu]
public class ItemManagerSO : ScriptableObject
{
    [Inject] private InventorySO _inventorySO;
    public event Action<ItemDataBase> OnUsedItemWithStackable;

    public void UseItem(ItemDataBase itemData)
    {
        if (itemData is IUsable usable)
        {
            usable.Use();

            if (itemData is IStackable stackable)
            {
                stackable.StackCount = Mathf.Max(0, stackable.StackCount - 1);
                bool isEmpty = stackable.StackCount <= 0;
                if (isEmpty)
                {
                    PJHDebug.LogColorPart($"Item used up! :{itemData.displayName} ", Color.yellow,
                        tag: "ItemManagerSO");
                    _inventorySO.RemoveItem(itemData);
                }
                else
                {
                    OnUsedItemWithStackable?.Invoke(itemData);
                }
            }
            else
                _inventorySO.RemoveItem(itemData);
        }
    }
}