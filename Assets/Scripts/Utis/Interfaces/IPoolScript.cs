using UnityEngine;
using System;
using System.Collections.Generic;
public interface IPoolScript
{
    public void SetScriptType(Defines.PoolScriptType type);
    public Defines.PoolScriptType GetScriptType();
    public void Reset();
}
