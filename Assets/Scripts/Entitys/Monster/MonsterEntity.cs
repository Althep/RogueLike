using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using static Defines;

public class MonsterEntity : LivingEntity
{
    #region [1] 기본 정보 및 식별자 (Identity & Component)
    [Header("Monster Identity")]
    [SerializeField] private MonsterState myState;
    private LivingEntity targetEntity; // MapEntity에서 LivingEntity로 구체화하여 전투 연동 최적화
    private bool isInSight = false;
    #endregion

    #region [2] 상태 및 스탯 (Stats & Level)
    [Header("Monster Stats (Runtime Cache)")]
    [SerializeField] private int maxHP = 0;
    [SerializeField] private int actPoint = 0;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float attackSpeed;
    [SerializeField] private float attackRange;

    public void InitMonster()
    {
        if (myStat == null) return;

        maxHP = Mathf.RoundToInt(myStat.GetBaseStat(StatType.MaxHP));
        moveSpeed = myStat.GetBaseStat(StatType.MoveSpeed);
        attackSpeed = myStat.GetBaseStat(StatType.AttackSpeed);
        attackRange = myStat.GetBaseStat(StatType.AttackRange);

        myStat.SetBaseStat(StatType.HP, maxHP);
        myUIController?.InitUIs(Mathf.RoundToInt(myStat.GetBaseStat(StatType.HP)), maxHP, id);
    }

    public override EntityStat GetMyStat() => myStat;

    public override void SetMyStat(EntityStat stat)
    {
        base.SetMyStat(stat);
        InitMonster();
    }

    public void Add_ActPoint(int point) => actPoint += point;
    public int Get_ActPoint() => actPoint;
    public void Set_MyState(MonsterState state) => myState = state;
    #endregion

    #region [4] 장비 및 인벤토리 (Inventory & Items)
    public MonsterEntity CopyData(MonsterEntity entity)
    {
        entity.SetMyTileType(this.GetMyType());
        entity.id = this.id;
        entity.Objname = this.Objname;
        entity.animSpeed = this.animSpeed;
        entity.myStat = this.myStat.CopyStat();
        return entity;
    }
    #endregion

    #region [5] 이동 및 길찾기 (Movement & Navigation)
    // [핵심 보완] 부모의 Moving 코루틴이 끝날 때까지 동기화하기 위한 플래그 추가
    private bool isMovingNow = false;
    List<Node> _path;
    protected override void Move_To(Vector2Int dir)
    {
        destination = CurrentTilePos + dir;
        Start_MonsterMove();
    }

    private void Start_MonsterMove()
    {
        // 부모인 LivingEntity의 Start_Move()를 바로 실행하여 
        // 모든 몬스터가 '동시에' 걷기 시작하게 만듭니다.
        base.Start_Move();
        DrawingPath();
    }

    private IEnumerator MonsterMovingLoop()
    {
        isMovingNow = true;
        yield return StartCoroutine(Moving()); // 부모의 Moving() 코루틴이 완전히 끝날 때까지 대기
        isMovingNow = false;
    }
    #endregion

    #region [6] 전투 및 판정 (Combat & Battle)
    public void Set_LastAttacker(LivingEntity attacker) => lastAttacker = attacker;

    public void Attack()
    {
        if (targetEntity == null) return;

        bool isCrit = false;
        CombatManager.instance.TryHit(this, targetEntity, out isCrit); //CombatManager 내부에 각 트리거 활성화한후 작동
        Debug.Log($"{this.gameObject.name}이(가) {targetEntity.gameObject.name}을(를) 공격했습니다!");
    }

    // [핵심] 부모의 ExecuteAction을 오버라이드하여 몬스터 전용 연출(피격/공격 애니메이션 등) 확장 가능구간 확보
    public virtual void DrawingPath()
    {
        _path = myStateMachine.Get_Path();
        //OnDrawGizmos();
    }

    private void OnDrawGizmos()
    {
        // 1. 예외 처리: 경로가 없거나 노드가 2개 미만(선을 그을 수 없음)이면 함수를 종료합니다.
        if (_path == null || _path.Count < 2) return;

        // 2. 색상 설정: 씬 뷰에서 눈에 잘 띄도록 기즈모의 색상을 빨간색으로 설정합니다.
        Gizmos.color = Color.red;

        // 3. 경로 순회 및 선 그리기
        for (int i = 0; i < _path.Count - 1; i++)
        {
            // Node 클래스 안에 월드 좌표를 나타내는 변수(예: worldPosition)가 있다고 가정합니다.
            Vector3 currentPos = new Vector3(_path[i].x, _path[i].y);
            Vector3 nextPos = new Vector3(_path[i+1].x, _path[i+1].y);

            // 현재 노드와 다음 노드 위치를 선(Line)으로 연결하여 그립니다.
            Gizmos.DrawLine(currentPos, nextPos);

            // (선택 사항) 각 노드의 위치를 정확히 보고 싶다면 작은 구(Sphere)를 그릴 수도 있습니다.
            Gizmos.DrawSphere(currentPos, 0.1f);
        }

        // 마지막 도착지 노드에도 구를 하나 그려줍니다.
        Gizmos.DrawSphere(new Vector3(_path[_path.Count-1].x, _path[_path.Count-1].y), 0.1f);
    }
    public override void OnDeadFunc()
    {
        base.OnDeadFunc();
    }
    #endregion

