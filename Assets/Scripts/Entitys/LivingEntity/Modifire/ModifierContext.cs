using static Defines;
using UnityEngine.TextCore.Text;
using System.Collections.Generic;
public class ModifierContext
{
    public ModifierTriggerType triggerType; // 발동조건
    public Dictionary<StatType,int> stats; // 각 능력치
    public Dictionary<StatType, float> multifle;
    public ModifierType ModifierType;
    public float BaseValue; // 원래 값
    public float ModifiedValue; // 수정된 값

    
}