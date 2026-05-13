using UnityEngine;
using TMPro;
using UnityEngine.UI;
using static Defines;

public class PlayerStatUI : PlayerInfoUI
{
    public StatType statType;
    public int value;
    
    public string originalText;
    
    public override void UpdateValue()
    {
        myTmp.text = $"{statType} : {value}";
    }
    public void Set_Value(StatType type, int value)
    {
        statType = type;
        this.value = value;
        UpdateValue();
    }
}
