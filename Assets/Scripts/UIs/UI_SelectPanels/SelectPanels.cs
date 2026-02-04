using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using static Defines;
using System.Collections;

public class SelectPanels : UI_Base
{

    public RaceSelectPanel raceSelect;
    public JobSelectPanel jobSelect;
    public ItemSelectPanel itemSelect;

    [SerializeField] GameObject raceSelectPanel;
    [SerializeField] GameObject jobSelectPanel;
    [SerializeField] GameObject itemSelectPanel;
    [SerializeField] UI_ConfirmPanel confirmPanel;
    Dictionary<SlotType,GameObject> itemSelectPanels = new Dictionary<SlotType, GameObject>();
    Dictionary<SlotType, List<SelectSubCell>> itemSubPanels = new Dictionary<SlotType, List<SelectSubCell>>();

    UIManager uiManager;

    public ModifierController checker = new ModifierController();

    PlayerEntity playerEntity;
    Dictionary<Type, Type> subCellBind = new Dictionary<Type, Type>()
    {
        {typeof(Defines.Jobs),typeof(JobSelectCell) },
        {typeof(Defines.Races),typeof(RaceSelectCell) },
        {typeof(Defines.SlotType),typeof(ItemSlotListCell) }
    };

    

    private void Awake()
    {
        Init();
        //confirmPanel.Open(ConfirmFunction);
    }
    private void OnEnable()
    {
        
    }

    void Init()
    {
        uiManager = GameManager.instance.Get_UIManager();
        //AddUIEvent(confirmPanel, ConfirmFunction, Defines.UIEvents.Click);

        raceSelect = raceSelectPanel.transform.GetComponent<RaceSelectPanel>();
        jobSelect = jobSelectPanel.transform.GetComponent<JobSelectPanel>();
        playerEntity = GameManager.instance.Get_PlayerObj().transform.GetComponent<PlayerEntity>();
    }

    public void ConfirmFunction()
    {
        StartCoroutine(SeUpAndDungeonLoad());
    }



    IEnumerator SeUpAndDungeonLoad()
    {
        yield return StartCoroutine(AddItems());
        Debug.Log("아이템 추가 완료");
        yield return StartCoroutine(LoadDungeonSceneCoroutine());
    }

    public IEnumerator AddItems()
    {
        Dictionary<SlotType, string> selectedItem = itemSelect.selectItems;
        Debug.Log("컨펌 펑션!");
        ItemManager itemManager = GameManager.instance.Get_ItemManager();
        if (itemManager == null)
        {
            Debug.Log("ItemManager Null");
            yield break;
        }
        List<string> items = new List<string>();
        foreach(string name in selectedItem.Values)
        {
            items.Add(name);
        }

        itemManager.AddStartItems(items);

        Debug.Log("아이템 생성 완료");
        yield return null;
    }
    public IEnumerator LoadDungeonSceneCoroutine()
    {
        // 1. 던전 씬 비동기 로드 시작
        Debug.Log("던전씬 비동기 로드 시작!");
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync("DungeonScene");

        // 로드가 완료될 때까지 기다립니다.
        // asyncOperation.isDone이 true가 될 때까지 코루틴 일시 정지
        while (!asyncOperation.isDone)
        {
            // 로딩 진행 상황 등을 UI에 표시하려면 여기서 로직을 추가할 수 있습니다.
            // Debug.Log($"Loading progress: {asyncOperation.progress * 100}%");
            yield return null; // 다음 프레임까지 기다림
        }

        // 2. 씬 로드가 완료된 후 던전 생성 로직 실행
        Debug.Log("던전씬 로드 완료. 던전 생성 시작.");
        GenerateDungeon();
    }

    public void GenerateDungeon()
    {
        // 이곳에 던전 생성 로직을 구현합니다.
        // 예: GameManager.instance.Get_DungeonGenerator().CreateNewDungeon();
        // 예: 맵 데이터를 로드하고 플레이어를 배치하는 등의 작업
        Debug.Log("던전 생성 완료");
    }

}
