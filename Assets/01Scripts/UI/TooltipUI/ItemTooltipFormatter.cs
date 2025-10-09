using System.Text;
using System.Text.RegularExpressions;
using PJH.Utility.Extensions;
using UnityEngine;

public static class ItemTooltipFormatter
{
    private static EnumStringMappingSO _enumStringMappingSO;
    private static ItemRankColorMappingSO _itemRankColorMappingSO;

    public static void Initialize(EnumStringMappingSO enumStringMappingSO,
        ItemRankColorMappingSO itemRankColorMappingSO)
    {
        _enumStringMappingSO = enumStringMappingSO;
        _itemRankColorMappingSO = itemRankColorMappingSO;
    }

    public static StringBuilder GetItemDisplayName(ItemDataBase item)
    {
        var sb = new StringBuilder(item.displayName);
        if (item is IStackable stackable)
        {
            sb.Append("<color=yellow>");
            sb.Append("X");
            sb.Append(stackable.StackCount);
            sb.Append("</color>");
        }

        return sb;
    }

    private static string SplitPascalCase(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;

        string pattern = @"(?<=[a-z])(?=[A-Z])|(?<=[A-Za-z])(?=[0-9])";

        return Regex.Replace(input, pattern, " ");
    }

    public static StringBuilder GetItemTypeDisplayName(ItemDataBase item)
    {
        StringBuilder sb = new StringBuilder();
        Color rankColor = _itemRankColorMappingSO[item.rank];
        sb.Append($"<color=#{ColorUtility.ToHtmlStringRGB(rankColor)}>");
        sb.Append(item.rank);
        sb.Append("</color> ");
        // 자동 띄어쓰기
        sb.Append(SplitPascalCase(item.detailType.ToString()));

        return sb;
    }

    public static bool IsUsable(ItemDataBase item) => item is IUsable;
    public static bool IsSplitable(ItemDataBase item) => item is IStackable { StackCount: > 1 };

    public static StringBuilder GetBaseInfo(ItemDataBase item)
    {
        var sb = new StringBuilder(item.baseAttributes.Count);

        for (int i = 0; i < item.baseAttributes.Count; i++)
        {
            ItemAttribute itemAttribute = item.baseAttributes[i];
            sb.Append("<size=140%>");
            sb.Append(itemAttribute.attributeValue);
            sb.Append("</size>");
            sb.Append(" ");
            sb.Append(itemAttribute.attributeName);
            if (i < item.baseAttributes.Count - 1)
                sb.AppendLine();
        }

        return sb;
    }

    public static StringBuilder GetDetailInfo(ItemDataBase item)
    {
        var sb = new StringBuilder();
        string displayItemTypeName = _enumStringMappingSO.itemTypeToString[item.itemType];
        if (!displayItemTypeName.IsNullOrEmpty())
        {
            sb.Append("<color=#EDC7C7>");
            sb.Append(displayItemTypeName);
            sb.Append("</color>");
        }

        return sb;
    }

    public static StringBuilder GetAdditionalAttributeInfo(ItemDataBase item)
    {
        var sb = new StringBuilder(item.additionalAttributes.Count + 1);
        sb.AppendLine("Modifiers");
        for (int i = 0; i < item.additionalAttributes.Count; i++)
        {
            AdditionalItemAttribute additionalAttribute = item.additionalAttributes[i];
            string operationType = "";
            sb.Append("\t");
            switch (additionalAttribute.operationType)
            {
                case OperationType.Sum:
                case OperationType.PercentAdd:
                    sb.Append("<color=green>");
                    operationType = "+";
                    break;
                case OperationType.Sub:
                case OperationType.PercentSub:
                    sb.Append("<color=red>");
                    operationType = "-";
                    break;
            }

            sb.Append("*");
            sb.Append(additionalAttribute.additionalAttribute.attributeName);
            sb.Append(": ");
            sb.Append(operationType);

            sb.Append(additionalAttribute.value);
            if (additionalAttribute.operationType == OperationType.PercentAdd ||
                additionalAttribute.operationType == OperationType.PercentSub)
                sb.Append("%");
            sb.Append("</color>");
            if (i < item.additionalAttributes.Count - 1)
                sb.AppendLine();
        }

        return sb;
    }

    public static bool HasAdditionalInfo(ItemDataBase item) => item.additionalAttributes.Count > 0;
}