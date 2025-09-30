using System;
using UnityEngine;
using UnityEngine.UI;

public class OptimizeScrollRect : ScrollRect
{
    public int Segments
    {
        set { _segments = Math.Max(value, 2); }
        get { return _segments; }
    }

    [SerializeField] private InterfaceReference<IOptimizeScrollRectDataSource> _dataSource;
    [SerializeField] private Vector2 _spacing;
    [SerializeField] private RectOffset _padding;
    [SerializeField] private int _segments;

    public VerticalRecyclingSystem RecyclingSystem => _recyclingSystem;
    private VerticalRecyclingSystem _recyclingSystem;
    private Vector2 _prevAnchoredPos;

    protected override void Start()
    {
        if (!Application.isPlaying) return;
        Initialize();
    }

    private void Initialize()
    {
        _recyclingSystem =
            new VerticalRecyclingSystem(_dataSource.Value.CellPrefab, viewport, content, _dataSource.Value, Segments,
                _spacing);

        vertical = true;
        horizontal = false;

        _prevAnchoredPos = content.anchoredPosition;
        onValueChanged.RemoveListener(OnValueChangedListener);
        StartCoroutine(_recyclingSystem.InitCoroutine(() =>
            onValueChanged.AddListener(OnValueChangedListener)
        ));
    }

    public void OnValueChangedListener(Vector2 normalizedPos)
    {
        Vector2 dir = content.anchoredPosition - _prevAnchoredPos;
        m_ContentStartPosition += _recyclingSystem.OnValueChangedListener(dir);
        _prevAnchoredPos = content.anchoredPosition;
    }

    public void ReloadData()
    {
        ReloadData(_dataSource.Value);
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