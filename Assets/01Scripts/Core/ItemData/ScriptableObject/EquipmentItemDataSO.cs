using System;
using System.Collections.Generic;
using MemoryPack;
using PJH.Utility;
using Reflex.Attributes;
using UnityEngine;

[MemoryPackable]
[Serializable]
public partial class EquipmentItemData : ItemDataBase, IEquipable
{
    [MemoryPackIgnore] [Inject] private PlayerStatus _playerStatus;
    public bool IsEquipped { get; set; }

    public string GetAdditionalAttributeKey()
    {
        return $"EquipmentItemData_AdditionalAttribute_{uniqueID}";
    }

    public string GetBaseAttributeKey()
    {
        return $"EquipmentItemData_{uniqueID}";
    }

    public void Equip()
    {
        PJHDebug.LogColorPart("Equip", Color.green, tag: "EquipmentItemData");
    }

    public void Unequip()
    {
        PJHDebug.LogColorPart("Unequip", Color.red, tag: "EquipmentItemData");
    }

    public override ItemDataBase Clone()
    {
        EquipmentItemData clone = new EquipmentItemData
        {
            displayName = displayName,
            detailType = detailType,
            description = description,
            ItemID = ItemID,
            itemType = itemType,
            iconKey = iconKey,
            rank = rank,
            baseAttributes = new List<ItemAttribute>(baseAttributes),
            additionalAttributes = new List<AdditionalItemAttribute>(additionalAttributes),
            uniqueID = Guid.NewGuid()
        };
        return clone;
    }
}

public class EquipmentItemDataSO : BaseItemDataSO
{
    public EquipmentItemData itemData = new();

    public override ItemDataBase GetItemData()
    {
        return itemData;
    }
}