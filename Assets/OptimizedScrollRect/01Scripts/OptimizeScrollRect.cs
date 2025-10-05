using System;
using Cysharp.Threading.Tasks;
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
        _dataSource = dataSource;
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

    public void OnValueChangedListener(Vector2 normalizedPos)
    {
        Vector2 dir = content.anchoredPosition - _prevAnchoredPos;
        m_ContentStartPosition += _recyclingSystem.OnValueChangedListener(dir);
        _prevAnchoredPos = content.anchoredPosition;
    }

    public void ReloadData(bool reset = true)
    {
        ReloadDataInternal(_dataSource, reset);
    }

    private void ReloadDataInternal(IOptimizeScrollRectDataSource dataSource, bool reset = true)
    {
        if (_recyclingSystem != null)
        {
            StopMovement();
            if (reset)
            {
                onValueChanged.RemoveListener(OnValueChangedListener);
                _recyclingSystem.SetDataSource(dataSource);
                StartCoroutine(_recyclingSystem.InitCoroutine(() =>
                    onValueChanged.AddListener(OnValueChangedListener)
                ));
                _prevAnchoredPos = content.anchoredPosition;
            }
            else
            {
                _recyclingSystem.SetDataSource(dataSource);
           _recyclingSystem.ReloadData();
            }
        }
    }

    public async UniTask GoToTop()
    {
        while (_recyclingSystem.CanGoToTop())
        {
            normalizedPosition = new Vector2(0f, 1f);
            await UniTask.WaitForSeconds(.002f);
        }

        normalizedPosition = new Vector2(0f, 1f);
    }

    public async UniTask GoToBottom()
    {
        while (_recyclingSystem.CanGoToBottom())
        {
            normalizedPosition = new Vector2(0f, -1f);
            await UniTask.WaitForSeconds(.002f);
        }

        normalizedPosition = new Vector2(0f, -1f);
    }
}