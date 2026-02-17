using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using static Defines;
public class ModifierDataManager : AsyncDataManager<ModifierDataManager>
{
    Dictionary<string, Modifier> Modifiers = new();

    EffectManager effectManager;

    protected ModifierDataManager() { }

    public override async UniTask Init()
    {
        if (effectManager == null)
        {
            effectManager = GameManager.instance.Get_EffectManager();
        }
    }

    public async UniTask SetUp(List<TextAsset> myAssets)
    {
        Debug.Log($"ModifierDataManager {myAssets.Count}개의 에셋 입력!");
        foreach (var asset in myAssets)
        {
            List<Dictionary<string, object>> originData = Utils.TextAssetParse(asset);
            Debug.Log($"{asset.name} 에셋 리딩!");
            if (asset.name.Contains("Stat"))
            {
                Debug.Log($"{asset.name}파일 읽음");
                await ReadStatModifier(originData);
            }
            else if (asset.name.Contains("Item"))
            {
                await ReadItemModifier(originData);
            }
            else if (asset.name.Contains("Damage"))
            {
                await ReadDamageModifier(originData);
            }
            else if (asset.name.Contains("Buff"))
            {
                await ReadBuffModifier(originData);
            }
            else if (asset.name.Contains("Action"))
            {
                await ReadActionModifier(originData);
            }
            else if (asset.name.Contains("EquipOptions"))
            {
                Debug.Log("ModifeirDataManager  장비 옵션 모디파이어 리딩 시작!");
                await ReadEquipOptions(originData);
            }
            else if (asset.name.Contains("ConsumOptions"))
            {
                Debug.Log("ModifierDataManager 소모품 모디파이어 리딩 시작");
                await ReadConsumData(originData);
            }
        }
    }

    public Modifier GetEmptyModifier(ModifierType type, string id)
    {
        if (Modifiers.ContainsKey(id))
        {
            Debug.Log("모디파이어 id 포함되어있음");
            return null;
        }
        switch (type)
        {
            case ModifierType.StatModifier:
                StatModifier newStat = new StatModifier();
                Modifiers.Add(id, newStat);
                return newStat;

            case ModifierType.BuffModifier:
                BuffModifier newBuff = new BuffModifier();
                Modifiers.Add(id, newBuff);
                return newBuff;
            case ModifierType.DamageModifier:
                DamageModifier newDamage = new DamageModifier();
                Modifiers.Add(id, newDamage);
                return newDamage;
            case ModifierType.ActionModifier:
                ActionModifier newAction = new ActionModifier();
                Modifiers.Add(id, newAction);
                return newAction;
            case ModifierType.ItemModifier:
                ItemModifier newItem = new ItemModifier();
                Modifiers.Add(id, newItem);
                return newItem;
            default:
                Debug.Log("StatType Error in GetEmptyModifier_ModifierFactory");
                return null;
        }
    }

    public Modifier Get_Modifier(string id)
    {
        if (!Modifiers.ContainsKey(id))
        {
            Debug.Log($"ModifierData Don't Contain{id}");
        }
        ModifierType type = Modifiers[id].modifierType;

        switch (type)
        {
            case ModifierType.StatModifier:
                StatModifier newStat = new StatModifier();
                Modifiers[id].Copy(newStat);
                return newStat as StatModifier;
            case ModifierType.ItemModifier:
                ItemModifier newItem = new ItemModifier();
                Modifiers[id].Copy(newItem);
                return newItem as ItemModifier;
            case ModifierType.BuffModifier:
                BuffModifier newBuff = new BuffModifier();
                Modifiers[id].Copy(newBuff);
                return newBuff as BuffModifier;
            case ModifierType.DamageModifier:
                DamageModifier newDamage = new DamageModifier();
                Modifiers[id].Copy(newDamage);
                return newDamage as DamageModifier;
            case ModifierType.ActionModifier:
                ActionModifier newAction = new ActionModifier();
                Modifiers[id].Copy(newAction);
                return newAction as ActionModifier;
            default:
                Debug.Log("StatType Error in Get_Modifier_ModifierFactory");
                return null;
        }
    }





