using PJH.Utility.Utils;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Mapping/EnumStringMappingSO")]
public class EnumStringMappingSO : ScriptableObject
{
    public SerializableDictionary<ItemType, string> itemTypeToString;
}