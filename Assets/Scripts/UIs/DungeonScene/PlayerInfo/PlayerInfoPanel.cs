using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static Defines;
public class PlayerInfoPanel : MonoBehaviour
{
    [SerializeField] PlayerGageUI expUI;
    [SerializeField] PlayerGageUI hpUI;
    [SerializeField] PlayerGageUI mpUI;
    [SerializeField] PlayerStatUI strText;
    [SerializeField] PlayerStatUI dexText;
    [SerializeField] PlayerStatUI intText;
    [SerializeField] TextMeshProUGUI levelText;

    List<StatType> uiStats = new List<StatType>();

    PlayerEntity player;
    private void Awake()
    {
        Init();
    }

    public void Init()
    {
        uiStats = new List<StatType>() {StatType.HP,StatType.MaxHP,StatType.MP,StatType.MaxMP,StatType.Exp,StatType.MaxExp, StatType.Str,StatType.Dex,StatType.Int };
        if(player == null)
        {
            player = GameManager.instance.Get_PlayerEntity();
        }
    }
    public void OnStart()
    {
        player.OnStatChanged+=OnStatChange;
        OnStatChange();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        OnStart();


    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnLevelUp()
    {

    }

    

    public void OnStatChange()
    {
        Dictionary<StatType, float> finalStat = player.CalculateContext(ModifierTriggerType.Passive);
        foreach(var key in uiStats)
        {
            HandlesStatChanged(key, (int)finalStat[key]);
        }
    }

    void HandlesStatChanged(StatType type, int value)
    {
        switch (type)
        {
            case StatType.Str: UpdateStrText(value); break;
            case StatType.Dex: UpdateDexText(value); break;
            case StatType.Int: UpdateIntText(value); break;
            case StatType.HP: UpdateCurrentHpText(value);break;
            case StatType.MaxHP: UpdateMaxHp(value);break;
            case StatType.MP: UpdateCurrentMpText(value);break;
            case StatType.MaxMP: UpdateMaxMp(value);break;
            case StatType.Exp: UpdateCurrentExp(value);break;
            case StatType.MaxExp: UpdateMaxExp(value);break;
                // 스탯 추가 될 시 케이스 추가
        }
    }

    public void UpdateStrText(int value) => UpdateStatUI(StatType.Str, strText,value);
    public void UpdateDexText(int value) => UpdateStatUI(StatType.Dex, dexText, value);
    public void UpdateIntText(int value) => UpdateStatUI(StatType.Int, intText, value);

    public void UpdateHpText(int maxValue, int currentValue) => UpdateGageUI(StatType.MaxHP,StatType.HP, hpUI, maxValue, currentValue);
    public void UpdateMpText(int maxValue, int currentValue) => UpdateGageUI(StatType.MaxMP,StatType.MP, mpUI, maxValue, currentValue);
    public void UpdateExpText(int maxValue, int currentValue) => UpdateGageUI(StatType.MaxExp,StatType.Exp, expUI, maxValue, currentValue);

    public void UpdateCurrentHpText(int value) => UpdateCurrentGageUI(StatType.HP,hpUI, value);
    public void UpdateCurrentMpText(int value) => UpdateCurrentGageUI(StatType.MP, mpUI, value);
    public void UpdateCurrentExp(int value) => UpdateCurrentGageUI(StatType.Exp, expUI, value);

    public void UpdateMaxHp(int value) =>UpdateMaxGageUI(StatType.MaxHP,hpUI, value);
    public void UpdateMaxMp(int value) => UpdateMaxGageUI(StatType.MaxMP, mpUI, value);
    public void UpdateMaxExp(int value)=> UpdateMaxGageUI(StatType.MaxExp, expUI, value);

    void UpdateStatUI(StatType type, PlayerStatUI statUI, int value)
    {
        statUI.Set_Value(type, value);
    }

    public void UpdateGageUI(StatType Maxtype, StatType currentType,PlayerGageUI gageUI,int maxValue, int currentValue)
    {
        gageUI.Set_Value(Maxtype, currentType, maxValue, currentValue);
    }
    public void UpdateCurrentGageUI(StatType currentStat,PlayerGageUI gageUI, int value)
    {
        gageUI.Set_CurrentValue(currentStat, value);
    }
    public void UpdateMaxGageUI(StatType maxStat,PlayerGageUI gageUI , int value)
    {
        gageUI.Set_MaxValue(maxStat, value);
    }
    public void UpdateLevel(int level)
    {
        levelText.text = $"LV : {level}";
    }
}
