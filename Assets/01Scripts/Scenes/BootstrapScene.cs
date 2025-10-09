using PJH.Utility;
using PJH.Utility.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;


[DefaultExecutionOrder(-9999)]
public class BootstrapScene : MonoBehaviour
{
    [SerializeField] private string nextSceneName = "InventoryScene";
    [SerializeField] private TextMeshProUGUI progressText;

    private async void Start()
    {
        await AddressableManager.LoadALlAsync<Object>("PreLoad", (key, loadCount, totalCount) =>
        {
            progressText.SetText($"Loading... {(int)((float)loadCount / totalCount * 100)}%");
            if (loadCount == totalCount)
            {
                PJHDebug.LogColorPart("All PreLoad Complete", Color.green, tag: "BootstrapScene");
            }
        });
        SceneManager.LoadScene(nextSceneName);
    }
}