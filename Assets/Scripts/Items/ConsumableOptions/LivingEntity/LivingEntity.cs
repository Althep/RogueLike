using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Defines;
public class LivingEntity : MapEntity
{
    GameObject myObj;
    protected EntityStat myStat;
    public string Objname;
    public string id;
    //private Dictionary<ModifierTriggerType, ModifierContext> modifierContext = new Dictionary<ModifierTriggerType, ModifierContext>();
    private Dictionary<SlotType, EquipItem> equipments = new Dictionary<SlotType, EquipItem>();
    [SerializeField] protected SPUM_MatchingList spriteMatching;
    Astar pathFinder;

    Vector2Int destination;

    Defines.MoveState moveState;

    public float animSpeed = 10f;

    ModifierController modifierController = new ModifierController();

    ModifierContext context;

    List<Modifier> buffs = new List<Modifier>();

    protected Races race;
    private void Awake()
    {
        modifierController.InitContext(this);
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
    }

    public virtual EntityStat GetMyStat()
    {
        return myStat;
    }
    public void SetMyStat(EntityStat stat)
    {
        myStat = stat;
    }
    #region SetData
    public void Set_RaceData(RaceData race)
    {
        List<Modifier> modifiers = race.modifiers;
        foreach (Modifier modi in modifiers)
        {
            modifierController.AddModifier(modi);
        }
        this.race = race.race;
        SetSprite();
    }
    public EntityStat Get_MyData()
    {
        return myStat;
    }
 
    public void Set_MyStat(EntityStat stat)
    {
        myStat = stat;
    }

    public void AddingStat(StatType type, float value)
    {
        myStat.SetBaseStat(type, value);
    }
    public Dictionary<StatType, float> GetEntityStat(ModifierTriggerType trigger)
    {
        Dictionary<StatType, float> baseStat = myStat.GetBase();
        Dictionary<StatType, float> finalStat = myStat.GetFinal();
        finalStat.Clear();
        ModifierContext context = modifierController.ApplyModifiers(trigger, this.context);
        ModifierContext passive = modifierController.ApplyModifiers(ModifierTriggerType.Passive, this.context);
        if (trigger == ModifierTriggerType.Passive)
        {
            foreach (var key in baseStat.Keys)
            {
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

    public Dictionary<SlotType, EquipItem> GetEquips()
    {
        return equipments;
    }
    public StatType Get_WeaponAttribueType()
    {
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

            foreach (Modifier modi in modis)
            {
                modifierController.RemoveModifier(modi);
            }
        }
        equipments[slot] = target;

        foreach (Modifier option in target.options)
        {
            modifierController.AddModifier(option);
        }

    }

    #endregion
    #region move
    protected virtual void Move_To(Vector2Int dest)
    {
        destination = dest;
        Vector2Int myPos = new Vector2Int((int)myObj.transform.position.x, (int)myObj.transform.position.y);
        Start_Move();
        GameManager.instance.EntityMove(myPos, dest, myType);
    }
    public void SetDestination(Vector2Int target)
    {
        destination = target;
    }

    public void Start_Move()
    {
        if (moveState == Defines.MoveState.Idle)
        {
            moveState = Defines.MoveState.Move;
            StartCoroutine("Moving");
        }
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
        myObj.transform.position = new Vector2(destination.x, destination.y);
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
    public Dictionary<StatType, float> GetAttackBonus()
    {
        return GetEntityStat(ModifierTriggerType.OnAttack);
    }
    public ModifierContext GetDefenseBonus()
    {
        return modifierController.ApplyModifiers(ModifierTriggerType.OnHited, this.context);
    }

    public ModifierContext Get_MeleeAttackContext()
    {
        return modifierController.ApplyModifiers(ModifierTriggerType.OnAttack, this.context);
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


    #region Modifier
    public ModifierContext GetContext()
    {
        return context;
    }
    public void AddModifier(Modifier modifier)
    {
        modifierController.AddModifier(modifier);
    }
    public void RemoveModifier(Modifier modifier)
    {
        modifierController.RemoveModifier(modifier);
    }
    public Dictionary<ModifierTriggerType, List<Modifier>> GetModifiers()
    {
        return modifierController.GetModifiers();
    }
    #endregion
}
