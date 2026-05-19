using UnityEngine;
using System;
public class LevelSystem 
{
    int Level { get; set; } = 1;
    int CurrentExp { get; set; } = 0;
    int MaxExp { get; set; } = 10;

    public event Action OnLevelUp;

    public void AddExp(int expAmount)
    {
        CurrentExp+=expAmount;
        Debug.Log($"°æÇèÄ¡ È¹µæ : {expAmount} ÇöÀç {CurrentExp}/{MaxExp}");

        while (CurrentExp>=MaxExp)
        {
            CurrentExp-=MaxExp;
            Level++;
            MaxExp = (int)(MaxExp*1.2);
            Debug.Log($"max Exp : {MaxExp}");
            OnLevelUp?.Invoke();
        }
        
    }

    public int Get_Level()
    {
        return Level;
    }

    public int Get_CurrentExp()
    {
        return CurrentExp;
    }
    public int Get_MaxExp()
    {
        return MaxExp;
    }
}