    public void ReadModifier(Dictionary<string, object> data, Modifier modifier)
    {
        Utils.TrySetValue<string>(data, "ID", ref modifier.id);
        Utils.TryConvertEnum<StatType>(data, "StatType", ref modifier.stat);
        Utils.TryConvertEnum<ModifierTriggerType>(data, "ModifierTrigger", ref modifier.triggerType);
        Utils.TrySetValue<int>(data, "Priority", ref modifier.priority);
        Utils.TrySetValue<bool>(data, "isMulti", ref modifier.isMulti);
        Utils.TrySetValue<float>(data, "Value", ref modifier.value);
    }
    public void ReadItemModifier(Dictionary<string, object> data, Modifier modifier)
    {
        Utils.TrySetValue<string>(data, "ID", ref modifier.id);
        if (Utils.TryConvertEnum<StatType>(data, "StringValue", ref modifier.stat))
        {

        }
        else
        {
            if (modifier is BuffModifier buff)
            {
                Utils.StringToEnum<BuffCategory>(data["BuffType"].ToString(), ref buff.buffCategory);
                effectManager.GetActionEffectScript(data["EffectID"].ToString());
            }
            else if (modifier is ActionModifier action)
            {
                effectManager.GetActionEffectScript(data["EffectID"].ToString());
            }
        }
        Utils.TryConvertEnum<ModifierTriggerType>(data, "ModifierTrigger", ref modifier.triggerType);
        Utils.TrySetValue<int>(data, "Priority", ref modifier.priority);
        Utils.TrySetValue<bool>(data, "isMulti", ref modifier.isMulti);
        Utils.TrySetValue<float>(data, "Value", ref modifier.value);
    }
    public async UniTask ReadStatModifier(List<Dictionary<string, object>> originData)
    {
        for (int i = 0; i < originData.Count; i++)
        {
            string id = originData[i]["ID"].ToString();
            if (Modifiers.ContainsKey(id))
            {
                continue;
            }
            StatType statType = StatType.MaxHP;
            StatModifier stat = new StatModifier();
            stat.stat = statType;
            stat.poolType = PoolScriptType.StatModifier;
            stat.id = id;
            ReadModifier(originData[i], stat);
            Modifiers.Add(id, stat);
            Debug.Log($"{Modifiers[id].id} 등록 완료");
            await Utils.WaitYield(i);
        }
    }
    public async UniTask ReadActionModifier(List<Dictionary<string, object>> originData)
    {
        for (int i = 0; i < originData.Count; i++)
        {
            string id = originData[i]["ID"].ToString();
            if (Modifiers.ContainsKey(id))
            {
                continue;
            }
            ModifierType type = ModifierType.StatModifier;
            if (Utils.StringToEnum(originData[i]["ModifierType"].ToString(), ref type))
            {
                Modifier newModifier = GetEmptyModifier(type, id);
                ReadItemModifier(originData[i], newModifier);
            }
            else
            {
                Debug.Log("모디파이어 타입 변환 실패! ModifierFactory ReadConsumEffectModifiers");
                continue;
            }
            await Utils.WaitYield(i);
        }
    }
    public async UniTask ReadItemModifier(List<Dictionary<string, object>> originData)
    {
        for (int i = 0; i < originData.Count; i++)
        {
            string id = originData[i]["ID"].ToString();
            if (Modifiers.ContainsKey(id))
            {
                continue;
            }
            ItemModifier item = new ItemModifier();
            item.poolType = PoolScriptType.ItemModifier;
            item.id = id;
            ReadModifier(originData[i], item);
            Utils.TryConvertEnum<ItemTargetType>(originData[i], "ItemTargetType", ref item.itemTargetType);
            Utils.TryConvertEnum<ItemCategory>(originData[i], "ItemCategory", ref item.itemCategory);
            Utils.TrySetValue<bool>(originData[i], "UnEquipable", ref item.unEquipable);
            item.specificType = Utils.Get_ItemSpecificType(originData[i].ToString());
            Modifiers.Add(id, item);
            await Utils.WaitYield(i);
        }
    }

