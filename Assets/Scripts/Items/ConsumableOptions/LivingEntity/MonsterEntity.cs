using UnityEngine;
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using static Defines;
public class MonsterEntity : LivingEntity
{
    //[SerializeField]protected MonsterStat monsterStat;

    [SerializeField] int MaxHP = 0;
    [SerializeField] int actPoint = 0;
    [SerializeField] MonsterState myState;
    [SerializeField] float moveSpeed;
    [SerializeField] float attackSpeed;
    [SerializeField] float attackRange;
    MonsterStateMachine myStateMachine;

    MapEntity targetEntity;

    
    private void Awake()
    {
        OnAwake();
        
    }
    protected override void OnAwake()
    {
        Init();
    }
    protected override void Init()
    {
        base.Init();
        modifierController.SetMyEntity(this);
        equipRenders = GetComponentsInChildren<SpriteRenderer>();
        if(myStateMachine == null)
        {
            myStateMachine = new MonsterStateMachine(pathFinder, this);
        }
    }
    public void InitMonster()
    {
        MaxHP = Mathf.RoundToInt(myStat.GetBaseStat(StatType.MaxHP));
        moveSpeed = myStat.GetBaseStat(StatType.MoveSpeed);
        attackSpeed = myStat.GetBaseStat(StatType.AttackSpeed);
        attackRange = myStat.GetBaseStat(StatType.AttackRange);
        myStat.SetBaseStat(StatType.HP,MaxHP);

        myUIController.InitUIs(Mathf.RoundToInt(myStat.GetBaseStat(StatType.HP)), MaxHP, id);
    }


    public override EntityStat GetMyStat()
    {
        return myStat;
    }

    public override void SetMyStat(EntityStat stat)
    {
        base.SetMyStat(stat);
        Debug.Log("Set My Stat!");
        InitMonster();
    }

    public void Add_ActPoint(int point)
    {
        actPoint+=point;
    }
    public int Get_ActPoint()
    {
        return actPoint;
    }
    public void Set_MyState(MonsterState state)
    {
        myState = state;
    }
    public MonsterEntity CopyData(MonsterEntity entity)
    {
        entity.SetMyTileType(this.GetMyType());
        entity.id = this.id;
        entity.Objname = this.Objname;
        entity.animSpeed = this.animSpeed;
        entity.myStat = this.myStat.CopyStat();
        return entity;
    }

    public void Attack()
    {
        Debug.Log("Attack!");
    }

    public void Set_LastAttacker(LivingEntity attacker)
    {
        lastAttacker = attacker;
    }
    // 1. UniTask를 활용하여 비동기(Async) 함수로 변경합니다.
    public async UniTask ReadyAction()
    {
        if (myStateMachine == null)
        {
            Debug.Log($"State Machine Not Ready! {this.gameObject.name}");
            return;
        }

        // 타겟 갱신 (이전 대화에서 조언드린 대로 LivingEntity나 MapEntity 형태로 
        // StateMachine 내부에서 관리하고 갱신하는 것이 더 확장성에 좋습니다.)
        targetEntity = PlayerController.instance.Get_PlayerEntity();

        // 2. actPoint가 0보다 큰 동안만 반복하도록 단순화합니다.
        while (actPoint > 0)
        {
            myStateMachine.UpdateMonsterState();

            // 3. 행동을 덮어놓고 실행하기 전에, 무슨 행동을 할지 '비용(Cost)'만 먼저 계산합니다.
            float requiredCost = CalculateActionCost();

            // 4. 요구 비용이 유효하고(0보다 크고), 현재 AP로 감당할 수 있는지 검사합니다.
            if (requiredCost > 0 && actPoint >= requiredCost)
            {
                destination = myStateMachine.Get_Destination();

                // 실제 행동 실행
                ExecuteAction();

                // AP 차감
                actPoint -= (int)requiredCost;

                // 5. [매우 중요] 행동 직후 약간의 대기 시간을 주어 플레이어가 
                // 몬스터가 이동하거나 공격하는 애니메이션을 볼 수 있게 합니다.
                // 애니메이션 길이에 맞춰 시간(0.2f 등)을 조절하시면 됩니다.
                if (false)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(0.2f));
                }

            }
            else
            {
                // AP가 부족하거나, 알 수 없는 상태라 비용이 0이라면 이번 턴을 강제로 종료(Break)합니다.
                // 이렇게 하면 무한 루프를 완벽하게 차단할 수 있습니다.
                break;
            }
        }
    }

    // 행동의 '비용'만 계산해서 반환하는 함수 (기존 TryAction에서 분리)
    private float CalculateActionCost()
    {
        switch (myStateMachine.currentState)
        {
            case MonsterState.Idle:
            case MonsterState.Sleep:
            case MonsterState.Chase:
            case MonsterState.Patrol:
                Debug.Log($"MovePoint {GetEntityStat(ModifierTriggerType.OnMove)[StatType.MoveSpeed]}");
                return GetEntityStat(ModifierTriggerType.OnMove)[StatType.MoveSpeed];

            case MonsterState.Attack:
                return GetEntityStat(ModifierTriggerType.OnAttack)[StatType.AttackSpeed]; // OnMove가 아닌 OnAttack 트리거 확인

            default:
                return 0; // 예외 상황
        }
    }

    // 실제 행동(로직)만 실행하는 함수 (기존 TryAction에서 분리)
    private void ExecuteAction()
    {
        switch (myStateMachine.currentState)
        {
            case MonsterState.Idle:
            case MonsterState.Sleep:
            case MonsterState.Chase:
            case MonsterState.Patrol:
                Start_Move();
                break;

            case MonsterState.Attack:
                Attack();
                break;
        }
    }

    public LivingEntitySaveData SaveMonster()
    {
        LivingEntitySaveData saveData = new LivingEntitySaveData();

        saveData.id = id;//Id셋

        Dictionary<StatType, float> stat = GetMyStat().GetBase();

        int currentHp = Mathf.RoundToInt(stat[StatType.HP]);

        saveData.currentHp = currentHp;//hp셋

        List<BuffSaveData> buffsave = modifierController.BuffSave();
        List<ModifierSaveData> modifierSaves = modifierController.MutetionSave();
        saveData.buffs = buffsave;
        saveData.modifiers = modifierSaves;
        foreach(var key in equipments.Keys)
        {
            ItemSaveData data = equipments[key].SaveData();
            saveData.equipMentsData.Add(data);//장비 추가
        }

        List<ItemSaveData> itemSaves = new List<ItemSaveData>();
        if(inventoryData == null)
        {
            return saveData;
        }
        for(int i = 0; i<inventoryData.inventory.Length; i++)
        {
            if (inventoryData.inventory[i] ==null)
            {
                continue;
            }

            ItemSaveData item = inventoryData.inventory[i].SaveData();
            itemSaves.Add(item);//인벤토리 아이템 추가
        }
        return saveData;
    }
}
