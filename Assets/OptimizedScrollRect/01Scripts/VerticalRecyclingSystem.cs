using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 셀 재활용 시스템(Vertical)
/// </summary>
public class VerticalRecyclingSystem
{
    private readonly RectTransform _viewport;
    private readonly RectTransform _content;
    private readonly RectTransform _cell;
    private readonly Vector3[] _corners = new Vector3[4];
    private readonly float _minPoolCoverage = 1.5f;
    private readonly int _minPoolSize = 10;
    private readonly float _recyclingThreshold = .2f;
    private readonly int _columns;

    private IOptimizeScrollRectDataSource _dataSource;
    private List<RectTransform> _cellPool;
    private List<ICell> _cachedCells;
    private Bounds _recyclableViewBounds;

    private float _cellWidth, _cellHeight;
    private bool _recycling;

    private int _currentItemCount;
    private int _topMostCellIndex, _bottomMostCellIndex;

    private int
        _topMostCellColumns,
        _bottomMostCellColumns;

    public VerticalRecyclingSystem(RectTransform cell, RectTransform viewport, RectTransform content,
        IOptimizeScrollRectDataSource dataSource, int columns)
    {
        _cell = cell;
        _viewport = viewport;
        _content = content;
        _columns = columns;
        SetDataSource(dataSource);
        _recyclableViewBounds = new Bounds();
    }

