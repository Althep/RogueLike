using UnityEngine;
using UnityEngine.EventSystems;
using System;
using System.Linq;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TMPro;

public class UI_ConfirmPanel : UI_Base
{
    [SerializeField] protected UI_Buttons confirmButton;
    [SerializeField] protected UI_Buttons cancelButton;

    [SerializeField] protected UI_StringKeyController confirmText;

    protected Action myAction;
    List<List<UI_Base>> gridData = new List<List<UI_Base>>();
    private void Awake()
    {
        OnAwake();
    }
    void OnAwake()
    {
        List<UI_Buttons> childs = transform.GetComponentsInChildren<UI_Buttons>(true).ToList();
        
        if(confirmButton == null)
        {
            confirmButton = childs.Find(c => c.name == "ConfirmButton");
        }
        if(cancelButton == null)
        {
            cancelButton = childs.Find(c => c.name == "CancleButton");
        }

        AddGridData();
    }
    public void AddGridData()
    {
        Debug.Log("AddGridData In ConfirmPanel");
        if(confirmButton != null)
        {
            List<UI_Base> confirm = new List<UI_Base>() { confirmButton };
            gridData.Add(confirm);
        }
        if(cancelButton != null)
        {
            List<UI_Base> cancel = new List<UI_Base>() { cancelButton };
            gridData.Add(cancel);
        }
    }
    public List<List<UI_Base>> Get_GridData()
    {
        return gridData;
    }
    // 패널 열기
    public void Open(Action confirmAction)
    {
        gameObject.SetActive(true);

        // 매번 교체 → 이전 액션은 전부 날아감
        myAction = confirmAction;
    }
    public void SetConfirmButtonFunc()
    {

    }
    public void SetCancelButtonFunc()
    {

    }
    // 패널 닫기
    public virtual void Close()
    {
        myAction = null;   // 리셋 
        gameObject.SetActive(false);
    }

    protected virtual void OnClickConfirm(PointerEventData evt)
    {
        myAction?.Invoke();
        Close(); // 확인 후 닫기 (원하면 유지도 가능)
    }
    public UI_Buttons GetConfrimButton()
    {
        return confirmButton;
    }
    public UI_Buttons GetCancelButton()
    {
        return cancelButton;
    }
    protected virtual void OnClickCancel(PointerEventData evt)
    {
        Close(); // 취소는 단순 닫기
    }

    public void SetConfirmAction(Action action) => confirmButton.SetMyAction(action);
    public void SetConfirmAction(Func<UniTask> action) => confirmButton.SetMyAction(action);

    // --- 취소 버튼 세팅 (오버로딩) ---
    public void SetCancelAction(Action action) => cancelButton.SetMyAction(action);
    public void SetCancelAction(Func<UniTask> action) => cancelButton.SetMyAction(action);
}