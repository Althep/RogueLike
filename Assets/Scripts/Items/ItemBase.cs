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
    public string tooltipKey;
    
    // 설계도(Template)가 데이터를 들고 있을 때 사용하는 옵션 키 리스트
    public List<string> optionKeys = new List<string>();
    
    // 실제 런타임 아이템 인스턴스가 생성되었을 때 풀에서 가져와 채워넣는 옵션 객체 리스트
    public List<ModifierOption> options = new List<ModifierOption>();

    public virtual void OnUse(LivingEntity entity)
    {
        for(int i = 0; i <options.Count; i++)
        {
            List<Modifier> mod = options[i].myMods;
            EffectApplier.ApplySynchronized(entity,mod);
        }
        
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

    // 빈 껍데기(Clone)에 기본 속성만 복사해 주는 함수
    public void CopyBaseProperties(ItemBase cloneObj)
    {
        cloneObj.id = this.id;
        cloneObj.name = this.name;
        cloneObj.itemCount = this.itemCount;
        cloneObj.maxStack = this.maxStack;
        cloneObj.tier = this.tier;
        cloneObj.category = this.category;
        cloneObj.weight = this.weight;

        // 설계도의 텍스트(Key)만 안전하게 복사해 넘김
        cloneObj.optionKeys = new List<string>(this.optionKeys);
        
        // 런타임 풀링 객체 리스트는 새로 할당할 준비만 해둠
        cloneObj.options = new List<ModifierOption>();
    }

    public virtual ItemSaveData SaveData()
    {
        ItemSaveData itemSaveData = new ItemSaveData();
        List<ModifierOptionSaveData> myModifiers = new List<ModifierOptionSaveData>();
        for(int i = 0; i<options.Count; i++)
        {
            myModifiers.Add(options[i].SaveData());
        }
        itemSaveData.itemId = id;
        itemSaveData.itemCount = itemCount;
        return itemSaveData;
    }
}
