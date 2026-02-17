using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using static Defines;
public class Cure : ModifierAction
{

    public Cure()
    {
        actionEffectType = Defines.ActionEffectType.Cure;
    }


    public override void Reset()
    {
        throw new System.NotImplementedException();
    }

    public override void Excute(LivingEntity entity)
    {
        Dictionary<ModifierTriggerType,List<Modifier>> modList = entity.GetModifiers();
        List<Modifier> debuffs = new List<Modifier>();
        foreach(var modi in modList.Keys)
        {
            List<Modifier> mods = modList[modi];
            debuffs.AddRange(
            mods.Where(
                mod => mod is BuffModifier buff &&
                buff.buffCategory == BuffCategory.Debuff)
                .ToList()
                );
        }
        for(int i = 0; i<debuffs.Count; i++)
        {
            entity.RemoveModifier(debuffs[i]);
        }
    }

    public override void SetValueToString()
    {
        throw new NotImplementedException();
    }
}
