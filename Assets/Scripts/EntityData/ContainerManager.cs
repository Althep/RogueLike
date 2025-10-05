using UnityEngine;
using System.Collections.Generic;
using System;
using static Defines;
public class ContainerManager : MonoBehaviour
{

    Dictionary<ModifierTriggerType, IDataContainer> dataContaners = new Dictionary<ModifierTriggerType, IDataContainer>();

    public void Init()
    {
        ModifierTriggerType[] triggers = (ModifierTriggerType[])Enum.GetValues(typeof(ModifierTriggerType));

        foreach(ModifierTriggerType trigger in triggers)
        {
            switch (trigger)
            {
                case ModifierTriggerType.OnEquip:
                    break;
                case ModifierTriggerType.OnUnequip:
                    break;
                case ModifierTriggerType.OnAttack:
                    break;
                case ModifierTriggerType.OnLevelUp:
                    break;
                case ModifierTriggerType.OnHit:
                    break;
                case ModifierTriggerType.OnActionStart:
                    break;
                case ModifierTriggerType.OnTurnStart:
                    break;
                case ModifierTriggerType.OnUseItem:
                    break;
                case ModifierTriggerType.Passive:
                    break;
                default:
                    break;
            }
        }
    }

    public IDataContainer GetContainer(ModifierTriggerType trigger)
    {
        if (dataContaners.ContainsKey(trigger))
        {
            return dataContaners[trigger];
        }
        else
        {
            Debug.Log("Can't Found Container");
            return null;
        }
    }

}
