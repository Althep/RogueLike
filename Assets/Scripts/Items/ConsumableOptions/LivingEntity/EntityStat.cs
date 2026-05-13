using UnityEngine;
using System;
using System.Collections.Generic;
using static Defines;
using System.Xml;

[Serializable]
public class EntityStat
{
    protected TileType tileType;
    protected Dictionary<StatType, float> baseStat = new();

    // ?? 패시브 캐싱용 바구니와 액티브 계산용 바구니를 철저히 분리합니다.
    protected Dictionary<StatType, float> finalStat = new();
    protected Dictionary<StatType, float> activeStatCache = new();

    protected bool isDirty = true;
    protected int level;
    protected string entityId;
    protected string entityName;
    bool isDead;

    ModifierController modifierController;
    LivingEntity myEntity;

    public EntityStat()
    {
        StatType[] types = Utils.Get_Enums<StatType>();
        foreach (var stat in types)
        {
            baseStat[stat] = 0f;
            finalStat[stat] = 0f;
            activeStatCache[stat] = 0f;
        }

        isDead = false;
        baseStat[StatType.HP] = baseStat[StatType.MaxHP];
    }
    public void Set_ModifierController(ModifierController mc)
    {
        modifierController = mc;
    }
    public void Set_MyEntity(LivingEntity entity)
    {
        myEntity = entity;
    }
    public void AddBaseStat(StatType type, float value)
    {
        if (baseStat.ContainsKey(type))
        {
            baseStat[type] += value;
            OnStatChanged();
        }
        else
        {
            baseStat.Add(type, value);
            OnStatChanged();
        }

    }
    public int Get_Level()
    {
        return level;
    }
    public void Add_Level()
    {
        level++;
    }
    public void SetBaseStat(StatType type, float value)
    {
        if (baseStat.ContainsKey(type))
        {
            baseStat[type] = value;

        }
        else
        {
            baseStat.Add(type, value);
        }
        OnStatChanged();
    }
    public void SetID(string id)
    {
        entityId = id;
    }
    public void SetName(string Name)
    {
        entityName = Name;
    }
    public void SetTileType(TileType type)
    {
        tileType = type;
    }
    public TileType GetTileType()
    {
        return tileType;
    }
    public string GetId()
    {
        return entityId;
    }
    public string GetName()
    {
        return entityName;
    }
    public int GetLevel()
    {
        return level;
    }
    public Dictionary<StatType,float> GetBase()
    {
        return baseStat;
    }

    public float GetBaseStat(StatType stat)
    {
        return baseStat[stat];
    }

    public virtual EntityStat CopyStat() 
    {
        EntityStat newStat = new EntityStat();
        foreach(StatType stat in baseStat.Keys)
        {
            newStat.SetBaseStat(stat, baseStat[stat]);
        }
        newStat.level = level;
        newStat.SetName(entityName);
        newStat.SetID(entityId);
        newStat.SetTileType(tileType);
        return newStat;
    }

    public int GetDamageToResist(DamageType type, int damage)
    {
        // ?? 0으로 나누기 방지: 저항값이 0이면 최소 1로 취급하여 에러를 막습니다.
        // (기획하신 저항력 공식에 맞춰 나중에 수정하시면 됩니다)
        float SafeResist(StatType resType) => Mathf.Max(1f, finalStat[resType]);

        switch (type)
        {
            case DamageType.Physical: break;
            case DamageType.Fire: damage = (int)(damage * (1f / SafeResist(StatType.FireResist))); break;
            case DamageType.Ice: damage = (int)(damage * (1f / SafeResist(StatType.IceResist))); break;
            case DamageType.Magic: damage = (int)(damage * (1f / SafeResist(StatType.MagicResist))); break;
            case DamageType.Thunder: damage = (int)(damage * (1f / SafeResist(StatType.ThunderResist))); break;
            case DamageType.Poison: break;
        }
        return damage;
    }

    public bool IsDead()
    {
        return isDead;
    }

    public void MarksAsDirty()
    {
        isDirty = true;
    }

    public void OnStatChanged()
    {
        if(myEntity != null)
        {
            myEntity.OnStatOrModifierChange();
        }
    }

    public Dictionary<StatType, float> Get_FinalStat(ModifierTriggerType trigger)
    {
        // 1. 패시브 요청이고 데이터가 깨끗하면 바로 캐시(finalStat) 반환
        if (!isDirty && trigger == ModifierTriggerType.Passive)
        {
            return finalStat;
        }

        // 패시브 컨텍스트는 무조건 가져옵니다.
        ModifierContext passive = modifierController.ApplyModifiers(ModifierTriggerType.Passive);

        // 2. 패시브 캐시가 더러우면(isDirty) 싹 재계산합니다.
        if (isDirty)
        {
            foreach (var key in baseStat.Keys)
            {
                float baseVal = baseStat[key];

                // 삼항 연산자로 ContainsKey 검사를 한 줄로 압축
                float passAdd = passive.stats.ContainsKey(key) ? passive.stats[key] : 0f;
                float passMul = passive.multifle.ContainsKey(key) ? passive.multifle[key] : 0f;
                finalStat[key] = (baseVal + passAdd) * (1f + passMul);

            }
            // 패시브 계산이 끝났으므로 청소 완료 깃발을 꽂습니다.
            isDirty = false;
        }

        // 3. 만약 패시브 스탯을 달라고 한 거였으면 여기서 종료!
        if (trigger == ModifierTriggerType.Passive)
        {
            return finalStat;
        }

        // 4. 액티브 트리거(예: 공격) 요청일 경우!
        // ?? 주의: finalStat을 건드리지 않고 별도의 activeStatCache에 담아서 줍니다.
        ModifierContext context = modifierController.ApplyModifiers(trigger);

        foreach (var key in baseStat.Keys)
        {
            float baseVal = baseStat[key];

            float passAdd = passive.stats.ContainsKey(key) ? passive.stats[key] : 0f;
            float passMul = passive.multifle.ContainsKey(key) ? passive.multifle[key] : 0f;

            float ctxAdd = context.stats.ContainsKey(key) ? context.stats[key] : 0f;
            float ctxMul = context.multifle.ContainsKey(key) ? context.multifle[key] : 0f;

            // 공식: (기본값 + 패시브 덧셈 + 액티브 덧셈) * (1 + 패시브 곱셈 + 액티브 곱셈)
            activeStatCache[key] = (baseVal + passAdd + ctxAdd) * (1f + passMul + ctxMul);
        }

        return activeStatCache;
    }
}
