using System.Collections.Generic;
using PJH.Utility.Managers;
using UnityEngine;

public class UIManager
{
    private Stack<IPopupUI> _popupStack = new Stack<IPopupUI>();

    public void ShowPopup<T>(string popupUIName, Transform parentTrm = null) where T : Component, IPopupUI
    {
        IPopupUI popupUI = AddressableManager.Instantiate<T>(popupUIName);
        popupUI.GameObject.transform.SetParent(parentTrm, false);
        popupUI.OpenPopup();
        _popupStack.Push(popupUI);
    }

    public void ClosePopup()
    {
        if (_popupStack.Count == 0) return;

        IPopupUI topPopup = _popupStack.Pop();
        topPopup.ClosePopup();
        Object.Destroy(topPopup.GameObject);
    }
}