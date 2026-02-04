using UnityEngine;
using System;
using Cysharp.Threading.Tasks;
public abstract class AsyncDataManager<T> where T: AsyncDataManager<T>
{
    private static T _instance;
    protected string dataName;
    protected AsyncDataManager() { }
    public abstract UniTask Init();
    public static async UniTask<T> CreateAsync()
    {
        if (_instance != null) return _instance;
        
        _instance = (T)Activator.CreateInstance(typeof(T),true);
        
        if(_instance != null)
        {
            await _instance.Init();
        }
        return _instance;
    }

}
