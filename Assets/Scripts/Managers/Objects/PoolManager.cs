using UnityEngine;
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
public class PoolManager : MonoBehaviour
{
    ObjectPooler pooler;
    [SerializeField]GameObject tilePrepab;
    [SerializeField] GameObject wallPrefab;
    [SerializeField] GameObject doorPrefab;
    [SerializeField] GameObject shallowWaterPrefab;
    [SerializeField] GameObject deepWaterPrefab;
    [SerializeField] GameObject upStairPrefab;
    [SerializeField] GameObject downStairPrefab;
    [SerializeField] GameObject playerPrefab;
    [SerializeField] GameObject monsterPrefab;
    [SerializeField] GameObject itemPrefab;

    GameObject mapObjectParent;
    private void Awake()
    {
        Init();
        
    }
    
    private void Init()
    {
        if(pooler == null)
        {
            pooler = new ObjectPooler(tilePrepab,wallPrefab,doorPrefab,shallowWaterPrefab,deepWaterPrefab,upStairPrefab,downStairPrefab,playerPrefab,monsterPrefab,itemPrefab);
        }
        if (mapObjectParent == null)
        {
            mapObjectParent = GameObject.Find("MapObjects");
        }
        pooler.SetObjectParent(mapObjectParent);
    }

    public GameObject ObjectPool(Defines.TileType type)
    {
        return pooler.Get(type);
    }

    public void Return(Defines.TileType type, GameObject target)
    {
        pooler.Return(type,target);
    }
    
    public async UniTask PreWarmObjects()
    {
        await pooler.PreWarmObjects();
    }
}
