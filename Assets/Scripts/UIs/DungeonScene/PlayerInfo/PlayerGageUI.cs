using UnityEngine;
using UnityEngine.UI;
using static Defines;
public class PlayerGageUI : PlayerInfoUI
{
    public int maxValue;
    public int currentValue;

    public StatType maxType;
    public StatType currentType;

    public Image fillImage;
    public override void UpdateValue()
    {
        if(maxValue == 0)
        {
            Debug.Log($"{maxType} value zero");
            return;
        }
        myTmp.text = $"{currentType} : {currentValue} / {maxValue}";
        fillImage.fillAmount = currentValue/maxValue;
    }


    public void Set_Value(StatType maxType, StatType currentType, int maxValue,int currentValue)
    {
        this.maxType = maxType;
        this.currentType = currentType;
        this.maxValue = maxValue;
        this.currentValue = currentValue;
        UpdateValue();
        
    }
    
    public void Set_CurrentValue(StatType currentType, int currentValue)
    {
        this.currentType = currentType;
        this.currentValue = currentValue;
        UpdateValue();
    }

    public void Set_MaxValue(StatType maxType, int maxValue)
    {
        this.maxType = maxType;
        this.maxValue = maxValue;
        UpdateValue();
    }


}
