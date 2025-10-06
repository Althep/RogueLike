using static Defines;

public abstract class Modifier
{
    public string id;
    public ModifierTriggerType triggerType;
    public StatType stat;
    public int priority;
    public bool isMulti;
    public float value;

    public abstract void Apply(ModifierContext context);
}

public class StatModifier : Modifier
{
    public StatModifier(string id, StatType stat, int value, float multifle, bool isMulty, int priority = 50)
    {
        this.id = id;
        this.stat = stat;
        this.value = value;
        this.isMulti = isMulty;
        this.priority = priority;
    }

    public override void Apply(ModifierContext context)
    {
        if (isMulti)
        {
            if (!context.multifle.ContainsKey(stat))
            {
                context.multifle.Add(stat, 0f);
            }
            context.multifle[stat] += value;
        }
        else
        {
            if (!context.stats.ContainsKey(stat))
            {
                context.stats.Add(stat, 0);
            }
            context.stats[stat] += (int)value;
        }
    }
}

public class ItemModifier : Modifier
{
    public ItemTargetType itemTargetType;
    public ItemCategory itemCategory;
    public bool unEquipable;
    public ItemModifier(string name, ItemTargetType targetType, ItemCategory category, float value, int priority,bool unEquipable)
    {
        this.id = name;
        this.itemTargetType = targetType;
        itemCategory = category;
        this.unEquipable = unEquipable;
    }
    public override void Apply(ModifierContext context)
    {
        if (isMulti)
        {
            if (!context.multifle.ContainsKey(stat))
            {
                context.multifle.Add(stat, 0f);
            }
            context.multifle[stat] += value;
        }
        else
        {
            if (!context.stats.ContainsKey(stat))
            {
                context.stats.Add(stat, 0);
            }
            context.stats[stat] += (int)value;
        }
    }
}

public class BuffModifier : Modifier
{
    int duration;

    public override void Apply(ModifierContext context)
    {
        throw new System.NotImplementedException();
    }
}