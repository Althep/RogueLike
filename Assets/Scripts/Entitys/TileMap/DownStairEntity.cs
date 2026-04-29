using UnityEngine;

public class DownStairEntity : StairEntity
{
    public int stairNumber;

    protected override void Init()
    {
        base.Init();
        myType = Defines.TileType.DownStair;
    }

    public override MapEntity CopyToEmpty()
    {
        return new DownStairEntity();
    }

    public override void ResetData()
    {
        stairNumber = 0;
    }

    public override SpecialObjectData Get_SaveData()
    {
        SpecialObjectData saveData = new SpecialObjectData {x = posKey.x,y=posKey.y,tileType = this.GetMyType() };
        saveData.x = posKey.x;
        saveData.y = posKey.y;
        return saveData;
    }

    public override void Interact()
    {
        SaveDataManager.instance.SaveAllFloorDatas();
    }
}
