using UnityEngine;
using System;
using System.Collections.Generic;
public class StringKeyManager : MonoBehaviour
{


    Dictionary<string, UI_StringKeyController> stringKeyData = new Dictionary<string, UI_StringKeyController>();
    Dictionary<Defines.Language, Dictionary<string, string>> stringDatas = new Dictionary<Defines.Language, Dictionary<string, string>>();
    [SerializeField] Defines.Language language = Defines.Language.Kr;
    List<UI_StringKeyController> stringKeys = new List<UI_StringKeyController>();


    public void All_Update()
    {

    }

    public void Add_StringKey(UI_StringKeyController target)
    {
        if (!stringKeys.Contains(target))
        {
            stringKeys.Add(target);
        }
    }

    public string Get_StringData(string key)
    {

        if (!stringDatas.ContainsKey(language))
        {
            Debug.Log($"Didn't Contain language Key");
            return key;
        }
        if (!stringDatas[language].ContainsKey(key))
        {
            Debug.Log("Didn't Contain Key");
            return key;
        }

        return stringDatas[language][key];

    }

}
