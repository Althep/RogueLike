using UnityEngine;
using UnityEngine.EventSystems;
using System;
using System.Collections.Generic;
using System.Collections;
using Cysharp.Threading.Tasks;
public class DungeonSceneTest : UI_Base
{
    GameManager gameManager;
    [SerializeField] MapManager mapManager;
    [SerializeField] MonsterManager monsterManager;
    
    private void Awake()
    {
        if(mapManager == null)
        {
            mapManager = GameManager.instance.Get_MapManager();
        }
        if(monsterManager == null)
        {
            monsterManager = GameManager.instance.Get_MonsterManager();
        }
        
    }
    void Start()
    {
        AddUIEvent(this.gameObject, OnClickAction, Defines.UIEvents.Click);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnClickAction(PointerEventData pve)
    {
        Action().Forget();
    }

    public async UniTask Action()
    {
        SceneController sceneController = GameManager.instance.Get_SceneController();
        await mapManager.ReturnAll();
        await mapManager.MapMake();
        await monsterManager.MonsterSpawnTest();
    }
}
