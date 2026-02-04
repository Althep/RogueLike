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

    List<BuffInstance> activeBuffs = new List<BuffInstance>();
    Queue<BuffInstance> InactiveBuffs = new Queue<BuffInstance>();
    private void Awake()
    {

    }

    public void Onit()
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

    public void AddBuff(LivingEntity entity, Modifier modifier, int duration)
    {
        Action removeAction = () =>
        {
            entity.RemoveModifier(modifier);
            Debug.Log($"Buff Removed from {entity.name}");
        };
        duration += Turn;
        BuffInstance newBuff = GetBuffInstance();
        newBuff.SetMyData(entity, modifier, duration, Turn, removeAction);
    }
    public void CheckAndRemoveBuff()
    {
        List<BuffInstance> buffsToRemove = new List<BuffInstance>();
        foreach (var buff in activeBuffs)
        {
            if (Turn >= buff.endTurn)
            {
                buffsToRemove.Add(buff);
            }
        }

        foreach (var buff in buffsToRemove)
        {
            buff.removalAction.Invoke();
            ReturnBuffInstance(buff);
        }
    }
    public void AddListnerToTurnEnd(Action action)
    {
        OnTurnEnd.AddListener(action.Invoke);
    }

    public BuffInstance GetBuffInstance()
    {
        BuffInstance result;
        if (InactiveBuffs.Count >= 1)
        {
            result = InactiveBuffs.Dequeue();
        }
        else
        {
            result = CreateNewBuffInstance();
        }
        activeBuffs.Add(result);
        return result;
    }
    public BuffInstance CreateNewBuffInstance()
    {
        return new BuffInstance();
    }
    public void ReturnBuffInstance(BuffInstance buffIns)
    {
        buffIns.Reset();
        InactiveBuffs.Enqueue(buffIns);
        if (activeBuffs.Contains(buffIns))
        {
            activeBuffs.Remove(buffIns);
        }
    }
}