    #region [6-1] AI 턴 처리 및 AP 계산 루프 (AI Brain & UniTask Loop)
    private MonsterStateMachine myStateMachine;
    /*
    public void OnPlayerMove(PlayerEntity player)
    {

    }
    */
    public ModifierTriggerType CalculateTrigger(MonsterState state)
    {
        ModifierTriggerType trigger = ModifierTriggerType.OnMove;
        switch (state)
        {
            case MonsterState.Attack:
                trigger = ModifierTriggerType.OnAttack;
                break;
            default:
                trigger = ModifierTriggerType.Passive;
                break;
        }
        return trigger;
    }

    public override void ExecuteAction(LivingEntity target, ModifierTriggerType trigger)
    {
        switch (myStateMachine.currentState)
        {
            case MonsterState.Idle:
            case MonsterState.Sleep:
            case MonsterState.Chase:
            case MonsterState.Patrol:
                Start_MonsterMove();
                break;
            case MonsterState.Attack:
                Attack();
                break;
        }
        base.ExecuteAction(target, trigger);
    }
    public void OnHeardTarget(Vector2Int pos)
    {
        myStateMachine.OnHearSound(pos);
    }
    public async UniTask ReadyAction()
    {
        if (myStateMachine == null) return;

        targetEntity = PlayerController.instance.Get_PlayerEntity() as LivingEntity;

        while (actPoint > 0)
        {
            myStateMachine.UpdateMonsterState();
            float requiredCost = CalculateActionCost();

            if (requiredCost > 0 && actPoint >= requiredCost)
            {
                destination = myStateMachine.Get_Destination();

                ModifierTriggerType trigger = CalculateTrigger(myState);
                ExecuteAction(targetEntity,trigger);

                actPoint -= (int)requiredCost;

                // [수정] 내부의 무거운 대기 코드를 전부 치워버립니다.
                // 몬스터가 화면에 보일 때만 아주 미세한 연출용 갭(예: 0.01초~0.03초)만 주거나 아예 생략합니다.
                if (isInSight)
                {
                    await UniTask.Yield(); // 단 1프레임만 양보해서 다음 몬스터와 '동시 실행'되도록 유도
                }
            }
            else
            {
                break;
            }
        }
    }

    private float CalculateActionCost()
    {
        if (myStat == null) return 0;

        switch (myStateMachine.currentState)
        {
            case MonsterState.Idle:
            case MonsterState.Sleep:
            case MonsterState.Chase:
            case MonsterState.Patrol:
                // 이동 속도 비용 계산
                return CalculateContext(ModifierTriggerType.OnMove)[StatType.MoveSpeed];

            case MonsterState.Attack:
                // 공격 속도 비용 계산 (OnAttack 트리거 사용)
                return CalculateContext(ModifierTriggerType.OnAttack)[StatType.AttackSpeed];

            default:
                return 0;
        }
    }

    #endregion

    #region [7] 시각 요소 및 렌더링 (Visuals & Rendering)
    public override void Set_Visibility(bool isVisible)
    {
        base.Set_Visibility(isVisible);
        isInSight = isVisible; // 무의미한 함수 제거하고 다이렉트 바인딩
    }
    #endregion

    #region [8] 유니티 라이프사이클 및 오버라이드 (Lifecycle & MapEntity Override)
    // [수정] Awake는 부모의 것을 그대로 타게 두고, 상속받은 Init과 OnAwake만 깔끔하게 오버라이드합니다.
    protected override void OnAwake()
    {
        base.OnAwake();
    }

    protected override void Init()
    {
        base.Init();

        modifierController.SetMyEntity(this);
        equipRenders = GetComponentsInChildren<SpriteRenderer>();

        if (myStateMachine == null)
        {
            myStateMachine = new MonsterStateMachine(pathFinder, GBFS,this);
        }
    }

    public override void Return()
    {
        MapManager.instance.RemoveMapEntity(Get_PosKey(), this);
        MonsterManager.Instance.RemoveMonster(this);
        PoolManager.instance.Return(GetMyType(), this.gameObject);
    }
    #endregion

    #region [9] 세이브 앤 로드 (Save & Load)
    public LivingEntitySaveData SaveMonster()
    {
        LivingEntitySaveData saveData = new LivingEntitySaveData { id = id };

        if (GetMyStat() != null)
        {
            Dictionary<StatType, float> stat = GetMyStat().GetBase();
            saveData.currentHp = Mathf.RoundToInt(stat[StatType.HP]);
        }

        Vector2Int posKey = Get_PosKey();
        saveData.x = posKey.x;
        saveData.y = posKey.y;

        if (modifierController != null)
        {
            saveData.buffs = modifierController.BuffSave();
            saveData.modifiers = modifierController.MutetionSave();
        }

        if (equipSystem != null)
        {
            saveData.equipMentsData.AddRange(equipSystem.SaveEquips());
        }

        if (inventoryData?.inventory != null)
        {
            for (int i = 0; i < inventoryData.inventory.Length; i++)
            {
                if (inventoryData.inventory[i] == null) continue;
                saveData.equipMentsData.Add(inventoryData.inventory[i].SaveData());
            }
        }
        return saveData;
    }
    #endregion
}