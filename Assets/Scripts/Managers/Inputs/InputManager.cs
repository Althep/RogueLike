using UnityEngine;
using System;
using System.Collections.Generic;
using static Defines;
public struct InputState
{
    public InputType type;
    public object target;

    public InputState(InputType type, object target)
    {
        this.type = type;
        this.target = target;
    }
}
public class InputManager:MonoBehaviour
{
    public static InputManager instance { get; private set; }
    GameControlls controls;
    IInputController currentInput;
    Dictionary<InputType, IInputController> inputs;
    private Stack<InputState> history = new Stack<InputState>();
    private InputState currentState;
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

        foreach(var cotr in inputs.Values)
        {
            cotr.SetUp(controls, this);
        }
        
    }
    public void ChangeContext(InputType type, object target, bool saveHistory = true)
    {
        // 1. 현재 상태를 이력에 저장 (UI를 새로 열 때만 저장)
        if (saveHistory && currentInput != null)
        {
            history.Push(currentState);
        }

        // 2. 새로운 컨텍스트 설정
        if (currentInput != null) currentInput.Exit();

        currentState = new InputState(type, target);
        currentInput = inputs[type];
        currentInput.Enter(target);
    }

    public void RestorePreviousContext()
    {
        if (history.Count > 0)
        {
            var previous = history.Pop();
            // 재귀 저장을 막기 위해 saveHistory는 false로 호출
            ChangeContext(previous.type, previous.target, false);
        }
        else
        {
            // 이력이 없으면 기본 플레이어 상태로
            //ChangeContext(InputType.Player, PlayerManager.instance.player, false);
        }
    }
}
