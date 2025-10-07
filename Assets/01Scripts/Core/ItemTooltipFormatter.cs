using System.Text;
using System.Text.RegularExpressions;
using PJH.Utility.Extensions;

// ItemDataBase에서 제거된 모든 UI/표현 로직을 담당
public static class ItemTooltipFormatter
{
    private static EnumStringMappingSO _enumStringMappingSO;

    public static void Initialize(EnumStringMappingSO enumStringMappingSO)
    {
        _enumStringMappingSO = enumStringMappingSO;
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

    public static string GetItemTypeDisplayName(ItemDataBase item)
    {
        string displayName = Regex.Replace(item.detailType.ToString(), "([A-Z])", " $1").Trim();
        return displayName;
    }

    // ItemDataBase가 아닌, 외부에서 인터페이스 구현 여부만 확인하도록 분리
    public static bool IsUsable(ItemDataBase item) => item is IUsable;
    public static bool IsSplitable(ItemDataBase item) => item is IStackable stackable && stackable.StackCount > 1;

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
        // 의존성 (_enumStringMappingSO)을 여기서 사용
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
        // ... (기존 GetAdditionalAttributeInfo 로직 그대로 이관) ...
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