using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public static class OptimizeScrollViewEditorTool
{
    private const string prefabPath = "Assets/OptimizedScrollRect/02Prefabs/OptimizedScrollView.prefab";

    [MenuItem("GameObject/UI/Optimized Scroll View")]
    private static void CreateRecyclableScrollView()
    {
        GameObject selected = Selection.activeGameObject;

        if (!selected || !(selected.transform is RectTransform))
        {
            selected = Object.FindAnyObjectByType<Canvas>().gameObject;
        }

        if (!selected) return;

        GameObject asset = AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject)) as GameObject;

        GameObject item = Object.Instantiate(asset);
        item.name = "Optimized Scroll View";

        item.transform.SetParent(selected.transform);
        item.transform.localPosition = Vector3.zero;
        Selection.activeGameObject = item;
        Undo.RegisterCreatedObjectUndo(item, "Create Optimized Scroll view");
    }
}