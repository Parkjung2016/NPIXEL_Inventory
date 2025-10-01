using UnityEngine.UI;
using UnityEditor.AnimatedValues;
using UnityEditor;
using UnityEditor.UI;

[CustomEditor(typeof(OptimizeScrollRect), true)]
[CanEditMultipleObjects]
public class OptimizeScrollRectEditor : ScrollRectEditor
{
    OptimizeScrollRect _script;

    protected override void OnEnable()
    {
        base.OnEnable();
        _script = (OptimizeScrollRect)target;

    }


    public override void OnInspectorGUI()
    {
        _script.Segments = EditorGUILayout.IntField("Columns", _script.Segments);
        base.OnInspectorGUI();
    }
}