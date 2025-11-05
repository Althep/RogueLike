using UnityEngine;

public static class Defines
{
    public enum UIEvents
    {
        Click,
        DragStart,
        Drag,
        DragEnd
    }
    public enum PoolScriptType
    {
        StatModifier,
        ItemModifier,
        BuffModifier,
        DamageModifier
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
        Divide
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
    
    #region Modifiers
    public enum ModifierType
    {
        StatModifier,
        ItemModifier,
        BuffModifier,
        DamageModifier
    }
    public enum ModifierTriggerType
    {
        OnEquip,
        OnUseItem,
        OnUnequip,
        OnAttack,
        OnHit,
        OnSpellCast,
        OnLevelUp,
        Passive
    }
    public enum ItemTargetType
    {
        Category,
        Specific
    }
    #endregion
    public enum StatType
    {
        HP,
        MaxHP,
        Str,
        Dex,
        Int,
        Evasion,
        Defense,
        DamageReduce,
        ShieldDefense,
        Damage,
        Accuracy,
        AttackRange,
        SpellDamage,
        SpellAccurancy,
        Disruption,
        FireResist,
        IceResist,
        MagicResist,
        ThunderResist
    }
    public enum DamageType
    {
        Physical,
        Fire,
        Ice,
        Magic,
        Thunder
    }
    public enum Races
    {
        Default,
        Elf,
        Dwarf,
        Human,
        Ork,
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
        Foot
    }
    #endregion
    
}
