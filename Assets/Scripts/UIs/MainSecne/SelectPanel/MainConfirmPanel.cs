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
        if(selectPanel == null)
        {
            selectPanel = parentsObj.transform.GetComponent<SelectPanels>();
        }
        if (sceneController == null)
        {
            sceneController = GameManager.instance.Get_SceneController();
        }
        if (dungeonManager == null)
        {
            dungeonManager = GameManager.instance.GetDungeonManager();
        }

        
    }

    async UniTask DungeonLoadFunc()
    {
        await selectPanel.AddItemsAsync();
        await sceneController.PrePostAwaits( null, Defines.Scenes.DungeonScene, dungeonManager.GenerateDungeon);
    }

}
