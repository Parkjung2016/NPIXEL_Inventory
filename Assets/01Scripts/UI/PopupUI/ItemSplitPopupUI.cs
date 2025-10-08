using System;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.UI;

public class ItemSplitPopupUI : UIBase, IPopupUI
{
    public event Action OnSplited;

    enum Buttons
    {
        Button_Cancel,
        Button_Split
    }

    enum Sliders
    {
        Slider
    }

    public GameObject GameObject => gameObject;
    [field: SerializeField] public bool AllowDuplicates { get; private set; } = false;
    [SerializeField] private SoundDataSO buttonClickSound;
    [SerializeField] private SoundDataSO splitSound;
    [Inject] private InventoryListSO _inventoryListSO;
    private ItemDataBase _itemData;

    public override void Init()
    {
        Bind<Button>(typeof(Buttons));
        Bind<Slider>(typeof(Sliders));
    }

    public void OpenPopup()
    {
        GetButton((byte)Buttons.Button_Cancel).onClick.AddListener(HandleClickCancelButton);
        GetButton((byte)Buttons.Button_Split).onClick.AddListener(HandleClickSplitButton);
    }

    public void ClosePopup()
    {
    }

    public void SetItemData(ItemDataBase itemData)
    {
        _itemData = itemData;
        if (itemData is IStackable stackable)
        {
            Slider slider = GetSlider((byte)Sliders.Slider);
            slider.maxValue = stackable.StackCount - 1;
            slider.value = Mathf.CeilToInt(slider.maxValue / 2);
        }
    }

    private void HandleClickCancelButton()
    {
        SoundManager.CreateSoundBuilder().Play(buttonClickSound);
        Managers.UI.ClosePopup(this);
    }

    private void HandleClickSplitButton()
    {
        SoundManager.CreateSoundBuilder().Play(splitSound);
        Slider slider = GetSlider((byte)Sliders.Slider);
        int splitCount = (int)slider.value;
        _inventoryListSO.SplitItem(_itemData, splitCount);

        OnSplited?.Invoke();
        Managers.UI.ClosePopup(this);
    }
}