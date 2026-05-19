using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections.Generic;
using static Defines;
public class UIManager : MonoBehaviour
{
    public static UIManager instance;
    
    Stack<UI_Base> uiStacks = new Stack<UI_Base>();
    
    //List<GameObject> opened_UIs = new List<GameObject>();
    InputUIManager inputUIManager;
    UI_ConfirmPanel confirmPanel;
    UIDataManager uidataManager;
    StringKeyManager _SKManager;
    [SerializeField]UIPooler uiPooler;
    [SerializeField]PlayerInfoPanel playerInfos;
    private void Awake()
    {
        Init();
    }


    private void Init()
    {
        if(instance == null)
        {
            instance = this;
        }
        if(uiPooler == null)
        {
            uiPooler = GameObject.Find("UIPooler").transform.GetComponent<UIPooler>();
        }
        if(inputUIManager == null)
        {
            inputUIManager = transform.GetComponent<InputUIManager>();
        }
        if(_SKManager == null)
        {
            _SKManager = transform.GetComponent<StringKeyManager>();
        }
        if(uiPooler == null)
        {
            GameObject go = new GameObject();
            uiPooler = Utils.GetOrAddComponent<UIPooler>(go);
            go.name = "UIPooler";
            GameObject Managers = GameObject.Find("Managers");
            go.transform.SetParent(Managers.transform);
        }
        if(uidataManager == null)
        {
            uidataManager = GameManager.instance.Get_DataManager().uiDataManager;
        }
        EventManager.instance.AddListnerToSceneChange(OnSceneChange);
    }
    public UIPooler Get_UIPooler()
    {
        return uiPooler;
    }
    public InputUIManager GetInputUIManager()
    {
        return inputUIManager;
    }

    public void BindPopUp(UI_PopUpObj target)
    {

    }
    public void Pop_Up_UI(string name)
    {
        GameObject go = uidataManager.GetTargetToName(name);
        if(go == null)
        {
            return;
        }
        if (go.activeSelf)
        {
            go.SetActive(false);
        }
        else
        {
            go.SetActive(true);
        }
    }
    public void OpenConfirmPanel(Action confirmAction)
    {
        if(confirmPanel == null)
        {

        }
        confirmPanel.Open(confirmAction);
    }

    public GameObject Get_PoolUI(UIDefines.UI_PrefabType type,GameObject parents)
    {
        return uiPooler.Get(type, parents);
    }

    public void Return_PoolUI(UIDefines.UI_PrefabType type,IPoolUI ui)
    {
        uiPooler.Return(type, ui);
    }

    public StringKeyManager GetStringKeyManager()
    {
        if(_SKManager == null)
        {
            _SKManager = transform.GetComponent<StringKeyManager>();
        }
        return _SKManager;
    }

    public void Set_ObjSize(GameObject parent,GameObject child)
    {
        RectTransform selectedRect = parent.GetComponent<RectTransform>();
        RectTransform selectObjRect = child.GetComponent<RectTransform>();

        if (selectedRect != null && selectObjRect != null)
        {
            // [핵심 2] Stretch(꽉 채우기) 앵커 설정
            // Awake 시점에 부모 크기가 0이더라도, 이후 레이아웃이 계산되어 부모 크기가 커지면 자동으로 맞춰집니다.
            selectObjRect.anchorMin = Vector2.zero; // (0, 0)
            selectObjRect.anchorMax = Vector2.one;  // (1, 1)
            selectObjRect.pivot = new Vector2(0.5f, 0.5f);

            // 앵커를 Stretch로 맞췄기 때문에 sizeDelta와 anchoredPosition을 0으로 주면 
            // 여백 없이 부모의 크기와 100% 동일해집니다.
            selectObjRect.sizeDelta = Vector2.zero;
            selectObjRect.anchoredPosition = Vector2.zero;
        }
    }

    public void OnSceneChange()
    {
        if(SceneController.Instance.currentScene == Scenes.DungeonScene)
        {
            playerInfos = GameObject.Find("PlayerInfo").transform.GetComponent<PlayerInfoPanel>();
        }
    }

}
