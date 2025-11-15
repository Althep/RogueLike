using UnityEngine;
using UnityEngine.EventSystems;
using System;
using TMPro;
public class UI_ConfirmPanel : UI_Base
{
    [SerializeField] protected GameObject confirmButton;
    [SerializeField] protected GameObject cancelButton;

    [SerializeField] protected UI_StringKeyController confirmText;

    protected Action myAction;
    private void Awake()
    {
        AddUIEvent(confirmButton, OnClickConfirm, Defines.UIEvents.Click);
        AddUIEvent(cancelButton, OnClickCancel, Defines.UIEvents.Click);
    }

    // 패널 열기
    public void Open(Action confirmAction)
    {
        gameObject.SetActive(true);

        // 매번 교체 → 이전 액션은 전부 날아감
        myAction = confirmAction;
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

    protected virtual void OnClickCancel(PointerEventData evt)
    {
        Close(); // 취소는 단순 닫기
    }
}