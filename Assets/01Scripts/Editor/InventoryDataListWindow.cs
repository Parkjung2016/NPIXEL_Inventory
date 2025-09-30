using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MemoryPack;
using UnityEditor;
using UnityEngine;

public class InventoryDataListWindow : EditorWindow
{
    private static InventoryDataListSO _targetSO;
    private Vector2 _leftScrollPos;
    private Vector2 _rightScrollPos;
    private int _selectedIndex = -1;

    private Type[] _itemTypes;
    private string[] _itemTypeNames;
    private int _selectedTypeIndex;

    [MenuItem("Tools/Inventory Data List")]
    public static void ShowWindow()
    {
        InventoryDataListWindow window = GetWindow<InventoryDataListWindow>("Inventory Data List");
        window.Show();
    }

    private void OnEnable()
    {
        // ItemData를 상속받은 타입 모두 찾기
        _itemTypes = Assembly.GetAssembly(typeof(ItemData))
            .GetTypes()
            .Where(t => typeof(ItemData).IsAssignableFrom(t) && !t.IsAbstract)
            .ToArray();

        _itemTypeNames = _itemTypes.Select(t => t.Name).ToArray();
    }

    private void OnGUI()
    {
        EditorGUILayout.Space();

        _targetSO = (InventoryDataListSO)EditorGUILayout.ObjectField("Target SO", _targetSO,
            typeof(InventoryDataListSO), false);

        if (_targetSO == null) return;

        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.BeginVertical(GUILayout.Width(position.width * 0.3f));
        DrawItemList();
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical(GUILayout.Width(position.width * 0.7f));
        DrawSelectedItemDetails();
        EditorGUILayout.EndVertical();

        EditorGUILayout.EndHorizontal();
    }

    private void DrawItemList()
    {
        if (_targetSO.inventoryDataList == null) return;

        EditorGUILayout.BeginHorizontal();
        {
            _selectedTypeIndex = EditorGUILayout.Popup("Item Type", _selectedTypeIndex, _itemTypeNames);
            Color prevColor = GUI.backgroundColor;
            GUI.backgroundColor = Color.green;

            if (GUILayout.Button("Add Item"))
            {
                Type selectedType = _itemTypes[_selectedTypeIndex];
                Debug.Log(selectedType);
                ItemData newItem = (ItemData)Activator.CreateInstance(selectedType);
                newItem.displayName = "New " + selectedType.Name;
                newItem.uniqueID = Guid.NewGuid();
                newItem.baseAttributes = new List<ItemAttribute>();
                newItem.additionalAttributes = new List<ItemAttributeOverride>();

                _targetSO.inventoryDataList.Add(newItem);
                _selectedIndex = _targetSO.inventoryDataList.Count - 1;
                EditorUtility.SetDirty(_targetSO);
            }

            if (_selectedIndex >= 0 &&
                _selectedIndex < _targetSO.inventoryDataList.Count)
            {
                GUI.backgroundColor = Color.red;
                if (GUILayout.Button("Delete Selected"))
                {
                    _targetSO.inventoryDataList.RemoveAt(_selectedIndex);
                    _selectedIndex = Mathf.Clamp(_selectedIndex - 1, 0, _targetSO.inventoryDataList.Count - 1);
                    EditorUtility.SetDirty(_targetSO);
                }
            }

            GUI.backgroundColor = prevColor;
        }
        EditorGUILayout.EndHorizontal();

        Rect listRect = GUILayoutUtility.GetRect(200, position.height - 50);
        GUI.Box(listRect, GUIContent.none);

        _leftScrollPos = GUI.BeginScrollView(listRect, _leftScrollPos,
            new Rect(0, 0, listRect.width - 20, _targetSO.inventoryDataList.Count * 30));
        {
            for (int i = 0; i < _targetSO.inventoryDataList.Count; i++)
            {
                var item = _targetSO.inventoryDataList[i];

                Rect buttonRect = new Rect(5, i * 30, listRect.width - 30, 25);

                Color prevColor = GUI.backgroundColor;
                if (i == _selectedIndex) GUI.backgroundColor = Color.cyan;
                if (GUI.Button(buttonRect, item.displayName))
                {
                    _selectedIndex = i;
                }

                GUI.backgroundColor = prevColor;
            }
        }
        GUI.EndScrollView();
    }

