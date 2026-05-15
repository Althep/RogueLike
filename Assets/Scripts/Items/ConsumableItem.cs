using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class ConsumableItem : ItemBase
{
    public Defines.ConsumableType consumableType;

    public override ItemBase Clone()
    {
        ConsumableItem newConsum = ItemManager.instance.ItemMake(name) as ConsumableItem;
        CopyBaseProperties(newConsum); // 여기서 newConsum.options가 깊은 복사로 새로 생성됨!
        newConsum.consumableType = consumableType;

        // 1. [수정됨] 원본이 아닌 '방금 복사된 새 아이템'의 옵션을 가져옵니다.
        List<ActionModifier> actionMods = newConsum.options.OfType<ActionModifier>().ToList();

        foreach (var actMod in actionMods)
        {
            // 2. ActionModifier 안에 있는 action에 일회용 꼬리표 달기
            actMod.SetConsumableFlag(); // 밖에서 직접 건드리지 말고 함수로 우아하게 명령!
        }

        return newConsum;
    }

    public override Enum GetSpecificType()
    {
        return consumableType;
    }
}
