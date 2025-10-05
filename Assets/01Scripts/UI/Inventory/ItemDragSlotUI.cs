using PJH.Utility.Managers;
using Reflex.Attributes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemDragSlotUI : UIBase
{
    enum Images
    {
        Image_Icon,
        Image_Outline,
    }

    enum Texts
    {
        Text_StackCount
    }

    [Inject] private ItemRankColorMappingSO _rankColorMappingSO;
    private GameEventChannelSO _uiEventChannelSO;
    private RectTransform _rectTransform;

    public override void Init()
    {
        _rectTransform = transform as RectTransform;
        _uiEventChannelSO = AddressableManager.Load<GameEventChannelSO>("UIEventChannelSO");
        Bind<Image>(typeof(Images));
        Bind<TMP_Text>(typeof(Texts));

        _uiEventChannelSO.AddListener<ItemSlotDragActionEvent>(HandleItemSlotBeginDrag);
        _uiEventChannelSO.AddListener<ItemSlotDragEvent>(HandleItemSlotDrag);
        SetData(null);
    }

    protected override void OnDestroy()
    {
        _uiEventChannelSO.RemoveListener<ItemSlotDragActionEvent>(HandleItemSlotBeginDrag);
        _uiEventChannelSO.RemoveListener<ItemSlotDragEvent>(HandleItemSlotDrag);
    }

    private void SetData(ItemDataBase itemData)
    {
        if (itemData == null)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);
        GetImage((byte)Images.Image_Icon).sprite = itemData.GetIcon();
        if (itemData is IStackable stackable)
        {
            GetText((byte)Texts.Text_StackCount).text = stackable.StackCount.ToString();
        }

        Color outlineColor = _rankColorMappingSO.GetOutlineColor(itemData.rank);
        GetImage((byte)Images.Image_Outline).color = outlineColor;
    }

    private void HandleItemSlotBeginDrag(ItemSlotDragActionEvent evt)
    {
        if (evt.itemSlot == null)
        {
            SetData(null);
            return;
        }

        SetData(evt.itemSlot.CurrentItemData);
        _rectTransform.sizeDelta = evt.slotSize;
        transform.position = evt.startPosition;
    }

    private void HandleItemSlotDrag(ItemSlotDragEvent evt)
    {
        transform.position = evt.currentPosition;
    }
}