using System;
using System.Collections.Generic;
using System.Linq;
using PJH.Utility.Managers;
using UnityEngine;
using Object = UnityEngine.Object;

public class UIManager
{
    private Stack<IPopupUI> _popupStack = new Stack<IPopupUI>();

    public T ShowPopup<T>(string popupUIName, IPopupParentable parentable = null) where T : Component, IPopupUI
    {
        IPopupUI popupUI = AddressableManager.Load<T>(popupUIName);
        if (!popupUI.AllowDuplicates)
        {
            if (CheckPopupInStack(popupUI.GetType())) return null;
        }

        popupUI = AddressableManager.Instantiate<T>(popupUIName);
        if (parentable != null)
        {
            popupUI.GameObject.transform.SetParent(parentable.ChildPopupUIParentTransform, false);
            parentable.ChildPopupUIStack.Push(popupUI);
        }

        popupUI.OpenPopup();
        _popupStack.Push(popupUI);
        return popupUI as T;
    }

    private bool CheckPopupInStack(Type type)
    {
        var popupUIList = _popupStack.ToList();
        bool exists = popupUIList.Exists(x => x.GetType() == type);
        return exists;
    }

    public void ClosePopup()
    {
        if (_popupStack.Count == 0) return;

        IPopupUI topPopup = _popupStack.Pop();
        if (topPopup == null) return;
        topPopup.ClosePopup();
        Object.Destroy(topPopup.GameObject);
    }

    public void ClosePopup(IPopupUI popupUI)
    {
        if (_popupStack.Count == 0) return;
        var popupList = _popupStack.ToList();
        popupList.Remove(popupUI);
        if (popupUI != null)
        {
            popupUI.ClosePopup();
            Object.Destroy(popupUI.GameObject);
        }

        _popupStack = new Stack<IPopupUI>(popupList);
    }
}