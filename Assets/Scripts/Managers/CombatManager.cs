using UnityEngine;
using System;
using System.Collections.Generic;
using static Defines;
using UnityEditor.Experimental.GraphView;

public class CombatInfo
{
    public LivingEntity attacker;
    public LivingEntity target;
    public bool isHit;
    public bool isMissile;
    public bool isCrit;
    public int damage;


    public void Clear()
    {
        attacker = null;
        target = null;
        isHit = false;
        isMissile = false;
        isCrit = false;
        damage = 0;
    }
}
public class CombatManager : MonoBehaviour
{
    public static CombatManager instance;

    public CombatInfo info;

    private void Awake()
    {
        if(CombatManager.instance == null)
        {
            CombatManager.instance = this;
        }
        if(info == null)
        {
            info = new CombatInfo();
        }
    }
    int Random2Avg(int max)
    {
        return (UnityEngine.Random.Range(0, max) + UnityEngine.Random.Range(0, max)) / 2;
    }
    public float GetSpellDisruption(Dictionary<StatType,float> baseStat, ModifierContext context,float strenthFactor)
    { //base 45%, factor3% 추천
        float spellDisruption = baseStat[StatType.Disruption] - (baseStat[StatType.Str] - 10) * strenthFactor;
        return spellDisruption;
    }

    public int Combat(LivingEntity attacker, LivingEntity target, out bool isCrit)
    {
        info.Clear();

        ModifierContext attackerContext = attacker.GetContext(ModifierTriggerType.OnAttack);
        ModifierContext targetContext = target.GetContext(ModifierTriggerType.OnHited);

        int roll = UnityEngine.Random.Range(1, 21);
        isCrit = roll == 20;//크리티컬 계산 상시 명중
        bool isHit = false;
        if (isCrit)
        {
            isHit = true;
        }
        else
        {
            isHit = TryHit(attacker, target, out isCrit);
        }
        bool isMissile = false;
        int damage = 0;
        if (isHit)
        {
            info.target = target;
            info.attacker = attacker;
            info.isCrit = isCrit;
            info.isMissile = isMissile;

            if (isMissile)
            {
                damage = Mathf.RoundToInt(CalcuateRangeAttackDamage(attacker, target, isCrit));
            }
            else
            {
                damage = Mathf.RoundToInt(CalculateMeleeAttackDamage(attacker, target,isCrit));
            }

            info.damage = damage;

            attacker.ExcuteModifierActions(ModifierTriggerType.OnAttack);

            GetDamage(attacker, target, damage);

            target.ExcuteModifierActions(ModifierTriggerType.OnHited);

            attacker.ExcuteModifierActions(ModifierTriggerType.AfterAttack);
        }
        return damage;
    }
    public bool TryHit(LivingEntity attacker, LivingEntity target, out bool isCritical)
    {
        int roll = UnityEngine.Random.Range(1, 21);
        var atk = attacker.GetContext(ModifierTriggerType.OnAttack).stats;
        var def = target.GetContext(ModifierTriggerType.OnAttack).stats;
        
        float levelFactor = 1f + attacker.Get_MyData().GetLevel() * 0.06f;

        // 공격자의 명중력
        int acc = Mathf.Max(1, (int)((atk[StatType.Accurancy] + atk[StatType.Dex] / 4 + atk[StatType.Str] / 5) * levelFactor));

        // 방어자의 회피력
        int ev = (int)(def[StatType.Defense] + def[StatType.Dex] / 3 + def[StatType.Evasion]);
        //ev = Mathf.Min(ev, 50); // 최대값 제한

        // 랜덤 보정 (약간의 변동성)
        int hitRoll = (int)(acc * UnityEngine.Random.Range(0.85f, 1.15f));
        int evadeRoll = (int)(ev * UnityEngine.Random.Range(0.85f, 1.15f));

        // 크리티컬 처리
        isCritical = roll == 20;
        if (isCritical) return true;

        // 명중 확률 계산
        float chance = (float)(hitRoll + roll) / (hitRoll + roll + evadeRoll);
        bool isHit = UnityEngine.Random.value < chance;
        if (isHit)
        {
            if(target is MonsterEntity monster)
            {
                monster.Set_LastAttacker(attacker);
            }
        }
        // 최소 명중률 적용 (후반부도 너무 낮지 않게)
        //float minHitChance = 0.55f;
        //chance = Mathf.Max(chance, minHitChance);
        
        return isHit;
    }

    private float CalculateMeleeAttackDamage(LivingEntity attacker, LivingEntity target, bool isCritical)
    {
        Dictionary<StatType, float> attackerData = attacker.CalculateContext(ModifierTriggerType.OnAttack);
        Dictionary<StatType, float> targetData = target.CalculateContext(ModifierTriggerType.OnHited);
        ModifierContext attackerContext = attacker.GetContext(ModifierTriggerType.OnAttack);
        
        DamageType damageType = attackerContext.damageType;
        
        StatType attribute = attacker.Get_WeaponAttribueType();
        int damage = (int)attackerData[StatType.Damage];
        damage = Random2Avg(damage);
        int attributeBonus = (int)attackerData[attribute];

        switch (attribute)
        {
            case StatType.Str:
                attributeBonus /= 3;
                break;
            case StatType.Dex:
                attributeBonus /= 3;
                break;
            case StatType.Int:
                attributeBonus /= 4;
                break;
            default:
                attributeBonus /= 5;
                break;
        }

        float totalDamage = damage ;
        if (isCritical)
        {
            totalDamage *= 1.5f;
        }
        float resist = Mathf.Clamp(CalculateElementResist(target, damageType), -0.75f, 0.75f);

        // 양수 → 저항, 음수 → 약점
        totalDamage *= (1f - resist);

        totalDamage += attributeBonus;
        totalDamage -= targetData[StatType.DamageReduce];
        int randomDamage = Mathf.RoundToInt(UnityEngine.Random.Range(0, totalDamage));
        Debug.Log($"Random damage : {randomDamage}");

        return randomDamage;        
    }
    public int CalculateElementResist(LivingEntity target, DamageType damageType)
    {
        int resist = 0;
        int damageReduce = 25;
        int magicReduce = 10;
        switch (damageType)
        {
            case DamageType.Physical:
                resist = 0;
                break;
            case DamageType.Fire:
                resist = (int)target.CalculateContext(ModifierTriggerType.OnHited)[StatType.FireResist];
                break;
            case DamageType.Ice:
                resist = (int)target.CalculateContext(ModifierTriggerType.OnHited)[StatType.IceResist];
                break;
            case DamageType.Magic:
                resist = (int)target.CalculateContext(ModifierTriggerType.OnHited)[StatType.MagicResist];
                return resist * magicReduce;
            case DamageType.Thunder:
                resist = (int)target.CalculateContext(ModifierTriggerType.OnHited)[StatType.ThunderResist];
                break;
            default:
                break;
        }
        return resist * damageReduce;
    }

    public int CalcuateRangeAttackDamage(LivingEntity attacker, LivingEntity target, bool isCritical)
    {
        //Todo!
        return 0;
    }

    public void GetDamage(LivingEntity attacker,LivingEntity targetEntity,int damage)
    {
        targetEntity.GetDamage(attacker,damage);
        Debug.Log($"Excute GetDamage{targetEntity.id}");
    }

    public void CombatInfoClear()
    {
        info.Clear();
    }
}
