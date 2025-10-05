using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PJH.Utility;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

public class InventoryDataListWindow : EditorWindow
{
    private const string apiUrl = "https://router.huggingface.co/v1/chat/completions";
    private const string apiToken = "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";
    private const string apiModel = "deepseek-ai/DeepSeek-V3.2-Exp:novita";
    private const float minLeftPanelWidth = 400f; // 최소 너비
    private const float leftListTopOffset = 50;

    private InventoryDataListSO _targetSO;
    private ItemAttributeListSO _attributeListSO;
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

    private Editor _cachedEditor;

    private string _aiPrompt;
    private Dictionary<ItemDetailType, bool> _foldoutStates = new Dictionary<ItemDetailType, bool>();

    [MenuItem("Tools/Inventory Data List")]
    public static void ShowWindow()
    {
        GetWindow<InventoryDataListWindow>("Inventory Data List").Show();
    }

    private void OnEnable()
    {
        _attributeListSO =
            AssetDatabase.LoadAssetAtPath<ItemAttributeListSO>("Assets/03SO/ItemAttributeListSO.asset");
        _targetSO =
            AssetDatabase.LoadAssetAtPath<InventoryDataListSO>("Assets/03SO/Inventory/InventoryDataListSO.asset");

        _leftPanelWidth = minLeftPanelWidth;
        _itemTypes = Assembly.GetAssembly(typeof(BaseItemDataSO))
            .GetTypes()
            .Where(t => typeof(BaseItemDataSO).IsAssignableFrom(t) && !t.IsAbstract)
            .ToArray();
        _itemTypeNames = _itemTypes.Select(t => t.Name).ToArray();
    }

    private bool _disbledGroup = false;

