using UnityEngine;

public interface IOptimizeScrollRectDataSource
{
    RectTransform CellPrefab { get; }
    int GetItemCount();
    void SetCell(ICell cell, int index);
}