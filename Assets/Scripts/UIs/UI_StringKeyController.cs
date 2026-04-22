using UnityEngine;
using System;
using System.Collections.Generic;
using TMPro;
public class UI_StringKeyController : MonoBehaviour
{
    [SerializeField] string myKey;
    TextMeshProUGUI myTmp;

    private void Awake()
    {
        Set_MyTMP();
    }

    public void Set_MyKey(string key)
    {
        myKey = key;
        StringKeyManager.Instance.FontUpdate(this);
        myTmp.text = StringKeyManager.Instance.Get_StringData(key);
    }

    public void UpdateText()
    {
        Set_MyTMP();
    }

    public void Set_MyText(string text)
    {
        if (myKey == null || myTmp == null)
        {
            Debug.Log($"MyKey or my TMP Null {this.gameObject.name}");
            return;
        }
        myTmp.text = text;
    }
    public void Set_MyTMP()
    {
        if (myTmp == null)
        {
            myTmp = Utils.GetOrAddComponent<TextMeshProUGUI>(this.gameObject);
        }
        StringKeyManager keyManager = StringKeyManager.Instance;
        keyManager.Add_StringKey(this);
        
    }

    public string Get_MyKey()
    {
        return myKey;
    }
    public TextMeshProUGUI Get_MyTmp()
    {
        return myTmp;
    }
}
