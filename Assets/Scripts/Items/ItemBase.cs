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

    public void OnUse(LivingEntity entity)
    {
        List<BuffModifier> buffs = options.OfType<BuffModifier>().ToList();
        
        if(buffs.Count>1)
        {
            int duration = UnityEngine.Random.Range(buffs[0].minTime, buffs[0].maxTime+1);
            foreach(var buff in buffs)
            {
                buff.duration = duration;
            }
        }
        foreach(var option in options)
        {
            option.Apply(entity);
        }
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
            Modifier copied = ModifierManager.instance.Get_Modifier(option.modifierType,option.id);
            cloneObj.options.Add(copied);
        }
    }
}
