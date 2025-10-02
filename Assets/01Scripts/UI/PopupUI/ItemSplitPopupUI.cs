using Reflex.Attributes;
using UnityEngine;
using UnityEngine.UI;

public class ItemSplitPopupUI : UIBase, IPopupUI
{
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
    [Inject] private InventorySO _inventorySO;
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
            slider.minValue = 1;
            slider.maxValue = stackable.StackCount - 1;
            slider.value = slider.maxValue / 2;
        }
    }

    private void HandleClickCancelButton()
    {
        Managers.UI.ClosePopup(this);
    }

    private void HandleClickSplitButton()
    {
        Slider slider = GetSlider((byte)Sliders.Slider);
        int splitCount = (int)slider.value;
        _inventorySO.Split(_itemData, splitCount);

        Managers.UI.ClosePopup(this);
    }
}