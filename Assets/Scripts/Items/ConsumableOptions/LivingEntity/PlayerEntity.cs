using UnityEngine;
using System;
using System.Collections.Generic;
using static Defines;
public class PlayerEntity : LivingEntity
{
    InventoryData inventory = new InventoryData();
    MapManager mapManager;
    FogOfWarManager fowManager;

    List<Vector2Int> previousView = new List<Vector2Int>();
    [SerializeField] List<string> items = new List<string>();

    public bool actable { get; private set; } = true;
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
        if (fowManager == null)
        {
            fowManager = FogOfWarManager.Instance;
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
        Dictionary<StatType, float> stats = myStat.GetBase();
        StatType[] statTypes = Utils.Get_Enums<StatType>();
        Debug.Log("PlayerEntityInitStat");
        foreach (var stat in statTypes)
        {
            if (stat == StatType.MaxHP || stat == StatType.MaxMP || stat == StatType.Str || stat == StatType.Dex || stat == StatType.Int||stat == StatType.MoveSpeed || stat == StatType.AttackSpeed||stat == StatType.MaxExp)
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

    public float TryMove(Vector2Int dir)
    {

        Vector2Int target = new Vector2Int(((int)this.transform.position.x+dir.x), (int)(this.transform.position.y+dir.y));
        TileType tileType = mapManager.Get_TargetType(target);
        float actPoint = 0;
        switch (tileType)
        {
            case TileType.Tile:
                Move_To(dir);
                actPoint = GetActPoint(ModifierTriggerType.OnMove);
                break;
            case TileType.Wall:
                Debug.Log("Wall Direction");
                break;
            case TileType.Door:
                DoorEntity door = mapManager.GetDoorEntity(target);
                if (door.IsOpen())
                {
                    Move_To(dir);
                    actPoint = GetActPoint(ModifierTriggerType.OnMove);
                }
                else
                {
                    InteractDoor(target);
                    actPoint = GetActPoint(ModifierTriggerType.OnMove);
                }
                break;
            case TileType.ShallowWater:
                break;
            case TileType.DeepWater:
                break;
            case TileType.Upstair:
                Move_To(dir);
                actPoint = GetActPoint(ModifierTriggerType.OnMove);
                break;
            case TileType.DownStair:
                Move_To(dir);
                actPoint = GetActPoint(ModifierTriggerType.OnMove);
                break;
            case TileType.Player:
                actPoint = GetActPoint(ModifierTriggerType.OnMove);
                break;
            case TileType.Monster:
                MonsterEntity monster = mapManager.GetMonsterEntity(target) as MonsterEntity;
                bool isCrit = false;
                if (CombatManager.instance.TryHit(this, monster, out isCrit))
                {
                    int damage = (int)CombatManager.instance.CalculateMeleeAttackDamage(this, monster, isCrit);
                    Debug.Log(damage);

                    actPoint = GetActPoint(ModifierTriggerType.OnAttack);
                }
                break;
            case TileType.Item:
                Move_To(dir);
                actPoint = GetActPoint(ModifierTriggerType.OnMove);
                break;
            default:
                break;
        }
        /*
        if (mapManager.CanMove(target))
        {
            Move_To(dir);
        }
        else
        {

        }
        */
        return actPoint;

    }

    public void AddItem(ItemBase item)
    {
        bool canGet = false;
        canGet = inventory.TryAddInventory(item);

    }

    public void TryGetItem()
    {
        if (groundItems==null)
        {
            return;
        }
        if (groundItems.Count<1)
        {
            return;
        }
        ItemBase item = groundItems[0].GetItem();
        Debug.Log($"Item Get {item.id}");
        items.Add(item.id);
        bool canGet = inventory.TryAddInventory(item);
        if (canGet)
        {
            groundItems[0].Return();
        }
    }

    public float InteractDoor(Vector2Int targetPos)
    {
        DoorEntity door = mapManager.GetDoorEntity(targetPos);
        if (door == null)
        {
            Debug.Log("Door Didn't Contain");
            return 0;
        }
        door.Interaction();
        return GetActPoint(ModifierTriggerType.OnMove);
    }
    protected override void Move_To(Vector2Int dir)
    {
        base.Move_To(dir);
        Vector2Int playerPos = new Vector2Int((int)transform.position.x+dir.x, (int)transform.position.y+dir.y);
        int vision = (int)GetEntityStat(ModifierTriggerType.OnMove)[StatType.Vision];
        UpdateFoV(playerPos, vision);
        ItemCheck(destination);
    }
    public void UpdateFoV(Vector2Int playerPos, int viewRadius)
    {
        foreach (Vector2Int pos in previousView)
        {
            FogOfWarManager.Instance.SetExplored(pos);

            MonsterEntity entity = mapManager.GetMonsterEntity(pos);
            if (entity != null) entity.Set_Visibility(false);
        }

        List<Vector2Int> currentView = new List<Vector2Int>();

        List<Vector2Int> circlePosList = mapManager.GetCoordinatesInCircle(playerPos, viewRadius);

        foreach (Vector2Int targetPos in circlePosList)
        {
            if (mapManager.CheckLineOfSight(playerPos, targetPos))
            {
                currentView.Add(targetPos);

                FogOfWarManager.Instance.SetVisible(targetPos);

                MonsterEntity entity = mapManager.GetMonsterEntity(targetPos);
                if (entity != null) entity.Set_Visibility(true);
            }
        }

        previousView = currentView;
    }

    public void Set_InventoryUI(UI_Inventory inventoryUI)
    {
        inventory.Set_InventoryUI(inventoryUI);
    }

    public float GetActPoint(ModifierTriggerType trigger)
    {
        float actPoint = 10;
        float normalAction = 10;
        switch (trigger)
        {
            case ModifierTriggerType.OnEquip:
                actPoint = normalAction;
                break;
            case ModifierTriggerType.OnUseItem:
                actPoint = normalAction;
                break;
            case ModifierTriggerType.OnUnequip:
                actPoint = normalAction;
                break;
            case ModifierTriggerType.OnAttack:
                actPoint = GetEntityStat(trigger)[StatType.AttackSpeed];
                break;
            case ModifierTriggerType.OnSpellCast:
                actPoint = GetEntityStat(trigger)[StatType.SpellSpeed];
                break;
            case ModifierTriggerType.OnMove:
                actPoint = GetEntityStat(trigger)[StatType.MoveSpeed];
                break;
            default:
                break;
        }

        return actPoint;
    }
    public void ChangeActable(bool actable)
    {
        this.actable = actable;
    }
    public StairEntity TryGetStair()
    {
        StairEntity stair = MapManager.instance.Try_GetStairEntity(Get_PosKey());

        return stair;
    }
}
