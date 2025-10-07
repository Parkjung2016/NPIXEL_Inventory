using PJH.Utility;

public static class EquipmentHandler
{
    private static PlayerStatus _playerStatus;

    public static void Initialize(PlayerStatus playerStatus)
    {
        _playerStatus = playerStatus;
    }

    public static void Equip(ItemDataBase itemData, string baseModifierKey, string additionalModifierKey)
    {
        if (itemData is not IEquipable equipable)
        {
            PJHDebug.LogError($"ItemData is not Equipable: {itemData}", tag: "EquipmentHandler");
            return;
        }

        for (int i = 0; i < itemData.baseAttributes.Count; i++)
        {
            ItemAttribute attribute = itemData.baseAttributes[i];
            if (_playerStatus.HasStat(attribute.attributeName))
                _playerStatus.AddValueModifier(attribute.attributeName, baseModifierKey, attribute.attributeValue);
        }

        for (int i = 0; i < itemData.additionalAttributes.Count; i++)
        {
            AdditionalItemAttribute additionalAttribute = itemData.additionalAttributes[i];
            if (_playerStatus.HasStat(additionalAttribute.additionalAttribute.attributeName))
            {
                switch (additionalAttribute.operationType)
                {
                    case OperationType.Sum:
                        _playerStatus.AddValueModifier(additionalAttribute.additionalAttribute.attributeName,
                            additionalModifierKey, additionalAttribute.value);
                        break;
                    case OperationType.Sub:
                        _playerStatus.AddValueModifier(additionalAttribute.additionalAttribute.attributeName,
                            additionalModifierKey, -additionalAttribute.value);
                        break;
                    case OperationType.PercentAdd:
                        _playerStatus.AddValuePercentModifier(additionalAttribute.additionalAttribute.attributeName,
                            additionalModifierKey, additionalAttribute.value);
                        break;
                    case OperationType.PercentSub:
                        _playerStatus.AddValuePercentModifier(additionalAttribute.additionalAttribute.attributeName,
                            additionalModifierKey, -additionalAttribute.value);
                        break;
                }
            }
        }

        equipable.IsEquipped = true;
        equipable.Equip();
    }

    public static void Unequip(ItemDataBase itemData, string baseModifierKey, string additionalModifierKey)
    {
        if (itemData is not IEquipable equipable)
        {
            PJHDebug.LogError($"ItemData is not Equipable: {itemData}", tag: "EquipmentHandler");
            return;
        }

        for (int i = 0; i < itemData.baseAttributes.Count; i++)
        {
            ItemAttribute attribute = itemData.baseAttributes[i];
            if (_playerStatus.HasStat(attribute.attributeName))
                _playerStatus.RemoveValueModifier(attribute.attributeName, baseModifierKey);
        }

        for (int i = 0; i < itemData.additionalAttributes.Count; i++)
        {
            AdditionalItemAttribute additionalAttribute = itemData.additionalAttributes[i];
            if (_playerStatus.HasStat(additionalAttribute.additionalAttribute.attributeName))
            {
                switch (additionalAttribute.operationType)
                {
                    case OperationType.Sum:
                        _playerStatus.RemoveValueModifier(additionalAttribute.additionalAttribute.attributeName,
                            additionalModifierKey);
                        break;
                    case OperationType.Sub:
                        _playerStatus.RemoveValueModifier(additionalAttribute.additionalAttribute.attributeName,
                            additionalModifierKey);
                        break;
                    case OperationType.PercentAdd:
                        _playerStatus.RemoveValuePercentModifier(additionalAttribute.additionalAttribute.attributeName,
                            additionalModifierKey);
                        break;
                    case OperationType.PercentSub:
                        _playerStatus.RemoveValuePercentModifier(additionalAttribute.additionalAttribute.attributeName,
                            additionalModifierKey);
                        break;
                }
            }
        }

        equipable.IsEquipped = false;
        equipable.Unequip();
    }
}