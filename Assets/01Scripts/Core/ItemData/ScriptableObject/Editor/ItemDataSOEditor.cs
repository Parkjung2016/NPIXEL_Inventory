using System.Collections.Generic;
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

    private void OnEnable()
    {
        _targetSO = (BaseItemDataSO)target;
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
        SerializedProperty itemDataProp = serializedObject.FindProperty("itemData");
        if (itemDataProp != null)
        {
            DrawPropertiesRecursively(itemDataProp);
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawRightPanel()
    {
        ItemData itemData = _targetSO.GetItemData();
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

    private void DrawIconSelector(ItemData itemData, List<AddressableAssetEntry> spriteEntries)
    {
        EditorGUILayout.LabelField("Select Icon", EditorStyles.boldLabel);

        float iconSize = 64f; // 아이콘 크기
        float spacing = 5f; // 아이콘 간격

        float availableWidth = EditorGUIUtility.currentViewWidth - 40f; // 여백 감안
        int iconsPerRow = Mathf.Max(1, Mathf.FloorToInt(availableWidth / (iconSize + spacing)));

        int rowCount = Mathf.CeilToInt((float)spriteEntries.Count / iconsPerRow);
        _iconScrollPos = EditorGUILayout.BeginScrollView(_iconScrollPos, true, true, GUILayout.Height(200));
        {
            for (int row = 0; row < rowCount; row++)
            {
                EditorGUILayout.BeginHorizontal();
                {
                    for (int col = 0; col < iconsPerRow; col++)
                    {
                        int index = row * iconsPerRow + col;
                        if (index >= spriteEntries.Count) break;

                        if (!spriteEntries[index].address.Contains(".sprite"))
                            continue;
                        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(
                            spriteEntries[index].GetAssetLoadPath(true));
                        Texture2D tex = sprite ? sprite.texture : Texture2D.whiteTexture;

                        Color prevColor = GUI.backgroundColor;
                        if (itemData.iconKey == spriteEntries[index].address)
                            GUI.backgroundColor = Color.green;

                        if (GUILayout.Button(tex, GUILayout.Width(iconSize), GUILayout.Height(iconSize)))
                        {
                            itemData.iconKey = spriteEntries[index].address;
                        }

                        GUI.backgroundColor = prevColor;
                        GUILayout.Space(spacing);
                    }
                }
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(spacing);
            }
        }

        EditorGUILayout.EndScrollView();
    }

    private void DrawPropertiesRecursively(SerializedProperty prop)
    {
        EditorGUILayout.PropertyField(prop, includeChildren: true);
    }
}