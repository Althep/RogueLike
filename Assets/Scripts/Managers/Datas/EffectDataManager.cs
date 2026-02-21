using Cysharp.Threading.Tasks;
using UnityEngine;
using System.Collections.Generic;
using static Defines;
public class EffectDataManager : AsyncDataManager<EffectDataManager>
{

    Dictionary<ActionEffectType, ModifierAction> actions = new Dictionary<ActionEffectType, ModifierAction>();
    Dictionary<string, ModifierAction> dataActions = new Dictionary<string, ModifierAction>();
    ModifierPooler modifierPooler;
    public override async UniTask Init()
    {
        ActionBaseCreate();
        Debug.Log("EffectManager Init!");
    }

    public async UniTask SetUp(List<TextAsset> assets)
    {
        Debug.Log($"EffectDataManager로 데이터 전달 {assets.Count}");
        foreach (var asset in assets)
        {
            Debug.Log($"EffectDataManager 에셋 전달  {asset.name}");
            List<Dictionary<string, object>> originData = Utils.TextAssetParse(asset);
            if (asset.name.Contains("ActionData"))
            {
                Debug.Log("SetEffects Start!");
                await SetEffects(originData);
            }
        }
    }

    public void ActionBaseCreate()
    {
        ActionEffectType[] types = Utils.Get_Enums<ActionEffectType>(ActionEffectType.Ghost);
        if(modifierPooler == null)
        {
            modifierPooler = GameManager.instance.Get_ModifierManager().GetModifierPooler();
        }
        foreach(var type in types)
        {
            ModifierAction action = CreateNewEffect(type);
            if (!actions.ContainsKey(type))
            {
                actions.Add(type, action);
                dataActions.Add(type.ToString(), action);
            }
        }
    }
    public ModifierAction GetAction(string id)
    {
        if (!dataActions.ContainsKey(id))
        {
            Debug.Log("키 포함되지않음! EffectDataManager");
            return null;
        }
        Debug.Log($"Effect ID : {id} EffectDataManager에 요청!");
        ModifierAction copy = CopyEffect(id);


        return copy;

    }
    

    async UniTask SetEffects(List<Dictionary<string, object>> originData)
    {
        for (int i = 0; i < originData.Count; i++)
        {
            ActionEffectType type = Utils.ConvertToEnum<ActionEffectType>(originData[i]["ActionType"].ToString());
            string id = originData[i]["ID"].ToString();
            ModifierAction actionEffect = CreateNewEffect(type);
            Utils.TryConvertEnum(originData[i], "ActionType", ref actionEffect.actionEffectType);
            Utils.TrySetValue(originData[i], "ID", ref actionEffect.effectID);
            Utils.TrySetValue(originData[i], "StringValue", ref actionEffect.stringValue);
            Utils.TrySetValue(originData[i], "IsMulti", ref actionEffect.isMulti);
            Utils.TrySetValue(originData[i], "Value", ref actionEffect.value);
            Utils.TryConvertEnum(originData[i], "ModifierTrigger", ref actionEffect.modifierTrigger);
            if (!dataActions.ContainsKey(id))
            {
                dataActions.Add(id, actionEffect);
            }
            Debug.Log($"Effect ID : {id} 등록 완료");
            await Utils.WaitYield(i);
        }
    }

    public ModifierAction CreateNewEffect(ActionEffectType type)
    {
        ModifierAction effect;
        switch (type)
        {
            case ActionEffectType.StatAdd:
                effect = new StatAdd();
                break;
            case ActionEffectType.StatBuff:
                effect = new StatBuff();
                break;
            case ActionEffectType.SlotBlock:
                effect = new SlotBlock();
                break;
            case ActionEffectType.SlotAdd:
                effect = new SlotAdd();
                break;
            case ActionEffectType.Cure:
                effect = new Cure();
                break;
            case ActionEffectType.GetDamage:
                effect = new GetDamage();
                break;
            case ActionEffectType.Confuse:
                effect = new Confuse();
                break;
            case ActionEffectType.Invisible:
                break;
            case ActionEffectType.Ghost:
                break;
            case ActionEffectType.LifeSteel:
                break;
            case ActionEffectType.Stun:
                break;
            case ActionEffectType.TurnSkip:
                break;
            case ActionEffectType.Blink:
                break;
            case ActionEffectType.Telleport:
                break;
            case ActionEffectType.DamageRange:
                break;
            case ActionEffectType.Xray:
                break;
            case ActionEffectType.Sleep:
                break;
            case ActionEffectType.Silence:
                break;
            case ActionEffectType.Wish:
                break;
            case ActionEffectType.Void:
                break;
            case ActionEffectType.EssenceExtract:
                break;
            default:
                break;
        }
        switch (type)
        {
            case ActionEffectType.StatAdd:
                effect = new StatAdd();
                break;
            case ActionEffectType.StatBuff:
                effect = new StatBuff();
                break;
            case ActionEffectType.SlotBlock:
                effect = new SlotBlock();
                break;
            case ActionEffectType.Cure:
                effect = new Cure();
                break;
            case ActionEffectType.Ghost:
                effect = new Ghost();
                break;
            case ActionEffectType.GetDamage:
                effect = new GetDamage();
                break;
            case ActionEffectType.Confuse:
                effect = new Confuse();
                break;
            case ActionEffectType.Invisible:
                effect = new Invisible();
                break;
            case ActionEffectType.LifeSteel:
                effect = new LifeSteel();
                break;
            case ActionEffectType.SlotAdd:
                effect = new SlotAdd();
                break;
            default:
                Debug.Log("Default! new StatAdd");
                effect = new StatAdd();
                break;
        }
        
        return effect;
    }

    ModifierAction CopyEffect(string id)
    {
        if (!dataActions.ContainsKey(id))
        {
            Debug.Log("id 포함되지않음 EffectDataManager CopyEffect");
            return null;
        }
        ModifierAction origin = dataActions[id];
        ModifierAction copy = CreateNewEffect(origin.actionEffectType);

        copy.effectID = origin.effectID;
        copy.effectName = origin.effectName;
        copy.stringValue = origin.stringValue;
        copy.value = origin.value;
        copy.isMulti = origin.isMulti;
        copy.modifierTrigger = origin.modifierTrigger;
        copy.actionEffectType = origin.actionEffectType;
        copy.minTime = origin.minTime;
        copy.maxTime = origin.maxTime;
        if(copy is StatBuff buff && origin is StatBuff originBuff)
        {
            buff.modifier = (BuffModifier)modifierPooler.GetModifier(originBuff.modifier.modifierType,originBuff.modifier.id);
        }
        Debug.Log($"{id} Effect 카피 완료 반환");
        return copy;
    }
}
