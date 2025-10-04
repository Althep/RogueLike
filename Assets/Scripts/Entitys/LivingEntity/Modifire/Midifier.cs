using static Defines;

public abstract class Modifier
{
    public string id;
    public string name;
    public Defines.ModifierType type;
    public Defines.ActionType actionType; // <- ÇÙ½É
    public int priority;
    public float value;
    public bool isPositive;
    public string targetTag;

    public abstract void Apply(ModifierContext context);
}

public class StatModifier : Modifier
{
    public StatType targetStat;

    public StatModifier(string name, StatType stat, float value, int priority = 50)
    {
        this.name = name;
        this.type = ModifierType.Stat;
        targetStat = stat;
        this.value = value;
        this.priority = priority;
    }

    public override void Apply(ModifierContext context)
    {
        if (context.StatType == targetStat)
            context.ModifiedValue += value;
    }
}

public class ActionModifier : Modifier
{
    public ActionType targetAction;

    public ActionModifier(string name, ActionType action, float value, int priority = 100)
    {
        this.name = name;
        this.type = ModifierType.Action;
        this.targetAction = action;
        this.value = value;
        this.priority = priority;
    }

    public override void Apply(ModifierContext context)
    {
        if (context.ActionType == targetAction)
            context.ModifiedValue += value;
    }

}

public class ItemModifier : Modifier
{
    ItemTargetType itemTargetType;
    ItemCategory itemCategory;
    
    public ItemModifier(string name,ItemTargetType targetType,ItemCategory category,float value, int priority)
    {
        this.name = name;
        this.type = ModifierType.Item;
    }
    public override void Apply(ModifierContext context)
    {
        
    }
}