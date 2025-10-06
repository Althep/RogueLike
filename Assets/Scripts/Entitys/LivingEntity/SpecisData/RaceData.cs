using UnityEngine;
using System;
using System.Collections.Generic;
using static Defines;
using UnityEditorInternal.VersionControl;

public class RaceData
{
    Defines.Races race;

    public string disPlayName;

    public int bonusDice;

    public List<string> startItems;

    ModifierManager modifierManager = new ModifierManager();

    public ModifierContext modifireContext;
    public List<ModifireSource> modifierSources = new List<ModifireSource>();



    public bool CanEquip()
    {
        foreach(ModifireSource source in modifierSources)
        {
            ModifierTriggerType type = source.triggerType;
        }
        return true;
    }

    /*
    public float GetItemModifier(ItemBase item, List<Penalty> penalties)
    {
        float modifier = 1.0f;

        foreach (var penalty in penalties)
        {
            if (penalty.restrict)
            {
                // 위에서 만든 IsRestricted 함수처럼 검사
                if (IsRestricted(item, penalty))
                    return 0f; // 아예 사용 불가
            }
            else
            {
                // 카테고리 단위 수정
                if (penalty.targetType == PenaltyTargetType.Category &&
                    penalty.category == item.category)
                {
                    modifier *= penalty.modifierValue;
                }

                // 세부 아이템 단위 수정
                if (penalty.targetType == PenaltyTargetType.Specific)
                {
                    switch (penalty.category)
                    {
                        case ItemCategory.Consumable:
                            if (item is ConsumableItem consumable &&
                                (int)consumable.consumableType == penalty.targetValue)
                                modifier *= penalty.modifierValue;
                            break;

                        case ItemCategory.Equipment:
                            if (item is EquipItem equip &&
                                (int)equip.equipmentType == penalty.targetValue)
                                modifier *= penalty.modifierValue;
                            break;
                    }
                }
            }
        }

        return modifier;
    }

    public bool CheckRestriction(ItemBase item)
    {
        foreach (var penalty in penalties)
        {
            if (IsRestricted(item, penalty))
                return true;
            
        }
        return false;
    }


    public bool IsRestricted(ItemBase item, Penalty penalty)
    {
        // 카테고리 전체 제한
        if (penalty.targetType == PenaltyTargetType.Category && penalty.category == item.category)
        {
            return true;
        }

        // 세부 아이템 제한
        if (penalty.targetType == PenaltyTargetType.Specific)
        {
            switch (penalty.category)
            {
                case ItemCategory.Equipment:
                    if (item is EquipItem equip && (int)equip.equipmentType == penalty.targetValue)
                        return true;
                    break;

                case ItemCategory.Consumable:
                    if (item is ConsumableItem consumable && (int)consumable.consumableType == penalty.targetValue)
                        return true;
                    break;

                case ItemCategory.Misc:
                    if (item is MiscItem misc && (int)misc.miscType == penalty.targetValue)
                        return true;
                    break;
            }
        }
        return false;
    }*/
}
