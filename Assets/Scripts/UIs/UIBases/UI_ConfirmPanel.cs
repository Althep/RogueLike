using UnityEngine;
using UnityEngine.EventSystems;
using System;
using TMPro;
public class UI_ConfirmPanel : UI_Base
{
    [SerializeField] GameObject confirmButton;
    [SerializeField] GameObject cancelButton;

    [SerializeField]UI_StringKeyController confirmText;

    Action myAction;

    private void Awake()
    {
        AddUIEvent(confirmButton, OnClickConfirm, Defines.UIEvents.Click);
        AddUIEvent(cancelButton, OnClickCancel, Defines.UIEvents.Click);
    }

    // �г� ����
    public void Open(Action confirmAction)
    {
        gameObject.SetActive(true);

        // �Ź� ��ü �� ���� �׼��� ���� ���ư�
        myAction = confirmAction;
    }

    // �г� �ݱ�
    public void Close()
    {
        myAction = null;   // ����
        gameObject.SetActive(false);
    }

    private void OnClickConfirm(PointerEventData evt)
    {
        myAction?.Invoke();
        Close(); // Ȯ�� �� �ݱ� (���ϸ� ������ ����)
    }

    private void OnClickCancel(PointerEventData evt)
    {
        Close(); // ��Ҵ� �ܼ� �ݱ�
    }
}