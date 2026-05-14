using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using static Defines;

public class ModifierController
{
    private Dictionary<ModifierTriggerType, List<Modifier>> buffs = new Dictionary<ModifierTriggerType, List<Modifier>>();
    private Dictionary<ModifierTriggerType, List<Modifier>> mutation = new Dictionary<ModifierTriggerType, List<Modifier>>();
    private Dictionary<ModifierTriggerType, List<Modifier>> equips = new Dictionary<ModifierTriggerType, List<Modifier>>();
    private Dictionary<ModifierTriggerType, ModifierContext> contexts = new Dictionary<ModifierTriggerType, ModifierContext>();

    
    private Dictionary<ModifierTriggerType, bool> dirtyFlags = new Dictionary<ModifierTriggerType, bool>();
    // 지연 갱신(Lazy Evaluation)을 위한 더티 플래그

    LivingEntity myEntity;

    public Action<Modifier> OnModifierChanged;

    public void InitAllContext(LivingEntity entity)
    {
        ModifierTriggerType[] triggers = Utils.Get_Enums<ModifierTriggerType>();

        foreach (ModifierTriggerType type in triggers)
        {
            if (!contexts.ContainsKey(type))
            {
                contexts.Add(type, new ModifierContext());
                contexts[type].InitContext(type);
                dirtyFlags.Add(type, false);
            }
        }
        SetMyEntity(entity);
    }

    public void SetMyEntity(LivingEntity entity)
    {
        myEntity = entity;
    }

    // ==========================================
    // 1. 모디파이어 추가/제거 (플래그만 변경)
    // ==========================================
    private void AddModifierInternal(Dictionary<ModifierTriggerType, List<Modifier>> targetDict, Modifier modifier)
    {
        ModifierTriggerType trigger = modifier.triggerType;

        if (!targetDict.ContainsKey(trigger))
        {
            targetDict.Add(trigger, new List<Modifier>());
        }

        targetDict[trigger].Add(modifier);
        targetDict[trigger].Sort((a, b) => a.priority.CompareTo(b.priority));

        // 재계산 없이 플래그만 오염
        dirtyFlags[trigger] = true;
        //myEntity.OnStatOrModifierChange();
        OnModifierChanged?.Invoke(modifier);
    }

    private void RemoveModifierInternal(Dictionary<ModifierTriggerType, List<Modifier>> targetDict, Modifier modifier)
    {
        if (modifier == null || !targetDict.ContainsKey(modifier.triggerType)) return;

        ModifierTriggerType trigger = modifier.triggerType;

        if (targetDict[trigger].Remove(modifier))
        {
            dirtyFlags[trigger] = true;
        }
        //myEntity.OnStatOrModifierChange();
        OnModifierChanged?.Invoke(modifier);
        ModifierManager.instance.Return_Modifier(modifier);
    }

    public void AddBuff(Modifier modifier) => AddModifierInternal(buffs, modifier);
    public void AddEquipment(Modifier modifier) => AddModifierInternal(equips, modifier);
    public void AddMutation(Modifier modifier) => AddModifierInternal(mutation, modifier);

    public void RemoveBuff(Modifier modifier) => RemoveModifierInternal(buffs, modifier);
    public void RemoveEquipment(Modifier modifier) => RemoveModifierInternal(equips, modifier);
    public void RemoveMutate(Modifier modifier) => RemoveModifierInternal(mutation, modifier);

    // ==========================================
    // 2. 컨텍스트 획득 (패시브 지연 갱신 처리)
    // ==========================================
    public ModifierContext Get_Context(ModifierTriggerType trigger)
    {
        if (!contexts.TryGetValue(trigger, out ModifierContext context))
        {
            context = new ModifierContext();
            context.InitContext(trigger);
            contexts.Add(trigger, context);
            dirtyFlags[trigger] = true;
        }

        // [패시브 전용] 캐싱 로직: 스탯 요구 시점에 더티 플래그가 true면 단 한 번만 계산
        if (trigger == ModifierTriggerType.Passive)
        {
            if (dirtyFlags.TryGetValue(trigger, out bool isDirty) && isDirty)
            {
                context.Clear();
                RefreshPassiveModifiers();
                dirtyFlags[trigger] = false; // 캐싱 완료
            }
        }

        return context;
    }

    private void RefreshPassiveModifiers()
    {
        ApplyPassivesFrom(mutation);
        ApplyPassivesFrom(equips);
        ApplyPassivesFrom(buffs);
    }

    private void ApplyPassivesFrom(Dictionary<ModifierTriggerType, List<Modifier>> targetDict)
    {
        if (targetDict.TryGetValue(ModifierTriggerType.Passive, out var list) && list.Count > 0)
        {
            foreach (var modifier in list)
            {
                modifier.Apply(myEntity);
            }
        }
    }

    // ==========================================
    // 3. 액티브 발동 (매 이벤트마다 초기화 후 장전)
    // ==========================================
    public ModifierContext ApplyModifiers(ModifierTriggerType type)
    {
        
        ModifierContext context = Get_Context(type);

        // 1. 패시브는 Get_Context에서 이미 지연 갱신이 완료됨
        if (type == ModifierTriggerType.Passive)
        {
            return context;
        }

        // 2. 액티브 타입 지연 갱신 적용
        if (dirtyFlags.TryGetValue(type, out bool isDirty) && isDirty)
        {
            // 플래그가 오염되었을 때만 그릇을 비우고 재계산
            context.Clear();

            bool hasActionConsumed = false;

            // RemoveAll의 결과(소모 여부)를 받아옵니다.
            hasActionConsumed |= ApplyActiveModifiersFrom(mutation, type);
            hasActionConsumed |= ApplyActiveModifiersFrom(equips, type);
            hasActionConsumed |= ApplyActiveModifiersFrom(buffs, type);

            // 핵심 로직: 
            // 1회성 아이템이 소모되었다면 다음 번엔 그 액션을 빼고 계산해야 하므로 true 유지.
            // 소모된 게 없다면 일반 장비/버프만 있는 것이므로 false로 캐싱 완료.
            dirtyFlags[type] = hasActionConsumed;
        }

        return context; // 밖에서 일괄 Invoke()
    }

