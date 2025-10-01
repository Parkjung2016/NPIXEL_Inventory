using System;
using UnityEngine;

public interface IOptimizeScrollRectDataSource
{
    public event Action OnUpdateItemCount;
    RectTransform CellPrefab { get; }
    int GetItemCount();
    void SetCell(ICell cell, int index);
}