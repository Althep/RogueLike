using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Defines;

public class LivingEntity : MapEntity
{
    #region [1] 기본 정보 및 식별자 (Identity & Component)
    [SerializeField] protected GameObject myObj;
    [SerializeField] protected SPUM_MatchingList spriteMatching;
    public string Objname;
    public string id;
    protected Races race;
    #endregion

    #region [2] 상태 및 스탯 (Stats & Level)
    [SerializeReference] protected EntityStat myStat;
    // [수정] 할당은 선언부나 Awake/Init 중 '한 곳'에서만 확실하게 하도록 선언부 초기화 유지
    protected ModifierController modifierController = new ModifierController();

    protected virtual void InitStat()
    {
        if (myStat == null)
        {
            myStat = new EntityStat();
        }
        myStat.Set_ModifierController(modifierController);
        myStat.OnStatChange += OnStatChange;
        myStat.OnDeadActions += OnDeadFunc;
    }

    public virtual EntityStat GetMyStat() => myStat;
    public EntityStat Get_MyData() => myStat;

    public virtual void SetMyStat(EntityStat stat)
    {
        if (myStat != null)
        {
            myStat.OnStatChange -= OnStatChange;
            myStat.OnDeadActions -= OnDeadFunc;
        }

        myStat = stat;

        if (myStat != null)
        {
            myStat.Set_ModifierController(modifierController);
            myStat.OnStatChange += OnStatChange;
            myStat.OnDeadActions += OnDeadFunc;
        }
    }

    public virtual void AddingStat(StatType type, float value) => myStat?.AddingStat(type, value);
    public Dictionary<StatType, float> CalculateContext(ModifierTriggerType trigger) => myStat.CalculateContext(trigger);

    public void OnStatChange(StatType stat, float value)
    {
        if (myStat == null) return;

        myStat.MarksAsDirty();
        Dictionary<StatType, float> finalStat = myStat.CalculateContext(ModifierTriggerType.Passive);

        if (stat == StatType.HP || stat == StatType.MaxHP)
        {
            myUIController?.UpdateUIs((int)finalStat[StatType.HP], (int)finalStat[StatType.MaxHP]);
        }
    }
    #endregion

    #region [3] 모디파이어 및 상태 이상 (Modifiers & Buffs)
    public virtual void Add_Equip(Modifier modifier) => modifierController.AddEquipment(modifier);

    public virtual void Add_Buff(Modifier modifier)
    {
        if (modifier is BuffModifier buff)
        {
            int duration = UnityEngine.Random.Range(buff.minTime, buff.maxTime + 1);
            EventManager.instance.AddBuff(this, buff, duration);
        }
        modifierController.AddBuff(modifier);
    }

    public virtual void Add_Mutation(Modifier modifier) => modifierController.AddMutation(modifier);
    public ModifierContext GetContext(ModifierTriggerType trigger) => modifierController.GetUpdatedContext(trigger);
    public void Remove_BuffModifier(Modifier modifier) => modifierController.RemoveBuff(modifier);
    public void Remove_Mutate(Modifier modifier) => modifierController.RemoveMutate(modifier);
    public void Remove_Equip(Modifier modifier) => modifierController.RemoveEquipment(modifier);
    public Dictionary<ModifierTriggerType, List<Modifier>> GetBuffs() => modifierController.Get_Buffs();

    public void OnModifierChange(Modifier modifier)
    {
        if (myStat == null) return;

        myStat.MarksAsDirty();
        Dictionary<StatType, float> finalStat = myStat.CalculateContext(modifier.triggerType);
        StatType stat = modifier.stat;

        if (stat == StatType.HP || stat == StatType.MaxHP)
        {
            myUIController?.UpdateUIs((int)finalStat[StatType.HP], (int)finalStat[StatType.MaxHP]);
        }
    }
    #endregion

    #region [4] 장비 및 인벤토리 (Inventory & Items)
    protected EquipSystem equipSystem = new EquipSystem();
    protected InventoryData inventoryData;
    protected List<ItemEntity> groundItems;

    public void Add_Item(ItemBase item) { }
    public StatType Get_WeaponAttribueType() => equipSystem.Get_WeaponAttribute();

    public void UseItem(ItemBase item)
    {
        if (item is EquipItem equip)
        {
            equipSystem.ItemEquip(equip);
        }
        else if (item is ConsumableItem consum)
        {
            consum.OnUse(this);
        }
    }

    public bool IsRistricted(ItemBase item, ModifierTriggerType trigger) => modifierController.IsRestricted(item, trigger);

    public void ItemCheck(Vector2Int pos)
    {
        groundItems = null;
        if (ItemManager.instance.GroundCheck(pos))
        {
            groundItems = ItemManager.instance.Get_GroundItems(pos);
        }
    }
    #endregion

