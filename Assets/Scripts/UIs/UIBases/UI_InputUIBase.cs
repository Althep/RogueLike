using UnityEngine;
using static Defines;
public class UI_InputUIBase : UI_Base
{

    public InputType myInputType;
    public InputType GetInputType() => myInputType;
    private void OnEnable()
    {
        InputManager.instance.ChangeContext(GetInputType(), this, true);
    }
    private void OnDisable()
    {
        
    }
    protected virtual void EnableFunc()
    {

    }
    public virtual void CloseUI()
    {
        //InputManager.instance.CloseInputUI(this);
    }

    public virtual void Set_Input()
    {
        InputManager.instance.ChangeContext(GetInputType(), this, true);
    }
}
