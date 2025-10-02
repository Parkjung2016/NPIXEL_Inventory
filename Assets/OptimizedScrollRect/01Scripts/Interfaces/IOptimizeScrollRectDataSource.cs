using System;

public interface IOptimizeScrollRectDataSource
{
    public Action OnUpdateItemCount { get; set; }
    int GetItemCount();
    void SetCell(ICell cell, int index);
}