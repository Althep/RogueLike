using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Defines;
using static UnityEditor.PlayerSettings;
using static UnityEngine.EventSystems.EventTrigger;

public class LivingEntity : MapEntity
{
    [SerializeField] protected GameObject myObj;
    [SerializeReference] protected EntityStat myStat;
    public string Objname;
    public string id;
    //private Dictionary<ModifierTriggerType, ModifierContext> modifierContext = new Dictionary<ModifierTriggerType, ModifierContext>();
    protected Dictionary<SlotType, EquipItem> equipments = new Dictionary<SlotType, EquipItem>();
    [SerializeField] protected SPUM_MatchingList spriteMatching;
    protected Astar pathFinder;

    protected Vector2Int destination;

    Defines.MoveState moveState;

    public float animSpeed = 10f;

    [SerializeField] protected ModifierController modifierController = new ModifierController();

    //ModifierContext context;

    protected List<ItemEntity> groundItems;
    protected Races race;
    protected LivingEntity lastAttacker;
    protected InventoryData inventoryData;  
    private void Awake()
    {
        if(myUIController!= null)
        {
            
        }
        if(inventoryData == null)
        {
            inventoryData = new InventoryData();
        }
    }

    private void Update()
    {
    }
    #region InitiateEntity
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
    }

    protected virtual void InitStat()
    {
        if (myStat == null)
        {
            myStat = new EntityStat();
        }
    }
    #endregion
    public virtual EntityStat GetMyStat()
    {
        return myStat;
    }
    public virtual void SetMyStat(EntityStat stat)
    {
        myStat = stat;
        //Dictionary<StatType, float> myBase = myStat.GetBase();
        //myUIController.InitUIs(Mathf.RoundToInt(myBase[StatType.HP]), Mathf.RoundToInt(myBase[StatType.MaxHP]), id);
    }
    #region SetData
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
    #endregion
    #region modifiers
    public void Add_Equip(Modifier modifier)
    {
        modifierController.AddEquipment(modifier);
    }
    public void Add_Buff(Modifier modifier)
    {
        if(modifier is BuffModifier buff)
        {
            int minTime = buff.minTime;
            int maxTime = buff.maxTime;
            int duration = UnityEngine.Random.Range(minTime, maxTime+1);
            EventManager.instance.AddBuff(this, buff, duration);
        }
        modifierController.AddBuff(modifier);
        
    }
    public void Add_Mutation(Modifier modifier)
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
    #endregion
    #region Stats
    public EntityStat Get_MyData()
    {
        return myStat;
    }
    /*
    public void Set_MyStat(EntityStat stat)
    {
        myStat = stat;
    }
    */
    public virtual void AddingStat(StatType type, float value)
    {
        myStat.AddBaseStat(type, value);
        if(type == StatType.HP)
        {
            isDead = myStat.IsDead();
        }

        Dictionary<StatType, float> baseStat = myStat.GetBase();
        switch (type)
        {
            case StatType.HP:
                if (baseStat[type]>baseStat[StatType.MaxHP])
                {
                    myStat.SetBaseStat(type, baseStat[StatType.MaxHP]);
                }
                else if (baseStat[type]<=0)
                {
                    isDead = true;
                }
                
                break;
            case StatType.MaxHP:
                break;
            case StatType.MP:
                if (baseStat[type] < baseStat[StatType.MaxMP])
                {
                    myStat.SetBaseStat(type, baseStat[StatType.MaxMP]);
                }
                if (baseStat[type]<=0)
                {
                    baseStat[type] = 0;
                }
                break;
            case StatType.MaxMP:
                break;
            case StatType.Str:
                break;
            case StatType.Dex:
                break;
            case StatType.Int:
                break;
            case StatType.Evasion:
                break;
            case StatType.Defense:
                break;
            case StatType.DamageReduce:
                break;
            case StatType.ShieldDefense:
                break;
            case StatType.Damage:
                break;
            case StatType.Accurancy:
                break;
            case StatType.AttackRange:
                break;
            case StatType.SpellDamage:
                break;
            case StatType.SpellAccurancy:
                break;
            case StatType.SpellSpeed:
                break;
            case StatType.Disruption:
                break;
            case StatType.FireResist:
                break;
            case StatType.IceResist:
                break;
            case StatType.MagicResist:
                break;
            case StatType.ThunderResist:
                break;
            case StatType.Vision:
                break;
            case StatType.Sound:
                break;
            case StatType.MoveSpeed:
                break;
            case StatType.AttackSpeed:
                break;
            case StatType.MaxExp:
                break;
            case StatType.Exp:
                if (baseStat[StatType.MaxExp]<1)
                {
                    return;
                }
                if (baseStat[StatType.Exp]>=baseStat[StatType.MaxExp])
                {
                    
                }
                break;
            case StatType.Regeneration:
                break;
            case StatType.Tir:
                break;
            case StatType.ExtraLife:
                break;
            case StatType.AwakeRate:
                break;
            default:
                break;
        }
    }
    public virtual void LevelUp()
    {
        modifierController.ApplyModifiers(ModifierTriggerType.OnLevelUp);
        Dictionary<StatType, float> baseStat = myStat.GetBase();
        if(baseStat[StatType.Exp]>=baseStat[StatType.MaxExp])
        {
            myStat.Add_Level();
            baseStat[StatType.Exp]-=baseStat[StatType.MaxExp];
        }
        Set_MaxExp();
    }
    public virtual void Set_MaxExp()
    {

    }
    public virtual void EntityDead()
    {
        Vector2 mypos = transform.position;
        Vector2Int posKey = new Vector2Int(Mathf.RoundToInt(mypos.x), Mathf.RoundToInt(mypos.y));
        if (lastAttacker == null)
        {
            Debug.Log("lastAttacker is null");
            MapManager.instance.RemoveMapEntity(posKey,this);
            EventManager.instance.RemoveEntity(this);
            return;
        }
            

        if(lastAttacker is PlayerEntity player)
        {
            float exp = GetEntityStat(ModifierTriggerType.Passive)[StatType.Exp];
            
            lastAttacker.AddingStat(StatType.Exp, exp);

            Debug.Log($"Get exp : {exp}, attackerExp {lastAttacker.GetEntityStat(ModifierTriggerType.Passive)[StatType.Exp]}");
        }
        MapManager.instance.RemoveMapEntity(posKey, this);
    }

    
    public Dictionary<StatType, float> GetEntityStat(ModifierTriggerType trigger)
    {
        Dictionary<StatType, float> baseStat = myStat.GetBase();
        Dictionary<StatType, float> finalStat = myStat.GetFinal();
        finalStat.Clear();
        ModifierContext context = modifierController.ApplyModifiers(trigger);
        ModifierContext passive = modifierController.ApplyModifiers(ModifierTriggerType.Passive);

        if (trigger == ModifierTriggerType.Passive)
        {
            foreach (var key in baseStat.Keys)
            {
                finalStat.Add(key, 0);
                float finalValue = (baseStat[key] + passive.stats[key])
                * (1 + passive.multifle[key]);

                if (!finalStat.ContainsKey(key))
                {
                    finalStat.Add(key, 0);
                }
                finalStat[key] = finalValue;
            }
            return finalStat;
        }
        foreach (var key in baseStat.Keys)
        {
            finalStat.Add(key, 0);
            if (!baseStat.ContainsKey(key))
            {
                Debug.Log($"Base Stat Din't Contain Key {key}");
                return null;
            }
            if (!context.stats.ContainsKey(key))
            {
                Debug.Log($"context Multi Din't Contain Key {key}");
                return null;
            }
            if (!passive.stats.ContainsKey(key))
            {
                Debug.Log($"passive Stat Din't Contain Key {key}");
                return null;
            }
            if (!passive.multifle.ContainsKey(key))
            {
                Debug.Log($"passive multifle Din't Contain Key {key}");
                return null;
            }

            if (!context.stats.ContainsKey(key))
            {
                Debug.Log($"context Stat Din't Contain Key {key}");
                return null;
            }
            float finalValue = (baseStat[key] + context.stats[key] + passive.stats[key])
                * (1 + context.multifle[key] + passive.multifle[key]);

            if (!finalStat.ContainsKey(key))
            {
                finalStat.Add(key, 0);
            }
            finalStat[key] = finalValue;

        }
        return finalStat;
    }


    #endregion

    #region Sprite&Anim
    void SetSprite()
    {

    }
    #endregion
    #region OnAttack
    public bool IsEvasion(LivingEntity target)
    {
        bool isEvasion = false;



        return isEvasion;
    }
    #endregion
    #region Item
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
            foreach(Modifier modi in addOptions)
            {
                modifierController.RemoveEquipment(modi);
            }
        }

        equipments[slot] = target;

        foreach (Modifier option in target.options)
        {
            modifierController.AddEquipment(option);
        }
        foreach(Modifier option in target.addOptions)
        {
            modifierController.AddEquipment(option);
        }
    }

    #endregion
    #region move

    public void SetDestination(Vector2Int target)
    {
        destination = target;
    }

    public void Start_Move()
    {
        // [중요] (int) 캐스팅이 아닌 반올림으로 정확한 타일 좌표 계산
        Vector2Int myPos = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y));

        // 출발하기 직전에 최종적으로 길이 막혔는지 확인!
        if (myPos != destination && !MapManager.instance.CanMove(destination))
        {
            // 길막 당했으면 이동 상태를 풀고 이번 행동은 취소
            moveState = Defines.MoveState.Idle;
            return;
        }

        GameManager.instance.EntityMove(this, myPos, destination);
        moveState = Defines.MoveState.Move;
        StartCoroutine("Moving");
    }

    protected virtual void Move_To(Vector2Int dir)
    {
        Vector2Int myPos = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y));
        destination = myPos + dir;

        Start_Move();
    }
    public IEnumerator Moving()
    {
        if (myObj == null)
        {
            myObj = this.gameObject;
        }
        Vector2 myPos = myObj.transform.position;
        Vector2 moveDirection = destination - myPos;
        float maxDistance = SetMoveDistance(moveDirection);
        while (Vector2.Distance(destination, myObj.transform.position) >= 0.2f)
        {
            myObj.transform.Translate(moveDirection * Time.deltaTime * animSpeed);
            if (Vector2.Distance(destination, myObj.transform.position) >= maxDistance)
            {
                Debug.Log("Break!");
                break;
            }
            yield return null;
        }
        End_Move();
    }
    public void End_Move()
    {
        myObj.transform.position = new Vector3(destination.x, destination.y, -1);
        moveState = Defines.MoveState.Idle;
    }
    float SetMoveDistance(Vector2 moveDirection)
    {
        float maxDixtance = 1.5f;
        if (Mathf.Abs(moveDirection.x) == Mathf.Abs(moveDirection.y))
        {
            maxDixtance = 1.4f;
        }
        else
        {
            maxDixtance = 1.1f;
        }
        return maxDixtance;
    }
    #endregion
    #region Battle
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
        myUIController.UpdateUIs(currentHp, maxHp);

    }
    public Dictionary<StatType, float> GetAttackBonus()
    {
        return GetEntityStat(ModifierTriggerType.OnAttack);
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
    #endregion
    public override MapEntity CopyToEmpty()
    {
        return new LivingEntity();
    }

    public override void ResetData()
    {
        myStat = null;
        Objname = null;
        id = null;
        equipments.Clear();
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

    public override void Return()
    {
        base.Return();
        Vector2 myPos = transform.position;
        Vector2Int posKey = new Vector2Int(Mathf.RoundToInt(myPos.x), Mathf.RoundToInt(myPos.y));
        MapManager.instance.dynamicMapData.Remove(posKey);
        
    }
    public void ItemCheck(Vector2Int pos)
    {
        groundItems = null;
        if (ItemManager.instance.GroundCheck(pos))
        {
            groundItems = ItemManager.instance.Get_GroundItems(pos);
        }
    }

    
}
