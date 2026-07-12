using UnityEngine;
using System.Collections.Generic;
using static Defines;
[System.Serializable]

public class FloorSaveData
{
    public int width;
    public int height;
    public List<TileEntityData> tileDatas;
    public List<ItemEntitySaveData> droppedItems;
    public List<LivingEntitySaveData> monsterDatas;
    public List<Vector2Int> visited;
}

public struct TileEntityData
{
    public int x;
    public int y;
    public int spriteNumber;
    public int stairNumber;
    public bool state;
    public TileType type;
}

public struct ModifierOptionSaveData
{
    public string optionKey;
    public List<ModifierSaveData> modifiers;
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
    public int leftDuration;
}
public class ItemSaveData
{
    public int spriteIndex;
    public int itemCount;
    public string itemId;
    public List<ModifierOptionSaveData> addOptionsData;
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
    public int x;
    public int y;
    public List<ItemSaveData> itemdata;//인벤토리
    public List<ItemSaveData> equipMentsData;//장착중인 아이템 등
    public List<ModifierOptionSaveData> modifiers;//종족특성,돌연변이등
    public List<BuffSaveData> buffs;//버프 데이터
    
    
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
