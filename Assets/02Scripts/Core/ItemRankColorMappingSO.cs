using PJH.Utility.Utils;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Item/ItemRankColorMappingSO")]
public class ItemRankColorMappingSO : ScriptableObject
{
    public SerializableDictionary<Define.ItemRank, Color> rankColorMapping;

    public Color this[Define.ItemRank rank] => rankColorMapping[rank];

    public Color GetOutlineColor(Define.ItemRank rank)
    {
        Color rankColor = rankColorMapping[rank];
        Color.RGBToHSV(rankColor, out float h, out float s, out float v);

        v = Mathf.Clamp01(v * 1.8f);

        Color brighterRankColor = Color.HSVToRGB(h, s, v);
        return brighterRankColor;
    }
}