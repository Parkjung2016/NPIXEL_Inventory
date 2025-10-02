using Reflex.Attributes;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemSlotUI : UIBase, ICell, IItemSlotUI
{
    public enum GameObjects
    {
        EmptyItemData,
        ExistingItemData
    }

    enum Images
    {
        Image_NoItemData,
        Image_Background,
        Image_Outline,
        Image_Icon,
    }

    enum Texts
    {
        Text_StackCount
    }

    [Inject] private ItemRankColorMappingSO _rankColorMappingSO;
    [Inject] private GameEventChannelSO _uiEventChannelSO;
    private Image _itemImage;
    private ItemSlotTooltipHandler _slotTooltipHandler;
    public int CellIndex { get; private set; }
    public ItemDataBase CurrentItemData => _slotTooltipHandler.CurrentItemData;

    public override void Init()
    {
        Bind<Image>(typeof(Images));
        Bind<TMP_Text>(typeof(Texts));
        Bind<GameObject>(typeof(GameObjects));
        BindEvent(GetImage((byte)Images.Image_Background).gameObject, HandleSlotClick,
            Define.UIEvent.Click);
        BindEvent(GetImage((byte)Images.Image_NoItemData).gameObject, HandleNoItemSlotClick,
            Define.UIEvent.Click);
        _slotTooltipHandler = GetImage((byte)Images.Image_Background).GetComponent<ItemSlotTooltipHandler>();
    }

    private void HandleSlotClick(PointerEventData pointerEvent)
    {
        var evt = UIEvents.ClickItemSlot;
        if (evt.isClicked && ReferenceEquals(evt.itemSlot, this)) return;
        evt.itemSlot = this;
        evt.isClicked = true;
        _uiEventChannelSO.RaiseEvent(evt);
    }

    private void HandleNoItemSlotClick(PointerEventData pointerEvent)
    {
        var evt = UIEvents.ClickItemSlot;
        if (!evt.isClicked) return;
        evt.itemSlot = null;
        evt.isClicked = false;
        _uiEventChannelSO.RaiseEvent(evt);
    }

    public virtual void SetItemData(ItemDataBase itemData)
    {
        _slotTooltipHandler.SetItemData(itemData);
        bool isEmptyItemData = itemData == null;
        GetObject((byte)GameObjects.EmptyItemData).SetActive(isEmptyItemData);
        GetObject((byte)GameObjects.ExistingItemData).SetActive(!isEmptyItemData);
        if (isEmptyItemData)
        {
            return;
        }

        GetImage((byte)Images.Image_Icon).sprite = itemData.GetIcon();
        Color rankColor = _rankColorMappingSO[itemData.rank];
        Color.RGBToHSV(rankColor, out float h, out float s, out float v);

        v = Mathf.Clamp01(v * 1.8f);

        Color brighterRankColor = Color.HSVToRGB(h, s, v);
        GetImage((byte)Images.Image_Outline).color = brighterRankColor;

        if (itemData is IStackable stackable)
        {
            GetText((byte)Texts.Text_StackCount).SetText(stackable.StackCount.ToString());
        }
        else
            GetText((byte)Texts.Text_StackCount).gameObject.SetActive(false);
    }


    public void ConfigureCell(ItemDataBase itemData, int cellIndex)
    {
        CellIndex = cellIndex;
        SetItemData(itemData);
    }
}