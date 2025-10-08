using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

public static class AIItemGenerator
{
    private static AIAPISO _apiSO;

    public static async void GenerateAsync(string prompt, BaseItemDataSO targetSO, ItemAttributeListSO attrList,
        Action onFinish)
    {
        if (_apiSO == null)
            _apiSO = AssetDatabase.LoadAssetAtPath<AIAPISO>("Assets/03SO/API/AIAPISO.asset");
        EditorUtility.DisplayProgressBar("AI Item Generation", "Generating item...", 0f);

        string jsonPrompt = BuildPrompt(prompt, attrList);
        JObject request = new()
        {
            ["model"] = _apiSO.apiModel,
            ["messages"] = new JArray(new JObject { ["role"] = "user", ["content"] = jsonPrompt }),
            ["stream"] = false
        };

        using var req = new UnityWebRequest(_apiSO.apiUrl, "POST")
        {
            uploadHandler =
                new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(request))),
            downloadHandler = new DownloadHandlerBuffer()
        };
        req.SetRequestHeader("Authorization", "Bearer " + _apiSO.apiToken);
        req.SetRequestHeader("Content-Type", "application/json");

        await req.SendWebRequest();
        if (req.result == UnityWebRequest.Result.Success)
            ParseResponse(req.downloadHandler.text, targetSO, attrList);
        else
            Debug.LogError("AI Error: " + req.error);

        EditorUtility.ClearProgressBar();
        onFinish?.Invoke();
    }

    private static string BuildPrompt(string prompt, ItemAttributeListSO list)
    {
        string attrNames = string.Join("|", list.itemAttributes.Select(a => a.name));
        string detail = string.Join("|", Enum.GetNames(typeof(ItemDetailType)));
        string types = string.Join("|", Enum.GetNames(typeof(ItemType)));
        string ranks = string.Join("|", Enum.GetNames(typeof(ItemRank)));
        string opTypes = string.Join("|", Enum.GetNames(typeof(OperationType)));

        return $@"
{prompt}
Details:
Create a game item JSON:

{{
  ""displayName"": ""string"",
  ""detailType"": ""{detail}"",
  ""description"": ""string"",
  ""itemType"": ""{types}"",
  ""rank"": ""{ranks}"",
  ""baseAttributes"": [{{""name"": ""{attrNames}"", ""value"": 0}}],
  ""additionalAttributes"": [{{""additionalAttribute"": ""{attrNames}"", ""value"": 0, ""operationType"": ""{opTypes}""}}]
}}
Rules:
- baseAttributes must not be empty.
- Only output valid JSON.";
    }

    private static void ParseResponse(string json, BaseItemDataSO targetSO, ItemAttributeListSO attrList)
    {
        try
        {
            string content = JObject.Parse(json)["choices"]?[0]?["message"]?["content"]?.ToString() ?? "";
            if (content.StartsWith("```"))
                content = content.Split('\n', 2)[1].Trim('`', '\n', ' ');

            JObject parsed = JObject.Parse(content);
            var item = targetSO.GetItemData();
            item.displayName = parsed["displayName"]?.ToString();
            item.description = parsed["description"]?.ToString();
            Enum.TryParse(parsed["itemType"]?.ToString(), out item.itemType);
            Enum.TryParse(parsed["detailType"]?.ToString(), out item.detailType);
            Enum.TryParse(parsed["rank"]?.ToString(), out item.rank);

            targetSO.attributes = ExtractBaseAttributes(parsed["baseAttributes"], attrList);
            targetSO.additionalAttributes = ExtractAdditionalAttributes(parsed["additionalAttributes"], attrList);

            targetSO.OnValidate();
            EditorUtility.SetDirty(targetSO);
            AssetDatabase.SaveAssets();
        }
        catch (Exception e)
        {
            Debug.LogError("AI Parse Error: " + e.Message);
        }
    }

    private static List<ItemAttributeOverride> ExtractBaseAttributes(JToken token, ItemAttributeListSO attrList)
    {
        var result = new List<ItemAttributeOverride>();
        if (token is not JArray arr) return result;

        foreach (var attr in arr)
        {
            string name = attr["name"]?.ToString();
            float val = attr["value"]?.ToObject<float>() ?? 0f;
            var attrSO = attrList.itemAttributes.FirstOrDefault(a => a.name == name);
            if (attrSO != null)
                result.Add(new ItemAttributeOverride { itemAttribute = attrSO, value = val });
        }

        return result;
    }

    private static List<AdditionalItemAttributeClass> ExtractAdditionalAttributes(JToken token,
        ItemAttributeListSO attrList)
    {
        var result = new List<AdditionalItemAttributeClass>();
        if (token is not JArray arr) return result;

        foreach (var attr in arr)
        {
            string name = attr["additionalAttribute"]?.ToString();
            float val = attr["value"]?.ToObject<float>() ?? 0f;
            Enum.TryParse(attr["operationType"]?.ToString(), out OperationType opType);

            var attrSO = attrList.itemAttributes.FirstOrDefault(a => a.name == name);
            if (attrSO != null)
                result.Add(new AdditionalItemAttributeClass
                    { additionalAttribute = attrSO, value = val, operationType = opType });
        }

        return result;
    }
}