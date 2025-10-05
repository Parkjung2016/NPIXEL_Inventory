using System.Threading;
using Cysharp.Threading.Tasks;
using PJH.Utility.Managers;
using UnityEngine;
using UnityEngine.EventSystems;

public class ItemSlotTooltipHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private const float hoverTime = 0.5f;
    private GameEventChannelSO _uiEventChannelSO;
    private CancellationTokenSource _hoverCancellationTokenSource;

    public ItemDataBase CurrentItemData { get; private set; }

    private void Awake()
    {
        _uiEventChannelSO = AddressableManager.Load<GameEventChannelSO>("UIEventChannelSO");
    }

    private void OnEnable()
    {
        _uiEventChannelSO.AddListener<ShowItemSlotTooltipUIEvent>(HandleShowItemSlotTooltipUI);
    }

    private void OnDisable()
    {
        DisposeHoverCancellationTokenSource();
        _uiEventChannelSO?.RemoveListener<ShowItemSlotTooltipUIEvent>(HandleShowItemSlotTooltipUI);
    }

    public void SetItemData(ItemDataBase itemData)
    {
        ItemDataBase prevItemData = CurrentItemData;
        CurrentItemData = itemData;
        if (CurrentItemData == null)
        {
            HideTooltip(prevItemData);
        }
    }

    private void DisposeHoverCancellationTokenSource()
    {
        if (_hoverCancellationTokenSource is { IsCancellationRequested: false })
        {
            _hoverCancellationTokenSource.Cancel();
            _hoverCancellationTokenSource.Dispose();
        }
    }

    private void HandleShowItemSlotTooltipUI(ShowItemSlotTooltipUIEvent evt)
    {
        if (evt.show)
        {
            DisposeHoverCancellationTokenSource();
        }
    }


    private async UniTaskVoid CheckHover(CancellationToken token)
    {
        try
        {
            await UniTask.WaitForSeconds(hoverTime, ignoreTimeScale: true, cancellationToken: token);

            var showItemSlotTooltipEvt = UIEvents.ShowItemSlotTooltip;
            showItemSlotTooltipEvt.show = true;
            showItemSlotTooltipEvt.itemData = CurrentItemData;
            _uiEventChannelSO.RaiseEvent(showItemSlotTooltipEvt);
        }
        catch
        {
        }
    }

    private async UniTaskVoid HideTooltipAfterDelay(CancellationToken token)
    {
        try
        {
            await UniTask.WaitForSeconds(0.1f, ignoreTimeScale: true, cancellationToken: token);

            var showItemSlotTooltipEvt = UIEvents.ShowItemSlotTooltip;
            if (showItemSlotTooltipEvt.show && showItemSlotTooltipEvt.itemData == CurrentItemData)
            {
                showItemSlotTooltipEvt.show = false;
                _uiEventChannelSO.RaiseEvent(showItemSlotTooltipEvt);
            }
        }
        catch
        {
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (ShouldBlockTooltip()) return;
        var showItemSlotTooltipEvt = UIEvents.ShowItemSlotTooltip;

        if (showItemSlotTooltipEvt.show)
        {
            showItemSlotTooltipEvt.itemData = CurrentItemData;
            _uiEventChannelSO.RaiseEvent(showItemSlotTooltipEvt);
        }
        else
        {
            DisposeHoverCancellationTokenSource();

            _hoverCancellationTokenSource = new CancellationTokenSource();
            CheckHover(_hoverCancellationTokenSource.Token).Forget();
        }
    }

    private void HideTooltip(ItemDataBase prevItemData)
    {
        DisposeHoverCancellationTokenSource();
        var showItemSlotTooltipEvt = UIEvents.ShowItemSlotTooltip;
        if (showItemSlotTooltipEvt.show && showItemSlotTooltipEvt.itemData == prevItemData)
        {
            showItemSlotTooltipEvt.show = false;
            _uiEventChannelSO.RaiseEvent(showItemSlotTooltipEvt);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        DisposeHoverCancellationTokenSource();
        if (ShouldBlockTooltip()) return;
        _hoverCancellationTokenSource = new CancellationTokenSource();
        HideTooltipAfterDelay(_hoverCancellationTokenSource.Token).Forget();
    }

    public bool ShouldBlockTooltip()
    {
        return UIEvents.ClickItemSlot.isClicked || UIEvents.ItemSlotDragAction.itemSlot != null;
    }
}