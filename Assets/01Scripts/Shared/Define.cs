public class Define
{
    public enum UIEvent
    {
        Click,
        BeginDrag,
        Drag,
        EndDrag,
        Drop
    }

    public enum ItemRank
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }

    public enum ItemType
    {
        Equipment,
        Consumable,
        Material
    }

    public enum ItemDetailType
    {
        Armor,
        Boots,
        Helmet,
        Leggings,
        MeleeWeapon,
        Shield,
        Potion,
        Material
    }

    public enum InventorySortType
    {
        ByName,
        ByRank,
        ByCount,
        ByType,
        ByAll
    }
}