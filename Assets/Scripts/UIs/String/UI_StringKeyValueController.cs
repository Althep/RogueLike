using UnityEngine;
using System.Collections.Generic;
public class UI_StringKeyValueController : UI_StringKeyController,IPoolUI
{
    float value;

    List<string> values = new List<string>();

    public void SetMyValue(float value)
    {
        this.value = value;
    }

    public void Add_MyValue(string value)
    {
        values.Add(value);
    }
    public override void Set_MyKey(string key)
    {
        myKey = key;
        string text = StringKeyManager.Instance.Get_StringData(key);
        if(myTmp == null)
        {
            Debug.Log($"{this.gameObject.name} myTmpNull");
            return;
        }
        for(int i = 0; i < values.Count; i++)
        {
            myTmp.text = string.Format(text, values[i]);
        }
        //myTmp.text = string.Format(text, value);
    }

    public void DisableFunction()
    {
        throw new System.NotImplementedException();
    }

    public void EnableFunction()
    {
        throw new System.NotImplementedException();
    }

    public GameObject Get()
    {
        return this.gameObject;
    }

    public void Init()
    {
        throw new System.NotImplementedException();
    }

    public void Return()
    {
        UIManager.instance.Return_PoolUI(UIDefines.UI_PrefabType.ItemOption, this);
        value = 0;
        values.Clear();
        myKey = null;
    }

}
