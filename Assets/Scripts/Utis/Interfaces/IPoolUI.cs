using UnityEngine;
using System;
using System.Collections.Generic;
public interface IPoolUI
{
    public void Init();
    public void EnableFunction();
    public void DisableFunction();
    public GameObject Get();
    public void Return();
}
