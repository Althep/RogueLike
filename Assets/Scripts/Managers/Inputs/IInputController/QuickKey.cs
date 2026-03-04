using UnityEngine;
using System;
using System.Collections.Generic;
public class QuickKey 
{

    public virtual void Function()
    {

    }
    
}
public class MoveKey
{
    protected Vector2Int dir;

    public virtual void Init()
    {

    }

    public Vector2Int Get_Dir()
    {
        return dir;
    }
}
public class Move_Up : MoveKey
{
    public Move_Up()
    {
        Init();
    }
    
    public override void Init()
    {
        dir = new Vector2Int(0, 1);
    }
}
public class Move_Down : MoveKey
{
    public Move_Down()
    {
        Init();
    }
    public override void Init()
    {
        dir = new Vector2Int(0, -1);
    }
}
public class Move_UpLeft: MoveKey
{
    public Move_UpLeft()
    {
        Init();
    }
    public override void Init()
    {
        dir = new Vector2Int(-1, 1);
    }
}

public class Move_UpRight : MoveKey
{
    public Move_UpRight()
    {
        Init();
    }
    public override void Init()
    {
        dir = new Vector2Int(1, 1);
    }
}
public class Stay : MoveKey
{
    public Stay()
    {
        Init();
    }
    public override void Init()
    {
        dir = new Vector2Int(0,0);
    }
}
public class Move_Left : MoveKey
{
    public Move_Left()
    {
        Init();
    }
    public override void Init()
    {
        dir = new Vector2Int(-1, 0);
    }
}
public class Move_Right: MoveKey
{
    public Move_Right()
    {
        Init();
    }
    public override void Init()
    {
        dir = new Vector2Int(1, 0);
    }
}

public class Move_DownLeft : MoveKey
{
    public Move_DownLeft()
    {
        Init();
    }
    public override void Init()
    {
        dir = new Vector2Int(-1, -1);
    }
}
public class Move_DownRight : MoveKey
{
    public Move_DownRight()
    {
        Init();
    }
    public override void Init()
    {
        dir = new Vector2Int(1, -1);
    }
}