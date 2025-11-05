using UnityEngine;
using System;
using System.Collections.Generic;
using static Defines;
public class CombatManager : MonoBehaviour
{
    int Random2Avg(int max)
    {
        return (UnityEngine.Random.Range(0, max) + UnityEngine.Random.Range(0, max)) / 2;
    }
    public float GetSpellDisruption(Dictionary<StatType,float> baseStat, ModifierContext context,float strenthFactor)
    { //base 45%, factor3% 추천
        float spellDisruption = baseStat[StatType.Disruption] - (baseStat[StatType.Str] - 10) * strenthFactor;
        return spellDisruption;
    }
    /*
    public bool TryHit(LivingEntity attacker, LivingEntity target, out bool isCritical)
    {
        int roll = UnityEngine.Random.Range(1, 21);
        Dictionary<StatType,float> attackBonus = attacker.GetAttackBonus(); // 종합스탯정보 공격자의 명중등을 참조
        Dictionary<StatType,float> defense = target.GetDefenseBonus();           // 방어자의 종합 스탯정보

        int accurancy = (int)(attackBonus[StatType.Accuracy]+(attackBonus[StatType.Dex]/3)+(attackBonus[StatType.Str]/5));
        accurancy = Random2Avg(accurancy);
        int ac = (int)(defense[StatType.Defense]);
        ac = Random2Avg(ac);
        isCritical = (roll == 20);
        if (isCritical) return true; // 자동명중

        return (roll + accurancy) >= ac;
    }*/
    public bool TryHit(LivingEntity attacker, LivingEntity target, out bool isCritical)
    {
        int roll = UnityEngine.Random.Range(1, 21);
        var atk = attacker.GetEntityStat(ModifierTriggerType.OnAttack);
        var def = target.GetEntityStat(ModifierTriggerType.OnHit);

        float levelFactor = 1f + attacker.Get_MyData().GetLevel() * 0.06f;

        // 공격자의 명중력
        int acc = Mathf.Max(1, (int)((atk[StatType.Accuracy] + atk[StatType.Dex] / 4 + atk[StatType.Str] / 5) * levelFactor));

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

        // 최소 명중률 적용 (후반부도 너무 낮지 않게)
        //float minHitChance = 0.55f;
        //chance = Mathf.Max(chance, minHitChance);

        return UnityEngine.Random.value < chance;
    }

    public float CalculateMeleeAttackDamage(LivingEntity attacker, LivingEntity target, bool isCritical)
    {

        Dictionary<StatType, float> attackerData = attacker.GetEntityStat(ModifierTriggerType.OnAttack);
        Dictionary<StatType, float> targetData = target.GetEntityStat(ModifierTriggerType.OnHit);
        ModifierContext attackerContext = attacker.Get_MeleeAttackContext();
        
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
        float resist = Mathf.Clamp(CalculateElementResist(target, damageType), -1f, 0.75f);

        // 양수 → 저항, 음수 → 약점
        totalDamage *= (1f - resist);

        totalDamage += attributeBonus;
        totalDamage -= targetData[StatType.DamageReduce];

        return Mathf.Max(0, totalDamage);        
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
                resist = (int)target.GetEntityStat(ModifierTriggerType.OnHit)[StatType.FireResist];
                break;
            case DamageType.Ice:
                resist = (int)target.GetEntityStat(ModifierTriggerType.OnHit)[StatType.IceResist];
                break;
            case DamageType.Magic:
                resist = (int)target.GetEntityStat(ModifierTriggerType.OnHit)[StatType.MagicResist];
                return resist * magicReduce;
            case DamageType.Thunder:
                resist = (int)target.GetEntityStat(ModifierTriggerType.OnHit)[StatType.ThunderResist];
                break;
            default:
                break;
        }
        return resist * damageReduce;
    }



}
