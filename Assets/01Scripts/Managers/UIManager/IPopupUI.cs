using UnityEngine;

public interface IPopupUI
{
    public GameObject GameObject { get; }
    public string PopupUIName { get; }
    public void OpenPopup();
    public void ClosePopup();
}