using UnityEngine.UI;
using UnityEditor.AnimatedValues;
using UnityEditor;
using UnityEditor.UI;

[CustomEditor(typeof(OptimizeScrollRect), true)]
[CanEditMultipleObjects]
public class OptimizeScrollRectEditor : ScrollRectEditor
{
    SerializedProperty _dataSource;

    OptimizeScrollRect _script;

    protected virtual void OnEnable()
    {
        base.OnEnable();
        _script = (OptimizeScrollRect)target;

        _dataSource = serializedObject.FindProperty("_dataSource");
    }

    protected virtual void OnDisable()
    {
        base.OnDisable();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(_dataSource);
        _script.Segments = EditorGUILayout.IntField("Columns", _script.Segments);
        serializedObject.ApplyModifiedProperties();
        base.OnInspectorGUI();
    }
}