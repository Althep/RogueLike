using UnityEngine;
using System;
using System.Collections.Generic;
public class DoorEntity : MapEntity
{
    bool isOpen = false;

    
    protected override void Init()
    {
        base.Init();
        myType = Defines.TileType.Door;
    }

    public void Set_OpenState(bool isOpen)
    {
        this.isOpen = isOpen;
    }
    public bool IsOpen()
    {
        return isOpen;
    }
    public void Interaction()
    {
        if (isOpen)
        {
            Close();
        }
        else
        {
            Open();
        }
    }

    void Open()
    {
        isOpen = true;
    }

    void Close()
    {
        isOpen = false;
    }



    public override void ResetData()
    {
        isOpen = false;
    }

    public override TileEntityData Get_SaveData()
    {
        Vector2Int posKey = Get_PosKey();
        TileEntityData data = new TileEntityData() { x = posKey.x, y = posKey.y, state = isOpen, type = GetMyType()};


        return data;
    }

    public override void Return()
    {
        PoolManager.instance.Return(GetMyType(), this.gameObject);
        isOpen = false;
    }
}
