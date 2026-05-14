using UnityEngine;
using System;
using System.Collections.Generic;
using static Defines;
using System.Xml;

[Serializable]
public class EntityStat
{
    #region Variables & Properties
    protected TileType tileType;
    protected Dictionary<StatType, float> baseStat = new();

    // 패시브 캐싱용 바구니와 액티브 계산용 바구니 분리
    protected Dictionary<StatType, float> finalStat = new();
    protected Dictionary<StatType, float> activeStatCache = new();

    protected bool isDirty = true;
    protected int level;
    protected string entityId;
    protected string entityName;
    protected bool isDead;

    protected ModifierController modifierController;
    protected LivingEntity myEntity;

    public Action<StatType,float> OnStatChange;
    public Action OnDeadActions;
    #endregion

    #region Constructor & Initialization
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
        // 초기화 시 HP는 MaxHP를 따라가도록 설정 (기획에 따라 변경 가능)
        baseStat[StatType.HP] = baseStat[StatType.MaxHP];
    }

    public void Set_ModifierController(ModifierController mc) => modifierController = mc;
    public void Set_MyEntity(LivingEntity entity) => myEntity = entity;
    #endregion

    #region Basic Getters & Setters
    public void SetID(string id) => entityId = id;
    public string GetId() => entityId;

    public void SetName(string Name) => entityName = Name;
    public string GetName() => entityName;

    public void SetTileType(TileType type) => tileType = type;
    public TileType GetTileType() => tileType;

    public int GetLevel() => level;
    public void Add_Level() => level++;

    public bool IsDead() => isDead;
    public void MarksAsDirty() => isDirty = true;

    public Dictionary<StatType, float> GetBase() => baseStat;
    public float GetBaseStat(StatType stat) => baseStat.ContainsKey(stat) ? baseStat[stat] : 0f;
    #endregion

    #region Base Stat Management
    public void SetBaseStat(StatType type, float value)
    {
        if (baseStat.ContainsKey(type))
            baseStat[type] = value;
        else
            baseStat.Add(type, value);

        OnStatChange?.Invoke(type,value);
    }

    public void AddBaseStat(StatType type, float value)
    {
        if (baseStat.ContainsKey(type))
            baseStat[type] += value;
        else
            baseStat.Add(type, value);

        OnStatChange?.Invoke(type,value);
    }

    /// <summary>
    /// 스탯 수치 변화 시 로직 처리 (HP/MP 제한 및 레벨업 체크 등)
    /// </summary>
    public virtual void AddingStat(StatType type, float value)
    {
        AddBaseStat(type, value);

        // 실시간 값 보정 및 상태 체크
        switch (type)
        {
            case StatType.HP:
                float maxHP = GetBaseStat(StatType.MaxHP);
                if (baseStat[type] > maxHP) SetBaseStat(type, maxHP);
                else if (baseStat[type] <= 0) isDead = true;
                break;

            case StatType.MP:
                float maxMP = GetBaseStat(StatType.MaxMP);
                if (baseStat[type] > maxMP) SetBaseStat(type, maxMP);
                else if (baseStat[type] <= 0) baseStat[type] = 0;
                break;

            case StatType.Exp:
                if (GetBaseStat(StatType.MaxExp) < 1) return;
                // TODO: 레벨업 로직 추가 필요 시 작성
                break;
        }

        isDead = (GetBaseStat(StatType.HP) <= 0);
        OnStatChange?.Invoke(type,value);
    }
    #endregion

    #region Core Calculation Logic (Final Stat)
    public Dictionary<StatType, float> CalculateContext(ModifierTriggerType trigger)
    {
        // 1. 패시브 요청이고 데이터가 깨끗하면 바로 캐시(finalStat) 반환
        if (!isDirty && trigger == ModifierTriggerType.Passive)
        {
            return finalStat;
        }

        // 패시브 컨텍스트 로드
        ModifierContext passive = modifierController.ApplyModifiers(ModifierTriggerType.Passive);

        // 2. 데이터가 Dirty 상태면 패시브 캐시 재계산
        if (isDirty)
        {
            foreach (var key in baseStat.Keys)
            {
                float baseVal = baseStat[key];
                float passAdd = passive.stats.TryGetValue(key, out float add) ? add : 0f;
                float passMul = passive.multifle.TryGetValue(key, out float mul) ? mul : 0f;

                finalStat[key] = (baseVal + passAdd) * (1f + passMul);
            }
            isDirty = false;
        }

        // 3. 패시브 요청이었다면 계산된 finalStat 반환
        if (trigger == ModifierTriggerType.Passive) return finalStat;

        // 4. 액티브 트리거(공격 등) 요청일 경우 별도 캐시 사용
        ModifierContext context = modifierController.ApplyModifiers(trigger);
        foreach (var key in baseStat.Keys)
        {
            float baseVal = baseStat[key];

            float passAdd = passive.stats.TryGetValue(key, out float pAdd) ? pAdd : 0f;
            float passMul = passive.multifle.TryGetValue(key, out float pMul) ? pMul : 0f;

            float ctxAdd = context.stats.TryGetValue(key, out float cAdd) ? cAdd : 0f;
            float ctxMul = context.multifle.TryGetValue(key, out float cMul) ? cMul : 0f;

            // 공식: (기본값 + 패시브합 + 액티브합) * (1 + 패시브곱 + 액티브곱)
            activeStatCache[key] = (baseVal + passAdd + ctxAdd) * (1f + passMul + ctxMul);
        }

        return activeStatCache;
    }
    #endregion

    #region Battle Calculation
    public int GetDamageToResist(DamageType type, int damage)
    {
        // 0으로 나누기 방지 및 최소 저항값 1 보장
        float SafeResist(StatType resType) => Mathf.Max(1f, finalStat[resType]);

        switch (type)
        {
            case DamageType.Fire: damage = (int)(damage * (1f / SafeResist(StatType.FireResist))); break;
            case DamageType.Ice: damage = (int)(damage * (1f / SafeResist(StatType.IceResist))); break;
            case DamageType.Magic: damage = (int)(damage * (1f / SafeResist(StatType.MagicResist))); break;
            case DamageType.Thunder: damage = (int)(damage * (1f / SafeResist(StatType.ThunderResist))); break;
            case DamageType.Physical:
            case DamageType.Poison:
            default: break;
        }
        return damage;
    }
    #endregion

    #region Utility
    public virtual EntityStat CopyStat()
    {
        EntityStat newStat = new EntityStat();
        foreach (StatType stat in baseStat.Keys)
        {
            newStat.SetBaseStat(stat, baseStat[stat]);
        }
        newStat.level = level;
        newStat.SetName(entityName);
        newStat.SetID(entityId);
        newStat.SetTileType(tileType);
        return newStat;
    }
    #endregion
}