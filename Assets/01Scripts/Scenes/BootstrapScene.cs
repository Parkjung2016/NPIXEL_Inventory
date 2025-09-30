using PJH.Utility;
using PJH.Utility.Managers;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

public class BootstrapScene : MonoBehaviour
{
    public string nextSceneName = "InventoryScene";

    private async void Start()
    {
        await AddressableManager.LoadALlAsync<Object>("PreLoad", (key, loadCount, totalCount) =>
        {
            if (loadCount == totalCount)
            {
                PJHDebug.LogColorPart("All PreLoad Complete", Color.green, tag: "BootstrapScene");
            }
        });
        SceneManager.LoadScene(nextSceneName);
    }
}