using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Defines;
using static UnityEditor.PlayerSettings;
using static UnityEngine.EventSystems.EventTrigger;

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
    [SerializeField] protected ModifierController modifierController = new ModifierController();


    protected virtual void InitStat()
    {
        if (myStat == null)
        {
            myStat = new EntityStat();
        }
        myStat.Set_ModifierController(modifierController);
        myStat.OnStatChange+=OnStatChange;
        myStat.OnDeadActions+=OnDeadFunc;
    }

    public virtual EntityStat GetMyStat()
    {
        return myStat;
    }

    public virtual void SetMyStat(EntityStat stat)
    {
        // 1. 기존 스탯이 있었다면 찌꺼기가 남지 않게 먼저 연결을 끊어줍니다.
        if (myStat != null)
        {
            myStat.OnStatChange -= OnStatChange;
            myStat.OnDeadActions -= OnDeadFunc;
        }

        // 2. 새로운 스탯으로 덮어씌웁니다.
        myStat = stat;
        myStat.Set_ModifierController(modifierController);

        // 3. 새 스탯의 이벤트 방송국에 주파수를 다시 맞춰줍니다.
        if (myStat != null)
        {
            myStat.OnStatChange += OnStatChange;
            myStat.OnDeadActions += OnDeadFunc;
        }
    }
    public EntityStat Get_MyData()
    {
        return myStat;
    }

    public virtual void AddingStat(StatType type, float value)
    {
        myStat.AddBaseStat(type, value);
    }

    public Dictionary<StatType, float> CalculateContext(ModifierTriggerType trigger)
    {
        return myStat.CalculateContext(trigger);
    }
    public void OnStatChange(StatType stat,float value)
    {
        if (myStat == null)
            return;
        myStat.MarksAsDirty();
        Dictionary<StatType, float> finalStat = myStat.CalculateContext(ModifierTriggerType.Passive);
        if (stat == StatType.HP || stat == StatType.MaxHP)
        {
            myUIController.UpdateUIs((int)finalStat[StatType.HP], (int)finalStat[StatType.MaxHP]);
        }
    }
    #endregion

    #region [3] 모디파이어 및 상태 이상 (Modifiers & Buffs)
    public virtual void Add_Equip(Modifier modifier)
    {
        modifierController.AddEquipment(modifier);
    }

    public virtual void Add_Buff(Modifier modifier)
    {
        if (modifier is BuffModifier buff)
        {
            int minTime = buff.minTime;
            int maxTime = buff.maxTime;
            int duration = UnityEngine.Random.Range(minTime, maxTime+1);
            EventManager.instance.AddBuff(this, buff, duration);
        }
        modifierController.AddBuff(modifier);

    }

    public virtual void Add_Mutation(Modifier modifier)
    {
        modifierController.AddMutation(modifier);
    }

    public ModifierContext GetContext(ModifierTriggerType trigger)
    {
        return modifierController.Get_Context(trigger);
    }

    public void Remove_BuffModifier(Modifier modifier)
    {
        modifierController.RemoveBuff(modifier);
    }

    public void Remove_Mutate(Modifier modifier)
    {
        modifierController.RemoveMutate(modifier);
    }

    public void Remove_Equip(Modifier modifier)
    {
        modifierController.RemoveEquipment(modifier);
    }

    public Dictionary<ModifierTriggerType, List<Modifier>> GetBuffs()
    {
        return modifierController.Get_Buffs();
    }
    public void OnModifierChange(Modifier modifier)
    {
        if (myStat == null) return;

        myStat.MarksAsDirty();
        Dictionary<StatType, float> finalStat = myStat.CalculateContext(modifier.triggerType);
        StatType stat = modifier.stat;
        if (stat == StatType.HP || stat == StatType.MaxHP)
        {
            myUIController.UpdateUIs((int)finalStat[StatType.HP], (int)finalStat[StatType.MaxHP]);
        }
    }
    #endregion

    #region [4] 장비 및 인벤토리 (Inventory & Items)
    protected Dictionary<SlotType, EquipItem> equipments = new Dictionary<SlotType, EquipItem>();
    protected InventoryData inventoryData;
    protected List<ItemEntity> groundItems;

    public void Add_Item(ItemBase item)
    {

    }

    public Dictionary<SlotType, EquipItem> GetEquips()
    {
        return equipments;
    }

    public StatType Get_WeaponAttribueType()
    {
        if (!equipments.ContainsKey(SlotType.MainHand))
        {
            return StatType.Str;
        }
        if (equipments[SlotType.MainHand] != null)
        {
            if (equipments[SlotType.MainHand] is Weapon weapon)
            {
                return weapon.attributeStat;
            }
            return StatType.Str;
        }
        else
        {
            return StatType.Str;
        }
    }

    public void UseItem(ItemBase item)
    {
        if (item is EquipItem equip)
        {
            ItemEquip(equip);
        }
    }

    public bool IsRistricted(ItemBase item, ModifierTriggerType trigger)
    {
        return modifierController.IsRestricted(item, trigger);
    }

    public void ItemEquip(EquipItem target)
    {
        SlotType slot = target.slot;

        if (equipments[slot] != null)
        {
            EquipItem origin = equipments[slot];

            List<Modifier> modis = origin.options;
            List<Modifier> addOptions = origin.addOptions;
            foreach (Modifier modi in modis)
            {
                modifierController.RemoveEquipment(modi);
            }
            foreach (Modifier modi in addOptions)
            {
                modifierController.RemoveEquipment(modi);
            }
        }

        equipments[slot] = target;

        foreach (Modifier option in target.options)
        {
            modifierController.AddEquipment(option);
        }
        foreach (Modifier option in target.addOptions)
        {
            modifierController.AddEquipment(option);
        }
    }

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

    // [추가] 매번 RoundToInt를 길게 쓰지 않도록 현재 타일 좌표를 바로 반환하는 프로퍼티를 하나 만듭니다.
    public Vector2Int CurrentTilePos => new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y));

    protected Astar pathFinder;
    protected Vector2Int destination;
    Defines.MoveState moveState;
    public float animSpeed = 10f;

    public void SetDestination(Vector2Int target)
    {
        destination = target;
    }

    protected virtual void Move_To(Vector2Int dir)
    {
        destination = CurrentTilePos + dir;
        Start_Move();
    }

    public void Start_Move()
    {
        // 1. 길막 검사 (목적지가 현재 위치와 다르고, 이동 불가능한 타일이라면 취소)
        if (CurrentTilePos != destination && !MapManager.instance.CanMove(destination))
        {
            Debug.Log($"이동 불가 타일: {destination}");
            moveState = Defines.MoveState.Idle;
            return;
        }

        // 2. 이동 승인 및 로직 시작
        GameManager.instance.EntityMove(this, CurrentTilePos, destination);
        moveState = Defines.MoveState.Move;

        // 문자열이 아닌 메서드 직접 호출로 변경
        StartCoroutine(Moving());
    }

    public IEnumerator Moving()
    {
        // 1. Z축 값을 기존 객체의 값으로 유지하여 하드코딩 방지
        Vector3 destVector3 = new Vector3(destination.x, destination.y, transform.position.z);

        // 2. Vector3.MoveTowards를 사용하면 거리를 직접 잴 필요 없이 정확히 목표 지점에서 멈춥니다.
        while (transform.position != destVector3)
        {
            transform.position = Vector3.MoveTowards(transform.position, destVector3, animSpeed * Time.deltaTime);
            yield return null;
        }

        // 3. 이동 완료 처리
        End_Move();
    }

    public void End_Move()
    {
        moveState = Defines.MoveState.Idle;
        // MoveTowards가 정확히 도달시켜 주므로, 여기서 강제로 position을 다시 맞출 필요가 사라졌습니다.
    }

    // 기존의 복잡했던 float SetMoveDistance(Vector2 moveDirection) 함수는 
    // MoveTowards의 도입으로 아예 필요가 없어져 삭제했습니다!
    #endregion

    #region [6] 전투 및 판정 (Combat & Battle)
    protected LivingEntity lastAttacker;

    public virtual void GetDamage(LivingEntity lastAttacker, int damage)
    {
        this.lastAttacker = lastAttacker;
        AddingStat(StatType.HP, -damage);
        if (isDead)
        {
            EntityDead();
        }
        int currentHp = Mathf.RoundToInt(myStat.GetBase()[StatType.HP]);
        int maxHp = Mathf.RoundToInt(myStat.GetBase()[StatType.MaxHP]);

        modifierController.ApplyModifiers(ModifierTriggerType.OnHited);
    }

    public virtual void EntityDead()
    {
        Vector2 mypos = transform.position;
        Vector2Int posKey = new Vector2Int(Mathf.RoundToInt(mypos.x), Mathf.RoundToInt(mypos.y));
        if (lastAttacker == null)
        {
            Debug.Log("lastAttacker is null");
            MapManager.instance.RemoveMapEntity(posKey, this);
            EventManager.instance.RemoveEntity(this);
            return;
        }


        if (lastAttacker is PlayerEntity player)
        {
            float exp = CalculateContext(ModifierTriggerType.Passive)[StatType.Exp];

            lastAttacker.AddingStat(StatType.Exp, exp);

            Debug.Log($"Get exp : {exp}, attackerExp {lastAttacker.CalculateContext(ModifierTriggerType.Passive)[StatType.Exp]}");
        }
        MapManager.instance.RemoveMapEntity(posKey, this);
    }

    public Dictionary<StatType, float> GetAttackBonus()
    {
        return CalculateContext(ModifierTriggerType.OnAttack);
    }

    public ModifierContext GetDefenseBonus()
    {
        return modifierController.ApplyModifiers(ModifierTriggerType.OnHited);
    }

    public ModifierContext Get_MeleeAttackContext()
    {
        return modifierController.ApplyModifiers(ModifierTriggerType.OnAttack);
    }

    public int GetResistDamage(DamageType type, int damage)
    {
        return myStat.GetDamageToResist(type, damage);
    }

    public bool IsEvasion(LivingEntity target)
    {
        bool isEvasion = false;
        return isEvasion;
    }
    public virtual void OnDeadFunc()
    {
        modifierController.OnDead();
        myStat.OnStatChange-=OnStatChange;
        myStat.OnDeadActions-=OnDeadFunc;
        Return();
    }
    #endregion

    #region [7] 시각 요소 및 렌더링 (Visuals & Rendering)
    void SetSprite()
    {

    }

    public virtual void Set_Visibility(bool isVisible)
    {
        bool isChange = false;

        foreach (SpriteRenderer sr in equipRenders)
        {
            if (sr.sprite == null)
                continue;

            if (sr.enabled != isVisible)
            {
                sr.enabled = isVisible;
                isChange = true;
                Debug.Log($"Set Visible {isVisible}");
            }
        }
        if (isChange)
        {
            Set_UIVisible(isVisible);
        }

    }

    public virtual void Set_UIVisible(bool isVisible)
    {
        if (myUIs!=null)
        {
            myUIs.SetActive(isVisible);
        }
    }
    #endregion

    #region [8] 유니티 라이프사이클 및 오버라이드 (Lifecycle & MapEntity Override)
    private void Awake()
    {
        if (myUIController!= null)
        {

        }
        if (inventoryData == null)
        {
            inventoryData = new InventoryData();
        }
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
        //modifierController.InitContext(this);
        myObj = this.gameObject;
        InitStat();
        if (pathFinder == null)
        {
            pathFinder = new Astar(this.gameObject);
        }
        if(modifierController == null)
        {
            modifierController = new ModifierController();
        }
        modifierController.OnModifierChanged += OnModifierChange;
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
        // 1. 이벤트 초기화 (중복 실행 방지!)
        if (modifierController != null)
        {
            modifierController.OnModifierChanged -= OnModifierChange;
        }

        // 2. 기존 데이터 비우기
        myStat = null;
        Objname = null;
        id = null;
        equipments.Clear();
    }

    public override void Return()
    {
        base.Return();
        Vector2 myPos = transform.position;
        Vector2Int posKey = Get_PosKey();
        MapManager.instance.dynamicMapData.Remove(posKey);

    }


    #endregion
}