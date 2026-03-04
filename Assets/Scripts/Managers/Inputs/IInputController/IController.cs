using UnityEngine;

public interface IInputController 
{
    void SetUp(GameControlls controls, InputManager manager);
    public void OnUpdate();
    public void Enter(object target);
    public void Exit();
}
