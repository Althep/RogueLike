using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

public interface IMapObj
{
    public string GetTypeName();
    public void Return();
    public void Reset();
    
}

public class MapObjScriptPooler : MonoBehaviour
{/*
    Dictionary<string, List<EntityData>> activeScript = new Dictionary<string, List<EntityData>>();
    Dictionary<string, Queue<EntityData>> inactiveScript = new Dictionary<string, Queue<EntityData>>();

    Dictionary<string, EntityData> scriptMapping = new Dictionary<string, EntityData>() 
    { 

    
    };




    public EntityData Get(string typeName)
    {
        if (!inactiveScript.ContainsKey(typeName))
        {
            inactiveScript.Add(typeName, new Queue<EntityData>());
        }
        if (inactiveScript[typeName].Count < 1)
        {
            return CreateNew(typeName);
        }
        var script = inactiveScript[typeName].Dequeue();
        if (!activeScript.ContainsKey(typeName))
        {
            activeScript.Add(typeName, new List<EntityData>());
        }
        activeScript[typeName].Add(script);
        return script;
    }
    public EntityData CreateNew(string name)
    {
        var empty = scriptMapping[name].CopyToEmpty();
        if (!inactiveScript.ContainsKey(name))
        {
            inactiveScript.Add(name, new Queue<EntityData>());
        }
        if (!activeScript.ContainsKey(name))
        {
            activeScript.Add(name, new List<EntityData>());
        }
        activeScript[name].Add(empty);
        return empty;
    }
    public void Return(EntityData sc)
    {
        string key = sc.GetMyType();

        if (activeScript.ContainsKey(key))
        {
            if (activeScript[key].Contains(sc))
            {
                activeScript[key].Remove(sc);
            }
        }

        if (!inactiveScript.ContainsKey(key))
        {
            inactiveScript.Add(key, new Queue<EntityData>());
        }
        sc.ResetData();
        inactiveScript[key].Enqueue(sc);
    }
    */
}
