using System.Collections.Generic;
using UnityEngine;

public interface IPopupParentable
{
    Transform ChildPopupUIParentTransform { get; }
    Stack<IPopupUI> ChildPopupUIStack { get; set; }
}

public interface IPopupUI
{
    GameObject GameObject { get; }
    bool AllowDuplicates { get; }
    void OpenPopup();
    void ClosePopup();
}