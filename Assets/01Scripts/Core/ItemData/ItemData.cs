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

public enum OperationType
{
    Sum,
    Sub
}

[Serializable]
public class ItemAttribute
{
    public string attributeName;
    public float attributeValue;
}

[Serializable]
public class ItemAttributeOverride
{
    public ItemAttribute overrideAttribute;
    public OperationType operationType;
}

[MemoryPackable]
[Serializable]
public partial class ItemData
{
    public string displayName;
    public string typeName;
    public string description;

    public ItemType itemType;

    public string iconKey;
    public ItemRank rank;
    public Guid uniqueID;
    public int itemID;
    public List<ItemAttribute> baseAttributes;
    public List<ItemAttributeOverride> additionalAttributes;

    [NonSerialized] [MemoryPackIgnore] [Inject]
    private EnumStringMappingSO _enumStringMappingSO;

    public virtual string GetItemTypeDisplayName()
    {
        return typeName;
    }

    public virtual bool Usable()
    {
        bool usable = this is IUsable;
        return usable;
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
            ItemAttributeOverride attribute = additionalAttributes[i];
            string operationType = "";
            sb.Append("\t");
            switch (attribute.operationType)
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
            sb.Append(attribute.overrideAttribute.attributeName);
            sb.Append(": ");
            sb.Append(operationType);

            sb.Append(attribute.overrideAttribute.attributeValue);
            sb.Append("</color>");
            if (i < additionalAttributes.Count - 1)
                sb.AppendLine();
        }

        return sb;
    }

    public virtual bool HasAdditionalInfo() => additionalAttributes.Count > 0;

    public Sprite GetIcon() => AddressableManager.Load<Sprite>(iconKey);
}