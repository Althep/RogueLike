using UnityEngine;
using static Defines;
public class MonsterStat : EntityStat
{
    int tier;

    public MonsterStat()
    {

    }

    public MonsterStat(MonsterStat originData)
    {
        
    }
    public int GetMyTier()
    {
        return tier;
    }
    public void SetMyTier(int tier)
    {
        this.tier = tier;
    }
    public MonsterStat CopyMonsterStat()
    {
        MonsterStat newStat = new MonsterStat();
        foreach (StatType stat in baseStat.Keys)
        {
            newStat.SetBaseStat(stat, baseStat[stat]);
        }
        newStat.level = level;
        newStat.SetName(entityName);
        newStat.SetID(entityId);
        newStat.SetTileType(tileType);
        newStat.SetMyTier(tier);
        return newStat;
    }
}
