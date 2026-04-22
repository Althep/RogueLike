using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using static Defines;
using UnityEditor.VersionControl;
using TMPro;
public class StringKeyManager : MonoBehaviour
{
    static StringKeyManager _instance;
    [SerializeField] TMP_FontAsset myFont;
    public static StringKeyManager Instance
    {
        get
        {
            if(_instance == null)
            {
                _instance = UnityEngine.Object.FindFirstObjectByType<StringKeyManager>();
            }

            if(_instance == null)
            {
                GameObject go = new GameObject("StringKeyManager");
                _instance = go.AddComponent<StringKeyManager>();
            }
            return _instance;
        }
    }
    Dictionary<string, Dictionary<Language, string>> stringDatas = new Dictionary<string, Dictionary<Language, string>>();
    [SerializeField] Defines.Language language = Defines.Language.Kr;
    List<UI_StringKeyController> stringKeys = new List<UI_StringKeyController>();

    private void Awake()
    {
        if(StringKeyManager._instance == null)
        {
            StringKeyManager._instance = this;
        }
    }

    public void FontUpdate(UI_StringKeyController viewUI)
    {
        TextMeshProUGUI tmp = viewUI.Get_MyTmp();
        if (tmp.font != myFont)
        {
            tmp.font = myFont;
        }
    }
    public void All_Update()
    {
        for(int i = 0; i<stringKeys.Count; i++)
        {
            UI_StringKeyController texts = stringKeys[i];
            FontUpdate(texts);
            string key = texts.Get_MyKey();
            if(key != null)
            {
                string value = Get_StringData(key);
                texts.Set_MyText(value);
            }

        }
    }

    public void Add_StringKey(UI_StringKeyController target)
    {
        if (!stringKeys.Contains(target))
        {
            stringKeys.Add(target);
        }
        string targetKey = target.Get_MyKey();
        if(targetKey == null)
        {
            return;
        }
        if (!stringDatas.ContainsKey(targetKey))
        {
            return;
        }
        string value = stringDatas[targetKey][language];
        target.Set_MyText(value);
    }

    public string Get_StringData(string key)
    {

        if (!stringDatas.ContainsKey(key))
        {
            Debug.Log($"Didn't Contain language Key");
            return key;
        }
        if (!stringDatas[key].ContainsKey(language))
        {
            Debug.Log("Didn't Contain Key");
            return key;
        }

        return stringDatas[key][language];

    }

    public void SetUp(TextAsset myAsset)
    {
        ReadData(myAsset);
    }

    public void ReadData(TextAsset myAsset)
    {
        if(myAsset == null)
        {
            Debug.Log("my asset is Null StringKeyManager");
        }
        List<Dictionary<string, object>> originData = Utils.TextAssetParse(myAsset);

        for(int i = 0; i<originData.Count; i++)
        {
            string keyName = originData[i]["KeyName"].ToString();
            if (!stringDatas.ContainsKey(keyName))
            {
                stringDatas.Add(keyName,new Dictionary<Language, string>());
            }
            Dictionary<Language, string> datas = stringDatas[keyName];
            string ENData = originData[i]["En"].ToString();
            string KRData = originData[i]["Kr"].ToString();
            datas[Language.En] = ENData;
            datas[Language.Kr] = KRData;
            
        }

        All_Update();
    }
}
