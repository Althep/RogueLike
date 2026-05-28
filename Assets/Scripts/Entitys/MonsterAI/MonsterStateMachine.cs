using UnityEngine;
using System;
using System.Collections.Generic;
using static Defines;
public class MonsterStateMachine
{
    Dictionary<MonsterState, MonsterBaseState> states = new Dictionary<MonsterState, MonsterBaseState>();

    Astar pathFindger;
    GBFS GBFS;
    MonsterEntity myEntity;
    public MonsterState currentState { get; private set; }
    MonsterBaseState currentMachine;
    public Vector2Int lastPos;
    // ?? [핵심 변경 1] MapEntity가 아닌 ITargetable로 타겟을 관리합니다.
    public ITargetable currentTarget { get; private set; }

    // 이벤트로 들은 소리 위치를 기억하는 가짜 타겟
    private LocationTarget heardTarget = null;

    public MonsterStateMachine(Astar astar, GBFS gbfs,MonsterEntity myEntity)
    {
        pathFindger = astar;
        GBFS = gbfs;
        states.Add(MonsterState.Idle, new MonsterState_Idle(astar));
        states.Add(MonsterState.Sleep, new MonsterState_Sleep(astar));
        states.Add(MonsterState.Patrol, new MonsterState_Patrol(GBFS));
        states.Add(MonsterState.Attack, new MonsterState_Attack(GBFS));
        states.Add(MonsterState.Chase, new MonsterState_Chase(GBFS));
        this.myEntity = myEntity;
    }

    public void InitState()
    {
        ChangeState(MonsterState.Idle);
    }

    public void OnHearSound(Vector2Int soundPos)
    {
        heardTarget = new LocationTarget(soundPos);
    }

    public Vector2Int Get_Destination()
    {
        // 타겟이 없으면 제자리 반환, 있으면 타겟의 GridPos 반환
        if (currentTarget == null || !currentTarget.IsValid) return myEntity.GridPos;
        return states[currentState].Get_Destination(myEntity.GridPos, currentTarget.GridPos);
    }
    public List<Node> Get_Path()
    {
        return states[currentState].Get_Path();
    }
    // ?? [핵심 변경 2] 인자로 타겟을 받지 않고, 내부에서 우선순위에 따라 타겟을 결정합니다.
    public void UpdateMonsterState()
    {
        // --- 1. 타겟 설정 (우선순위: 눈앞의 적 > 들었던 소리) ---

        // Player 싱글톤이나 맵매니저에서 플레이어 객체를 가져옵니다 (단 1번의 참조)
        PlayerEntity player = GameManager.instance.Get_PlayerEntity();

        if (player != null && IsInVision(player))
        {
            currentTarget = player; // 시야에 있으면 진짜 플레이어로 타겟 재할당 (캐스팅 불필요)
            lastPos = player.Get_PosKey();
            heardTarget = null;     // 플레이어를 발견했으므로 소리 기억은 지움
        }

        else if (heardTarget != null)
        {
            currentTarget = heardTarget; // 안 보이지만 소리를 들었다면 가짜 타겟(좌표)을 할당
        }
        else
        {
            currentTarget = null; // 아무 일도 없음
        }


        // --- 2. 상태 결정 흐름 (Early Return 패턴) ---

        // 타겟이 존재할 때
        if (currentTarget != null && currentTarget.IsValid)
        {
            // 공격 가능 여부 (행동력 보유 && 실제 엔티티 && 사거리 내)
            if (CheckAttackSpeed() && IsAttackable(currentTarget))
            {
                ChangeState(MonsterState.Attack);
                return;
            }

            // 이동 가능 여부 (행동력 보유)
            if (CheckMoveSpeed())
            {
                // 타겟이 진짜 생명체(MapEntity)라면 쫓아가고, 가짜 타겟(소리 좌표)이면 정찰 감
                if (currentTarget is MapEntity)
                {
                    ChangeState(MonsterState.Chase);
                }
                else if (currentTarget is LocationTarget)
                {
                    ChangeState(MonsterState.Patrol);
                }
                return;
            }
        }

        // 타겟이 없거나 행동력이 없을 때
        if (IsSleep())
        {
            ChangeState(MonsterState.Sleep);
            return;
        }

        ChangeState(MonsterState.Idle);
    }

    public void ChangeState(MonsterState state)
    {
        if (currentState == state) return;

        Debug.Log($"Change State: {currentState} -> {state}");
        currentState = state;
        currentMachine = states[state];
        myEntity.Set_MyState(currentState);
    }

    public bool CheckAttackSpeed()
    {
        var stat = myEntity.CalculateContext(ModifierTriggerType.OnAttack);
        return myEntity.Get_ActPoint() >= stat[StatType.AttackSpeed];
    }

    public bool CheckMoveSpeed()
    {
        var stat = myEntity.CalculateContext(ModifierTriggerType.OnMove);
        return myEntity.Get_ActPoint() >= stat[StatType.MoveSpeed];
    }

    // ?? [핵심 변경 3] transform.position이 아닌 순수 정수 GridPos를 사용하여 연산 최적화
    public bool IsAttackable(ITargetable target)
    {
        if (target == null || !target.IsValid) return false;
        if (!(target is MapEntity)) return false;

        if (target is MapEntity targetEntity)
        {
            float attackRange = myEntity.CalculateContext(ModifierTriggerType.Passive)[StatType.AttackRange];

            Vector2Int gridPos = myEntity.Get_PosKey();
            Vector2Int targetPos = targetEntity.Get_PosKey();
            // [상세 설명]
            // 1. x축으로 몇 칸 떨어져 있는지 절댓값으로 구합니다.
            float dx = Mathf.Abs(gridPos.x - target.GridPos.x);

            // 2. y축으로 몇 칸 떨어져 있는지 절댓값으로 구합니다.
            float dy = Mathf.Abs(gridPos.y - target.GridPos.y);

            // 3. 체비쇼프 거리 공식: dx와 dy 중 더 '큰' 값을 진짜 거리로 취급합니다.
            // 이렇게 하면 대각선(dx=1, dy=1)이어도 거리가 2가 아닌 1로 계산됩니다.
            float maxDist = Mathf.Max(dx, dy);

            // 4. 이제 거듭제곱(sqrMagnitude) 연산 없이 깔끔하게 사거리와 비교합니다.
            return maxDist <= attackRange && MapManager.instance.CheckLineOfSight(myEntity.GridPos, target.GridPos);

        }
        return false;
    }

    public bool IsInVision(ITargetable target)
    {
        if (target == null || !target.IsValid) return false;

        float vision = myEntity.CalculateContext(ModifierTriggerType.Passive)[StatType.Vision];
        if(target is MapEntity mapEntity)
        {
            Vector2Int myPos = myEntity.Get_PosKey();
            Vector2Int targetPos = mapEntity.Get_PosKey();
            float dx = Mathf.Abs(myPos.x-targetPos.x);
            float dy = MathF.Abs(myPos.y-targetPos.y);

            float maxDist = MathF.Max(dx, dy);

            return maxDist <= vision && MapManager.instance.CheckLineOfSight(myPos,targetPos);
        }
        return false;
    }

    public bool IsSleep()
    {
        if (currentState != MonsterState.Sleep) return false;

        int awakeRate = (int)myEntity.CalculateContext(ModifierTriggerType.Passive)[StatType.AwakeRate];
        if (awakeRate <= 0) return true;

        return UnityEngine.Random.Range(0, 100) >= awakeRate;
    }
}