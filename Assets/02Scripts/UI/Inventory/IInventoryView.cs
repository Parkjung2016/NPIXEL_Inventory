using UnityEngine;
using UnityEngine.EventSystems;
using System;

public interface IInventoryView
{
    void SetItemCountText(string text);
    void SetSortTypeText(string text);
    void SetSortButtonActive(bool active);
    void SetAutoSortToggle(bool isOn);
    void SetStackAllButtonActive(bool active);
    void SetInventoryTypeSelected(Define.ItemType type, bool isSelected);
    void ReloadScrollData(bool resetData = true);
    void BlockInteraction(bool block);
    void GoToTop();
    void GoToBottom();
    void ChangeInventoryType(InventorySO inventorySO);

    Action<bool> OnAutoSortToggled { get; set; }
    event Action OnChangeSortTypeClicked;
    event Action OnSortClicked;
    event Action OnStackAllClicked;
    event Action OnGoToTopClicked;
    event Action OnGoToBottomClicked;
    event Action<Define.ItemType> OnInventoryTypeChanged;
    event Action<PointerEventData> OnViewportDrop;
    event Action OnViewportClicked;
    event Action<bool> OnBlocked;
    event Action<Vector2> OnScrollValueChanged;
}