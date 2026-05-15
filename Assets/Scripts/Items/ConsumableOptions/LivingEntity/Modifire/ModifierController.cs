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
    public void EquipItem(EquipItem equip)
    {
        List<Modifier> equipModifiers = equip.options;
        List<Modifier> equipAddOptions = equip.addOptions;


        if (equipModifiers != null)
        {
            for (int i = 0; i < equipModifiers.Count; i++) AddEquipment(equipModifiers[i]);
        }

        if (equipAddOptions != null) // 안전장치!
        {
            for (int i = 0; i < equipAddOptions.Count; i++) AddEquipment(equipAddOptions[i]);
        }
    }
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

    public void ConsumeOneTimeActions(ModifierTriggerType trigger)
    {
        // 해당 트리거(예: Attack)의 컨텍스트를 가져옵니다.
        if (contexts.TryGetValue(trigger, out ModifierContext context))
        {
            // 1회성 액션(isConsumable == true)만 핀셋으로 골라서 제거합니다.
            int removedCount = context.modifierActions.RemoveAll(a => a.isConsumable);

            // [핵심] 만약 뭔가 지워졌다면, 그릇의 상태가 변한 것이므로 
            // 다음번 호출 시 재계산되도록 더티 플래그를 다시 오염(true)시킵니다!
            if (removedCount > 0)
            {
                dirtyFlags[trigger] = true;
            }
        }
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

        if (type == ModifierTriggerType.Passive) return context;

        if (dirtyFlags.TryGetValue(type, out bool isDirty) && isDirty)
        {
            context.Clear();

            // 1. 장비, 변이, 버프에서 액티브 스킬들을 안전하게 장전만 합니다.
            ApplyActiveModifiersFrom(mutation, type);
            ApplyActiveModifiersFrom(equips, type);
            ApplyActiveModifiersFrom(buffs, type);

            // 2. 삭제되는게 없으므로 무조건 캐싱 완료(false) 처리합니다.
            dirtyFlags[type] = false;
        }

        return context;
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
                    // [수정된 부분] myEntity에 Apply하지 말고, tempContext의 제한 수치만 조작!
                    if (itemModi.isMulti)
                    {
                        if (!tempContext.multifle.ContainsKey(itemModi.stat)) tempContext.multifle.Add(itemModi.stat, 0f);
                        tempContext.multifle[itemModi.stat] += itemModi.value;
                    }
                    else
                    {
                        if (!tempContext.stats.ContainsKey(itemModi.stat)) tempContext.stats.Add(itemModi.stat, 0f);
                        tempContext.stats[itemModi.stat] += itemModi.value;
                    }
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
        foreach (List<Modifier> modis in buffs.Values)
        {
            for (int i = 0; i<modis.Count; i++)
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