    private void OnGUI()
    {
        EditorGUI.BeginDisabledGroup(_disbledGroup);
        {
            EditorGUILayout.Space();
            if (_targetSO == null) return;

            EditorGUILayout.BeginHorizontal();
            {
                // 좌측 패널
                GUILayout.BeginVertical(GUILayout.Width(_leftPanelWidth));
                {
                    DrawLeftPanel();
                }
                EditorGUILayout.EndVertical();

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

        EditorGUI.EndDisabledGroup();
    }

    // -----------------------------
    // 좌측 패널
    // -----------------------------

    private void DrawLeftPanel()
    {
        if (_targetSO.InventoryDataList == null) return;

        EditorGUILayout.BeginHorizontal();
        _selectedTypeIndex = EditorGUILayout.Popup("Item Type", _selectedTypeIndex, _itemTypeNames);

        Color prevColor = GUI.backgroundColor;
        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("Add Item")) AddNewItem();

        if (_selectedIndex >= 0 && _selectedIndex < _targetSO.InventoryDataList.Count)
        {
            GUI.backgroundColor = Color.red;
            if (GUILayout.Button("Delete Selected")) DeleteSelectedItem();
        }

        GUI.backgroundColor = prevColor;
        EditorGUILayout.EndHorizontal();

        _leftListRect = GUILayoutUtility.GetRect(200, position.height - leftListTopOffset);
        GUI.Box(_leftListRect, GUIContent.none);

        _leftScrollPos = GUI.BeginScrollView(_leftListRect, _leftScrollPos,
            new Rect(0, 0, _leftListRect.width - 20, _targetSO.InventoryDataList.Count * 30));
        {
            // ItemDetailType별로 그룹핑
            var grouped = _targetSO.InventoryDataList
                .Where(item => item != null)
                .GroupBy(item => item.GetItemData().detailType);

            float yOffset = 0f;

            foreach (var group in grouped)
            {
                if (!_foldoutStates.ContainsKey(group.Key))
                    _foldoutStates[group.Key] = true;

                // 카테고리 Foldout
                Rect foldoutRect = new Rect(5, yOffset, _leftListRect.width - 30, 20);
                _foldoutStates[group.Key] =
                    EditorGUI.Foldout(foldoutRect, _foldoutStates[group.Key], group.Key.ToString(), true);
                yOffset += 20;

                if (_foldoutStates[group.Key])
                {
                    foreach (var item in group)
                    {
                        int index = _targetSO.InventoryDataList.IndexOf(item);
                        Rect buttonRect = new Rect(20, yOffset, _leftListRect.width - 45, 20);

                        if (index == _selectedIndex) GUI.backgroundColor = Color.cyan;

                        if (GUI.Button(buttonRect, item.GetItemData().displayName))
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
        GUI.EndScrollView();
    }

    private void AddNewItem()
    {
        Type selectedType = _itemTypes[_selectedTypeIndex];
        BaseItemDataSO newItem = (BaseItemDataSO)ScriptableObject.CreateInstance(selectedType);

        newItem.name = $"New {selectedType.Name}"; // 에셋 이름 지정
        ItemDataBase itemData = newItem.GetItemData();
        itemData.displayName = selectedType.Name;
        itemData.itemID = _targetSO.InventoryDataList.Count;

        // Assets 폴더에 에셋으로 저장
        string path = $"Assets/03SO/InventoryData/{newItem.name}_{Guid.NewGuid()}.asset";
        AssetDatabase.CreateAsset(newItem, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        _targetSO.InventoryDataList.Add(newItem);
        _selectedIndex = _targetSO.InventoryDataList.Count - 1;

        EditorUtility.SetDirty(_targetSO);
    }

    private void DeleteSelectedItem()
    {
        if (_selectedIndex < 0 || _selectedIndex >= _targetSO.InventoryDataList.Count)
            return;

        BaseItemDataSO itemToDelete = _targetSO.InventoryDataList[_selectedIndex];

        // 리스트에서 제거
        _targetSO.InventoryDataList.RemoveAt(_selectedIndex);

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

        _selectedIndex = Mathf.Clamp(_selectedIndex - 1, 0, _targetSO.InventoryDataList.Count - 1);
        EditorUtility.SetDirty(_targetSO);
    }

    // -----------------------------
    // 우측 패널
    // -----------------------------
    private void DrawRightPanel()
    {
        if (_selectedIndex < 0 || _selectedIndex >= _targetSO.InventoryDataList.Count) return;

        var selectedItem = _targetSO.InventoryDataList[_selectedIndex];

        // --- AI 프롬프트 입력 영역 ---
        EditorGUILayout.LabelField("Generate Item Data With AI", EditorStyles.boldLabel);
        _aiPrompt = EditorGUILayout.TextField("Promport", _aiPrompt);

        if (GUILayout.Button("Generate"))
        {
            SendAIRequest(_aiPrompt, selectedItem);
        }

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

    private async void SendAIRequest(string prompt, BaseItemDataSO targetItemSO)
    {
        string[] detailTypes = Enum.GetNames(typeof(ItemDetailType));
        string detailTypeOptions = string.Join("|", detailTypes);

        string[] itemTypes = Enum.GetNames(typeof(ItemType));
        string itemTypeOptions = string.Join("|", itemTypes);

        string[] rankTypes = Enum.GetNames(typeof(ItemRank));
        string rankTypeOptions = string.Join("|", rankTypes);

        string[] attributeNames = _attributeListSO.itemAttributes.Select(a => a.name).ToArray();
        string attributeOptions = string.Join("|", attributeNames);
// OperationType 열거형
        string[] operationTypes = Enum.GetNames(typeof(OperationType));
        string operationTypeOptions = string.Join("|", operationTypes);

        prompt = $@"
{prompt}
Details:
Create a game item in JSON format. 
The JSON should contain the following fields:

{{
  ""displayName"": ""string"",
  ""detailType"": ""{detailTypeOptions}"",
  ""description"": ""string"",
  ""itemType"": ""{itemTypeOptions}"",
  ""rank"": ""{rankTypeOptions}"",
  ""baseAttributes"": [
    {{""name"": ""{attributeOptions}"", ""value"": 0}}
  ],
  ""additionalAttributes"": [
    {{""additionalAttribute"": ""{attributeOptions}"", ""value"": 0, ""operationType"": ""{operationTypeOptions}""}}
  ]
}}

Rules:
- Fill multiple attributes in the arrays using the available names.
- The baseAttributes array **must not be empty**.
- Values should be realistic numbers.
- OperationType must be one of: {operationTypeOptions}.
- Only output valid JSON without any extra text.
";
        // 요청 JSON 객체 생성
        JObject requestObject = new JObject
        {
            ["model"] = apiModel,
            ["messages"] = new JArray
            {
                new JObject
                {
                    ["role"] = "user",
                    ["content"] = prompt
                }
            },
            ["stream"] = false
        };

        string jsonData = JsonConvert.SerializeObject(requestObject, Formatting.Indented);
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);

        using (UnityWebRequest request = new UnityWebRequest(apiUrl, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Authorization", "Bearer " + apiToken);
            request.SetRequestHeader("Content-Type", "application/json");
            EditorUtility.DisplayProgressBar("AI Item Generation", "Generating item...", 0f);
            _disbledGroup = true;
            await request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseText = request.downloadHandler.text;
                JObject responseJson = JObject.Parse(responseText);

                // AI 메시지(content) 추출
                string aiMessage = responseJson["choices"]?[0]?["message"]?["content"]?.ToString();

                if (!string.IsNullOrEmpty(aiMessage))
                {
                    if (aiMessage.StartsWith("```"))
                    {
                        int firstLineEnd = aiMessage.IndexOf('\n');
                        int lastLineStart = aiMessage.LastIndexOf("```");
                        if (firstLineEnd >= 0 && lastLineStart > firstLineEnd)
                        {
                            aiMessage = aiMessage.Substring(firstLineEnd + 1, lastLineStart - firstLineEnd - 1);
                        }
                    }

                    try
                    {
                        // JSON 파싱 후 ItemData에 바로 채우기
                        Undo.RecordObject(targetItemSO, "AI Generated Item Data");
                        JObject itemJson = JObject.Parse(aiMessage);
                        ItemDataBase targetItem = targetItemSO.GetItemData();
                        targetItem.displayName = itemJson["displayName"]?.ToString();
                        targetItem.description = itemJson["description"]?.ToString();

                        if (Enum.TryParse(itemJson["itemType"]?.ToString(), out ItemType type))
                            targetItem.itemType = type;
                        if (Enum.TryParse(itemJson["detailType"]?.ToString(), out ItemDetailType detailType))
                            targetItem.detailType = detailType;
                        if (Enum.TryParse(itemJson["rank"]?.ToString(), out ItemRank rank))
                            targetItem.rank = rank;

                        // baseAttributes와 additionalAttributes 처리
                        targetItemSO.attributes = new List<ItemAttributeOverride>();
                        if (itemJson["baseAttributes"] is JArray baseArr)
                        {
                            foreach (var attr in baseArr)
                            {
                                string attrName = attr["name"]?.ToString();
                                float value = attr["value"]?.ToObject<float>() ?? 0f;

                                var attrSO = _attributeListSO.itemAttributes.FirstOrDefault(a => a.name == attrName);
                                if (attrSO != null)
                                {
                                    targetItemSO.attributes.Add(new ItemAttributeOverride
                                    {
                                        itemAttribute = attrSO,
                                        value = value
                                    });
                                }
                            }
                        }

                        targetItemSO.additionalAttributes = new List<AdditionalItemAttributeClass>();
                        if (itemJson["additionalAttributes"] is JArray addArr)
                        {
                            foreach (var attr in addArr)
                            {
                                string attrName = attr["additionalAttribute"]?.ToString();
                                float value = attr["value"]?.ToObject<float>() ?? 0f;
                                OperationType operationType = OperationType.Sum; // 기본값

                                // AI에서 operationType 제공하면 처리
                                if (Enum.TryParse(attr["operationType"]?.ToString(), out OperationType op))
                                    operationType = op;

                                var attrSO = _attributeListSO.itemAttributes.FirstOrDefault(a => a.name == attrName);
                                if (attrSO != null)
                                {
                                    targetItemSO.additionalAttributes.Add(new AdditionalItemAttributeClass
                                    {
                                        additionalAttribute = attrSO,
                                        value = value,
                                        operationType = operationType
                                    });
                                }
                            }
                        }

                        targetItemSO.OnValidate();
                        EditorUtility.SetDirty(targetItemSO);
                        AssetDatabase.SaveAssets();
                    }
                    catch (Exception ex)
                    {
                        PJHDebug.LogError("AI Response Parsing Error:" + ex.Message);
                    }
                }
                else
                {
                    PJHDebug.LogError("There are no messages in the AI response.");
                }
            }
            else
            {
                PJHDebug.LogError("Error: " + request.error);
            }
        }

        // 요청 끝나면 창 닫기
        _disbledGroup = false;
        EditorUtility.ClearProgressBar();
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
                _leftPanelWidth =
                    Mathf.Clamp(e.mousePosition.x, minLeftPanelWidth, position.width - 100f); // 최소/최대 제한
                Repaint();
            }
            else if (e.type == EventType.MouseUp)
            {
                _isResizing = false;
            }
        }
    }
}