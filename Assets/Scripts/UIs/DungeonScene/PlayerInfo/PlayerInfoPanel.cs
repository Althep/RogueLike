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
    [SerializeField] LevelUI levelText;
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
        player.OnStatChangedUI+=OnStatChange;
        player.OnLevelUp+=OnLevelUp;
        player.OnAddExp+=OnExpAdd;
        UpdateAllUIs();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        OnStart();
    }

    public void OnLevelUp()
    {
        Debug.Log("UI 수신: 레벨업 연출 및 텍스트 갱신 시작");

        // 1. 레벨 숫자를 바꾼다.
        UpdateLevelUI();

        // 2. 경험치 게이지의 최대치와 현재치를 최신화한다.
        int currentExp = player.Get_CurrentExp();
        int maxExp = player.Get_MaxExp();
        UpdateAllExpText(maxExp, currentExp);
    }


    public void OnStatChange(StatType type)
    {
        Dictionary<StatType, float> finalStat = player.CalculateContext(ModifierTriggerType.Passive);
        HandlesStatChanged(type, (int)finalStat[type]);
    }

    public void OnExpAdd()
    {
        UpdateCurrentExp();
    }

    public void UpdateAllUIs()
    {
        // 모든 스탯 UI 한 번에 새로고침
        OnStatChange(StatType.HP);
        OnStatChange(StatType.MaxHP);
        OnStatChange(StatType.MP);
        OnStatChange(StatType.MaxMP);
        OnStatChange(StatType.Str);
        OnStatChange(StatType.Dex);
        OnStatChange(StatType.Int);
        // 레벨 및 경험치 관련 UI 한 번에 새로고침
        OnLevelUp();
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
                // 스탯 추가 될 시 케이스 추가
        }
    }

    public void UpdateStrText(int value) => UpdateStatUI(StatType.Str, strText,value);
    public void UpdateDexText(int value) => UpdateStatUI(StatType.Dex, dexText, value);
    public void UpdateIntText(int value) => UpdateStatUI(StatType.Int, intText, value);

    public void UpdateHpText(int maxValue, int currentValue) => UpdateGageUI(StatType.MaxHP,StatType.HP, hpUI, maxValue, currentValue);
    public void UpdateMpText(int maxValue, int currentValue) => UpdateGageUI(StatType.MaxMP,StatType.MP, mpUI, maxValue, currentValue);
    public void UpdateAllExpText(int maxValue, int currentValue) => UpdateGageUI(StatType.MaxExp,StatType.Exp, expUI, maxValue, currentValue);

    public void UpdateCurrentHpText(int value) => UpdateCurrentGageUI(StatType.HP,hpUI, value);
    public void UpdateCurrentMpText(int value) => UpdateCurrentGageUI(StatType.MP, mpUI, value);
    public void UpdateExp(int value) => UpdateCurrentGageUI(StatType.Exp, expUI, value); 

    public void UpdateMaxHp(int value) =>UpdateMaxGageUI(StatType.MaxHP,hpUI, value);
    public void UpdateMaxMp(int value) => UpdateMaxGageUI(StatType.MaxMP, mpUI, value);
    public void UpdateMaxExpText(int value) => UpdateMaxGageUI(StatType.MaxExp, expUI, value);

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

    public void UpdateMaxExp()
    {
        int maxExp = player.Get_MaxExp();
        UpdateMaxExpText(maxExp);
    }
    public void UpdateCurrentExp()
    {
        int currentExp = player.Get_CurrentExp();
        UpdateExp(currentExp);
    }

    public void UpdateLevelUI()
    {
        int level = player.Get_Level();
        int currentExp = player.Get_CurrentExp();
        int maxExp = player.Get_MaxExp();
        levelText.Set_LevelText(level);
        levelText.UpdateLevelText();
    }
}
