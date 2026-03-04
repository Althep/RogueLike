using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class UI_GridSelect : UI_InputUIBase // 부모 클래스에 맞춰 상속 변경
{
    [Header("Grid Data")]
    protected List<List<UI_Base>> _grid = new List<List<UI_Base>>();
    protected int _currentRow = 0;
    protected int _currentCol = 0;

    [Header("Visual Feedback")]
    [SerializeField] private GameObject SelectObj; // 유저에게 보여줄 하이라이트 오브젝트

    private void Awake()
    {
        // 인풋 타입 설정 (기존 VirticalUI와 구분 필요하면 새로운 타입 정의)
        myInputType = Defines.InputType.GridUI;
    }

    /// <summary>
    /// 외부에서 데이터를 주입할 때 사용
    /// </summary>
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

        // 1. 행(Row) 이동: 위(+1)는 인덱스 감소, 아래(-1)는 인덱스 증가
        int nextRow = Mathf.Clamp(_currentRow - direction.y, 0, _grid.Count - 1);

        // 2. 열(Col) 이동
        int nextCol = _currentCol;
        if (nextRow != _currentRow)
        {
            // 행이 바뀔 때: 바뀐 행의 길이에 맞춰 X축 보정
            nextCol = Mathf.Clamp(_currentCol, 0, _grid[nextRow].Count - 1);
        }
        else
        {
            // 같은 행일 때: 좌/우 이동
            nextCol = Mathf.Clamp(_currentCol + direction.x, 0, _grid[_currentRow].Count - 1);
        }

        // 3. 값 적용 및 연출 갱신
        _currentRow = nextRow;
        _currentCol = nextCol;

        UpdateSelectionEffect();
    }

    private void UpdateSelectionEffect()
    {
        if (_grid.Count == 0 || _grid[_currentRow].Count == 0) return;

        UI_Base targetCell = _grid[_currentRow][_currentCol];

        if (targetCell != null && SelectObj != null)
        {
            // VirticalSelect 방식대로 SelectObj를 대상의 자식으로 이동
            SelectObj.transform.SetParent(targetCell.transform);

            // UI 좌표계(RectTransform)를 고려한 위치 초기화
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

    public void CancleMenu()
    {
        // 이전 화면으로 돌아가기 등의 로직
        Debug.Log("Grid Menu Cancelled");
    }
}