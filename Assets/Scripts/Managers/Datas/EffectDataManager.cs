using Cysharp.Threading.Tasks;
using UnityEngine;
using System.Collections.Generic;
using static Defines;
public class EffectDataManager : AsyncDataManager<EffectDataManager>
{

    Dictionary<string, ModifierAction> actions = new Dictionary<string, ModifierAction>();


    public override async UniTask Init()
    {
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
    public ModifierAction GetAction(string id)
    {
        if (!actions.ContainsKey(id))
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
            Utils.TrySetValue(originData[i], "EffectName", ref actionEffect.effectName);
            Utils.TrySetValue(originData[i], "StringValue", ref actionEffect.stringValue);
            Utils.TrySetValue(originData[i], "IsMulti", ref actionEffect.isMulti);
            Utils.TrySetValue(originData[i], "Value", ref actionEffect.value);
            Utils.TryConvertEnum(originData[i], "ModifierTrigger", ref actionEffect.modifierTrigger);
            Utils.TrySetValue(originData[i], "MinTime", ref actionEffect.minTime);
            Utils.TrySetValue(originData[i], "MaxTime", ref actionEffect.maxTime);
            actionEffect.SetModifier();
            if (!actions.ContainsKey(id))
            {
                actions.Add(id, actionEffect);
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
            case ActionEffectType.BuffEffect:
                effect = new BuffEffect();
                break;
            case ActionEffectType.SlotBlock:
                effect = new SlotBlock();
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
        if (!actions.ContainsKey(id))
        {
            Debug.Log("id 포함되지않음 EffectDataManager CopyEffect");
            return null;
        }
        ModifierAction origin = actions[id];
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
        copy.SetModifier();
        if(copy is BuffEffect buff && origin is BuffEffect originBuff)
        {
            buff.modifier = originBuff.modifier;
            buff.statusEffect = originBuff.statusEffect;
        }
        Debug.Log($"{id} Effect 카피 완료 반환");
        return copy;
    }
}
