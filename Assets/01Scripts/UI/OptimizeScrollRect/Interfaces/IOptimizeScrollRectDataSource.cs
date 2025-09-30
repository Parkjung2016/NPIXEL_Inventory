using UnityEngine;

public interface IOptimizeScrollRectDataSource
{
    public RectTransform CellPrefab { get; }
    int GetItemCount();
    void SetCell(ICell cell, int index);
}