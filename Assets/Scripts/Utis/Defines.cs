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
    public enum UI_PrefabType
    {
        Confirm,
        MainSelect
    }
    #region Modifiers
    public enum ModifierType
    {
        Stat,
        Buff,
        Debuff
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
        Str,
        Dex,
        Int,
        Evasion,
        Defense,
        DamageReduce,
        Damage
    }
    public enum DamageType
    {
        Physical,
        Magic,
        Fire,
        Ice,
        Thunder,
        Earth
    }
    public enum Races
    {
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
        Warrior,
        Hunter,
        Rogue,
        Mage,
        BattleMaster,
        Assassin,
        Engineer
    }


    #region items
    public enum WeaponType
    {
        ShortSword
    }

    public enum ItemCategory
    {
        Equipment,
        Consumable,
        Misc
    }

    public enum EquipmentType
    {

    }

    public enum ConsumableType
    {
        Potion,
        Scroll,
        Food
    }

    public enum MiscType
    {

    }
    #endregion
    
}