    public async UniTask ReadEquipOptions(List<Dictionary<string, object>> originData)
    {
        for (int i = 0; i < originData.Count; i++)
        {
            string id = originData[i]["ID"].ToString();
            ModifierType modiType = ModifierType.StatModifier;
            Utils.TryConvertEnum<ModifierType>(originData[i], "ModifierType", ref modiType);

            if (Modifiers.ContainsKey(id))
            {
                continue;
            }
            switch (modiType)
            {
                case ModifierType.StatModifier:
                    StatModifier newStat = new StatModifier();
                    newStat.id = id;
                    Utils.TryConvertEnum<StatType>(originData[i], "StringValue", ref newStat.stat);
                    Utils.TrySetValue<bool>(originData[i], "IsMulti", ref newStat.isMulti);
                    Utils.TryConvertEnum<ModifierTriggerType>(originData[i], "ModifierTrigger", ref newStat.triggerType);
                    Utils.TrySetValue<int>(originData[i], "Priority", ref newStat.priority);
                    Modifiers.Add(id, newStat);
                    Debug.Log($"{newStat.id} Modifier 등록!");
                    break;
                case ModifierType.BuffModifier:
                    BuffModifier newBuff = new BuffModifier();
                    break;
                case ModifierType.DamageModifier:
                    DamageModifier newDamager = new DamageModifier();
                    break;
                case ModifierType.ActionModifier:
                    ActionModifier newAction = new ActionModifier();
                    newAction.id = id;
                    //Utils.TrySetValue<StatType>(originData[i], "StatType", ref newAction.stat);
                    string actionName = null;
                    Utils.TrySetValue(originData[i], "ActionName", ref actionName);
                    Utils.TrySetValue<bool>(originData[i], "IsMulti", ref newAction.isMulti);
                    Utils.TryConvertEnum<ModifierTriggerType>(originData[i], "ModifierTrigger", ref newAction.triggerType);
                    Utils.TrySetValue(originData[i], "Priority", ref newAction.priority);
                    ModifierAction action = effectManager.GetActionEffectScript(actionName);

                    Modifiers.Add(id, newAction);
                    Debug.Log($"{newAction.id} Modifier 등록!");
                    break;
                case ModifierType.ItemModifier:
                    ItemModifier newItem = new ItemModifier();
                    Utils.TrySetValue(originData[i], "ID", ref newItem.id);
                    break;
                default:
                    break;
            }
            await Utils.WaitYield(i);
        }
    }

