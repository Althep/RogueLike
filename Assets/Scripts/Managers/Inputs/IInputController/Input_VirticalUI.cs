using System;
using UnityEngine;
public class Input_VirticalUI : IInputController
{
    GameControlls control;
    UI_VirticalSelect targetUI;


    private void OnMenuMove(int direction)
    {
        // UI 매니저에게 메뉴 커서를 이동하라고 명령합니다.
        // direction이 1이면 위, -1이면 아래로 인덱스를 계산하게 됩니다.
        if (targetUI != null)
        {
            targetUI.ChangeSelection(direction);
        }
        
    }

    private void OnMenuSelect()
    {
        // 현재 하이라이트된 메뉴를 실행합니다.
        Debug.Log("Excute");
        if (targetUI != null)
        {
            targetUI.ExecuteSelectedMenu();
        }
    }

    private void OnCancle()
    {
        targetUI.CancleMenu();
    }

    public void OnUpdate() { } // 메인메뉴는 이벤트 기반이므로 비워둡니다.


    public void Enter(object target)
    {

        control.VirticalUI.Enable();
        targetUI = target as UI_VirticalSelect;
    }

    public void Exit()
    {
        control.VirticalUI.Disable();
    }

    public void SetUp(GameControlls controls, InputManager manager)
    {

        // 1. 위/아래 이동 (Navigate 액션 활용)
        // 버튼 방식이므로 '위' 버튼과 '아래' 버튼에 각각 이벤트를 연결합니다.
        controls.VirticalUI.MoveN.performed += ctx => OnMenuMove(-1);  // 위
        controls.VirticalUI.MoveS.performed += ctx => OnMenuMove(1); // 아래
        // 2. 선택 버튼 (Select 액션)
        controls.VirticalUI.Select.performed += ctx => OnMenuSelect();
        controls.VirticalUI.Cancle.performed += ctx => OnCancle();
        control = controls;
    }

   

}