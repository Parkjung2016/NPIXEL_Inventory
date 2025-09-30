using PJH.Utility.Managers;
using Reflex.Core;
using Reflex.Extensions;
using Reflex.Injectors;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneInstaller : MonoBehaviour, IInstaller
{
    public void InstallBindings(ContainerBuilder containerBuilder)
    {
        InventorySO inventorySO = AddressableManager.Load<InventorySO>("InventorySO");
        EnumStringMappingSO enumStringMappingSO = AddressableManager.Load<EnumStringMappingSO>("EnumStringMappingSO");
        InventoryDataListSO inventoryDataListSO = AddressableManager.Load<InventoryDataListSO>("InventoryDataListSO");
        ItemManagerSO itemManagerSO = AddressableManager.Load<ItemManagerSO>("ItemManagerSO");
        GameEventChannelSO uiEventChannelSO = AddressableManager.Load<GameEventChannelSO>("UIEventChannelSO");
        ItemRankColorMappingSO itemRankColorMappingSO =
            AddressableManager.Load<ItemRankColorMappingSO>("ItemRankColorMappingSO");
        SaveManagerSO saveManagerSO = AddressableManager.Load<SaveManagerSO>("SaveManagerSO");
        containerBuilder.AddSingleton(inventorySO);
        containerBuilder.AddSingleton(enumStringMappingSO);
        containerBuilder.AddSingleton(inventoryDataListSO);
        containerBuilder.AddSingleton(itemManagerSO);
        containerBuilder.AddSingleton(uiEventChannelSO);
        containerBuilder.AddSingleton(itemRankColorMappingSO);
        containerBuilder.AddSingleton(saveManagerSO);
    }

    private void Awake()
    {
        ItemManagerSO itemManagerSO = AddressableManager.Load<ItemManagerSO>("ItemManagerSO");
        AttributeInjector.Inject(itemManagerSO, SceneManager.GetActiveScene().GetSceneContainer());

        InventorySO inventorySO = AddressableManager.Load<InventorySO>("InventorySO");
        AttributeInjector.Inject(inventorySO, SceneManager.GetActiveScene().GetSceneContainer());
        inventorySO.Init();
    }
}