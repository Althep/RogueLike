using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
public class ModifierOption : IPoolScript
{
    public string optionKey;
    public List<Modifier> myMods = new List<Modifier>();
    public List<string> ModKeys = new List<string>();
    public string descKey;


    public void Apply(LivingEntity entity)
    {
        foreach (Modifier modifier in myMods)
        {
            modifier.Apply(entity);
        }
    }

    public Defines.PoolScriptType GetScriptType()
    {
        return Defines.PoolScriptType.ModifierOption;
    }

    public void Reset()
    {
        // Delegates to the pool so options are returned exactly once, together with their modifiers.
        Return();
    }

    public void Return()
    {
        ModifierManager.instance.Return_ModifierOption(this);
    }

    public void SetScriptType(Defines.PoolScriptType type)
    {
        type = Defines.PoolScriptType.ModifierOption;
    }

    public ModifierOption Copy()
    {
        ModifierOption mod = new ModifierOption();
        mod.descKey = this.descKey;
        mod.optionKey = this.optionKey;
        return mod;
    }

    public void AddModifier(Modifier mod)
    {
        myMods.Add(mod);
    }

    public void AddNotIncludeMod(Modifier mod)
    {
        if (mod == null) return;
        for (int i = 0; i < myMods.Count; i++)
        {
            if (myMods[i].valueId == mod.valueId) return;
        }
        myMods.Add(mod);
    }

    // Load path: this option was already assembled from data (rented by value row).
    // Overwrites only the saved runtime value on the modifier with the same valueId.
    // Old saves without valueId fall back to the base id (same index first, then first match).
    public void RestoreSavedValues(List<ModifierSaveData> saved)
    {
        if (saved == null) return;
        for (int s = 0; s < saved.Count; s++)
        {
            ModifierSaveData save = saved[s];
            Modifier target = null;
            if (!string.IsNullOrEmpty(save.valueId))
            {
                for (int i = 0; i < myMods.Count; i++)
                {
                    if (myMods[i].valueId == save.valueId)
                    {
                        target = myMods[i];
                        break;
                    }
                }
            }
            else if (!string.IsNullOrEmpty(save.modifierId))
            {
                if (s < myMods.Count && myMods[s].baseID == save.modifierId)
                {
                    target = myMods[s];
                }
                else
                {
                    for (int i = 0; i < myMods.Count; i++)
                    {
                        if (myMods[i].baseID == save.modifierId)
                        {
                            target = myMods[i];
                            break;
                        }
                    }
                }
            }

            if (target != null)
            {
                target.value = save.value;
            }
            else
            {
                Debug.LogWarning("[ModifierOption] saved modifier not found in option " + optionKey + ": " + save.valueId + " / " + save.modifierId);
            }
        }
    }

    public ModifierOptionSaveData SaveData()
    {
        ModifierOptionSaveData newData = new ModifierOptionSaveData();
        newData.optionKey = optionKey;
        newData.modifiers = new List<ModifierSaveData>(myMods.Count);

        foreach(Modifier mod in myMods)
        {
            ModifierSaveData modSave = mod.SaveData();
            newData.modifiers.Add(modSave);
        }
        return newData;
    }
}