    // 헬퍼 함수가 '소모된 액션이 있는지'를 bool로 반환하도록 수정
    private bool ApplyActiveModifiersFrom(Dictionary<ModifierTriggerType, List<Modifier>> targetDict, ModifierTriggerType type)
    {
        bool removedAny = false;

        if (targetDict.TryGetValue(type, out var list) && list.Count > 0)
        {
            foreach (var modifier in list)
            {
                modifier.Apply(myEntity); // context.modifierActions에 액션 Add
            }

            // RemoveAll은 지워진 요소의 개수를 int로 반환합니다. 
            // 0보다 크면 무언가 삭제(소모)되었다는 뜻입니다.
            int removedCount = list.RemoveAll(m => m is ActionModifier);
            removedAny = removedCount > 0;
        }

        return removedAny;
    }

    // ==========================================
    // 4. 아이템 장착 제한 (안전한 단락 평가 적용)
    // ==========================================
    public bool IsRestricted(ItemBase item, ModifierTriggerType trigger)
    {
        bool hasMutation = mutation.TryGetValue(trigger, out var mutateList);
        bool hasEquip = equips.TryGetValue(trigger, out var equipList);

        if (!hasMutation && !hasEquip)
            return false;

        ModifierContext tempContext = new ModifierContext();
        tempContext.InitContext(trigger);

        if (hasMutation) CheckRestrictionModifiers(mutateList, item, tempContext);
        if (hasEquip) CheckRestrictionModifiers(equipList, item, tempContext);

        foreach (var kvp in tempContext.multifle)
        {
            if (kvp.Value <= -1f)
                return true;
        }

        return false;
    }

    private void CheckRestrictionModifiers(List<Modifier> modifierList, ItemBase item, ModifierContext tempContext)
    {
        if (modifierList == null) return;

        foreach (var modifier in modifierList)
        {
            if (modifier is ItemModifier itemModi)
            {
                bool isMatch = false;
                switch (itemModi.itemTargetType)
                {
                    case ItemTargetType.Category:
                        isMatch = (itemModi.itemCategory == item.category);
                        break;
                    case ItemTargetType.Specific:
                        isMatch = (itemModi.itemCategory == item.category && itemModi.specificType.Equals(item.GetSpecificType()));
                        break;
                }

                if (isMatch)
                {
                    modifier.Apply(myEntity);
                }
            }
        }
    }

    // ==========================================
    // 기타 유틸리티 및 세이브/로드
    // ==========================================
    public void Reset_Mutations() => ResetModifiers(mutation);
    public void Reset_Buffs() => ResetModifiers(buffs);
    private void ResetModifiers(Dictionary<ModifierTriggerType, List<Modifier>> target) => target.Clear();

    public Dictionary<ModifierTriggerType, List<Modifier>> Get_Mutations() => mutation;
    public Dictionary<ModifierTriggerType, List<Modifier>> Get_Buffs() => buffs;
    public Dictionary<ModifierTriggerType, List<Modifier>> Get_Equips() => equips;

    public List<ModifierSaveData> MutetionSave() => SaveModifierInternal(mutation);

    private List<ModifierSaveData> SaveModifierInternal(Dictionary<ModifierTriggerType, List<Modifier>> target)
    {
        List<ModifierSaveData> modifierSaves = new List<ModifierSaveData>();
        foreach (var triggerModifiers in target.Values)
        {
            for (int i = 0; i < triggerModifiers.Count; i++)
            {
                modifierSaves.Add(triggerModifiers[i].SaveData());
            }
        }
        return modifierSaves;
    }

    public List<BuffSaveData> BuffSave()
    {
        List<BuffInstance> buffInstances = EventManager.instance.Get_EntityBuffList(myEntity);
        if (buffInstances == null || buffInstances.Count == 0) return null;

        List<BuffSaveData> buffSaveDatas = new List<BuffSaveData>();
        for (int i = 0; i < buffInstances.Count; i++)
        {
            BuffInstance buffInstance = buffInstances[i];
            BuffSaveData buffData = new BuffSaveData
            {
                modifierId = buffInstance.modifier.id,
                value = buffInstance.modifier.value,
                leftDuration = buffInstance.endTurn - TurnManager.instance.GlovalTurn
            };
            buffSaveDatas.Add(buffData);
        }
        return buffSaveDatas;
    }


    public void OnDead()
    {
        foreach(List<Modifier> modis in buffs.Values)
        {
            for(int i = 0; i<modis.Count; i++)
            {
                modis[i].Return();
            }

        }
        buffs.Clear();
        foreach (List<Modifier> modis in mutation.Values)
        {
            for (int i = 0; i<modis.Count; i++)
            {
                modis[i].Return();
            }
        }
        mutation.Clear();
        //모디파이어 리셋 , 장비는 UnEquip작성 후 모두 UnEquip하기
    }
}