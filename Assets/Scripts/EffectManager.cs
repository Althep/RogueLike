using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using System.Linq;
//using System.Threading.Tasks;
using UnityEngine;
using static Defines;
public class EffectManager : MonoBehaviour
{
    Dictionary<string, List<ModifierAction>> activeActionEffects = new Dictionary<string, List<ModifierAction>>();
    Dictionary<string, Queue<ModifierAction>> inactiveActionEffect = new Dictionary<string, Queue<ModifierAction>>();

    EffectDataManager effectDataManager;

    private void Awake()
    {
        Init().AsTask();
        //effectDatas.Add("TwoHanded",new TwoHanded());
        //Init();
        Debug.Log("EffectManager Init 함수 비활성화 되어있음!");
    }
    public async UniTask Init()
    {
        effectDataManager = await EffectDataManager.CreateAsync();
    }

    public async UniTask SetUp(List<TextAsset> myAssets)
    {
        
    }

    public void ReadActionData(TextAsset asset)
    {

    }
    #region 이펙트
    public ModifierAction GetActionEffectScript(string effectid)
    {
        ModifierAction action;
        if (!inactiveActionEffect.ContainsKey(effectid))
        {
            inactiveActionEffect.Add(effectid, new Queue<ModifierAction>());
        }
        if (inactiveActionEffect[effectid].Count >= 1)
        {
            action = inactiveActionEffect[effectid].Dequeue();
        }
        else
        {
            action = effectDataManager.GetAction(effectid);
        }
        if (!activeActionEffects.ContainsKey(effectid))
        {
            activeActionEffects.Add(effectid, new List<ModifierAction>());
        }
       

        activeActionEffects[effectid].Add(action);

        return action;
    }


    public void ReturnItemEffectScript(ModifierAction effect)
    {
        ActionEffectType type = effect.actionEffectType;
        string effectid = effect.effectID;
        if (!activeActionEffects.ContainsKey(effectid))
        {
            activeActionEffects.Add(effectid, new List<ModifierAction>());
        }
        if (activeActionEffects[effectid].Contains(effect))
        {
            activeActionEffects[effectid].Remove(effect);
        }
        if (!inactiveActionEffect.ContainsKey(effectid))
        {
            inactiveActionEffect.Add(effectid, new Queue<ModifierAction>());
        }
        inactiveActionEffect[effectid].Enqueue(effect);
    }


    public void GetEffectScript()
    {
        
    }
    #endregion
}
