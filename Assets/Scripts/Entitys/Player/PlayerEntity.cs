using UnityEngine;
using System;
using System.Collections.Generic;
using static Defines;

public class PlayerEntity : LivingEntity
{
    #region [1] 기본 정보 및 식별자 (Identity & Component)
    [Header("Player Systems")]
    private MapManager mapManager;
    private FogOfWarManager fowManager;
    private LevelSystem levelSystem;

    [Header("State Flags")]
    public bool actable { get; private set; } = true;
    #endregion

    #region [2] 상태 및 스탯 (Stats & Level)
    public event Action<StatType> OnStatChangedUI;
    public event Action OnLevelUp;
    public event Action OnAddExp;

    protected override void InitStat()
    {
        base.InitStat();
        Dictionary<StatType, float> stats = myStat.GetBase();
        StatType[] statTypes = Utils.Get_Enums<StatType>();

        Debug.Log("PlayerEntityInitStat");
        foreach (var stat in statTypes)
        {
            if (stat == StatType.MaxHP || stat == StatType.MaxMP || stat == StatType.Str ||
                stat == StatType.Dex || stat == StatType.Int || stat == StatType.MoveSpeed ||
                stat == StatType.AttackSpeed || stat == StatType.MaxExp)
            {
                myStat.AddingStat(stat, 10);
                OnStatChangedUI?.Invoke(stat);
            }
        }

        myStat.SetBaseStat(StatType.Damage, 1);
        myStat.SetBaseStat(StatType.Accurancy, 1);
        myStat.SetBaseStat(StatType.AttackRange, 1);
        myStat.SetBaseStat(StatType.Vision, 6);
        myStat.SetBaseStat(StatType.Sound, 6);
        myStat.SetBaseStat(StatType.Regeneration, 0.2f);
        myStat.SetBaseStat(StatType.HP, stats[StatType.MaxHP]);
        myStat.SetBaseStat(StatType.MP, stats[StatType.MaxMP]);

        OnStatChangedUI?.Invoke(StatType.HP);
        OnStatChangedUI?.Invoke(StatType.MP);
    }

    public override void AddingStat(StatType type, float value)
    {
        base.AddingStat(type, value);
        OnStatChangedUI?.Invoke(type);
    }
    #endregion

    #region [3] 모디파이어 및 상태 이상 (Modifiers & Buffs)
    public override void Add_Equip(Modifier modifier)
    {
        base.Add_Equip(modifier);
        if (modifier.triggerType == ModifierTriggerType.Passive)
        {
            // [최적화] 불필요한 CalculateContext 연산 캐싱 제거
            OnStatChangedUI?.Invoke(modifier.stat);
        }
    }

    public override void Add_Buff(Modifier modifier)
    {
        base.Add_Buff(modifier);
        if (modifier.triggerType == ModifierTriggerType.Passive)
        {
            OnStatChangedUI?.Invoke(modifier.stat);
        }
    }

    public override void Add_Mutation(Modifier modifier)
    {
        base.Add_Mutation(modifier);
        if (modifier.triggerType == ModifierTriggerType.Passive)
        {
            OnStatChangedUI?.Invoke(modifier.stat);
        }
    }
    #endregion

    #region [4] 장비 및 인벤토리 (Inventory & Items)
    [Header("Inventory & Drops")]
    private InventoryData inventory = new InventoryData();
    [SerializeField] private List<string> items = new List<string>();

    public InventoryData GetInventory() => inventory;

    public void AddItem(ItemBase item)
    {
        inventory.TryAddInventory(item);
    }

    public void TryGetItem()
    {
        if (groundItems == null || groundItems.Count < 1) return;

        ItemBase item = groundItems[0].GetItem();
        Debug.Log($"Item Get {item.id}");
        items.Add(item.id);

        bool canGet = inventory.TryAddInventory(item);
        if (canGet)
        {
            groundItems[0].Return();
        }
    }

    public void Set_InventoryUI(UI_Inventory inventoryUI)
    {
        inventory.Set_InventoryUI(inventoryUI);
    }
    #endregion

    #region [5] 이동 및 길찾기 (Movement & Navigation)
    public float TryMove(Vector2Int dir)
    {
        Vector2Int target = new Vector2Int((int)transform.position.x + dir.x, (int)transform.position.y + dir.y);
        TileType tileType = mapManager.Get_TargetType(target);

        float actPoint = 0;

        // [리팩토링] 복잡했던 스위치문을 깔끔하게 통합하고 분리
        switch (tileType)
        {
            case TileType.Tile:
            case TileType.Upstair:
            case TileType.DownStair:
            case TileType.Item:
                Move_To(dir);
                actPoint = GetActPoint(ModifierTriggerType.OnMove);
                break;

            case TileType.Wall:
                Debug.Log("Wall Direction");
                break;

            case TileType.Door:
                actPoint = HandleDoorInteraction(target, dir);
                break;

            case TileType.Monster:
                actPoint = HandleAttackInteraction(target);
                break;

            case TileType.Player:
                actPoint = GetActPoint(ModifierTriggerType.OnMove);
                break;

            case TileType.ShallowWater:
            case TileType.DeepWater:
            default:
                break;
        }
        return actPoint;
    }

