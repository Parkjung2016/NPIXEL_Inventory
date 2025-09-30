using UnityEngine;

[CreateAssetMenu]
public class ItemRankColorMappingSO : ScriptableObject
{
    public SerializableDictionary<ItemRank, Color> rankColorMapping;

    public Color this[ItemRank rank] => rankColorMapping[rank];
}