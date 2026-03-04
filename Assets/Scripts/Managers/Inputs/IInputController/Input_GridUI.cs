using UnityEngine;
using System.Collections.Generic;
public class Input_GridUI : IInputController
{

    GameControlls control;
    UI_GridSelect targetUI;


    private readonly Vector2Int[] _dirVectors = new Vector2Int[]
        {
        new Vector2Int(0, 1),  // Up (ÀÎµ¦½º 0)
        new Vector2Int(0, -1), // Down (ÀÎµ¦½º 1)
        new Vector2Int(-1, 0), // Left (ÀÎµ¦½º 2)
        new Vector2Int(1, 0)   // Right (ÀÎµ¦½º 3)
        };
    public void Enter(object target)
    {
        control.GridUI.Enable();
        targetUI = target as UI_GridSelect;
    }

    public void Exit()
    {
        control.GridUI.Disable();
    }

    public void OnUpdate()
    {

    }

    public void SetUp(GameControlls controls, InputManager manager)
    {
        controls.GridUI.MoveN.performed += ctx => OnMenuMove(_dirVectors[0]);
        controls.GridUI.MoveS.performed += ctx => OnMenuMove(_dirVectors[1]);
        controls.GridUI.MoveE.performed += ctx => OnMenuMove(_dirVectors[3]);
        controls.GridUI.MoveS.performed += ctx => OnMenuMove(_dirVectors[2]);
        controls.GridUI.Select.performed += ctx => ExcuteSelectMenu();
        controls.GridUI.Cancle.performed += ctx => CancleMenu();
        control = controls;
    }

    private void OnMenuMove(Vector2Int direction)
    {
        targetUI.ChangeSelection(direction);
    }

    private void ExcuteSelectMenu()
    {
        targetUI.ExecuteSelectedMenu();
    }

    private void CancleMenu()
    {
        targetUI.CancleMenu();
    }
}
