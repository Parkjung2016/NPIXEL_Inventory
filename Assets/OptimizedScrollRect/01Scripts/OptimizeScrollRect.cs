using System;
using UnityEngine;
using UnityEngine.UI;

public class OptimizeScrollRect : ScrollRect
{
    public int Segments
    {
        set { segments = Math.Max(value, 2); }
        get { return segments; }
    }

    [SerializeField] private int segments;
    [SerializeField] private RectTransform cellPrefab;

    private IOptimizeScrollRectDataSource _dataSource;
    public VerticalRecyclingSystem RecyclingSystem => _recyclingSystem;
    private VerticalRecyclingSystem _recyclingSystem;
    private Vector2 _prevAnchoredPos;

    protected override void Start()
    {
        if (!Application.isPlaying) return;
        Initialize();
    }

    public void SetDataSource(IOptimizeScrollRectDataSource dataSource)
    {
        if (_dataSource != null)
            _dataSource.OnUpdateItemCount -= HandleUpdateItemCount;
        _dataSource = dataSource;
        if (_dataSource != null)
            _dataSource.OnUpdateItemCount += HandleUpdateItemCount;
    }

    private void Initialize()
    {
        _recyclingSystem =
            new VerticalRecyclingSystem(cellPrefab, viewport, content, _dataSource, Segments);

        vertical = true;
        horizontal = false;

        _prevAnchoredPos = content.anchoredPosition;
        onValueChanged.RemoveListener(OnValueChangedListener);
        StartCoroutine(_recyclingSystem.InitCoroutine(() =>
            onValueChanged.AddListener(OnValueChangedListener)
        ));
    }

    protected override void OnDestroy()
    {
        if (_dataSource != null)
            _dataSource.OnUpdateItemCount -= HandleUpdateItemCount;
        base.OnDestroy();
    }

    private void HandleUpdateItemCount()
    {
        Debug.Log(_dataSource);
        ReloadData();
    }

    public void OnValueChangedListener(Vector2 normalizedPos)
    {
        Vector2 dir = content.anchoredPosition - _prevAnchoredPos;
        m_ContentStartPosition += _recyclingSystem.OnValueChangedListener(dir);
        _prevAnchoredPos = content.anchoredPosition;
    }

    public void ReloadData()
    {
        ReloadData(_dataSource);
    }

    public void ReloadData(IOptimizeScrollRectDataSource dataSource)
    {
        if (_recyclingSystem != null)
        {
            StopMovement();
            onValueChanged.RemoveListener(OnValueChangedListener);
            _recyclingSystem.SetDataSource(dataSource);
            StartCoroutine(_recyclingSystem.InitCoroutine(() =>
                onValueChanged.AddListener(OnValueChangedListener)
            ));
            _prevAnchoredPos = content.anchoredPosition;
        }
    }
}