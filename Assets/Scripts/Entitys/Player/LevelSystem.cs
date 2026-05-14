using UnityEngine;
using System;
public class LevelSystem 
{
    int Level { get; set; } = 1;
    public float CurrentExp { get; set; } = 0;
    public float MaxExp { get; set; } = 10;

    public event Action<int> OnLevelUp;


    public void AddExp(float expAmount)
    {
        CurrentExp+=expAmount;
        Debug.Log($"°æÇèÄ¡ È¹µæ : {expAmount} ÇöÀç {CurrentExp}/{MaxExp}");

        while (CurrentExp>=MaxExp)
        {
            CurrentExp-=MaxExp;
            Level++;
            MaxExp *= 1.2f;

            OnLevelUp?.Invoke(Level);
        }
    }


}
