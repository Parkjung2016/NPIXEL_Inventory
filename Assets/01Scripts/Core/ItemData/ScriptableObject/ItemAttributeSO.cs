using UnityEditor;
using UnityEngine;
[CreateAssetMenu(menuName = "SO/Item/Attribute/AttributeSO")]
public class ItemAttributeSO : ScriptableObject
{
    public ItemAttribute attribute = new();

    private void OnValidate()
    {
        RenameAsset();
    }

    private void RenameAsset()
    {
        if (this == null) return;
        string oldName = $"ItemAttribute_{attribute.attributeName}";
        if (oldName != name)
        {
            string assetPath = AssetDatabase.GetAssetPath(this);
            if (!string.IsNullOrEmpty(assetPath))
                AssetDatabase.RenameAsset(assetPath, oldName);
        }
    }
}