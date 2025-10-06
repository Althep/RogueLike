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
        LivingEntity myEntity = this.gameObject.transform.GetComponent<LivingEntity>();
        BattleData battleContainer = new BattleData(myEntity);
        EntityContainer entityContainer = new EntityContainer(myEntity);
        ItemUserContainer itemUseContainer = new ItemUserContainer(myEntity);
        foreach(ModifierTriggerType trigger in triggers)
        {
            switch (trigger)
            {
                case ModifierTriggerType.OnEquip:
                    dataContaners.Add(trigger, itemUseContainer);
                    break;
                case ModifierTriggerType.OnUnequip:
                    dataContaners.Add(trigger, itemUseContainer);
                    break;
                case ModifierTriggerType.OnAttack:
                    dataContaners.Add(ModifierTriggerType.OnAttack, battleContainer);
                    break;
                    break;
                case ModifierTriggerType.OnUseItem:
                    dataContaners.Add(trigger, itemUseContainer);
                    break;
                case ModifierTriggerType.OnSpellCast:

                case ModifierTriggerType.Passive:
                    dataContaners.Add(trigger, entityContainer);
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
