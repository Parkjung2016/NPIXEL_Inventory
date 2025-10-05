using PJH.Utility.Managers;
using Reflex.Core;
using Reflex.Extensions;
using Reflex.Injectors;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-99999)]
public class SceneInstaller : MonoBehaviour, IInstaller
{
    public void InstallBindings(ContainerBuilder containerBuilder)
    {
        EnumStringMappingSO enumStringMappingSO = AddressableManager.Load<EnumStringMappingSO>("EnumStringMappingSO");
        InventoryDataListSO inventoryDataListSO = AddressableManager.Load<InventoryDataListSO>("InventoryDataListSO");
        InventoryListSO inventoryListSO = AddressableManager.Load<InventoryListSO>("InventoryListSO");
        ItemManagerSO itemManagerSO = AddressableManager.Load<ItemManagerSO>("ItemManagerSO");
        ItemRankColorMappingSO itemRankColorMappingSO =
            AddressableManager.Load<ItemRankColorMappingSO>("ItemRankColorMappingSO");
        SaveManagerSO saveManagerSO = AddressableManager.Load<SaveManagerSO>("SaveManagerSO");
        containerBuilder.AddSingleton(enumStringMappingSO);
        containerBuilder.AddSingleton(inventoryDataListSO);
        containerBuilder.AddSingleton(inventoryListSO);
        containerBuilder.AddSingleton(itemManagerSO);
        containerBuilder.AddSingleton(itemRankColorMappingSO);
        containerBuilder.AddSingleton(saveManagerSO);
    }

    private void Awake()
    {
        ItemManagerSO itemManagerSO = AddressableManager.Load<ItemManagerSO>("ItemManagerSO");
        AttributeInjector.Inject(itemManagerSO, SceneManager.GetActiveScene().GetSceneContainer());
    }
}