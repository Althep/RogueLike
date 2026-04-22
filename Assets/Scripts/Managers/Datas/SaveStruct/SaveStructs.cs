using UnityEngine;
using System.Collections.Generic;
using static Defines;
[System.Serializable]
public struct MapSaveData
{
    public int width;
    public int height;
    public int[] tileIds;
    public int[] wallIds;
    public List<SpecialObjectData> specialObjs;
}
public struct SpecialObjectData
{
    public int x, y;
    public TileType tileType;
    public bool state;
}

public struct ModifierSaveData
{
    public float value;
    public string modifierId;
}
public struct BuffSaveData
{
    public float value;
    public string modifierId;
    public int leftTime;
}
public class ItemSaveData
{
    public int spriteIndex;
    public List<ModifierSaveData> itemModifiers;
    public List<ModifierSaveData> addOptionsData;
}


public class ItemEntitySaveData
{
    public int x, y;
    public ItemSaveData itemData;
}

public class LivingEntitySaveData
{
    public string id;
    public float currentHp;
    public List<ItemSaveData> itemdata;//인벤토리, 장착중인 아이템 등
    public List<ModifierSaveData> modifiers;//종족특성,돌연변이등
    public List<BuffSaveData> buffs;//버프 데이터
    public List<ItemSaveData> equipMentsData;
    
}
public class PlayerSaveData
{

}
public class ExplorationData
{
    public int _width;
    public int _height;
    public bool[] visited;
}
