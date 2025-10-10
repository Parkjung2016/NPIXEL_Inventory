using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Item/Attribute/AttributeListSO")]
public class ItemAttributeListSO : ScriptableObject
{
    public ItemAttributeSO[] itemAttributes;

#if UNITY_EDITOR
    [ContextMenu("Auto Fill From Folder")]
    private void OnValidate()
    {
        string folderPath = "Assets/04SO/Attributes";

        string[] guids = AssetDatabase.FindAssets("t:ItemAttributeSO", new[] { folderPath });
        itemAttributes = guids
            .Select(guid => AssetDatabase.LoadAssetAtPath<ItemAttributeSO>(AssetDatabase.GUIDToAssetPath(guid)))
            .Where(asset => asset != null)
            .ToArray();

        EditorUtility.SetDirty(this);
    }
#endif
}