    private void DrawSelectedItemDetails()
    {
        if (_selectedIndex < 0 || _selectedIndex >= _targetSO.inventoryDataList.Count) return;

        var selectedItem = _targetSO.inventoryDataList[_selectedIndex];

        _rightScrollPos = EditorGUILayout.BeginScrollView(_rightScrollPos);

        // --- 필드 ---
        var fields = selectedItem.GetType()
            .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(f => !f.IsDefined(typeof(NonSerializedAttribute)) && !f.IsDefined(typeof(MemoryPackIgnoreAttribute)))
            .ToArray();

        foreach (var field in fields)
        {
            object value = field.GetValue(selectedItem);
            DrawField(field.FieldType, field.Name, ref value);
            field.SetValue(selectedItem, value);
        }

        // --- 프로퍼티 ---
        var properties = selectedItem.GetType()
            .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(p => p.CanRead && p.CanWrite)
            .Where(p => !p.IsDefined(typeof(NonSerializedAttribute)) && !p.IsDefined(typeof(MemoryPackIgnoreAttribute)))
            .ToArray();

        foreach (var prop in properties)
        {
            object value = prop.GetValue(selectedItem);
            DrawField(prop.PropertyType, prop.Name, ref value);
            prop.SetValue(selectedItem, value);
        }

        EditorGUILayout.EndScrollView();

        // SO 저장 표시
        if (GUI.changed)
            EditorUtility.SetDirty(_targetSO);
    }

    // 필드/프로퍼티 타입에 따라 자동으로 EditorGUILayout 생성
    private void DrawField(System.Type type, string name, ref object value)
{
    if (type == typeof(int))
        value = EditorGUILayout.IntField(name, (int)value);
    else if (type == typeof(float))
        value = EditorGUILayout.FloatField(name, (float)value);
    else if (type == typeof(string))
        value = EditorGUILayout.TextField(name, (string)value);
    else if (type.IsEnum)
        value = EditorGUILayout.EnumPopup(name, (Enum)value);
    else if (type == typeof(bool))
        value = EditorGUILayout.Toggle(name, (bool)value);
    else if (typeof(UnityEngine.Object).IsAssignableFrom(type))
        value = EditorGUILayout.ObjectField(name, (UnityEngine.Object)value, type, true);
    else if (type == typeof(Guid))
        return; // GUID는 무시
    else if (typeof(IList).IsAssignableFrom(type)) // 리스트 처리
    {
        var list = value as IList;
        if (list == null)
        {
            value = Activator.CreateInstance(type);
            list = value as IList;
        }

        EditorGUILayout.LabelField(name, EditorStyles.boldLabel);
        EditorGUI.indentLevel++;

        for (int i = 0; i < list.Count; i++)
        {
            object element = list[i];
            DrawField(element.GetType(), $"Element {i}", ref element);
            list[i] = element;

            if (GUILayout.Button($"Delete Element {i}", GUILayout.Width(100)))
            {
                list.RemoveAt(i);
                i--;
            }
        }

        if (GUILayout.Button($"Add Element to {name}"))
        {
            Type elementType = type.IsArray ? type.GetElementType() : type.GetGenericArguments()[0];
            list.Add(Activator.CreateInstance(elementType));
        }

        EditorGUI.indentLevel--;
    }
    else // 커스텀 클래스 처리
    {
        if (value == null)
            value = Activator.CreateInstance(type);

        EditorGUILayout.LabelField(name, EditorStyles.boldLabel);
        EditorGUI.indentLevel++;

        var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(f => !f.IsDefined(typeof(NonSerializedAttribute)) && !f.IsDefined(typeof(MemoryPackIgnoreAttribute)));

        foreach (var field in fields)
        {
            object fieldValue = field.GetValue(value);
            DrawField(field.FieldType, field.Name, ref fieldValue);
            field.SetValue(value, fieldValue);
        }

        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(p => p.CanRead && p.CanWrite)
            .Where(p => !p.IsDefined(typeof(NonSerializedAttribute)) && !p.IsDefined(typeof(MemoryPackIgnoreAttribute)));

        foreach (var prop in properties)
        {
            object propValue = prop.GetValue(value);
            DrawField(prop.PropertyType, prop.Name, ref propValue);
            prop.SetValue(value, propValue);
        }

        EditorGUI.indentLevel--;
    }
}
}