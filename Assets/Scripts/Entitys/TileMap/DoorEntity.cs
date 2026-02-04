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

    public override MapEntity CopyToEmpty()
    {
        DoorEntity newDoor = new DoorEntity();
        if (newDoor.IsOpen())
        {
            newDoor.Interaction();
        }
        return newDoor;
    }

    public override void ResetData()
    {
        isOpen = false;
    }
}
