using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChangeInventoryTypeButton : MonoBehaviour
{
    [SerializeField] private Color _selectedColor = Color.yellow;

    private Color _originColor;
    private Button _button;
    private Image _image;
    private TextMeshProUGUI _text;

    private InventoryUI _inventoryUI;
    private ItemType _itemType;


    public void Init(InventoryUI inventoryUI, ItemType itemType)
    {
        _text = transform.Find("Text_InventoryType").GetComponent<TextMeshProUGUI>();
        _itemType = itemType;
        _inventoryUI = inventoryUI;
        _button = GetComponent<Button>();
        _image = GetComponent<Image>();
        _originColor = _image.color;
        _button.onClick.AddListener(HandleClickChangeInventoryTypeButton);
        _text.SetText(itemType.ToString());
    }

    private void HandleClickChangeInventoryTypeButton()
    {
        _inventoryUI.ChangeInventoryType(_itemType);
    }

    public void SetSelected(bool isSelected)
    {
        Color color = isSelected ? _selectedColor : _originColor;
        _image.color = color;
    }
}