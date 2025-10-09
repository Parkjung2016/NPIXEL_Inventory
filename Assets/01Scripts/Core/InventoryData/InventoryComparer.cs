using System.Collections.Generic;

/// <summary>
/// 인벤토리 내 아이템 정렬을 위한 비교자 인터페이스
/// </summary>
public interface IInventoryComparer : IComparer<ItemDataBase>
{
}

public struct NameComparer : IInventoryComparer
{
    public int Compare(ItemDataBase a, ItemDataBase b)
    {
        if (a == null && b == null) return 0;
        if (a == null) return 1;
        if (b == null) return -1;
        return string.Compare(a.displayName, b.displayName, System.StringComparison.Ordinal);
    }
}

public struct RankComparer : IInventoryComparer
{
    public int Compare(ItemDataBase a, ItemDataBase b)
    {
        if (a == null && b == null) return 0;
        if (a == null) return 1;
        if (b == null) return -1;
        // 내림차순: 높은 랭크 먼저
        return b.rank.CompareTo(a.rank);
    }
}

public struct CountComparer : IInventoryComparer
{
    public int Compare(ItemDataBase a, ItemDataBase b)
    {
        if (a == null && b == null) return 0;
        if (a == null) return 1;
        if (b == null) return -1;

        IStackable stackableA = a as IStackable;
        IStackable stackableB = b as IStackable;

        int itemCountA = stackableA?.StackCount ?? 0;
        int itemCountB = stackableB?.StackCount ?? 0;

        // 내림차순: 큰 수 먼저
        return itemCountB.CompareTo(itemCountA);
    }
}

public struct TypeComparer : IInventoryComparer
{
    public int Compare(ItemDataBase a, ItemDataBase b)
    {
        if (a == null && b == null) return 0;
        if (a == null) return 1;
        if (b == null) return -1;
        return a.detailType.CompareTo(b.detailType);
    }
}

public struct AllComparer : IInventoryComparer
{
    private static readonly RankComparer _rankComparer = new RankComparer();
    private static readonly CountComparer _countComparer = new CountComparer();
    private static readonly NameComparer _nameComparer = new NameComparer();

    public int Compare(ItemDataBase a, ItemDataBase b)
    {
        if (a == null && b == null) return 0;
        if (a == null) return 1;
        if (b == null) return -1;

        int rankCompare = _rankComparer.Compare(a, b);
        if (rankCompare != 0) return rankCompare;

        int countCompare = _countComparer.Compare(a, b);
        if (countCompare != 0) return countCompare;

        return _nameComparer.Compare(a, b);
    }
}