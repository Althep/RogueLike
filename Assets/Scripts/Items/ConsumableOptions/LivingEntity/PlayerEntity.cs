using UnityEngine;
using System;
using System.Collections.Generic;
using static Defines;
public class PlayerEntity : LivingEntity
{
    InventoryData inventory = new InventoryData();
    MapManager mapManager;
    #region Initiate
    private void Awake()
    {
        OnAwake();
    }
    
    protected override void OnAwake()
    {
        base.OnAwake();
        if (mapManager == null)
        {
            mapManager = GameManager.instance.Get_MapManager();
        }
    }
    protected override void Init()
    {
        base.Init();
        modifierController.SetMyEntity(this);
    }

    protected override void InitStat()
    {
        base.InitStat();
        Dictionary<StatType,float> stats = myStat.GetBase();
        StatType[] statTypes = Utils.Get_Enums<StatType>();
        Debug.Log("PlayerEntityInitStat");
        foreach(var stat in statTypes)
        {
            if(stat == StatType.MaxHP || stat == StatType.MaxMP || stat == StatType.Str || stat == StatType.Dex || stat == StatType.Int||stat == StatType.MoveSpeed || stat == StatType. AttackSpeed||stat == StatType.MaxExp)
            {
                myStat.AddBaseStat(stat, 10);
            }
            
        }
        myStat.AddBaseStat(StatType.Damage, 1);
        myStat.AddBaseStat(StatType.Accurancy, 1);
        myStat.AddBaseStat(StatType.AttackRange, 1);
        myStat.AddBaseStat(StatType.Vision, 6);
        myStat.AddBaseStat(StatType.Sound, 6);
        myStat.AddBaseStat(StatType.Regeneration, 0.2f);
        myStat.AddBaseStat(StatType.HP, stats[StatType.MaxHP]);
        myStat.AddBaseStat(StatType.MP, stats[StatType.MaxMP]);

        
    }
    #endregion
    public InventoryData GetInventory()
    {
        return inventory;
    }

    public void TryMove(Vector2Int dir)
    {
        
        Vector2Int target = new Vector2Int(((int)this.transform.position.x+dir.x), (int)(this.transform.position.y+dir.y));
        TileType tileType = mapManager.Get_TargetType(target);

        switch (tileType)
        {
            case TileType.Tile:
                Move_To(dir);
                break;
            case TileType.Wall:
                Debug.Log("Wall Direction");
                break;
            case TileType.Door:
                DoorEntity door = mapManager.GetDoorEntity(target);
                if (door.IsOpen())
                {
                    Move_To(dir);
                }
                else
                {
                    InteractDoor(target);
                }
                break;
            case TileType.ShallowWater:
                break;
            case TileType.DeepWater:
                break;
            case TileType.Upstair:
                Move_To(dir);
                break;
            case TileType.DownStair:
                Move_To(dir);
                break;
            case TileType.Player:
                Debug.Log("Map Data Error! Tile Type is Player Entity");
                break;
            case TileType.Monster:
                MonsterEntity monster = mapManager.GetMonsterEntity(target) as MonsterEntity;
                bool isCrit = false;
                if (CombatManager.instance.TryHit(this, monster,out isCrit))
                {
                    int damage = (int)CombatManager.instance.CalculateMeleeAttackDamage(this, monster, isCrit);
                    Debug.Log(damage);
                }
                break;
            case TileType.Item:
                Move_To(dir);
                break;
            default:
                break;
        }
        if (mapManager.CanMove(target))
        {
            Move_To(dir);
        }
        else
        {

        }

    }

    public void AddItem(ItemBase item)
    {
        inventory.AddinInventory(item);
    }

    public void InteractDoor(Vector2Int targetPos)
    {
        DoorEntity door = mapManager.GetDoorEntity(targetPos);
        if(door == null)
        {
            Debug.Log("Door Didn't Contain");
            return;
        }
        door.Interaction();
    }
}
