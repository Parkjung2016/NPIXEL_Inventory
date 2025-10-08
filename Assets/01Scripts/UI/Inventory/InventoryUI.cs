using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Reflex.Extensions;
using Reflex.Injectors;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InventoryUI : UIBase, IInventoryView
{
    enum Buttons
    {
        Button_ChangeSortType,
        Button_Sort,
        Button_StackAll,
        Button_GoToTop,
        Button_GoToBottom,
    }

    enum Texts
    {
        Text_SortType,
        Text_ItemCount
    }

    enum Toggles
    {
        Toggle_AutoSort
    }

    enum Objects
    {
        Viewport,
        TopGroups
    }

    enum CanvasGroups
    {
        BlockInteraction
    }

    private readonly Dictionary<ItemType, ChangeInventoryTypeButton> _changeInventoryTypeButtons =
        new Dictionary<ItemType, ChangeInventoryTypeButton>();

    [SerializeField] private ChangeInventoryTypeButton changeInventoryTypeButtonPrefab;
    [SerializeField] private InventoryScrollRectDataSourceSO scrollRectDataSourceSO;
    [SerializeField] private ItemDragSlotUI itemDragSlotUIPrefab;
    [SerializeField] private ItemType inventoryType;

    private InventoryPresenter _presenter;
    private OptimizeScrollRect _optimizeScrollRect;

    public Action<bool> OnAutoSortToggled { get; set; }
    public event Action OnChangeSortTypeClicked;
    public event Action OnSortClicked;
    public event Action OnStackAllClicked;
    public event Action OnGoToTopClicked;
    public event Action OnGoToBottomClicked;
    public event Action<ItemType> OnInventoryTypeChanged;
    public event Action<PointerEventData> OnViewportDrop;
    public event Action<bool> OnBlocked;
    public event Action OnViewportClicked;
    public event Action<Vector2> OnScrollValueChanged;

    public ItemType InventoryType => _presenter.CurrentInventoryType;

    private void SetPresenter(InventoryPresenter presenter)
    {
        _presenter = presenter;
        AttributeInjector.Inject(_presenter, SceneManager.GetActiveScene().GetSceneContainer());
        _presenter.Init();
    }

    public override void Init()
    {
        Instantiate(itemDragSlotUIPrefab, transform);
        _optimizeScrollRect = GetComponent<OptimizeScrollRect>();
        _optimizeScrollRect.SetDataSource(scrollRectDataSourceSO);

        Bind<GameObject>(typeof(Objects));
        Bind<Button>(typeof(Buttons));
        Bind<TMP_Text>(typeof(Texts));
        Bind<Toggle>(typeof(Toggles));
        Bind<CanvasGroup>(typeof(CanvasGroups));

        if (changeInventoryTypeButtonPrefab != null)
        {
            Transform topGroups = GetObject((byte)Objects.TopGroups).transform;
            foreach (ItemType itemType in Enum.GetValues(typeof(ItemType)))
            {
                ChangeInventoryTypeButton changeInventoryTypeButton =
                    Instantiate(changeInventoryTypeButtonPrefab, topGroups);
                // 버튼 클릭 시 View 이벤트 호출
                changeInventoryTypeButton.Init(itemType, (type) => OnInventoryTypeChanged?.Invoke(type));
                _changeInventoryTypeButtons.Add(itemType, changeInventoryTypeButton);
            }
        }

        BindEvent(GetObject((byte)Objects.Viewport),
            pointerEvent =>
            {
                if (pointerEvent.button == PointerEventData.InputButton.Left) OnViewportClicked?.Invoke();
            }, Define.UIEvent.Click);
        BindEvent(GetObject((byte)Objects.Viewport), (pointerEvent) => OnViewportDrop?.Invoke(pointerEvent),
            Define.UIEvent.Drop);

        GetButton((byte)Buttons.Button_ChangeSortType).onClick.AddListener(() => OnChangeSortTypeClicked?.Invoke());
        GetButton((byte)Buttons.Button_Sort).onClick.AddListener(() => OnSortClicked?.Invoke());
        GetButton((byte)Buttons.Button_StackAll).onClick.AddListener(() => OnStackAllClicked?.Invoke());
        GetButton((byte)Buttons.Button_GoToTop).onClick.AddListener(() => OnGoToTopClicked?.Invoke());
        GetButton((byte)Buttons.Button_GoToBottom).onClick.AddListener(() => OnGoToBottomClicked?.Invoke());

        Toggle autoSortToggle = GetToggle((byte)Toggles.Toggle_AutoSort);
        autoSortToggle.onValueChanged.AddListener((isOn) => OnAutoSortToggled?.Invoke(isOn));
        _optimizeScrollRect.onValueChanged.AddListener((value) => OnScrollValueChanged?.Invoke(value));

        SetPresenter(new InventoryPresenter(this, inventoryType));
    }

    protected override void OnDestroy()
    {
        _presenter?.OnDestroy();
    }

    public void ChangeInventoryType(InventorySO inventorySO)
    {
        scrollRectDataSourceSO.SetInventorySO(inventorySO);
    }

    public void SetItemCountText(string text)
    {
        GetText((byte)Texts.Text_ItemCount).text = text;
    }

    public void SetSortTypeText(string text)
    {
        GetText((byte)Texts.Text_SortType).text = text;
    }

    public void SetSortButtonActive(bool active)
    {
        GetButton((byte)Buttons.Button_Sort).gameObject.SetActive(active);
    }

    public void SetStackAllButtonActive(bool active)
    {
        GetButton((byte)Buttons.Button_StackAll).gameObject.SetActive(active);
    }

    public void SetAutoSortToggle(bool isOn)
    {
        GetToggle((byte)Toggles.Toggle_AutoSort).SetIsOnWithoutNotify(isOn);
    }

    public void SetInventoryTypeSelected(ItemType type, bool isSelected)
    {
        if (_changeInventoryTypeButtons.TryGetValue(type, out var button))
        {
            button.SetSelected(isSelected);
        }
    }

    public void ReloadScrollData(bool resetData = true)
    {
        _optimizeScrollRect.ReloadData(resetData);
    }

    public void BlockInteraction(bool block)
    {
        CanvasGroup blockInteractionCanvasGroup = GetCanvasGroup((byte)CanvasGroups.BlockInteraction);
        blockInteractionCanvasGroup.alpha = block ? 1 : 0;
        blockInteractionCanvasGroup.blocksRaycasts = block;
        OnBlocked?.Invoke(block);
    }

    public void GoToTop()
    {
        BlockDuringAsync(_optimizeScrollRect.GoToTop());
    }

    public void GoToBottom()
    {
        BlockDuringAsync(_optimizeScrollRect.GoToBottom());
    }

    private async void BlockDuringAsync(UniTask task)
    {
        BlockInteraction(true);
        await task;
        BlockInteraction(false);
    }
}