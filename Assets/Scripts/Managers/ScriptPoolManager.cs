using UnityEngine;
using System;
using System.Collections.Generic;
using static Defines;
public class ScriptPoolManager : MonoBehaviour
{
    Dictionary<PoolScriptType, Queue<IPoolScript>> inactivePool = new Dictionary<PoolScriptType, Queue<IPoolScript>>();

    Dictionary<PoolScriptType, List<IPoolScript>> activePool = new Dictionary<PoolScriptType, List<IPoolScript>>();



    public IPoolScript Get(PoolScriptType type)
    {
        IPoolScript value;
        if (!inactivePool.ContainsKey(type))
        {
            inactivePool[type] = new Queue<IPoolScript>();
        }
        if (!activePool.ContainsKey(type))
        {
            activePool[type] = new List<IPoolScript>();
        }
        if (inactivePool[type].Count < 1)
        {
            CreateNew(type);
        }
        value = inactivePool[type].Dequeue();
        if(activePool[type] == null)
        {
            activePool[type] = new List<IPoolScript>();
        }
        activePool[type].Add(value);
        return value;
    }

    public IPoolScript CreateNew(PoolScriptType type)
    {
        IPoolScript value = null;
        switch (type)
        {
            case PoolScriptType.StatModifier:
                value = new StatModifier();
                break;
            case PoolScriptType.ItemModifier:
                value = new ItemModifier();
                break;
            case PoolScriptType.BuffModifier:
                value = new BuffModifier();
                break;
            default:
                break;
        }
        value.SetScriptType(type);
        if (!inactivePool.ContainsKey(type))
        {
            inactivePool[type] = new Queue<IPoolScript>();
        }
        inactivePool[type].Enqueue(value);
        return value;
    }

    public void Return(IPoolScript script)
    {
        PoolScriptType type = script.GetScriptType();
        activePool[type].Remove(script);
        if (!activePool.ContainsKey(type))
        {
            activePool[type] = new List<IPoolScript>();
        }
        if (!inactivePool[type].Contains(script))
        {
            inactivePool[type].Enqueue(script);
        }
    }

}
