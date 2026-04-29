using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;
using System.Collections.Generic;
public class EventManager : MonoBehaviour
{
    public int Turn;

    public static EventManager instance;

    public UnityEvent OnPlayerAction = new UnityEvent();
    public UnityEvent OnFloorChange = new UnityEvent();
    public UnityEvent OnTurnEnd = new UnityEvent();
    public UnityEvent OnSceneChange = new UnityEvent();
    Dictionary<int, List<BuffInstance>> activeBuffs = new Dictionary<int, List<BuffInstance>>();
    Dictionary<LivingEntity, List<BuffInstance>> entityBuffs = new Dictionary<LivingEntity, List<BuffInstance>>();
    //List<BuffInstance> activeBuffs = new List<BuffInstance>();
    Queue<BuffInstance> InactiveBuffs = new Queue<BuffInstance>();
    private void Awake()
    {
        Init();
    }

    public void Init()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    private void Update()
    {

    }

    void InvokeActions(List<Action> actions)
    {
        foreach (Action a in actions)
        {
            a.Invoke();
        }
    }
    public void EndTurn()
    {
        OnTurnEnd.Invoke();
        Turn++;
        Debug.Log($"Turn {Turn} Started ");
    }
    #region buffs
    public void AddBuff(LivingEntity entity, Modifier modifier, int duration)
    {
        // 1. 끝나는 턴(endTurn)을 명확하게 계산합니다.
        int endTurn = Turn + duration;

        Action removeAction = () =>
        {
            entity.Remove_BuffModifier(modifier);
            Debug.Log($"Buff Removed from {entity.name}");
        };

        // 2. 풀에서 빈 껍데기를 하나 가져옵니다. (GetBuffInstance 내부 로직 축소 필요)
        BuffInstance newBuff = GetBuffInstance(entity);
        newBuff.SetMyData(entity, modifier, duration, Turn, removeAction);

        // 3. ?? 여기가 핵심! 현재 턴(Turn)이 아니라 끝나는 턴(endTurn)에 기록해야 합니다!
        if (!activeBuffs.TryGetValue(endTurn, out List<BuffInstance> instances))
        {
            instances = new List<BuffInstance>();
            activeBuffs.Add(endTurn, instances);
        }
        if (!entityBuffs.ContainsKey(entity))
        {
            entityBuffs.Add(entity, new List<BuffInstance>());
        }
        entityBuffs[entity].Add(newBuff);
        instances.Add(newBuff);
    }

    public void ProcessPassedTurn(int startTurn, int endTurn)
    {
        this.Turn = TurnManager.instance.GlovalTurn;

        // 1. 수정: < 가 아니라 <= 로 해야 이번 턴(endTurn)에 끝나는 버프까지 확실하게 지웁니다!
        for (int t = startTurn; t <= endTurn; t++)
        {
            CheckAndRemoveBuff(t);
        }
    }

    public void CheckAndRemoveBuff(int turn)
    {
        if (!activeBuffs.TryGetValue(turn, out List<BuffInstance> buffs) || buffs.Count == 0)
        {
            return;
        }

        for (int i = buffs.Count - 1; i >= 0; i--)
        {
            BuffInstance buff = buffs[i];

            // 1. [안전장치] 타겟이 이미 null이거나, 생존자 명부(entityBuffs)에 없으면 사망 처리
            if (buff.target == null || !entityBuffs.ContainsKey(buff.target))
            {
                buff.target = null;
            }

            // 2. 타겟이 살아있을 때만 버프 해제 로직 실행
            if (buff.target != null)
            {
                buff.removalAction?.Invoke();

                // 생존자 명부(엔티티 장부)에서는 이 버프를 지워줌
                if (entityBuffs.TryGetValue(buff.target, out var eBuffs))
                {
                    eBuffs.Remove(buff);
                }
            }

            // 3. 풀링 큐(Queue)로 객체 반환 (리스트 인자 전달 X)
            // ※ EventManager의 ReturnBuffInstance 함수에서도 list.Remove 로직은 지워주세요!
            ReturnBuffInstance(buffs,buff);
        }

        // 4. 어차피 여기서 리스트(페이지)가 통째로 날아가므로, 루프 안에서 개별 요소를 지울 필요가 없습니다.
        activeBuffs.Remove(turn);
    }
    public void AddListnerToTurnEnd(Action action)
    {
        OnTurnEnd.AddListener(action.Invoke);
    }
    public void AddListnerToSceneChange(Action action)
    {
        OnSceneChange.AddListener(action.Invoke);
    }
    public BuffInstance GetBuffInstance(LivingEntity entity)
    {
        BuffInstance result;

        // Count >= 1 도 좋지만, 관례적으로 Count > 0을 더 자주 사용합니다.
        if (InactiveBuffs.Count > 0)
        {
            result = InactiveBuffs.Dequeue();
        }
        else
        {
            result = CreateNewBuffInstance();
        }

        // 최적화: 딕셔너리를 한 번만 검색하여 바로 리스트를 꺼내옵니다.
        if (!activeBuffs.TryGetValue(Turn, out List<BuffInstance> instances))
        {
            // 리스트가 없다면 새로 만들어서 딕셔너리에 넣어주고,
            // 이 때 새로 만든 리스트의 참조를 instances 변수가 그대로 들고 있습니다.
            instances = new List<BuffInstance>();
            activeBuffs.Add(Turn, instances);
        }

        // 이 instances는 기존에 있던 것이든, 방금 새로 만든 것이든 원본을 가리킵니다.
        instances.Add(result);
        return result;
    }
    public BuffInstance CreateNewBuffInstance()
    {
        return new BuffInstance();
    }
    public void ReturnBuffInstance(List<BuffInstance> list ,BuffInstance target)
    {
        target.Reset();
        InactiveBuffs.Enqueue(target);
        if (list.Contains(target))
        {
            list.Remove(target);
        }
    }

    public void RemoveEntity(LivingEntity entity)
    {
        if (entity.IsDead())
        {
            entityBuffs.Remove(entity);
        }
    }

    public List<BuffInstance> Get_EntityBuffList(LivingEntity entity)
    {
        if (entityBuffs.ContainsKey(entity))
        {
            return entityBuffs[entity];
        }
        return null;
    }
    #endregion
}
