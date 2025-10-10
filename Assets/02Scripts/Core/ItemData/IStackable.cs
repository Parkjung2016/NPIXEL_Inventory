public interface IStackable
{
    int ItemID { get; }
    int StackCount { get; set; }
    int MaxStackCount { get; set; }
}