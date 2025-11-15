using UnityEngine;
using System;
using System.Collections.Generic;
public class MainConfirmPanel : UI_ConfirmPanel
{
    [SerializeField]protected GameObject parentsObj;

    private void Awake()
    {
        AddUIEvent(confirmButton, OnClickConfirm, Defines.UIEvents.Click);
        AddUIEvent(cancelButton, OnClickCancel, Defines.UIEvents.Click);
    }

    public override void Close()
    {
        
        parentsObj.SetActive(false);
        //같은 기능을 실행할 고정 버튼에는 기능빼기X
    }

}
