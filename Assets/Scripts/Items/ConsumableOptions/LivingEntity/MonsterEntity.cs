using UnityEngine;
using System;
using System.Collections.Generic;
public class MonsterEntity : LivingEntity
{
    protected MonsterStat monsterStat => myStat as MonsterStat;

    public override EntityStat GetMyStat()
    {
        return monsterStat;
    }
    public MonsterEntity CopyData(MonsterEntity entity)
    {
        entity.SetMyTileType(this.GetMyType());
        entity.id = this.id;
        entity.Objname = this.Objname;
        entity.animSpeed = this.animSpeed;
        entity.myStat = this.monsterStat.CopyMonsterStat();
        return entity;
    }
}