    protected override void Move_To(Vector2Int dir)
    {
        base.Move_To(dir);
        Vector2Int playerPos = new Vector2Int((int)transform.position.x + dir.x, (int)transform.position.y + dir.y);
        int vision = (int)CalculateContext(ModifierTriggerType.OnMove)[StatType.Vision];

        UpdateFoV(playerPos, vision);
        ItemCheck(destination);
    }

    private float HandleDoorInteraction(Vector2Int targetPos, Vector2Int dir)
    {
        DoorEntity door = mapManager.GetDoorEntity(targetPos);
        if (door.IsOpen())
        {
            Move_To(dir);
        }
        else
        {
            InteractDoor(targetPos);
        }
        return GetActPoint(ModifierTriggerType.OnMove);
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
    #endregion

    #region [6] 전투 및 판정 (Combat & Battle)
    private float HandleAttackInteraction(Vector2Int targetPos)
    {
        MonsterEntity monster = mapManager.GetMonsterEntity(targetPos) as MonsterEntity;
        if (monster == null) return 0;

        bool isCrit = false;
        if (CombatManager.instance.TryHit(this, monster, out isCrit))
        {
            // [대통합 연동] 데미지가 들어가기 전에 장비/패시브의 추가타 및 버프를 터트립니다!
            base.ExecuteAction(monster, ModifierTriggerType.OnAttack);

            int damage = (int)CombatManager.instance.Combat(this, monster, out isCrit);
            Debug.Log(damage);

            return GetActPoint(ModifierTriggerType.OnAttack);
        }
        return 0; // 빗나갔을 때의 AP 소모 기획에 따라 수정 가능
    }

    // 플레이어 전용 타격 연출(카메라 쉐이크 등)이 필요할 경우 여기서 오버라이드
    public override void ExecuteAction(LivingEntity target, ModifierTriggerType trigger)
    {
        base.ExecuteAction(target, trigger);
    }
    #endregion

    #region [6-1] AP 및 턴 제어 (Turn & AP System)
    public float GetActPoint(ModifierTriggerType trigger)
    {
        float actPoint = 10;
        float normalAction = 10;

        switch (trigger)
        {
            case ModifierTriggerType.OnEquip:
            case ModifierTriggerType.OnUseItem:
            case ModifierTriggerType.OnUnequip:
                actPoint = normalAction;
                break;
            case ModifierTriggerType.OnAttack:
                actPoint = CalculateContext(trigger)[StatType.AttackSpeed];
                break;
            case ModifierTriggerType.OnSpellCast:
                actPoint = CalculateContext(trigger)[StatType.SpellSpeed];
                break;
            case ModifierTriggerType.OnMove:
                actPoint = CalculateContext(trigger)[StatType.MoveSpeed];
                break;
        }
        return actPoint;
    }

    public void ChangeActable(bool actable) => this.actable = actable;
    #endregion

    #region [6-2] 레벨 및 경험치 서브시스템 (Level & Exp)
    public void Add_Exp(int exp)
    {
        levelSystem.AddExp(exp);
        OnAddExp?.Invoke();
    }

    private void HandleLevelUp()
    {
        Dictionary<StatType, float> stats = myStat.GetBase();
        myStat.SetBaseStat(StatType.HP, stats[StatType.MaxHP]);

        OnLevelUp?.Invoke();
        OnStatChangedUI?.Invoke(StatType.HP);
        OnStatChangedUI?.Invoke(StatType.MP);
    }

    public int Get_Level() => levelSystem.Get_Level();
    public int Get_MaxExp() => levelSystem.Get_MaxExp();
    public int Get_CurrentExp() => levelSystem.Get_CurrentExp();
    #endregion

    #region [6-3] 던전 기믹 및 오브젝트 (Dungeon Gimmicks)
    private int stairNumber = 0;
    private TileType stairType = TileType.DownStair;

    public StairEntity TryGetStair() => MapManager.instance.Try_GetStairEntity(Get_PosKey());

    public void IneractStair(TileType type, int stairNumber)
    {
        this.stairNumber = stairNumber;
        stairType = type;
    }

    public int Get_StiarNumber() => stairNumber;
    public TileType StairType() => stairType;
    #endregion

    #region [7] 시각 요소 및 시야 처리 (Visuals & FoV)
    [Header("FoV Cache")]
    private List<Vector2Int> previousView = new List<Vector2Int>();

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
    #endregion

    #region [8] 유니티 라이프사이클 및 초기화 (Lifecycle & Init)
    private void Awake()
    {
        OnAwake();
    }

    protected override void OnAwake()
    {
        base.OnAwake();
    }

    protected override void Init()
    {
        base.Init();

        if (mapManager == null) mapManager = GameManager.instance.Get_MapManager();
        if (fowManager == null) fowManager = FogOfWarManager.Instance;

        if (levelSystem == null)
        {
            levelSystem = new LevelSystem();
        }

        modifierController.SetMyEntity(this);

        levelSystem.OnLevelUp -= HandleLevelUp;
        levelSystem.OnLevelUp += HandleLevelUp;
    }
    #endregion
}