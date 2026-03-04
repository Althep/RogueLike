using System.Collections.Generic;
using UnityEngine;
using static Defines;
public class InputUIManager : MonoBehaviour
{
    Stack<UI_InputUIBase> inputUIStack = new Stack<UI_InputUIBase>();
    Dictionary<string, UI_InputUIBase> Input_popUpObjs = new Dictionary<string, UI_InputUIBase>();
    Dictionary<string, string> input_ButtonBind = new Dictionary<string, string>();//버튼이름 키 , 열릴 오브젝트 밸류 , 이부분 CSV로 매핑처리 필요



    public void OpenInputUI(UI_InputUIBase OpendUI, InputType inputType)
    {
        OpendUI.gameObject.SetActive(true);
        inputUIStack.Push(OpendUI);
        InputManager.instance.ChangeContext(inputType, OpendUI);
        OpendUI.transform.SetAsLastSibling();
    }

    public void CloseInputUI(UI_InputUIBase closedUI, InputType inputType)
    {

    }

    public void PopUpInputUI(string buttonName)
    {
        Debug.Log($"Button Name {buttonName}, OpenUI Name {Input_popUpObjs[buttonName]}");

        if (Input_popUpObjs.ContainsKey(buttonName))
        {
            string targetName = input_ButtonBind[buttonName];
            if (!Input_popUpObjs.ContainsKey(targetName))
            {
                Debug.Log("Name Didn't contain");
            }
            UI_InputUIBase inputBase = Input_popUpObjs[targetName];
            InputType type = inputBase.GetInputType();
            OpenInputUI(inputBase, type);
        }
    }
}
