using UnityEngine;
using System;
using System.Collections.Generic;
public class InputController 
{

    public virtual void Init()
    {

    }


    public virtual Vector2Int OnMoveKey(KeyCode key)
    {
        return Vector2Int.zero;
    }

    public virtual void OnQuickKey(KeyCode key)
    {
        
    }

    public virtual void OnKey()
    {

    }
}
