using PJH.Utility.Managers;
using Reflex.Attributes;
using UnityEngine;

[DefaultExecutionOrder(-9999)]
public class InventoryScene : MonoBehaviour
{
    [Inject] private InventoryListSO _inventoryListSO;
    [Inject] private SaveManagerSO _saveManagerSO;
    private GameEventChannelSO _uiEventChannelSO;

    private void Awake()
    {
        _uiEventChannelSO = AddressableManager.Load<GameEventChannelSO>("UIEventChannelSO");
        _inventoryListSO.Init();
        _saveManagerSO.OnLoadCompleted += HandleLoadCompleted;
    }

    private void HandleLoadCompleted()
    {
        var evt = UIEvents.ClickItemSlot;
        evt.isClicked = false;
        evt.itemSlot = null;
        _uiEventChannelSO.RaiseEvent(evt);
    }
}