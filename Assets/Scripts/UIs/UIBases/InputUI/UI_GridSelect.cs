using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class UI_GridSelect : UI_InputUIBase 
{
    [Header("Grid Data")]
    protected List<List<UI_Base>> _grid = new List<List<UI_Base>>();
    protected int _currentRow = 0;
    protected int _currentCol = 0;

    [Header("Visual Feedback")]
    [SerializeField] protected GameObject SelectObj; // 유저에게 보여줄 하이라이트 오브젝트

    private void Awake()
    {
        // 인풋 타입 설정 (기존 VirticalUI와 구분 필요하면 새로운 타입 정의)
        myInputType = Defines.InputType.GridUI;
    }
    private void OnEnable()
    {
        EnableFunc();
    }
    protected override void EnableFunc()
    {
        InputManager.instance.ChangeContext(GetInputType(), this, true);
    }

    public void SetGridData(List<List<UI_Base>> newData)
    {
        _grid = newData;

        // 데이터 주입 시 범위를 벗어나지 않게 보정
        _currentRow = Mathf.Clamp(_currentRow, 0, Mathf.Max(0, _grid.Count - 1));
        if (_grid.Count > 0)
            _currentCol = Mathf.Clamp(_currentCol, 0, Mathf.Max(0, _grid[_currentRow].Count - 1));

        UpdateSelectionEffect();
    }

    public virtual void ChangeSelection(Vector2Int direction)
    {
        if (_grid == null || _grid.Count == 0) return;

        int nextRow = Mathf.Clamp(_currentRow - direction.y, 0, _grid.Count - 1);

        //열(Col) 이동
        int nextCol = _currentCol;
        if (nextRow != _currentRow)
        {
            nextCol = Mathf.Clamp(_currentCol, 0, _grid[nextRow].Count - 1);
        }
        else
        {
            nextCol = Mathf.Clamp(_currentCol + direction.x, 0, _grid[_currentRow].Count - 1);
        }

        _currentRow = nextRow;
        _currentCol = nextCol;

        UpdateSelectionEffect();
    }

    protected virtual void UpdateSelectionEffect()
    {
        if (_grid.Count == 0 || _grid[_currentRow].Count == 0) return;

        UI_Base targetCell = _grid[_currentRow][_currentCol];

        if (targetCell != null && SelectObj != null)
        {
            SelectObj.transform.SetParent(targetCell.transform);

            RectTransform selectRT = SelectObj.GetComponent<RectTransform>();
            if (selectRT != null)
            {
                selectRT.anchoredPosition = Vector2.zero;

                selectRT.sizeDelta = targetCell.GetComponent<RectTransform>().sizeDelta;
            }
            else
            {
                SelectObj.transform.localPosition = Vector3.zero;
            }
        }
    }

    public virtual void ExecuteSelectedMenu()
    {
        if (_grid[_currentRow][_currentCol] != null)
        {
            _grid[_currentRow][_currentCol].Excute();
        }
    }

    
}