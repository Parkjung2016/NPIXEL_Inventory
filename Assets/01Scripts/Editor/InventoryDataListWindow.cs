using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class InventoryDataListWindow : EditorWindow
{
    private const float minLeftPanelWidth = 350f; // 최소 너비
    private const float leftListTopOffset = 50;
    private static InventoryDataListSO _targetSO;
    private Vector2 _leftScrollPos;
    private Vector2 _rightScrollPos;
    private int _selectedIndex = -1;

    private Type[] _itemTypes;
    private string[] _itemTypeNames;
    private int _selectedTypeIndex;
    private float _leftPanelWidth; // 초기 왼쪽 패널 너비
    private bool _isResizing = false;
    private Rect _resizeHandle;
    private Rect _leftListRect;

    [MenuItem("Tools/Inventory Data List")]
    public static void ShowWindow()
    {
        GetWindow<InventoryDataListWindow>("Inventory Data List").Show();
    }

    private void OnEnable()
    {
        _leftPanelWidth = minLeftPanelWidth;
        _itemTypes = Assembly.GetAssembly(typeof(BaseItemDataSO))
            .GetTypes()
            .Where(t => typeof(BaseItemDataSO).IsAssignableFrom(t) && !t.IsAbstract)
            .ToArray();
        _itemTypeNames = _itemTypes.Select(t => t.Name).ToArray();
    }

    private void OnGUI()
    {
        EditorGUILayout.Space();
        _targetSO = (InventoryDataListSO)EditorGUILayout.ObjectField(
            "Target SO", _targetSO, typeof(InventoryDataListSO), false);

        if (_targetSO == null) return;

        EditorGUILayout.BeginHorizontal();
        {
            // 좌측 패널
            GUILayout.BeginVertical(GUILayout.Width(_leftPanelWidth));
            {
                DrawLeftPanel();
            }
            EditorGUILayout.EndVertical();

            // --- 리사이즈 핸들 ---
            _resizeHandle = new Rect(_leftPanelWidth, leftListTopOffset, 5f, position.height);
            EditorGUI.DrawRect(_resizeHandle, Color.gray); // 시각적인 구분
            EditorGUIUtility.AddCursorRect(_resizeHandle, MouseCursor.ResizeHorizontal);

            HandleResize(); // 드래그 처리

            // 우측 패널
            GUILayout.BeginVertical(GUILayout.ExpandWidth(true));
            {
                DrawRightPanel();
            }
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndHorizontal();
    }

    // -----------------------------
    // 좌측 패널
    // -----------------------------
    private void DrawLeftPanel()
    {
        if (_targetSO.inventoryDataList == null) return;

        EditorGUILayout.BeginHorizontal();
        _selectedTypeIndex = EditorGUILayout.Popup("Item Type", _selectedTypeIndex, _itemTypeNames);

        Color prevColor = GUI.backgroundColor;
        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("Add Item")) AddNewItem();

        if (_selectedIndex >= 0 && _selectedIndex < _targetSO.inventoryDataList.Count)
        {
            GUI.backgroundColor = Color.red;
            if (GUILayout.Button("Delete Selected")) DeleteSelectedItem();
        }

        GUI.backgroundColor = prevColor;
        EditorGUILayout.EndHorizontal();
        _leftListRect = GUILayoutUtility.GetRect(200, position.height - leftListTopOffset);
        GUI.Box(_leftListRect, GUIContent.none);

        _leftScrollPos = GUI.BeginScrollView(_leftListRect, _leftScrollPos,
            new Rect(0, 0, _leftListRect.width - 20, _targetSO.inventoryDataList.Count * 30));

        for (int i = 0; i < _targetSO.inventoryDataList.Count; i++)
        {
            var item = _targetSO.inventoryDataList[i];
            if (item == null)
            {
                _targetSO.inventoryDataList.RemoveAt(i);
                continue;
            }

            Rect buttonRect = new Rect(5, i * 30, _leftListRect.width - 30, 25);

            if (i == _selectedIndex) GUI.backgroundColor = Color.cyan;

            if (GUI.Button(buttonRect, item.GetItemData().displayName))
                _selectedIndex = i;

            GUI.backgroundColor = prevColor;
        }

        GUI.EndScrollView();
    }

    private void AddNewItem()
    {
        Type selectedType = _itemTypes[_selectedTypeIndex];
        BaseItemDataSO newItem = (BaseItemDataSO)ScriptableObject.CreateInstance(selectedType);

        newItem.name = $"New {selectedType.Name}"; // 에셋 이름 지정
        ItemData itemData = newItem.GetItemData();
        itemData.displayName = selectedType.Name;
        itemData.itemID = _targetSO.inventoryDataList.Count;

        // Assets 폴더에 에셋으로 저장
        string path = $"Assets/03SO/InventoryData/{newItem.name}_{Guid.NewGuid()}.asset";
        AssetDatabase.CreateAsset(newItem, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        _targetSO.inventoryDataList.Add(newItem);
        _selectedIndex = _targetSO.inventoryDataList.Count - 1;

        EditorUtility.SetDirty(_targetSO);
    }

    private void DeleteSelectedItem()
    {
        if (_selectedIndex < 0 || _selectedIndex >= _targetSO.inventoryDataList.Count)
            return;

        BaseItemDataSO itemToDelete = _targetSO.inventoryDataList[_selectedIndex];

        // 리스트에서 제거
        _targetSO.inventoryDataList.RemoveAt(_selectedIndex);

        // 에셋 삭제
        if (itemToDelete != null)
        {
            string assetPath = AssetDatabase.GetAssetPath(itemToDelete);
            if (!string.IsNullOrEmpty(assetPath))
            {
                AssetDatabase.DeleteAsset(assetPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

        _selectedIndex = Mathf.Clamp(_selectedIndex - 1, 0, _targetSO.inventoryDataList.Count - 1);
        EditorUtility.SetDirty(_targetSO);
    }

    private Editor _cachedEditor;

    // -----------------------------
    // 우측 패널
    // -----------------------------
    private void DrawRightPanel()
    {
        if (_selectedIndex < 0 || _selectedIndex >= _targetSO.inventoryDataList.Count) return;

        var selectedItem = _targetSO.inventoryDataList[_selectedIndex];

        _rightScrollPos = EditorGUILayout.BeginScrollView(_rightScrollPos);

        if (_cachedEditor == null || _cachedEditor.target != selectedItem)
        {
            _cachedEditor = Editor.CreateEditor(selectedItem);
        }

        _cachedEditor.OnInspectorGUI();

        EditorGUILayout.EndScrollView();

        if (GUI.changed)
            EditorUtility.SetDirty(_targetSO);
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
                _leftPanelWidth = Mathf.Clamp(e.mousePosition.x, 400f, position.width - 100f); // 최소/최대 제한
                Repaint();
            }
            else if (e.type == EventType.MouseUp)
            {
                _isResizing = false;
            }
        }
    }
}