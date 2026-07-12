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


    public void Apply(LivingEntity target)
    {
        foreach (Modifier modifier in myMods)
        {
            modifier.Apply(target);
        }
    }

    public Defines.PoolScriptType GetScriptType()
    {
        return Defines.PoolScriptType.ModifierOption;
    }

    public void Reset()
    {
        optionKey = null;
        descKey = null;
        foreach (var mod in myMods)
        {
            mod.Return();
        }
        
    }

    public void Return()
    {
        for(int i = 0; i<myMods.Count; i++)
        {
            myMods[i].Return();
        }
        myMods.Clear();
        optionKey = null;
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
        if(!myMods.Any(m=> m.id!= mod.id))
        {
            myMods.Add(mod);
        }
    }

    public ModifierOptionSaveData SaveData()
    {
        ModifierOptionSaveData newData = new ModifierOptionSaveData();

        foreach(Modifier mod in myMods)
        {
            ModifierSaveData modSave = mod.SaveData();
            newData.modifiers.Add(modSave);
        }
        return newData;
    }
}
