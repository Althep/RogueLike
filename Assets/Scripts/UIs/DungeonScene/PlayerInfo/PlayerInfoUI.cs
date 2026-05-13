using TMPro;
using UnityEngine;
using static Defines;
public class PlayerInfoUI : UI_Base
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected TextMeshProUGUI myTmp;

    private void Awake()
    {
        if (myTmp == null)
        {
            myTmp = transform.GetComponentInChildren<TextMeshProUGUI>();
        }
    }


    public virtual void UpdateValue()
    {

    }
}
