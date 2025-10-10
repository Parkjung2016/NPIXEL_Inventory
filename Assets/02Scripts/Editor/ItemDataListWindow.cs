using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class ItemDataListWindow : EditorWindow
{
    private const float MinLeftPanelWidth = 400f;
    private const float LeftListTopOffset = 50f;

    private ItemDataListSO _targetSO;
    private ItemAttributeListSO _attributeListSO;
    private Vector2 _leftScrollPos;
    private Vector2 _rightScrollPos;
    private int _selectedIndex = -1;

    private Type[] _itemTypes;
    private string[] _itemTypeNames;
    private int _selectedTypeIndex;
    private float _leftPanelWidth = MinLeftPanelWidth;

    private bool _isResizing;
    private Rect _resizeHandle;
    private Rect _leftListRect;
    private Editor _cachedEditor;

    private string _aiPrompt;
    private bool _disabledGroup;

    private readonly Dictionary<Define.ItemType, bool> _itemTypeFoldoutStates = new();
    private readonly Dictionary<string, bool> _itemDetailTypeFoldoutStates = new();

    [MenuItem("Tools/Item Data List")]
    public static void ShowWindow()
    {
        GetWindow<ItemDataListWindow>("Item Data List").Show();
    }

    private void OnEnable()
    {
        _attributeListSO = ItemDataListWindowUtility.LoadAsset<ItemAttributeListSO>("04SO/ItemAttributeListSO.asset");
        _targetSO = ItemDataListWindowUtility.LoadAsset<ItemDataListSO>("04SO/ItemDataListSO.asset");

        _itemTypes = Assembly.GetAssembly(typeof(BaseItemDataSO))
            .GetTypes()
            .Where(t => typeof(BaseItemDataSO).IsAssignableFrom(t) && !t.IsAbstract)
            .ToArray();

        _itemTypeNames = _itemTypes.Select(t => t.Name).ToArray();
    }

    private void OnGUI()
    {
        EditorGUI.BeginDisabledGroup(_disabledGroup);
        {
            if (_targetSO == null)
                return;

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            {
                DrawLeftPanel();
                DrawResizeHandle();
                DrawRightPanel();
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUI.EndDisabledGroup();
    }

    private void DrawLeftPanel()
    {
        GUILayout.BeginVertical(GUILayout.Width(_leftPanelWidth));
        {
            DrawToolbar();

            _leftListRect = GUILayoutUtility.GetRect(200, position.height - LeftListTopOffset);
            GUI.Box(_leftListRect, GUIContent.none);
            _leftScrollPos = GUI.BeginScrollView(_leftListRect, _leftScrollPos,
                new Rect(0, 0, _leftListRect.width - 20, _targetSO.ItemDataList.Count * 30));

            DrawItemGroups();

            GUI.EndScrollView();
        }
        GUILayout.EndVertical();
    }

    private void DrawToolbar()
    {
        EditorGUILayout.BeginHorizontal();
        _selectedTypeIndex = EditorGUILayout.Popup("Item Type", _selectedTypeIndex, _itemTypeNames);

        Color prev = GUI.backgroundColor;
        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("Add Item"))
        {
            BaseItemDataSO itemData = ItemDataListWindowUtility.AddNewItem(_targetSO, _itemTypes[_selectedTypeIndex]);
            GUI.FocusControl(null);
            _selectedIndex = itemData.GetItemData().ItemID;
        }

        GUI.backgroundColor = Color.red;
        if (_selectedIndex >= 0 && GUILayout.Button("Delete Selected"))
            ItemDataListWindowUtility.DeleteItem(_targetSO, _selectedIndex);

        GUI.backgroundColor = prev;
        EditorGUILayout.EndHorizontal();
    }

    private void DrawItemGroups()
    {
        if (_targetSO.ItemDataList == null) return;

        float yOffset = 0f;
        Color prevColor = GUI.backgroundColor;

        var grouped = _targetSO.ItemDataList
            .Where(i => i != null)
            .GroupBy(i => i.GetItemData().itemType);

        foreach (var typeGroup in grouped)
        {
            _itemTypeFoldoutStates.TryAdd(typeGroup.Key, true);
            Rect typeRect = new(5, yOffset, _leftListRect.width - 30, 20);
            _itemTypeFoldoutStates[typeGroup.Key] =
                EditorGUI.Foldout(typeRect, _itemTypeFoldoutStates[typeGroup.Key], typeGroup.Key.ToString(), true);
            yOffset += 20;

            if (_itemTypeFoldoutStates[typeGroup.Key])
            {
                foreach (var detailGroup in typeGroup.GroupBy(i => i.GetItemData().detailType))
                {
                    string key = $"{typeGroup.Key}_{detailGroup.Key}";
                    _itemDetailTypeFoldoutStates.TryAdd(key, true);

                    Rect detailRect = new(20, yOffset, _leftListRect.width - 45, 20);
                    _itemDetailTypeFoldoutStates[key] =
                        EditorGUI.Foldout(detailRect, _itemDetailTypeFoldoutStates[key], detailGroup.Key.ToString(),
                            true);
                    yOffset += 20;

                    if (_itemDetailTypeFoldoutStates[key])
                    {
                        foreach (var item in detailGroup)
                        {
                            int index = _targetSO.ItemDataList.IndexOf(item);
                            Rect btnRect = new(35, yOffset, _leftListRect.width - 60, 20);
                            if (index == _selectedIndex)
                                GUI.backgroundColor = Color.cyan;

                            if (GUI.Button(btnRect, item.GetItemData().displayName))
                            {
                                GUI.FocusControl(null);
                                _selectedIndex = index;
                            }

                            GUI.backgroundColor = prevColor;
                            yOffset += 22;
                        }
                    }
                }
            }
        }
    }

    private void DrawResizeHandle()
    {
        _resizeHandle = new Rect(_leftPanelWidth, LeftListTopOffset, 5f, position.height);
        EditorGUI.DrawRect(_resizeHandle, Color.gray);
        EditorGUIUtility.AddCursorRect(_resizeHandle, MouseCursor.ResizeHorizontal);
        HandleResize();
    }

    private void DrawRightPanel()
    {
        if (_selectedIndex < 0 || _selectedIndex >= _targetSO.ItemDataList.Count)
            return;

        var selectedItem = _targetSO.ItemDataList[_selectedIndex];

        GUILayout.BeginVertical(GUILayout.ExpandWidth(true));
        {
            EditorGUILayout.LabelField("Generate Item Data With AI", EditorStyles.boldLabel);
            _aiPrompt = EditorGUILayout.TextField("Prompt", _aiPrompt);

            if (GUILayout.Button("Generate"))
            {
                _disabledGroup = true;
                AIItemGenerator.GenerateAsync(_aiPrompt, selectedItem, _attributeListSO,
                    onFinish: () => _disabledGroup = false);
            }

            _rightScrollPos = EditorGUILayout.BeginScrollView(_rightScrollPos);
            if (_cachedEditor == null || _cachedEditor.target != selectedItem)
                _cachedEditor = Editor.CreateEditor(selectedItem);

            _cachedEditor.OnInspectorGUI();
            EditorGUILayout.EndScrollView();

            if (GUI.changed)
                EditorUtility.SetDirty(_targetSO);
        }
        GUILayout.EndVertical();
    }

    private void HandleResize()
    {
        var e = Event.current;
        if (e.type == EventType.MouseDown && _resizeHandle.Contains(e.mousePosition))
        {
            _isResizing = true;
            e.Use();
        }

        if (_isResizing)
        {
            if (e.type == EventType.MouseDrag)
            {
                _leftPanelWidth = Mathf.Clamp(e.mousePosition.x, MinLeftPanelWidth, position.width - 100f);
                Repaint();
            }
            else if (e.type == EventType.MouseUp)
                _isResizing = false;
        }
    }
}