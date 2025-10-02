using System;
using System.Collections.Generic;
using System.Text;
using MemoryPack;
using PJH.Utility.Extensions;
using PJH.Utility.Managers;
using Reflex.Attributes;
using UnityEngine;


public enum ItemRank
{
    Low,
    Middle,
    High
}

public enum ItemType
{
    Equipment,
    Consumable,
    Material
}

[MemoryPackable]
[MemoryPackUnion(0, typeof(ItemData))]
[MemoryPackUnion(1, typeof(PotionItemData))]
[Serializable]
public abstract partial class ItemDataBase
{
    public string displayName;
    public string typeName;
    public string description;
    public int itemID;

    public ItemType itemType;

    [HideInInspector] public string iconKey;
    public ItemRank rank;
    public Guid uniqueID;
    [HideInInspector] public List<ItemAttribute> baseAttributes;
    [HideInInspector] public List<AdditionalItemAttribute> additionalAttributes;

    [NonSerialized] [MemoryPackIgnore] [Inject]
    private EnumStringMappingSO _enumStringMappingSO;

    public void SetEnumStringMappingSO(EnumStringMappingSO enumStringMappingSO)
    {
        _enumStringMappingSO = enumStringMappingSO;
    }

    public virtual StringBuilder GetItemDisplayName()
    {
        var sb = new StringBuilder(displayName);
        if (this is IStackable stackable)
        {
            sb.Append("<color=yellow>");
            sb.Append("X");
            sb.Append(stackable.StackCount);
            sb.Append("</color>");
        }

        return sb;
    }

    public virtual string GetItemTypeDisplayName()
    {
        return typeName;
    }

    public virtual bool Usable()
    {
        bool usable = this is IUsable;
        return usable;
    }

    public virtual bool Splitable()
    {
        if (this is IStackable splitable)
        {
            if (splitable.StackCount > 1)
                return true;
        }

        return false;
    }

    public virtual StringBuilder GetBaseInfo()
    {
        var sb = new StringBuilder(baseAttributes.Count);

        for (int i = 0; i < baseAttributes.Count; i++)
        {
            ItemAttribute itemAttribute = baseAttributes[i];
            sb.Append("<size=140%>");
            sb.Append(itemAttribute.attributeValue);
            sb.Append("</size>");
            sb.Append(" ");
            sb.Append(itemAttribute.attributeName);
            if (i < baseAttributes.Count - 1)
                sb.AppendLine();
        }

        return sb;
    }

    public virtual StringBuilder GetDetailInfo()
    {
        var sb = new StringBuilder();
        Debug.Log(uniqueID);
        string displayItemTypeName = _enumStringMappingSO.itemTypeToString[itemType];
        if (!displayItemTypeName.IsNullOrEmpty())
        {
            sb.Append("<color=#EDC7C7>");
            sb.Append(displayItemTypeName);
            sb.Append("</color>");
        }

        return sb;
    }

    public virtual StringBuilder GetAdditionalAttributeInfo()
    {
        var sb = new StringBuilder(additionalAttributes.Count + 1);
        sb.AppendLine("Modifiers");
        for (int i = 0; i < additionalAttributes.Count; i++)
        {
            AdditionalItemAttribute additionalAttribute = additionalAttributes[i];
            string operationType = "";
            sb.Append("\t");
            switch (additionalAttribute.operationType)
            {
                case OperationType.Sum:
                    sb.Append("<color=green>");
                    operationType = "+";
                    break;
                case OperationType.Sub:
                    sb.Append("<color=red>");
                    operationType = "-";
                    break;
            }

            sb.Append("*");
            sb.Append(additionalAttribute.additionalAttribute.attributeName);
            sb.Append(": ");
            sb.Append(operationType);

            sb.Append(additionalAttribute.value);
            sb.Append("</color>");
            if (i < additionalAttributes.Count - 1)
                sb.AppendLine();
        }

        return sb;
    }

    public virtual bool HasAdditionalInfo() => additionalAttributes.Count > 0;

    public Sprite GetIcon()
    {
        return AddressableManager.Load<Sprite>(iconKey);
    }
}