    public void SetDataSource(IOptimizeScrollRectDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    /// <summary>
    /// 초기화를 함수
    /// UI 갱신을 위해 한 프레임 대기하였다가 초기화 진행
    /// </summary>
    /// <param name="initializedCallback">초기화가 완료되었을 때 호출되는 콜백</param>
    public IEnumerator InitCoroutine(System.Action InitializedCallBack)
    {
        SetTopAnchor(_content);
        _content.anchoredPosition = Vector3.zero;
        yield return null;
        SetRecyclingBounds();
        CreateCellPool();
        _currentItemCount = _cellPool.Count;
        _topMostCellIndex = 0;
        _bottomMostCellIndex = _cellPool.Count - 1;

        int noOfRows = (int)Mathf.Ceil((float)_cellPool.Count / (float)_columns);

        float contentYSize = noOfRows * _cellHeight + (noOfRows - 1);
        float contentXSize = _columns * _cellWidth + (_columns - 1);


        _content.sizeDelta = new Vector2(contentXSize, contentYSize);
        SetTopAnchor(_content);

        InitializedCallBack?.Invoke();
    }

    /// <summary>
    /// 셀을 재활용하기 위한 영역 설정
    /// </summary>
    private void SetRecyclingBounds()
    {
        _viewport.GetWorldCorners(_corners);
        float threshHold = _recyclingThreshold * (_corners[2].y - _corners[0].y);
        _recyclableViewBounds.min = new Vector3(_corners[0].x, _corners[0].y - threshHold);
        _recyclableViewBounds.max = new Vector3(_corners[2].x, _corners[2].y + threshHold);
    }

    /// <summary>
    /// 재활용을 위한 Cell Pool 생성
    /// </summary>
    private void CreateCellPool()
    {
        if (_cellPool != null)
        {
            _cellPool.ForEach((RectTransform item) => UnityEngine.Object.Destroy(item.gameObject));
            _cellPool.Clear();
            _cachedCells.Clear();
        }
        else
        {
            _cachedCells = new List<ICell>();
            _cellPool = new List<RectTransform>();
        }

        _cell.gameObject.SetActive(true);
        SetTopLeftAnchor(_cell);

        _topMostCellColumns = _bottomMostCellColumns = 0;

        float currentPoolCoverage = 0;
        int poolSize = 0;
        float posX = 0;
        float posY = 0;

        float totalSpacingX = _columns - 1;
        _cellWidth = (_viewport.rect.width - totalSpacingX) / _columns;

        _cellHeight = _cell.sizeDelta.y / _cell.sizeDelta.x * _cellWidth;

        float requiredCoverage = _minPoolCoverage * _viewport.rect.height;
        int minPoolSize = Math.Min(this._minPoolSize, _dataSource.GetItemCount());

        // 풀 영역이 채워지고 최소 풀 크기를 만족할 때까지 셀 생성 반복
        while ((poolSize < minPoolSize || currentPoolCoverage < requiredCoverage) &&
               poolSize < _dataSource.GetItemCount())
        {
            RectTransform item = (UnityEngine.Object.Instantiate(_cell.gameObject))
                .GetComponent<RectTransform>();
            item.name = "Cell";
            item.sizeDelta = new Vector2(_cellWidth, _cellHeight);
            _cellPool.Add(item);
            item.SetParent(_content, false);

            posX = _bottomMostCellColumns * _cellWidth;
            item.anchoredPosition = new Vector2(posX, posY);
            if (++_bottomMostCellColumns >= _columns)
            {
                _bottomMostCellColumns = 0;
                posY -= _cellHeight;
                currentPoolCoverage += item.rect.height;
            }

            _cachedCells.Add(item.GetComponent<ICell>());
            _dataSource.SetCell(_cachedCells[_cachedCells.Count - 1], poolSize);

            poolSize++;
        }

        _bottomMostCellColumns = (_bottomMostCellColumns - 1 + _columns) % _columns;


        // 이미 씬에 존재하는 오브젝트라면 비활성화(프리팹이 아닌 경우)
        if (_cell.gameObject.scene.IsValid())
        {
            _cell.gameObject.SetActive(false);
        }
    }

    public Vector2 OnValueChangedListener(Vector2 direction)
    {
        if (_recycling || _cellPool == null || _cellPool.Count == 0) return Vector2.zero;

        // 재활용 영역 갱신
        SetRecyclingBounds();

        if (direction.y > 0 && _cellPool[_bottomMostCellIndex].MaxY() > _recyclableViewBounds.min.y)
        {
            return RecycleTopToBottom();
        }

        if (direction.y < 0 && _cellPool[_topMostCellIndex].MinY() < _recyclableViewBounds.max.y)
        {
            return RecycleBottomToTop();
        }

        return Vector2.zero;
    }

    /// <summary>
    /// 위에서 아래로 셀 재활용.
    /// </summary>
    private Vector2 RecycleTopToBottom()
    {
        _recycling = true;

        int n = 0;
        float posY = _cellPool[_bottomMostCellIndex].anchoredPosition.y;
        float posX = 0;

        int additionalRows = 0;
        // 맨 위 셀이 사용 가능하고 현재 아이템 수가 최대 데이터 수보다 작을 때까지 재활용
        while (_cellPool[_topMostCellIndex].MinY() > _recyclableViewBounds.max.y &&
               _currentItemCount < _dataSource.GetItemCount())
        {
            if (++_bottomMostCellColumns >= _columns)
            {
                n++;
                _bottomMostCellColumns = 0;
                posY = _cellPool[_bottomMostCellIndex].anchoredPosition.y - _cellHeight;
                additionalRows++;
            }

            // 위쪽 셀을 아래로 이동
            posX = _bottomMostCellColumns * _cellWidth;
            _cellPool[_topMostCellIndex].anchoredPosition = new Vector2(posX, posY);

            if (++_topMostCellColumns >= _columns)
            {
                _topMostCellColumns = 0;
                additionalRows--;
            }

            _dataSource.SetCell(_cachedCells[_topMostCellIndex], _currentItemCount);

            _bottomMostCellIndex = _topMostCellIndex;
            _topMostCellIndex = (_topMostCellIndex + 1) % _cellPool.Count;

            _currentItemCount++;
        }

        // Content 크기 조정(추가된 행만큼 높이 늘리기 위해)
        _content.sizeDelta += Vector2.up * (additionalRows * _cellHeight);
        if (additionalRows > 0)
        {
            n -= additionalRows;
        }

        // Content 앵커 위치를 조정
        _cellPool.ForEach((RectTransform cell) =>
            cell.anchoredPosition += Vector2.up * (n * _cellPool[_topMostCellIndex].sizeDelta.y));
        _content.anchoredPosition -= Vector2.up * (n * _cellPool[_topMostCellIndex].sizeDelta.y);
        _recycling = false;
        return -new Vector2(0, n * _cellPool[_topMostCellIndex].sizeDelta.y);
    }

    /// <summary>
    /// 아래에서 위로 셀 재활용.
    /// </summary>
    private Vector2 RecycleBottomToTop()
    {
        _recycling = true;

        int n = 0;
        float posY = _cellPool[_topMostCellIndex].anchoredPosition.y;
        float posX = 0;

        int additionalRows = 0;
        // 맨 아래 쪽 셀이 사용 가능하고 현재 아이템 수가 최대 데이터 수보다 작을 때까지 재활용
        while (_cellPool[_bottomMostCellIndex].MaxY() < _recyclableViewBounds.min.y &&
               _currentItemCount > _cellPool.Count)
        {
            if (--_topMostCellColumns < 0)
            {
                n++;
                _topMostCellColumns = _columns - 1;
                posY = _cellPool[_topMostCellIndex].anchoredPosition.y + _cellHeight;
                additionalRows++;
            }

            // 아래쪽 셀을 위쪽으로 이동
            posX = _topMostCellColumns * _cellWidth;
            _cellPool[_bottomMostCellIndex].anchoredPosition = new Vector2(posX, posY);

            if (--_bottomMostCellColumns < 0)
            {
                _bottomMostCellColumns = _columns - 1;
                additionalRows--;
            }

            _currentItemCount--;

            _dataSource.SetCell(_cachedCells[_bottomMostCellIndex], _currentItemCount - _cellPool.Count);

            _topMostCellIndex = _bottomMostCellIndex;
            _bottomMostCellIndex = (_bottomMostCellIndex - 1 + _cellPool.Count) % _cellPool.Count;
        }

        // Content 크기 조정(추가된 행만큼 높이 늘리기 위해)
        _content.sizeDelta += Vector2.up * (additionalRows * _cellHeight);
        if (additionalRows > 0)
        {
            n -= additionalRows;
        }


        _cellPool.ForEach((RectTransform cell) =>
            cell.anchoredPosition -= Vector2.up * (n * _cellPool[_topMostCellIndex].sizeDelta.y));
        _content.anchoredPosition += Vector2.up * (n * _cellPool[_topMostCellIndex].sizeDelta.y);
        _recycling = false;
        return new Vector2(0, n * _cellPool[_topMostCellIndex].sizeDelta.y);
    }


    /// <summary>
    /// 지정된 셀과 Content RectTransform을 상단 기준으로 고정(위치 재조정하기 위해)
    /// </summary>
    private void SetTopAnchor(RectTransform rectTransform)
    {
        float width = rectTransform.rect.width;
        float height = rectTransform.rect.height;

        rectTransform.anchorMin = new Vector2(0.5f, 1);
        rectTransform.anchorMax = new Vector2(0.5f, 1);
        rectTransform.pivot = new Vector2(0.5f, 1);

        rectTransform.sizeDelta = new Vector2(width, height);
    }

    /// <summary>
    /// 지정된 셀과 Content RectTransform을 상단 좌측 기준으로 고정(위치 재조정하기 위해)
    /// </summary>
    private void SetTopLeftAnchor(RectTransform rectTransform)
    {
        float width = rectTransform.rect.width;
        float height = rectTransform.rect.height;

        rectTransform.anchorMin = new Vector2(0, 1);
        rectTransform.anchorMax = new Vector2(0, 1);
        rectTransform.pivot = new Vector2(0, 1);

        rectTransform.sizeDelta = new Vector2(width, height);
    }
}