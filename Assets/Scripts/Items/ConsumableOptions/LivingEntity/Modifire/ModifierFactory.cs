using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using static Defines;

public class ModifierFactory
{
    private ModifierDataManager modifierDataManager;
    public static ModifierFactory _instance;
    ModifierPooler modifierPooler;

    // 생성자
    private ModifierFactory() { }

    // 비동기 팩토리 생성
    public static async UniTask<ModifierFactory> CreateAsync()
    {
        if (_instance == null)
        {
            _instance = new ModifierFactory();
            _instance.modifierDataManager = await ModifierDataManager.CreateAsync();
        }
        return _instance;
    }

    // 1. 단일 Modifier 생성 (풀러에서 호출)
    public Modifier CreateNewModifier(string id)
    {
        ModifierData modifierDatas = modifierDataManager.GetModifierData(id);
        Modifier newMod = CreateNewModifierToType(modifierDatas.modifierType);
        return newMod;
    }

    public Modifier CreateNewModifierToType(ModifierType type)
    {
        Modifier newMod = null;
        switch (type)
        {
            case ModifierType.StatModifier:
                newMod = new StatModifier();
                break;
            case ModifierType.ItemModifier:
                newMod = new ItemModifier();
                break;
            case ModifierType.BuffModifier:
                newMod = new BuffModifier();
                break;
            case ModifierType.DamageModifier:
                newMod = new DamageModifier();
                break;
            case ModifierType.ActionModifier:
                newMod = new ActionModifier();
                break;
            default:
                break;
        }
        return newMod;
    }

    // 2. ModifierOption (옵션 그룹) 생성 (풀러에서 호출)
    public ModifierOption CreatNewOption(string optionKey)
    {
        // 1. 순수하게 빈 ModifierOption 객체를 생성
        ModifierOption newOption = new ModifierOption();
        newOption.optionKey = optionKey;

        // 2. 하위 데이터 조립은 AssembleOption에 위임
        AssembleOption(newOption, optionKey);

        return newOption;
    }

    // 풀에서 꺼낸(재사용되는) ModifierOption이나 새로 생성된 ModifierOption에 
    // 하위 Modifier들을 가져와 조립하는 역할만 수행
    public void AssembleOption(ModifierOption option, string optionKey)
    {
        List<string> modIds = modifierDataManager.Get_ModifierOptionKeys(optionKey);

        if (modIds == null || modIds.Count == 0)
        {
            Debug.LogWarning($"[ModifierFactory] {optionKey}에 해당하는 모디파이어 ID가 없습니다.");
            return;
        }

        if(modifierPooler == null)
        {
            modifierPooler = GameManager.instance.Get_ModifierManager().GetModifierPooler();
        }

        // 혹시 모를 더미 데이터를 막기 위해 리스트를 비움
        option.myMods.Clear();

        // 풀러를 통해 개별 Modifier 객체들을 가져와서 현재 옵션 그룹에 조립
        // 개별 Modifier를 가져올 때 풀러를 호출하는 것은 단방향 생명주기이므로 안전함
        foreach(var id in modIds)
        {
            Modifier newMod = modifierPooler.GetModifier(id);
            if (newMod != null)
            {
                option.myMods.Add(newMod);
            }
        }
    }

    // 템플릿 기반 복사 생성 로직 (필요 시 사용)
    private Modifier GetCopyModifier(Modifier template)
    {
        switch (template.modifierType)
        {
            case ModifierType.StatModifier:
                StatModifier newStat = new StatModifier();
                if (template is StatModifier statTemplate) statTemplate.Copy(newStat);
                return newStat;

            case ModifierType.BuffModifier:
                BuffModifier newBuff = new BuffModifier();
                if (template is BuffModifier buffTemplate) buffTemplate.Copy(newBuff);
                return newBuff;

            case ModifierType.DamageModifier:
                DamageModifier newDamage = new DamageModifier();
                if (template is DamageModifier dmgTemplate) dmgTemplate.Copy(newDamage);
                return newDamage;

            case ModifierType.ActionModifier:
                ActionModifier newAction = new ActionModifier();
                if (template is ActionModifier actionTemplate) actionTemplate.Copy(newAction);
                return newAction;

            default:
                Debug.LogError($"[ModifierFactory] 정의되지 않은 ModifierType입니다: {template.modifierType}");
                return null;
        }
    }
}