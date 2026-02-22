using UnityEngine;
using System;
#region classes
public class BuffInstance : IPoolScript
{
    public LivingEntity target;
    public int endTurn;
    public Modifier modifier;
    public Action removalAction;
    Defines.PoolScriptType poolType;
    public BuffInstance(LivingEntity target,Modifier modifier, int duration, int currentTurn, Action removal)
    {
        this.target = target;
        this.modifier = modifier;
        this.endTurn = currentTurn + duration;
        this.removalAction = removal;
    }
    public BuffInstance()
    {

    }
    public void SetMyData(LivingEntity target, Modifier modifier, int duration, int currentTurn, Action removal)
    {
        this.target = target;
        this.modifier = modifier;
        this.endTurn = currentTurn + duration;
        this.removalAction = removal;
    }
    public Defines.PoolScriptType GetScriptType()
    {
        return poolType;
    }

    public void Reset()
    {
        target = null;
        endTurn = 0;
        modifier = null;
        removalAction = null;
    }

    public void SetScriptType(Defines.PoolScriptType type)
    {
        poolType = type;
    }
}


#endregion

public static class Defines
{
    public enum UIEvents
    {
        Click,
        DragStart,
        Drag,
        DragEnd
    }
    public enum Scenes
    {
        MainScene,
        DungeonScene,
        EndingScene
    }
    public enum PoolScriptType
    {
        StatModifier,
        BuffModifier,
        DamageModifier,
        BuffInstance,
        StatAddEffect,
        StatBuffEffect,
        ItemModifier,
    }
    public enum TileType
    {
        Tile,
        Wall,
        Door,
        ShallowWater,
        DeepWater,
        Upstair,
        DownStair,
        Player,
        Monster,
        Item
    }
    
    public enum MapMakeType
    {
        Divide,
        Saved,
        CellularAutomata,
        Prim,
        BSP
    }

    public enum DungeonTheme
    {
        RuinedDungeon
    }

    public enum VisibleState
    {
        Visible,
        Discovered,
        Hidden
    }

    public enum MoveState
    {
        Idle,
        Move,
        Attack
    }

    public enum Language
    {
        Kr,
        En
    }
    #region
    public enum EntityForm
    {
        Humanoid, 
        Animal
    }
    #endregion
    #region Modifiers
    public enum BuffCategory
    {
        None,
        Buff,
        Debuff
    }
    public enum BuffType
    {
        StatBuff,
        ActionBuff
    }
    public enum ActionEffectType
    {
        StatAdd, 
        StatBuff,
        SlotBlock,
        SlotAdd,
        Cure,
        GetDamage,
        Confuse,
        Invisible,
        Ghost,
        LifeSteel,
        TurnSkip,
        Blink,
        Telleport,
        DamageRange,
        Xray,
        Sleep,
        Silence,
        Wish,
        Void,
        EssenceExtract,
        RandomEffect,
        Mutate,
        Identify,
        UnEquipable,
        Purification,
        TelleVision,
        ItemReinforce
    }
    public enum ModifierType
    {
        StatModifier,
        ItemModifier,
        BuffModifier,
        DamageModifier,
        ActionModifier
    }
    public enum ModifierTriggerType
    {
        OnEquip,
        OnUseItem,
        OnUnequip,
        OnAttack,
        OnHited,
        OnSpellCast,
        OnLevelUp,
        Passive,
        OnMove,
        AfterAttack,
        OnTurnEnd,
        StartMove
    }
    public enum ItemTargetType
    {
        Category,
        Specific
    }
    #endregion
    #region Ä³¸¯ÅÍ
    
    public enum StatType
    {
        HP,
        MaxHP,
        MP,
        MaxMP,
        Str,
        Dex,
        Int,
        Evasion,
        Defense,
        DamageReduce,
        ShieldDefense,
        Damage,
        Accurancy,
        AttackRange,
        SpellDamage,
        SpellAccurancy,
        SpellSpeed,
        Disruption,
        FireResist, 
        IceResist,
        MagicResist,
        ThunderResist,
        Vision,
        Sound,
        MoveSpeed,
        AttackSpeed,
        MaxExp,
        Exp,
        Regeneration,
        Tir,
        ExtraLife
    }
    public enum DamageType
    {
        Physical,
        Fire,
        Ice,
        Magic,
        Thunder,
        Poison
    }
    public enum Races
    {
        Default,
        Elf,
        Dwarf,
        Human,
        Orc,
        SpellBane,
        DungeonBorn,
        StoneBorn,
        Rabbitfolk
    }

    public enum Jobs
    {
        Default,
        Warrior,
        Hunter,
        Rogue,
        Mage,
        BattleMaster,
        Assassin,
        Wonderer,
        Engineer
    }
    
    public enum EStatusEffect
    {
        Poison,
        Confuse,
        Slow,
        Haste,
        Petrification
    }
    
    public enum StatuseType
    {
        Buff,
        Debuff
    }
    #endregion
    #region items
    public enum ItemType
    {
        Weapon,
        Armor,
        Helm,
        Glove,
        Shoose,
        Ring,
        Consumable
    }
    public enum WeaponType
    {
        ShortSword,
        Sword
    }

    public enum ItemCategory
    {
        Equipment,
        Consumable,
        Misc
    }

    public enum EquipmentType
    {
        ShortSword,
        Sword,
        Axe,
        Spear,
        Blunt,
        Bow,
        LightArmour,
        HeavyArmour,
        Helmet,
        Glove,
        Boots,
        Ring,
        Amulet
    }

    public enum ConsumableType
    {
        Potion,
        Scroll,
        Food
    }

    public enum MiscType
    {
        MagicWand,
        EvokeItem
    }

    public enum SlotType
    {
        MainHand,
        Head,
        Neck,
        Body,
        Back,
        Hand,
        Foot,
        SubHand
    }
    #endregion
    #region Monster
    public enum SpawnType
    {
        Solo,
        Group
    }
    #endregion
}
