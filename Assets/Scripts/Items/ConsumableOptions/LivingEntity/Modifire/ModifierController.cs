using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using static Defines;

public class ModifierController
{
    

    //private Dictionary<ModifierTriggerType, List<Modifier>> modifiers = new Dictionary<ModifierTriggerType, List<Modifier>>();
    private Dictionary<ModifierTriggerType, List<Modifier>> buffs = new Dictionary<ModifierTriggerType, List<Modifier>>();
    private Dictionary<ModifierTriggerType, List<Modifier>> mutation = new Dictionary<ModifierTriggerType, List<Modifier>>();
    private Dictionary<ModifierTriggerType, List<Modifier>> equips = new Dictionary<ModifierTriggerType, List<Modifier>>();
    private Dictionary<ModifierTriggerType, ModifierContext> contexts = new Dictionary<ModifierTriggerType, ModifierContext>();

    LivingEntity myEntity;

    bool isDirty;
    public void InitAllContext(LivingEntity entity)
    {
        Debug.Log("Init Context");
        ModifierTriggerType[] triggers = Utils.Get_Enums<ModifierTriggerType>();
        Debug.Log($"Trigger Count ! :    {triggers.Length}");
        foreach (ModifierTriggerType type in triggers)
        {
            Debug.Log($"Init context Trigger : {type}");
            if (contexts.ContainsKey(type))
            {
                Debug.Log("Trigger Contained");
                continue;
            }
            else
            {
                contexts.Add(type, new ModifierContext());
                contexts[type].InitContext(type);
            }
        }
        SetMyEntity(entity);
    }
    public ModifierContext Get_Context(ModifierTriggerType trigger)
    {
        //딕셔너리 초기화 오래걸리니 지연 초기화 사용
        if (contexts.TryGetValue(trigger, out ModifierContext existingContext))
        {
            existingContext.Clear();
            return existingContext;
        }

        ModifierContext newContext = new ModifierContext();
        newContext.InitContext(trigger);
        contexts.Add(trigger, newContext);
        return newContext;
    }
    public void SetMyEntity(LivingEntity entity)
    {
        myEntity = entity;
    }

    private void AddModifierInternal(Dictionary<ModifierTriggerType, List<Modifier>> targetDict, Modifier modifier)
    {
        ModifierTriggerType trigger = modifier.triggerType;

        // 1. 리스트 존재 여부 확인 및 생성
        if (!targetDict.ContainsKey(trigger))
        {
            targetDict.Add(trigger, new List<Modifier>());
        }

        // 2. 데이터 삽입 및 정렬
        targetDict[trigger].Add(modifier);
        targetDict[trigger].Sort((a, b) => a.priority.CompareTo(b.priority));

        // 3. 패시브 즉시 갱신 (공통 로직)
        if (trigger == ModifierTriggerType.Passive)
        {
            Get_Context(trigger);
            RefreshPassiveModifiers(); // 공통화된 갱신 메서드
        }
    }

    // 외부 노출용 래퍼 함수
    public void AddBuff(Modifier modifier) => AddModifierInternal(buffs, modifier);
    public void AddEquipment(Modifier modifier) => AddModifierInternal(equips, modifier);
    public void AddMutation(Modifier modifier) => AddModifierInternal(mutation, modifier);

    private void RemoveModifierInternal(Dictionary<ModifierTriggerType,List<Modifier>> targetDict, Modifier modifier)
    {
        if (modifier == null || !targetDict.ContainsKey(modifier.triggerType)) return;

        var trigger = modifier.triggerType;
        var list = targetDict[trigger];

        // 참조 비교로 딱 하나만 삭제
        if (list.Remove(modifier))
        {
            if (trigger == ModifierTriggerType.Passive)
            {
                Get_Context(trigger);
                RefreshPassiveModifiers();
            }
        }
    }

    public void RemoveBuff(Modifier modifier) => RemoveModifierInternal(buffs, modifier);
    public void RemoveEquipment(Modifier modifier) => RemoveModifierInternal(equips, modifier);
    public void RemoveMutate(Modifier modifier) => RemoveModifierInternal(mutation, modifier);

    public void RefreshPassiveModifiers()
    {
        ApplyPassivesFrom(mutation); // 돌연변이 먼저 적용
        ApplyPassivesFrom(equips);  // 그 다음 장비
        ApplyPassivesFrom(buffs);   // 마지막으로 버프
    }

    private void ApplyPassivesFrom(Dictionary<ModifierTriggerType, List<Modifier>> targetDict)
    {
        // 패시브 키가 존재하고, 리스트에 요소가 있을 때만 실행
        if (targetDict.TryGetValue(ModifierTriggerType.Passive, out var list) && list.Count > 0)
        {
            // for문이 foreach보다 유니티 환경에서 미세한 성능(가비지 컬렉션 등) 이점이 있을 수 있으나, 
            // 최신 C#에서는 List<T>의 foreach 최적화가 잘 되어 있어 그대로 유지하셔도 무방합니다.
            foreach (var modifier in list)
            {
                modifier.Apply(myEntity);
            }
        }
    }

