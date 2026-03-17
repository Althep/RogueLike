using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UIDataManager : AsyncDataManager<UIDataManager>
{
    Dictionary<string, string> _InputNameBunds = new Dictionary<string, string>();
    Dictionary<string, GameObject> _UIBinds = new Dictionary<string, GameObject>();
    
    UIManager _UIManager;
    Transform UIRoot;
    public override async UniTask Init()
    {
        if(_UIManager == null)
        {
            _UIManager = UIManager.instance;
        }
    }
    
    public async UniTask SetUp(List<TextAsset> myAssets)
    {
        await Init();
        foreach (var asset in myAssets)
        {
            List<Dictionary<string,object>> originData = Utils.TextAssetParse(asset);

            if (asset.name.Contains("Input"))
            {
                await ReadInputUIBinds(originData);
            }
        }
        SetUIRoot();
        FindAndBindUIs();
    }

    public async UniTask ReadInputUIBinds(List<Dictionary<string,object>> data)
    {
        for(int i = 0; i < data.Count; i++)
        {
            string key = data[i]["UIName"].ToString();
            string value = data[i]["TargetName"].ToString();
            _InputNameBunds.Add(key, value);
            await Utils.WaitYield(i);
        }
    } 
    void SetUIRoot()
    {
        Canvas canvasObj = UnityEngine.Object.FindAnyObjectByType<Canvas>();
        UIRoot = canvasObj.transform;
    }
    void FindAndBindUIs()
    {
        UI_Base[] allUIBase = UIRoot.GetComponentsInChildren<UI_Base>(true);
        UI_InputUIBase[] allInputs = UIRoot.GetComponentsInChildren<UI_InputUIBase>(true);
        foreach(UI_Base uiBase in allUIBase)
        {
            string uiName = uiBase.name;
            if (!_UIBinds.ContainsKey(uiName))
            {
                _UIBinds.Add(uiName, uiBase.gameObject);
                Debug.Log($"{uiName} 바인딩 완료");
            }

        }

        foreach (var input in allInputs)
        {
            Debug.Log(11);
            string uiName = input.name;
            if (!_UIBinds.ContainsKey(uiName))
            {
                _UIBinds.Add(uiName,input.gameObject);
                Debug.Log($"{uiName} 바인딩 완료");
            }
        }
        Debug.Log($"총 {_UIBinds.Count}개의 UI 바인딩 완료");
    }

    public GameObject GetTargetToName(string name)
    {
        string targetName = _InputNameBunds[name];
        if (targetName!=null && _UIBinds.ContainsKey(targetName))
        {
            return _UIBinds[targetName];
        }
        else
        {
            Debug.Log("Target Din't Contain");
        }
        return null;
    }
}
