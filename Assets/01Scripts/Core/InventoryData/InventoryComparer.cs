using System.Collections.Generic;

// InventorySortType�� ���� �ڵ忡 ���ǵǾ� �ִٰ� �����մϴ�.
// public enum InventorySortType { ByName, ByRank, ByCount, ByType, ByAll }

public interface IInventoryComparer : IComparer<ItemDataBase>
{
    // InventoryData�� SortData �޼��忡�� ���˴ϴ�.
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
        // ��������: ���� ��ũ ����
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

        // ��������: ū �� ����
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
        return a.itemType.CompareTo(b.itemType);
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