    async UniTask ReadConsumData(List<Dictionary<string, object>> originData)
    {
        for (int i = 0; i < originData.Count; i++)
        {
            string id = originData[i]["ID"].ToString();
            Debug.Log($"소모 아이템 모디파이어 {id} 리드");
            string name = originData[i]["Name"].ToString();
            ModifierType mod_Type = Utils.ConvertToEnum<ModifierType>(originData[i]["ModifierType"].ToString());
            if (Modifiers.ContainsKey(id))
            {
                continue;
            }
            switch (mod_Type)
            {
                case ModifierType.StatModifier:
                    break;
                case ModifierType.ItemModifier:
                    break;
                case ModifierType.BuffModifier:
                    BuffModifier buff = GetEmptyModifier(mod_Type, id)as BuffModifier;
                    Utils.TrySetValue(originData[i], "Value", ref buff.value);
                    Utils.TrySetValue(originData[i], "StringValue", ref buff.stringValue);
                    Utils.TrySetValue(originData[i], "MinTime", ref buff.minTime);
                    Utils.TrySetValue(originData[i], "MaxTime", ref buff.maxTime);
                    Utils.TryConvertEnum<BuffType>(originData[i], "BuffType", ref buff.buffType);
                    Utils.TrySetValue(originData[i], "EffectName", ref buff.effectName);
                    if (Utils.TryConvertEnum<StatType>(originData[i], "StringValue", ref buff.stat))
                    {

                    }
                    else
                    {
                        string effectName_ = buff.effectName;
                        ModifierAction mod_Action = effectManager.GetActionEffectScript(effectName_);
                        buff.SetMyAction(mod_Action);
                    }
                    
                    break;
                case ModifierType.DamageModifier:
                    break;
                case ModifierType.ActionModifier:
                    ActionModifier action = GetEmptyModifier(mod_Type, id) as ActionModifier;
                    Utils.TrySetValue(originData[i], "EffectName", ref action.effectName);
                    Debug.Log($"EffectName : {action.effectName}");
                    ActionEffectType effectType = Utils.ConvertToEnum<ActionEffectType>(originData[i]["EffectName"].ToString());
                    string effectName = action.stringValue;
                    ModifierAction modAction = effectManager.GetActionEffectScript(action.effectName);
                    Utils.TrySetValue(originData[i], "StringValue", ref action.stringValue);
                    Utils.TrySetValue(originData[i], "Value", ref action.value);
                    Utils.TrySetValue(originData[i], "MinTime", ref action.minValue);
                    Utils.TrySetValue(originData[i], "MaxTime", ref action.maxValue);
                    action.SetAction(modAction);
                    break;
                default:
                    break;
            }

            Debug.Log($"{id} 리드 완료");
            await Utils.WaitYield(i);

        }
    }
    public async UniTask ReadBuffModifier(List<Dictionary<string, object>> originData)
    {
        for (int i = 0; i < originData.Count; i++)
        {

            string id = originData[i]["ID"].ToString();
            if (Modifiers.ContainsKey(id))
            {
                continue;
            }
            Debug.Log($"버프 모디파이어 {id} 등록중");
            BuffModifier buff = new BuffModifier();
            buff.poolType = PoolScriptType.BuffModifier;
            buff.id = id;
            string effectName = originData[i]["EffectName"].ToString();
            ReadModifier(originData[i], buff);
            Utils.TrySetValue(originData[i], "StringValue", ref buff.stringValue);
            Utils.TryConvertEnum(originData[i], "BuffType", ref buff.buffCategory);
            Utils.TrySetValue(originData[i], "MinTime", ref buff.minTime);
            Utils.TrySetValue(originData[i], "MaxTime", ref buff.maxTime);
            Utils.TryConvertEnum(originData[i], "BuffType", ref buff.buffType);
          
            switch (buff.buffType)
            {
                case BuffType.StatBuff:
                    Utils.TryConvertEnum<StatType>(originData[i], "StringValue", ref buff.stat);
                    break;
                case BuffType.ActionBuff:
                    ModifierAction action = effectManager.GetActionEffectScript(effectName);
                    action.stringValue = buff.stringValue;
                    action.SetValueToString();
                    break;
                default:
                    break;
            }
            Modifiers.Add(id, buff);
            Debug.Log($"버프 모디파이어 {id} 등록 완료");
            await Utils.WaitYield(i);
        }
    }
    public async UniTask ReadDamageModifier(List<Dictionary<string, object>> originData)
    {
        for (int i = 0; i < originData.Count; i++)
        {
            string id = originData[i]["ID"].ToString();
            if (Modifiers.ContainsKey(id))
            {
                continue;
            }
            DamageModifier damage = new DamageModifier();
            damage.poolType = PoolScriptType.ItemModifier;
            damage.id = id;
            ReadModifier(originData[i], damage);
            Utils.TryConvertEnum<DamageType>(originData[i], "DamageType", ref damage.damageType);
            Utils.TrySetValue<float>(originData[i], "EvokeRate", ref damage.evokeRate);
            Modifiers.Add(id, damage);
            await Utils.WaitYield(i);
        }
    }


    public Modifier GetModifier(string id)
    {
        return Modifiers.GetValueOrDefault(id);
    }


}
