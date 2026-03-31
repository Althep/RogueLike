using UnityEngine;
using System;
using System.Collections.Generic;
using static Defines;

public class MonsterStateMachine
{
    Dictionary<MonsterState, MonsterBaseState> states = new Dictionary<MonsterState, MonsterBaseState>();

    Astar pathFindger;
    MonsterEntity myEntity;

    MonsterState currentState;
    MonsterBaseState currentMachine;

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
        ChangeState(MonsterState.Idle); // 초기 상태 설정 예시
    }

    // [핵심 조립 포인트] 플로우차트의 흐름을 그대로 구현한 상태 결정 함수
    public void UpdateMonsterState(LivingEntity target)
    {
        // 1. 공격 행동력 체크 (AttackSpeed < ActPoint)
        if (CheckAttackSpeed())
        {
            // 플로우차트의 첫 번째 InVision (사거리 및 시야 내에 있어 공격 가능한가?)
            if (IsAttackable(target))
            {
                ChangeState(MonsterState.Attack);
                return; // 상태 결정 완료, 함수 종료
            }
        }

        // 2. 공격을 못 하거나, 공격 행동력이 부족한 경우 이동 행동력 체크 (MoveCheck)
        if (CheckMoveSpeed()) // MoveSpeed < ActPoint (True 분기)
        {
            if (IsInVision(target))
            {
                ChangeState(MonsterState.Chase);
            }
            else
            {
                if (IsHearSound(target))
                {
                    ChangeState(MonsterState.Patrol);
                }
                else
                {
                    if (IsSleep())
                    {
                        ChangeState(MonsterState.Sleep);
                    }
                    else
                    {
                        ChangeState(MonsterState.Idle);
                    }
                }
            }
        }
        else // MoveSpeed < ActPoint (False 분기)
        {
            if (IsHearSound(target))
            {
                ChangeState(MonsterState.Patrol);
            }
            else
            {
                if (IsSleep())
                {
                    ChangeState(MonsterState.Sleep);
                }
                else
                {
                    ChangeState(MonsterState.Idle);
                }
            }
        }
    }

    public void ChangeState(MonsterState state)
    {
        if (currentState == state) return; // 이미 같은 상태면 무시

        currentState = state;
        currentMachine = states[state];
        myEntity.Set_MyState(currentState);
        // 여기에 currentMachine.Enter() 같은 상태 진입 코드를 추가하면 좋습니다.
    }

    public bool CheckAttackSpeed()
    {
        Dictionary<StatType, float> stat = myEntity.GetEntityStat(ModifierTriggerType.Passive);
        return myEntity.Get_ActPoint() >= stat[StatType.AttackSpeed];
    }

    public bool CheckMoveSpeed()
    {
        Dictionary<StatType, float> stat = myEntity.GetEntityStat(ModifierTriggerType.Passive);
        return myEntity.Get_ActPoint() >= stat[StatType.MoveSpeed];
    }

    // 오타 수정: IsHearSond -> IsHearSound
    public bool IsHearSound(LivingEntity target)
    {
        bool isHear = false;
        // TODO: 청각 감지 로직 구현 필요 (예: 일정 범위 내의 소음 이벤트 체크)
        return isHear;
    }

    public bool IsAttackable(LivingEntity target)
    {
        if (target == null) return false;

        Dictionary<StatType, float> attackStat = myEntity.GetEntityStat(ModifierTriggerType.Passive);
        float attackRange = attackStat[StatType.AttackRange];

        Vector2Int myPos = new Vector2Int((int)myEntity.transform.position.x, (int)myEntity.transform.position.y);
        Vector2Int targetPos = new Vector2Int((int)target.transform.position.x, (int)target.transform.position.y);
        float distance = Vector2.Distance(myPos, targetPos);

        return distance < attackRange && MapManager.instance.CheckLineOfSight(myPos, targetPos);
    }

    public bool IsInVision(LivingEntity target)
    {
        if (target == null) return false;

        Dictionary<StatType, float> passiveStat = myEntity.GetEntityStat(ModifierTriggerType.Passive);
        float vision = passiveStat[StatType.Vision];

        Vector2Int myPos = new Vector2Int((int)myEntity.transform.position.x, (int)myEntity.transform.position.y);
        Vector2Int targetPos = new Vector2Int((int)target.transform.position.x, (int)target.transform.position.y);
        float distance = Vector2.Distance(myPos, targetPos);

        return distance < vision && MapManager.instance.CheckLineOfSight(myPos, targetPos);
    }

    public bool IsSleep()
    {
        // 1. 현재 수면 상태가 아니라면 자고 있는 것이 아님
        if (currentState != MonsterState.Sleep)
        {
            return false;
        }

        // 2. 수면 상태라면 깨어날지 말지 확률 계산
        Dictionary<StatType, float> stat = myEntity.GetEntityStat(ModifierTriggerType.Passive);
        int awakeRate = (int)stat[StatType.AwakeRate];

        // 예외 처리: awakeRate가 0 이하일 경우 0 나누기 에러 방지 및 절대 못 깸 처리
        if (awakeRate <= 0) return true;

        // [버그 수정] Random.Range(0, awakeRate)는 0부터 awakeRate-1까지의 값을 반환합니다.
        int rate = UnityEngine.Random.Range(0, 100);

        if (rate<awakeRate)
        {
            return false;
        }

        return true; // 계속 수면
    }
}