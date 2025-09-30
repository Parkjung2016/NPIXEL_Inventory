using UnityEngine;

[CreateAssetMenu]
public class EnumStringMappingSO : ScriptableObject
{
    public SerializableDictionary<ItemType, string> itemTypeToString;
}