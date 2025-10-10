#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

public class CustomTemplateKeyword : AssetModificationProcessor
{
    private static void OnWillCreateAsset(string assetName)
    {
        assetName = assetName.Replace(".meta", "");

        if (Path.GetExtension(assetName) != ".cs")
            return;

        int idx = Application.dataPath.LastIndexOf("Assets");
        string path = Application.dataPath.Substring(0, idx) + assetName;

        if (!File.Exists(path))
            return;

        string fileContent = File.ReadAllText(path);

        string scriptName = Path.GetFileNameWithoutExtension(assetName);

        string noSoName = scriptName.EndsWith("SO")
            ? scriptName.Substring(0, scriptName.Length - 2)
            : scriptName;

        fileContent = fileContent.Replace("#SCRIPTNAME_NO_SO#", noSoName);

        File.WriteAllText(path, fileContent);

        AssetDatabase.Refresh();
    }
}
#endif