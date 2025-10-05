using Reflex.Attributes;
using UnityEngine;

[DefaultExecutionOrder(-9999)]
public class InventoryScene : MonoBehaviour
{
    [Inject] private InventoryListSO _inventoryListSO;

    private void Awake()
    {
        _inventoryListSO.Init();
    }
}