using PJH.Utility;
using PJH.Utility.Managers;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;


[DefaultExecutionOrder(-9999)]
public class BootstrapScene : MonoBehaviour
{
    public string nextSceneName = "InventoryScene";

    private async void Start()
    {
        await AddressableManager.LoadALlAsync<Object>("PreLoad", (key, loadCount, totalCount) =>
        {
            PJHDebug.LogColorPart($"PreLoad Progress: {loadCount}/{totalCount}", Color.cyan, tag: "BootstrapScene");
            if (loadCount == totalCount)
            {
                PJHDebug.LogColorPart("All PreLoad Complete", Color.green, tag: "BootstrapScene");
            }
        });
        SceneManager.LoadScene(nextSceneName);
    }
}