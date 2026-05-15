using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
public static class EffectApplier
{
    public static void ApplySynchronized(LivingEntity entity, List<Modifier> options)
    {
        List<BuffModifier> buffs = options.OfType<BuffModifier>().ToList();

        if (buffs.Count>1)
        {
            int syncDuration = UnityEngine.Random.Range(buffs[0].minTime, buffs[0].maxTime+1);

            foreach(var buff in buffs)
            {
                buff.duration = syncDuration;
            }

            foreach(var option in options)
            {
                option.Apply(entity);
            }
        }
    }
}
