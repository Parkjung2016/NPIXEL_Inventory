public interface IEquipable
{
    string GetAdditionalAttributeKey();
    string GetBaseAttributeKey();
    bool IsEquipped { get; set; }
    void Equip();
    void Unequip();
}