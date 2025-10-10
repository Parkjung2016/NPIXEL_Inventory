using System.Collections.Generic;
using System.Linq;
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
        inventory.ClearData(); // 데이터 초기화
    }

    [Test]
    public void AddItem_ShouldIncreaseSlotCount()
    {
        var item = CreateMockItem(1, "Potion");
        inventory.AddItem(item);

        Assert.AreEqual(1, inventory.currentInventorySlotCount);
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

        IStackable addedItem = inventory.AddItem(item1) as IStackable;
        inventory.AddItem(item2);


        Assert.AreEqual(1, inventory.currentInventorySlotCount);
        Assert.AreEqual(2, addedItem.StackCount);
    }

    [Test]
    public void SplitItem_ShouldCreateNewItem()
    {
        var item = CreateMockStackableItem(10, stackCount: 5);
        inventory.AddItem(item);
        inventory.AddInventorySlotCapacity(20);

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

    [Test]
    public void ChangeItemDataIndex_ShouldSwapItemsBetweenSlots()
    {
        inventory.AddInventorySlotCapacity(20);
        var itemA = CreateMockItem(1, "Sword");
        var itemB = CreateMockItem(2, "Shield");

        var addedA = inventory.AddItem(itemA);
        var addedB = inventory.AddItem(itemB);

        int indexA = inventory.GetItemDataIndex(addedA);
        int indexB = inventory.GetItemDataIndex(addedB);

        inventory.ChangeItemDataIndex(addedA, indexA, indexB);

        // 위치 바뀌었는지 확인
        Assert.AreEqual(addedA, inventory.GetItemDataAt(indexB));
        Assert.AreEqual(addedB, inventory.GetItemDataAt(indexA));
    }

    [Test]
    public void ChangeItemDataIndex_SameStackableItem_ShouldMergeStacks()
    {
        var item1 = CreateMockStackableItem(100, 2);
        var item2 = CreateMockStackableItem(100, 3);

        var added1 = inventory.AddItem(item1);
        ItemDataBase itemData = inventory.AddItem(item2);
        Assert.AreEqual(null, itemData);
        bool result = inventory.InternalSplitItem(added1, 3, out var newItemData);
        Assert.AreEqual(true, result);

        int index1 = inventory.GetItemDataIndex(added1);
        int index2 = inventory.GetItemDataIndex(newItemData);
        inventory.ChangeItemDataIndex(newItemData, index2, index1);

        var mergedItem = inventory.GetItemDataAt(index1) as IStackable;

        Assert.AreEqual(5, mergedItem.StackCount);
        Assert.IsNull(inventory.GetItemDataAt(index2)); // 하나로 합쳐졌으니까 null
    }

    [Test]
    public void SortData_ByName_ShouldSortAlphabetically()
    {
        var itemC = CreateMockItem(1, "C_Item");
        var itemA = CreateMockItem(2, "A_Item");
        var itemB = CreateMockItem(3, "B_Item");

        inventory.AddItems(new List<ItemDataBase> { itemC, itemA, itemB });
        inventory.sortType = Define.InventorySortType.ByName;
        inventory.SortData();

        var resultNames = inventory.currentInventoryDataList
            .Where(x => x != null)
            .Select(x => x.displayName)
            .ToList();

        CollectionAssert.AreEqual(new List<string> { "A_Item", "B_Item", "C_Item" }, resultNames);
    }

    [Test]
    public void StackAll_ShouldMergeAllPossibleStacks()
    {
        inventory.ClearData();
        var item1 = CreateMockStackableItem(10, 3);
        var item2 = CreateMockStackableItem(10, 4);
        var item3 = CreateMockStackableItem(10, 2);
        var addedItem = inventory.AddItem(item1);
        inventory.AddItems(new List<ItemDataBase> { item2, item3 });
        inventory.SplitItem(addedItem, 4);
        inventory.SplitItem(addedItem, 2);
        Assert.AreEqual(3, (addedItem as IStackable).StackCount);
        Assert.AreEqual(3, inventory.currentInventorySlotCount);
        inventory.StackAll();

        var stacks = inventory.currentInventoryDataList
            .Where(x => x is IStackable)
            .Cast<IStackable>()
            .ToList();

        // 합쳐져서 하나로 남아야 함
        Assert.AreEqual(1, stacks.Count);
        Assert.AreEqual(9, stacks[0].StackCount);
    }

    [Test]
    public void FindEmptySlotIndex_ShouldReturnCorrectIndex()
    {
        var item = CreateMockItem(1, "Sword");
        inventory.AddItem(item);

        int emptyIndex = inventory.FindEmptySlotIndex();
        Assert.AreNotEqual(-1, emptyIndex);
        Assert.IsTrue(inventory.GetItemDataAt(emptyIndex) == null);
    }

    [Test]
    public void AddInventorySlotCapacity_ShouldIncreaseCapacityAndEmptySlots()
    {
        int prevCapacity = inventory.inventorySlotCapacity;
        inventory.AddInventorySlotCapacity(3);

        Assert.AreEqual(prevCapacity + 3, inventory.inventorySlotCapacity);
        Assert.AreEqual(inventory.inventorySlotCapacity, inventory.currentInventoryDataList.Count);
    }

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