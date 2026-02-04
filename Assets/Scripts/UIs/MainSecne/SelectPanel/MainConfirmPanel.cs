using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System.Collections;
using Cysharp.Threading.Tasks;
public class MainConfirmPanel : UI_ConfirmPanel
{
    [SerializeField]protected GameObject parentsObj;
    SelectPanels selectPanel;
    SceneController sceneController;
    DungeonManager dungeonManager;
    private void Awake()
    {
        selectPanel = parentsObj.transform.GetComponent<SelectPanels>();
        AddUIEvent(confirmButton, OnClickConfirm, Defines.UIEvents.Click);
        AddUIEvent(cancelButton, OnClickCancel, Defines.UIEvents.Click);
        if(sceneController == null)
        {
            sceneController = GameManager.instance.Get_SceneController();
        }
        if(dungeonManager == null)
        {
            dungeonManager = GameManager.instance.GetDungeonManager();
        }
    }

    public override void Close()
    {
        
        parentsObj.SetActive(false);
        //같은 기능을 실행할 고정 버튼에는 기능빼기X
    }

    protected override void OnClickConfirm(PointerEventData evt)
    {
        DungeonLoadFunc().Forget();
    }

    async UniTask DungeonLoadFunc()
    {
        StartCoroutine(selectPanel.AddItems());
        await sceneController.PrePostAwaits( null, Defines.Scenes.DungeonScene, dungeonManager.GenerateDungeon);
    }

}
