using NUnit.Framework;
using UnityEngine;

public class InventoryTests
{
    private InventoryData inventory;


    [SetUp]
    public void Setup()
    {
        inventory = new InventoryData
        {
            inventorySlotCapacity = 5
        };
        inventory.ClearData(); // ÃÊ±âÈ­
    }

    [Test]
    public void AddItem_ShouldIncreaseSlotCount()
    {
        var item = CreateMockItem(1, "Potion");
        inventory.AddItem(item);

        Assert.AreEqual(1, inventory.currentInventoryDataList.FindAll(x => x != null).Count);
    }

    [Test]
    public void AddItem_WhenInventoryFull_ShouldReturnNull()
    {
        for (int i = 0; i < 5; i++)
            inventory.AddItem(CreateMockItem(i, $"Item{i}"));

        var result = inventory.AddItem(CreateMockItem(99, "ExtraItem"));
        Assert.IsNull(result);
        Assert.IsTrue(inventory.IsFull());
    }

    [Test]
    public void AddStackableItem_ShouldStackInsteadOfAddNew()
    {
        var item1 = CreateMockStackableItem(10, stackCount: 1);
        var item2 = CreateMockStackableItem(10, stackCount: 1);

        inventory.AddItem(item1);
        inventory.AddItem(item2);

        var addedItem = inventory.currentInventoryDataList.Find(x => x != null) as IStackable;

        Assert.AreEqual(1, inventory.currentInventoryDataList.FindAll(x => x != null).Count);
        Assert.AreEqual(2, addedItem.StackCount);
    }

    [Test]
    public void AddInventorySlotCapacity_ShouldUpdateCapacity()
    {
        inventory.AddInventorySlotCapacity(500);
    }

    [Test]
    public void SplitItem_ShouldCreateNewItem()
    {
        var item = CreateMockStackableItem(10, stackCount: 5);
        inventory.AddItem(item);
        AddInventorySlotCapacity_ShouldUpdateCapacity();
        bool result = inventory.SplitItem(item, 2);

        Assert.AreEqual(true, result);
    }

    [Test]
    public void RemoveItem_ShouldFreeSlot()
    {
        var item = CreateMockItem(1, "Potion");
        var added = inventory.AddItem(item);
        inventory.RemoveItem(added);

        Assert.AreEqual(0, inventory.currentInventoryDataList.FindAll(x => x != null).Count);
        Assert.IsFalse(inventory.IsFull());
    }

    // ==== Mock Helper Methods ====

    private ItemDataBase CreateMockItem(int id, string name)
    {
        return new ItemData
        {
            ItemID = id,
            displayName = name,
            itemType = Define.ItemType.Consumable,
            detailType = Define.ItemDetailType.Potion,
            rank = Define.ItemRank.Common
        };
    }

    private ItemDataBase CreateMockStackableItem(int id, int stackCount = 1)
    {
        return new StackableItemData
        {
            ItemID = id,
            displayName = "Potion",
            itemType = Define.ItemType.Consumable,
            detailType = Define.ItemDetailType.Potion,
            rank = Define.ItemRank.Common,
            StackCount = stackCount,
            MaxStackCount = 10
        };
    }
}