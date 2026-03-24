using UnityEngine;
using System;
using System.Collections.Generic;
using static Defines;
public class MonsterEntity : LivingEntity
{
    //[SerializeField]protected MonsterStat monsterStat;

    [SerializeField] int MaxHP = 0;

    private void Awake()
    {
        OnAwake();
        
    }
    protected override void OnAwake()
    {
        Init();
    }
    protected override void Init()
    {
        base.Init();
        modifierController.SetMyEntity(this);
        spriteRenderer = GetComponentsInChildren<SpriteRenderer>();
    }
    public void InitMonster()
    {
        MaxHP = (int)myStat.GetBaseStat(StatType.MaxHP);
    }


    public override EntityStat GetMyStat()
    {
        return myStat;
    }

    public override void SetMyStat(EntityStat stat)
    {
        base.SetMyStat(stat);
        Debug.Log("Set My Stat!");
        InitMonster();
    }
    public MonsterEntity CopyData(MonsterEntity entity)
    {
        entity.SetMyTileType(this.GetMyType());
        entity.id = this.id;
        entity.Objname = this.Objname;
        entity.animSpeed = this.animSpeed;
        entity.myStat = this.myStat.CopyStat();
        return entity;
    }
    
}
