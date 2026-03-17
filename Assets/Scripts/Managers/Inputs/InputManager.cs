using UnityEngine;
using System;
using System.Collections.Generic;
using static Defines;
/*
public struct InputState
{
    public InputType type;
    public UI_InputUIBase target;

    public InputState(InputType type, UI_InputUIBase target)
    {
        this.type = type;
        this.target = target;
    }
}*/
public class InputManager : MonoBehaviour
{
    public static InputManager instance { get; private set; }
    GameControlls controls;
    IInputController currentInput;
    Dictionary<InputType, IInputController> inputs;
    private List<UI_InputUIBase> history = new List<UI_InputUIBase>();
    [SerializeField]private UI_InputUIBase currentUI;
    private void Awake()
    {
        OnAwake();

    }

    private void Start()
    {

    }

    private void Update()
    {

    }

    public void OnAwake()
    {
        if (InputManager.instance == null)
        {
            instance = this;
        }
        InitControllers();
    }
    public void InitControllers()
    {
        controls = new GameControlls();
        inputs = new Dictionary<InputType, IInputController>
        {
            { InputType.VirticalUI, new Input_VirticalUI() },
            {InputType.HorizonUI, new Input_HorizonUI() },
            { InputType.Player, new Input_Player() },
            { InputType.GridUI, new Input_GridUI() },
            { InputType.SelectTarget, new Input_SelectTarget() }
        };

        foreach (var cotr in inputs.Values)
        {
            cotr.SetUp(controls, this);
        }

    }
    public void ChangeContext(InputType type, UI_InputUIBase target, bool saveHistory = true)
    {
        // 1. 현재 UI가 있고, 저장이 필요한 경우 '현재 UI'를 히스토리에 추가
        if (saveHistory && currentUI != null && currentUI != target)
        {
            // 중복 저장 방지
            if (!history.Contains(currentUI))
                history.Add(currentUI);
        }

        // 2. 기존 컨트롤러 종료
        if (currentInput != null) currentInput.Exit();

        // 3. 새 컨텍스트 설정
        currentUI = target;
        currentInput = inputs[type];
        currentInput.Enter(target);
    }

    public void CloseInputUI(UI_InputUIBase target)
    {
        if (target == null) return;

        target.gameObject.SetActive(false); 
        target.CloseUI(); 
        if (history.Contains(target))
        {
            history.Remove(target);
        }

        // 2. 닫으려는 대상이 '현재 활성화된 UI'인 경우에만 포커스 전환
        if (target == currentUI)
        {
            currentInput.Exit();
            currentUI = null; // 초기화

            if (history.Count > 0)
            {
                int lastIndex = history.Count - 1;
                UI_InputUIBase nextUI = history[lastIndex];

                // 리스트에서 꺼내어 활성화하므로 history에서 제거
                history.RemoveAt(lastIndex);
                ChangeContext(nextUI.myInputType, nextUI, false);
            }
            else
            {
                // 모든 UI가 닫혔을 때 기본 상태(예: Player)로 복귀 로직
                // ChangeContext(InputType.Player, playerReference, false);
            }
        }
    }
    public void RestorePreviousContext()
    {
        if (history.Count > 0)
        {
            var previous = history[history.Count-1];
            // 재귀 저장을 막기 위해 saveHistory는 false로 호출
            ChangeContext(previous.myInputType, previous, false);
        }
        else
        {
            //디폴트 컨트롤러로 변경
            ChangeDefaultState();
        }
    }

    public void ChangeDefaultState()
    {
        Scenes currentScene = SceneController.Instance.Get_CurrentScene();
        switch (currentScene)
        {
            case Scenes.MainScene:
                Set_MainSceneDefault();
                break;
            case Scenes.DungeonScene:
                Set_DungeonSceneDefault();
                break;
            case Scenes.EndingScene:
                Set_EndSceneDefault();
                break;
            default:
                break;
        }
    }
    public void ClearInput()
    {
        history.Clear();
    }
    public void OnSceneChange()
    {
        ClearInput();
        ChangeDefaultState();
    }
    public void Set_MainSceneDefault()
    {
        GameObject go = GameObject.Find("MainMenu");
        UI_VirticalSelect target = go.transform.GetComponent<UI_VirticalSelect>();
        ChangeContext(target.myInputType, target, true);
    }
    public void Set_DungeonSceneDefault()
    {
        PlayerInputController playercontroller = GameManager.instance.Get_PlayerInputController();
        playercontroller.Set_DefaultController();
    }
    public void Set_EndSceneDefault()
    {

    }
}
