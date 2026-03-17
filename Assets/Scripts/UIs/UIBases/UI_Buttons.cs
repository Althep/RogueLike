using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using Cysharp.Threading.Tasks;
public class UI_Buttons : UI_Base
{
    [SerializeField]Func<UniTask> myAction;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetMyAction(Func<UniTask> action)
    {
        myAction = action;
        AddUIEvent(this.gameObject, OnClickEvent, Defines.UIEvents.Click);
    }

    public void SetMyAction(Action action)
    {
        // 일반 Action을 UniTask 반환형으로 변환하여 저장
        myAction = () => {
            action?.Invoke();
            return UniTask.CompletedTask;
        };
        AddUIEvent(this.gameObject, OnClickEvent, Defines.UIEvents.Click);
    }
    public override void Excute()
    {
        Debug.Log($"{this.gameObject.name} Excute!");
        myAction.Invoke();
    }

    public void OnClickEvent(PointerEventData pve)
    {
        Excute();
    }
}
