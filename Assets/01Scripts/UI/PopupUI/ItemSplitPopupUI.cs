using UnityEngine;

public class ItemSplitPopupUI : MonoBehaviour, IPopupUI
{
    public GameObject GameObject => gameObject;
    public string PopupUIName => gameObject.name;

    public void OpenPopup()
    {
    }

    public void ClosePopup()
    {
    }
}