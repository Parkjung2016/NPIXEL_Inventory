using UnityEngine.UI;
using UnityEditor.AnimatedValues;
using UnityEditor;
using UnityEditor.UI;

[CustomEditor(typeof(OptimizeScrollRect), true)]
[CanEditMultipleObjects]
public class OptimizeScrollRectEditor : ScrollRectEditor
{
    private OptimizeScrollRect _script;
    private SerializedProperty _cellPrefab;

    protected override void OnEnable()
    {
        base.OnEnable();
        _script = (OptimizeScrollRect)target;
        _cellPrefab = serializedObject.FindProperty("cellPrefab");
    }


    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(_cellPrefab);
        _script.Segments = EditorGUILayout.IntField("Columns", _script.Segments);
        serializedObject.ApplyModifiedProperties();
        base.OnInspectorGUI();
    }
}