    public ModifierContext ApplyModifiers(ModifierTriggerType type)
    {
        ModifierContext context = Get_Context(type);

        // 1. 패시브는 별도의 갱신(Refresh) 로직을 타므로 여기서 적용하지 않음
        if (type == ModifierTriggerType.Passive)
        {
            return context;
        }

        // 2. 도우미 함수를 통해 각 딕셔너리를 안전하게 순회하며 적용
        ApplyActiveModifiersFrom(mutation, type);
        ApplyActiveModifiersFrom(equips, type);
        ApplyActiveModifiersFrom(buffs, type);

        return context;
    }

    // 중복 로직 및 에러 방지를 위한 헬퍼 함수
    private void ApplyActiveModifiersFrom(Dictionary<ModifierTriggerType, List<Modifier>> targetDict, ModifierTriggerType type)
    {
        // TryGetValue로 해당 타입의 리스트가 있는지 '안전하게' 확인
        if (targetDict.TryGetValue(type, out var list) && list.Count > 0)
        {
            // 1. 등록된 모디파이어들 발동
            foreach (var modifier in list)
            {
                modifier.Apply(myEntity);
            }

            // 2. 1회성/액션 모디파이어(ActionModifier) 사용 후 즉시 제거
            // 기존의 임시 리스트 생성 + for 루프 + Remove 방식을 
            // RemoveAll 한 줄로 바꾸면 메모리 할당 없이 엄청나게 빠르게 처리됩니다.
            list.RemoveAll(m => m is ActionModifier);
        }
    }

    public void Reset_Mutations()
    {
        ResetModifiers(mutation);
    }
    public void Reset_Buffs()
    {
        ResetModifiers(buffs);
    }
    void ResetModifiers(Dictionary<ModifierTriggerType, List<Modifier>> target)
    {
        target.Clear();
    }
    public Dictionary<ModifierTriggerType,List<Modifier>> Get_Mutations()
    {
        return GetModifiers(mutation);
    }
    public Dictionary<ModifierTriggerType, List<Modifier>> Get_Buffs()
    {
        return GetModifiers(buffs);
    }
    public Dictionary<ModifierTriggerType,List<Modifier>> Get_Equips()
    {
        return GetModifiers(equips);
    }
    Dictionary<ModifierTriggerType, List<Modifier>> GetModifiers(Dictionary<ModifierTriggerType,List<Modifier>> target)
    {
        return target;
    }

    public bool IsRestricted(ItemBase item, ModifierTriggerType trigger)
    {
        if (!mutation.TryGetValue(trigger, out var mutateList))
            return false;
        if (!equips.TryGetValue(trigger, out var equipList))
            return false;
        var context = Get_Context(trigger);
        context.Clear();

        foreach (var modifier in mutateList)
        {
            if (modifier is ItemModifier itemModi)
            {
                switch (itemModi.itemTargetType)
                {
                    case ItemTargetType.Category:
                        if(itemModi.itemCategory == item.category)
                        {
                            modifier.Apply(myEntity);
                        }
                        break;
                    case ItemTargetType.Specific:
                        // 카테고리 및 세부 타입 일치 확인
                        if (itemModi.itemCategory == item.category &&
                            itemModi.specificType.Equals(item.GetSpecificType()))
                        {
                            modifier.Apply(myEntity);
                        }
                        break;
                    default:
                        break;
                }
                
            }
        }
        foreach (var modifier in equipList)
        {
            if (modifier is ItemModifier itemModi)
            {
                switch (itemModi.itemTargetType)
                {
                    case ItemTargetType.Category:
                        if (itemModi.itemCategory == item.category)
                        {
                            modifier.Apply(myEntity);
                        }
                        break;
                    case ItemTargetType.Specific:
                        // 카테고리 및 세부 타입 일치 확인
                        if (itemModi.itemCategory == item.category &&
                            itemModi.specificType.Equals(item.GetSpecificType()))
                        {
                            modifier.Apply(myEntity);
                        }
                        break;
                    default:
                        break;
                }

            }
        }
        // 제한 조건 확인
        foreach (var kvp in context.multifle)
        {
            if (kvp.Value <= -1f)
                return true;
        }

        return false;
    }
   
    List<ModifierSaveData> SaveModifierInternal(Dictionary<ModifierTriggerType, List<Modifier>> target)
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
    public List<ModifierSaveData> MutetionSave()
    {
        return SaveModifierInternal(mutation);
    }
    public List<ModifierSaveData> BuffSave()
    {
        return SaveModifierInternal(buffs);
    }
}