    #region [5] 이동 및 길찾기 (Movement & Navigation)
    public Vector2Int CurrentTilePos => new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y));

    protected Astar pathFinder;
    protected Vector2Int destination;
    protected Defines.MoveState moveState;
    public float animSpeed = 10f;

    public void SetDestination(Vector2Int target) => destination = target;

    protected virtual void Move_To(Vector2Int dir)
    {
        destination = CurrentTilePos + dir;
        Start_Move();
    }

    public void Start_Move()
    {
        if (CurrentTilePos != destination && !MapManager.instance.CanMove(destination))
        {
            Debug.Log($"이동 불가 타일: {destination}");
            moveState = Defines.MoveState.Idle;
            return;
        }

        GameManager.instance.EntityMove(this, CurrentTilePos, destination);
        moveState = Defines.MoveState.Move;
        StartCoroutine(Moving());
    }

    public IEnumerator Moving()
    {
        Vector3 destVector3 = new Vector3(destination.x, destination.y, transform.position.z);

        while (transform.position != destVector3)
        {
            transform.position = Vector3.MoveTowards(transform.position, destVector3, animSpeed * Time.deltaTime);
            yield return null;
        }

        End_Move();
    }

    public void End_Move() => moveState = Defines.MoveState.Idle;
    #endregion

    #region [6] 전투 및 판정 (Combat & Battle)
    protected LivingEntity lastAttacker;

    public virtual void GetDamage(LivingEntity lastAttacker, int damage)
    {
        this.lastAttacker = lastAttacker;
        AddingStat(StatType.HP, -damage);
        modifierController.GetUpdatedContext(ModifierTriggerType.OnHited);

    }

    public void ExcuteModifierActions(ModifierTriggerType trigger)
    {
        ModifierContext context = modifierController.GetUpdatedContext(trigger);
        context.ExcuteModifierActions();
    }

    public virtual void OnDeadFunc()
    {
        modifierController.OnDead();
        if (myStat != null)
        {
            myStat.OnStatChange -= OnStatChange;
            myStat.OnDeadActions -= OnDeadFunc;
        }
        EventManager.instance.RemoveEntity(this);

        if (lastAttacker is PlayerEntity player)
        {
            int exp = (int)CalculateContext(ModifierTriggerType.Passive)[StatType.Exp];
            Debug.Log($"플레이어에게 {exp} 경험치 전달");
            player.Add_Exp(exp);
        }
        Return();
    }


    public virtual void ExecuteAction(LivingEntity target, ModifierTriggerType trigger)
    {

    }
    #endregion

    #region [7] 시각 요소 및 렌더링 (Visuals & Rendering)
    void SetSprite() { }

    public virtual void Set_Visibility(bool isVisible)
    {
        bool isChange = false;
        // equipRenders 바인딩 체크 필요 (현재 선언부 누락 대비 스크립트 에러 방지용 주석)
        // 자식 클래스나 데이터 확인 후 활성화 필요
    }

    public virtual void Set_UIVisible(bool isVisible)
    {
        myUIs?.SetActive(isVisible);
    }
    #endregion

    #region [8] 유니티 라이프사이클 및 오버라이드 (Lifecycle & MapEntity Override)
    private void Awake()
    {
        if (inventoryData == null)
        {
            inventoryData = new InventoryData();
        }
        OnAwake();
    }

    protected virtual void OnAwake()
    {
        if (spriteMatching == null)
        {
            spriteMatching = transform.GetComponentInChildren<SPUM_MatchingList>();
        }
        Init();
    }

    protected override void Init()
    {
        base.Init();
        myObj = this.gameObject;
        InitStat();

        if (pathFinder == null)
        {
            pathFinder = new Astar(this.gameObject);
        }

        // [수정] 중복 new() 방지 및 이벤트 리스너 중복 방지 타이트하게 제어
        modifierController.OnModifierChanged -= OnModifierChange;
        modifierController.OnModifierChanged += OnModifierChange;

        equipSystem.InitSystem(modifierController);
    }

    public void Set_RaceData(RaceData race)
    {
        List<Modifier> modifiers = race.modifiers;
        foreach (Modifier modi in modifiers)
        {
            modifierController.AddMutation(modi);
        }
        this.race = race.race;
        SetSprite();
    }

    public override void ResetData()
    {
        // 1. 이벤트 확실하게 해제해서 오브젝트 풀링 시 메모리 누수 차단
        if (modifierController != null)
        {
            modifierController.OnModifierChanged -= OnModifierChange;
        }

        if (myStat != null)
        {
            myStat.OnStatChange -= OnStatChange;
            myStat.OnDeadActions -= OnDeadFunc;
        }

        // 2. 기존 데이터 비우기
        myStat = null;
        Objname = null;
        id = null;
        equipSystem?.ClearEquips();
    }

    public override void Return()
    {
        base.Return();
        Vector2Int posKey = Get_PosKey();
        MapManager.instance.dynamicMapData.Remove(posKey);
    }
    #endregion
}