using UnityEngine;
using System;
using System.Collections.Generic;
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
    }

    public GameObject ObjectPool(Defines.TileType type)
    {
        return pooler.Get(type);
    }
}
