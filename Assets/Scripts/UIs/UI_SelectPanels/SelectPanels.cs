using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;

// UI_GridSelect를 상속하여 직접 그리드 조작 기능을 가짐
public class SelectPanels : UI_GridSelect
{
    [Header("Sub Panels")]
    public RaceSelectPanel raceSelect;
    public JobSelectPanel jobSelect;
    public ItemSelectPanel itemSelect;

    [Header("Additional UI")]
    [SerializeField] UI_ConfirmPanel confirmPanel;

    public ModifierController checker = new ModifierController();
    private UIManager uiManager;

    public UI_Base CurrentSelected;
    private void Awake()
    {
        // UI_GridSelect(부모)의 초기화 로직 수행
        // 만약 부모에 Awake가 있다면 base.Awake() 호출
        Init();
    }

    void Init()
    {
        uiManager = GameManager.instance.Get_UIManager();

        // 서브 패널들에게 중재자(자신)를 알려줌
        raceSelect.SetParentController(this);
        jobSelect.SetParentController(this);
        itemSelect.SetParentController(this);

        // 첫 단계: 종족 선택 화면 활성화
        OpenRaceSelect();
    }

    // --- 그리드 데이터 주입 및 패널 전환 ---
    public void OpenRaceSelect()
    {
        raceSelect.gameObject.SetActive(true);
        jobSelect.gameObject.SetActive(false);
        itemSelect.gameObject.SetActive(false);

        // 부모(UI_GridSelect)의 메서드를 사용하여 데이터 세팅
        SetGridData(raceSelect.GetGridData());
    }

    public void OnRaceSelected(Defines.Races race)
    {
        jobSelect.gameObject.SetActive(true);
        jobSelect.RefreshContents(race);

        // 직업 패널의 데이터를 그리드로 주입
        SetGridData(jobSelect.GetGridData());
    }

    public void OnJobSelected(Defines.Jobs job)
    {
        itemSelect.gameObject.SetActive(true);
        itemSelect.Init();

        // 아이템 패널의 데이터를 그리드로 주입
        SetGridData(itemSelect.GetGridData());
    }

    // --- 메뉴 실행 로직 오버라이드 ---
    public override void ExecuteSelectedMenu()
    {
        // 현재 그리드에서 선택된 요소를 가져옴
        UI_Base current = CurrentSelected;
        if (current == null) return;

        // 선택된 셀의 Excute를 호출 (RaceSelectCell, JobSelectCell 등이 실행됨)
        current.Excute();
    }

    // --- 게임 시작 및 씬 로드 (UniTask) ---
    public void ConfirmFunction() => SetUpAndDungeonLoad().Forget();

    private async UniTaskVoid SetUpAndDungeonLoad()
    {
        await AddItemsAsync();
        await LoadDungeonSceneAsync();
    }

    private async UniTask AddItemsAsync()
    {
        var selectedItem = itemSelect.selectItems;
        ItemManager itemManager = GameManager.instance.Get_ItemManager();
        if (itemManager == null) return;

        itemManager.AddStartItems(new List<string>(selectedItem.Values));
        await UniTask.Yield();
    }

    private async UniTask LoadDungeonSceneAsync()
    {
        await SceneManager.LoadSceneAsync("DungeonScene").ToUniTask();
        Debug.Log("Dungeon Scene Loaded");
    }
}