using UnityEngine;
using System;
using System.Collections.Generic;
public class InGameInput : InputController
{
    PlayerEntity playerEntity;

    Dictionary<string, MoveKey> moveKeyFunctions = new Dictionary<string, MoveKey>();
    Dictionary<KeyCode, string> moveKeyCodes = new Dictionary<KeyCode, string>();

    public Dictionary<KeyCode, string> keyToName = new Dictionary<KeyCode, string>();
    public Dictionary<string, QuickKey> nameToFunction = new Dictionary<string, QuickKey>();

    public InGameInput()
    {
        Init();
    }
   
    public override void Init()
    {
        if (playerEntity == null)
        {
            playerEntity = GameManager.instance.Get_PlayerObj().transform.GetComponent<PlayerEntity>();
        }
    }

    public override Vector2Int OnMoveKey(KeyCode key)
    {
        if (moveKeyCodes.ContainsKey(key))
        {
            string name = moveKeyCodes[key];
            return moveKeyFunctions[name].Get_Dir();
        }
        else
        {
            return Vector2Int.zero;
        }
    }

    public override void OnQuickKey(KeyCode key)
    {
        if (keyToName.ContainsKey(key))
        {
            string name = keyToName[key];
            nameToFunction[name].Function();
        }
    }

}
