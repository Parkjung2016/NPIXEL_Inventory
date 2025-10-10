using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;

[CustomEditor(typeof(BaseItemDataSO), true)]
public class BaseItemDataSOEditor : Editor
{
    private const float leftPanelWidth = 500f;
    private BaseItemDataSO _targetSO;

    private int _pickerControlID;
    private Vector2 _iconScrollPos;
    private SerializedProperty _itemDataProp;

    private void OnEnable()
    {
        if (target == null) return;
        _targetSO = (BaseItemDataSO)target;
        _itemDataProp = serializedObject.FindProperty("itemData");
    }

    public override void OnInspectorGUI()
    {
        if (_targetSO.GetItemData() == null)
        {
            EditorGUILayout.HelpBox("ItemData가 없습니다.", MessageType.Warning);
            return;
        }

        serializedObject.Update(); // 필드 값 동기화

        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        {
            // 좌측 패널
            EditorGUILayout.BeginVertical(GUILayout.Width(leftPanelWidth));
            {
                DrawLeftPanel();
            }
            EditorGUILayout.EndVertical();

            // 우측 패널
            EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
            {
                DrawRightPanel();
            }
            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.EndHorizontal();

        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }
    }

    private void DrawLeftPanel()
    {
        if (_itemDataProp != null)
        {
            EditorGUILayout.PropertyField(_itemDataProp, includeChildren: true);
        }
        //
        // EditorGUILayout.PropertyField(_attributes);
        // EditorGUILayout.PropertyField(_additionalAttributes);
        
        serializedObject.Update();

        string[] excludeFields = { "m_Script", "itemData" }; 

        var iterator = serializedObject.GetIterator();
        bool enterChildren = true;

        while (iterator.NextVisible(enterChildren))
        {
            enterChildren = false;

            if (System.Array.Exists(excludeFields, name => name == iterator.name))
                continue;

            EditorGUILayout.PropertyField(iterator, includeChildren: true);
        }
        serializedObject.ApplyModifiedProperties();
    }

    private void DrawRightPanel()
    {
        ItemDataBase itemData = _targetSO.GetItemData();
        // Addressable에 등록된 Sprite 가져오기
        List<AddressableAssetEntry> spriteEntries = new List<AddressableAssetEntry>();
        AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
        settings.GetAllAssets(spriteEntries, includeSubObjects: false, groupFilter: null,
            entryFilter: e => e.MainAssetType == typeof(Texture2D));
        Sprite icon = null;
        if (spriteEntries.Count > 0)
        {
            var spriteAssetEntry = spriteEntries.Find(x => x.address == itemData.iconKey);
            if (spriteAssetEntry != null)
            {
                icon = AssetDatabase.LoadAssetAtPath<Sprite>(spriteAssetEntry.GetAssetLoadPath(true));
            }
        }

        Texture2D iconTexture = icon ? icon.texture : Texture2D.whiteTexture;
        GUILayout.Label(iconTexture, GUILayout.Width(64), GUILayout.Height(64));

        DrawIconSelector(itemData, spriteEntries);
    }

    private void DrawIconSelector(ItemDataBase itemData, List<AddressableAssetEntry> spriteEntries)
    {
        EditorGUILayout.LabelField("Select Icon", EditorStyles.boldLabel);

        float iconSize = 64f; // 아이콘 크기
        float spacing = 5f; // 아이콘 간격

        float availableWidth = EditorGUIUtility.currentViewWidth - 40f; // 여백 감안
        int iconsPerRow = Mathf.Max(1, Mathf.FloorToInt(availableWidth / (iconSize + spacing)));
        _iconScrollPos = EditorGUILayout.BeginScrollView(_iconScrollPos, true, true, GUILayout.Height(200));
        {
            int colCount = 0;
            EditorGUILayout.BeginHorizontal();
            {
                foreach (var entry in spriteEntries)
                {
                    if (!entry.address.Contains(".sprite")) continue;
                    string detailTypeDisplayName =
                        Regex.Replace(itemData.detailType.ToString(), "([A-Z])", " $1").Trim();
                    if (!entry.AssetPath.Contains(
                            $"{itemData.itemType}/{detailTypeDisplayName}/"))
                        continue;

                    Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(entry.GetAssetLoadPath(true));
                    Texture2D tex = sprite ? sprite.texture : Texture2D.whiteTexture;

                    Color prevColor = GUI.backgroundColor;
                    if (itemData.iconKey == entry.address)
                        GUI.backgroundColor = Color.green;

                    if (GUILayout.Button(tex, GUILayout.Width(iconSize), GUILayout.Height(iconSize)))
                    {
                        itemData.iconKey = entry.address;
                    }

                    GUI.backgroundColor = prevColor;
                    GUILayout.Space(spacing);

                    colCount++;
                    if (colCount >= iconsPerRow)
                    {
                        colCount = 0;
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.BeginHorizontal();
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();
    }
}