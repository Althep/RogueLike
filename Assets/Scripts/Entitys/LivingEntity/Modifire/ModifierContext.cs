using static Defines;
using UnityEngine.TextCore.Text;
using System.Collections.Generic;
public class ModifierContext
{
    public ModifierTriggerType ActionType; // 공격, 회피, 시전 등
    public Dictionary<StatType,int> stats; // 힘, 민첩, 체력 등
    public ModifierType ModifierType;
    public float BaseValue; // 원래 값
    public float ModifiedValue; // 수정된 값
}