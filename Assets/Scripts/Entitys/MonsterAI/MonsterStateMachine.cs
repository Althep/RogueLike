using UnityEngine;
using System;
using System.Collections.Generic;
using static Defines;
public class MonsterStateMachine
{
    Dictionary<MonsterState, MonsterBaseState> states = new Dictionary<MonsterState, MonsterBaseState>();

    Astar pathFindger;
    MonsterEntity myEntity;
    public MonsterState currentState { get; private set; }
    MonsterBaseState currentMachine;

    // ?? [핵심 변경 1] MapEntity가 아닌 ITargetable로 타겟을 관리합니다.
    public ITargetable currentTarget { get; private set; }

    // 이벤트로 들은 소리 위치를 기억하는 가짜 타겟
    private LocationTarget heardTarget = null;

    public MonsterStateMachine(Astar astar, MonsterEntity myEntity)
    {
        pathFindger = astar;
        states.Add(MonsterState.Idle, new MonsterState_Idle(astar));
        states.Add(MonsterState.Sleep, new MonsterState_Sleep(astar));
        states.Add(MonsterState.Patrol, new MonsterState_Patrol(astar));
        states.Add(MonsterState.Attack, new MonsterState_Attack(astar));
        states.Add(MonsterState.Chase, new MonsterState_Chase(astar));
        this.myEntity = myEntity;
    }

    public void InitState()
    {
        ChangeState(MonsterState.Idle);
    }

    // [추가] 외부(SoundEventManager 등)에서 소리를 들었을 때 이 함수를 호출해 줍니다.
    public void OnHearSound(Vector2Int soundPos)
    {
        heardTarget = new LocationTarget(soundPos); // 순회 없이 좌표만 생성해서 기억!
    }

    public Vector2Int Get_Destination()
    {
        // 타겟이 없으면 제자리 반환, 있으면 타겟의 GridPos 반환
        if (currentTarget == null || !currentTarget.IsValid) return myEntity.GridPos;
        return states[currentState].Get_Destination(myEntity.GridPos, currentTarget.GridPos);
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
        var stat = myEntity.GetEntityStat(ModifierTriggerType.Passive);
        return myEntity.Get_ActPoint() >= stat[StatType.AttackSpeed];
    }

    public bool CheckMoveSpeed()
    {
        var stat = myEntity.GetEntityStat(ModifierTriggerType.Passive);
        return myEntity.Get_ActPoint() >= stat[StatType.MoveSpeed];
    }

    // ?? [핵심 변경 3] transform.position이 아닌 순수 정수 GridPos를 사용하여 연산 최적화
    public bool IsAttackable(ITargetable target)
    {
        if (target == null || !target.IsValid) return false;

        // 패턴 매칭: 타겟이 LocationTarget(단순 허공 좌표)라면 공격할 수 없음
        if (!(target is MapEntity)) return false;

        float attackRange = myEntity.GetEntityStat(ModifierTriggerType.Passive)[StatType.AttackRange];

        // GridPos 기반 거리 계산 (루트 연산 없이 빠름)
        float distSqr = ((Vector2)myEntity.GridPos - (Vector2)target.GridPos).sqrMagnitude;

        return distSqr < (attackRange * attackRange) && MapManager.instance.CheckLineOfSight(myEntity.GridPos, target.GridPos);
    }

    public bool IsInVision(ITargetable target)
    {
        if (target == null || !target.IsValid) return false;

        float vision = myEntity.GetEntityStat(ModifierTriggerType.Passive)[StatType.Vision];

        // GridPos 기반 거리 계산
        float distSqr = ((Vector2)myEntity.GridPos - (Vector2)target.GridPos).sqrMagnitude;

        return distSqr < (vision * vision) && MapManager.instance.CheckLineOfSight(myEntity.GridPos, target.GridPos);
    }

    public bool IsSleep()
    {
        if (currentState != MonsterState.Sleep) return false;

        int awakeRate = (int)myEntity.GetEntityStat(ModifierTriggerType.Passive)[StatType.AwakeRate];
        if (awakeRate <= 0) return true;

        return UnityEngine.Random.Range(0, 100) >= awakeRate;
    }
}