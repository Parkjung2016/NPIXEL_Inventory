#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Item/Attribute/AttributeSO")]
public class ItemAttributeSO : ScriptableObject
{
    public ItemAttribute attribute = new();

#if UNITY_EDITOR
    private void OnValidate()
    {
        EditorApplication.delayCall -= RenameAsset;
        EditorApplication.delayCall += RenameAsset;
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
#endif
}