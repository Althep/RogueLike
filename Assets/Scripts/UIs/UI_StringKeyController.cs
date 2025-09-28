using UnityEngine;
using System;
using System.Collections.Generic;
using TMPro;
public class UI_StringKeyController : MonoBehaviour
{
    StringKeyManager skManager;
    string myKey;
    string myText;
    TextMeshProUGUI myTmp;

    private void Awake()
    {
        
    }

    public void Init()
    {
        Set_MyTMP();
        skManager = GameManager.instance.Get_StringKeyManager();
        skManager.Add_StringKey(this);
    }

    public void Set_MyKey(string key)
    {
        myKey = key;
    }

    public void UpdateText()
    {
        Set_MyText();
        Set_MyTMP();
    }

    public void Set_MyText()
    {
        if (skManager == null)
        {
            skManager = GameManager.instance.Get_StringKeyManager();
        }
        if (myKey == null)
        {
            Debug.Log($"MyKey Null {this.gameObject.name}");
            return;
        }
        myText = skManager.Get_StringData(myKey);
    }
    public void Set_MyTMP()
    {
        if (myTmp == null)
        {
            myTmp = Utils.GetOrAddComponent<TextMeshProUGUI>(this.gameObject);
        }
        myTmp.text = myText;
    }
}
