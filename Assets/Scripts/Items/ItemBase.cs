using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using Unity.VisualScripting;

public abstract class ItemBase 
{
    public string id;
    public string name;
    public int itemCount;
    public int maxStack;
    public int tier;
    public Defines.ItemCategory category;
    public float weight;
    public List<Modifier> options = new List<Modifier>();

    public virtual void OnUse(LivingEntity entity)
    {
        // 1. 장전: 모디파이어들을 엔티티의 주머니(컨텍스트)에 넣습니다.
        EffectApplier.ApplySynchronized(entity, options);

        // 2. 격발 및 청소: 방금 넣은 1회용 액션들을 즉시 실행하고 비워줍니다.
        // (이전에 논의했던 엔티티의 실행 함수를 여기서 바로 호출해버립니다!)

        // 예시: 소비 아이템 전용 트리거를 사용해 실행
        entity.ExecuteAction(entity, Defines.ModifierTriggerType.OnUseItem);
    }
    public bool CanStack(int amount)
    {
        return category == Defines.ItemCategory.Consumable;
    }

    public int ReturnRest(int amount)
    {
        int rest = 0;

        if (itemCount + amount > maxStack)
        {
            rest = itemCount + amount - maxStack;
        }

        return rest;
    }
    public abstract Enum GetSpecificType();

    public abstract ItemBase Clone();

    // ?? 공통 데이터 복사를 위한 도우미 함수 (상세 설명)
    protected void CopyBaseProperties(ItemBase cloneObj)
    {
        cloneObj.id = this.id;
        cloneObj.name = this.name;
        cloneObj.itemCount = this.itemCount;
        cloneObj.maxStack = this.maxStack;
        cloneObj.tier = this.tier;
        cloneObj.category = this.category;
        cloneObj.weight = this.weight;

        // 엔티티 참조는 그대로 넘겨주거나 null로 초기화합니다. (상황에 맞게 선택)

        // ?? 핵심: 리스트 내부의 옵션들까지 '깊은 복사' 수행
        cloneObj.options = new List<Modifier>();
        foreach (var option in this.options)
        {
            // option.Clone()을 호출하여 Modifier도 새롭게 생성해서 넣습니다.
            Modifier copied = ModifierManager.instance.Get_Modifier(option.id);
            Debug.Log($"아이템 데이터 클로닝 아이디{id}, 모디파이어 아이디{copied.id}");
            cloneObj.options.Add(copied);
        }
    }

    public virtual ItemSaveData SaveData()
    {
        ItemSaveData itemSaveData = new ItemSaveData();
        List<ModifierSaveData> myModifiers = new List<ModifierSaveData>();
        for(int i = 0; i<options.Count; i++)
        {
            myModifiers.Add(options[i].SaveData());
        }
        itemSaveData.itemId = id;
        itemSaveData.itemCount = itemCount;
        return itemSaveData;
    }
}
