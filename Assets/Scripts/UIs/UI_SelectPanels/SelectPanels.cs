using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;
using static Defines;

public class SelectPanels : UI_GridSelect
{
    public RaceSelectPanel raceSelect;
    public JobSelectPanel jobSelect;
    public ItemSelectPanel itemSelect;

    [SerializeField] GameObject raceSelectPanelObj;
    [SerializeField] GameObject jobSelectPanelObj;
    [SerializeField] GameObject itemSelectPanelObj;
    [SerializeField] UI_ConfirmPanel confirmPanel;
    //[SerializeField] GameObject selectViewObj;
    public ModifierController checker = new ModifierController();
    
    // [수정] 미리보기(착용 제한 검사 등)를 위해 풀에서 가져온 임시 모디파이어 객체들 보관
    public List<ModifierOption> previewOptions = new List<ModifierOption>();

    private UIManager uiManager;

    UI_Base CurrentSelected;

    Vector2Int pos = Vector2Int.zero;
    private void Awake()
    {
        myInputType = Defines.InputType.GridUI;
        
        if(SelectObj == null)
        {
            SelectObj = UIManager.instance.Get_PoolUI(UIDefines.UI_PrefabType.SelectView, this.gameObject);
        }
        Init();

    }
    private void OnEnable()
    {
        EnableFunc();
    }
    protected override void EnableFunc()
    {
        base.EnableFunc();
        RefreshEverything();
    }
    void Init()
    {
        if(uiManager == null)
        {
            uiManager = GameManager.instance.Get_UIManager();
        }

        raceSelect.SetParentController(this);
        jobSelect.SetParentController(this);
        itemSelect.SetParentController(this);

        confirmPanel.SetConfirmAction(LoadDungeonSceneAsync);
        confirmPanel.SetCancelAction(CloseButton);
        RefreshEverything();
    }

    public void RefreshEverything()
    {
        raceSelect.Init();

        if (_grid != null && _grid.Count > 0 && _grid[0].Count > 0)
        {
            _currentCol = 0;
            _currentRow = 0;

            ChangeSelection(Vector2Int.zero);
        }
    }


    public void AddGridData(List<UI_Base> uis)
    {
        _grid.Add(uis);
        Debug.Log($"Call Add GridData{_grid.Count}");
    }
    public void RemoveGridData(List<UI_Base> uis)
    {
        _grid.Remove(uis);
    }
    public int GetGridCount()
    {
        return _grid.Count;
    }
    
    // [수정] 종족이 선택될 때 풀링 옵션 갱신
    public void OnRaceSelected(Races race)
    {
        UpdateCheckerModifiers();
        jobSelect.RefreshContents(race);
    }

    // [수정] 직업이 선택될 때 풀링 옵션 갱신
    public void OnJobSelected(Jobs job)
    {
        UpdateCheckerModifiers();
        itemSelect.RefreshContents(job);
    }

    // [수정] 현재 선택된 종족과 직업을 바탕으로 checker(ModifierController)를 새로 세팅
    private void UpdateCheckerModifiers()
    {
        // 1. 기존에 들고 있던 옵션들을 모두 풀로 연쇄 반납 (메모리 누수 방지)
        foreach (var opt in previewOptions)
        {
            ModifierManager.instance.Return_ModifierOption(opt);
        }
        previewOptions.Clear();

        // checker 내부 캐시 초기화
        checker = new ModifierController(); // 안전하게 새로 생성
        checker.InitAllContext(null); // 아직 Entity가 없으므로 null 또는 임시 객체

        DataManager dm = GameManager.instance.Get_DataManager();
        Races selectRace = raceSelect.selectRace;
        Jobs selectJob = jobSelect.selectJob;

        // 2. 종족 특성 읽어오기 및 런타임 풀 객체 조립
        if (dm.startDataManager.raceModDatas.ContainsKey(selectRace))
        {
            RaceModData raceData = dm.startDataManager.raceModDatas[selectRace];
            foreach (string optKey in raceData.optionKeys)
            {
                ModifierOption newOption = ModifierManager.instance.Get_ModifierOption(optKey);
                if (newOption != null)
                {
                    previewOptions.Add(newOption);
                    checker.AddMutation(newOption); // checker에 주입
                }
            }
        }

        // 3. 직업 특성 읽어오기 및 런타임 풀 객체 조립
        if (dm.startDataManager.jobStartDatas.ContainsKey(selectJob))
        {
            JobStartData jobData = dm.startDataManager.jobStartDatas[selectJob];
            foreach (string optKey in jobData.optionKeys)
            {
                ModifierOption newOption = ModifierManager.instance.Get_ModifierOption(optKey);
                if (newOption != null)
                {
                    previewOptions.Add(newOption);
                    checker.AddMutation(newOption); // checker에 주입
                }
            }
        }
    }

    public void OnItemSelected()
    {

    }
    public override void ChangeSelection(Vector2Int direction)
    {
        if (_grid == null || _grid.Count == 0) return;
        if(direction.x != 0)
        {
            _currentRow = 0;
        }
        else
        {
            _currentRow -= direction.y;
        }
        _currentCol += direction.x;
        _currentCol = Mathf.Clamp(_currentCol, 0, _grid.Count-1);
        if (_currentRow >= _grid[_currentCol].Count)
        {
            _currentRow = 0;
        }
        _currentRow = Mathf.Clamp(_currentRow, 0, _grid[_currentCol].Count);

        UI_Base selected = _grid[_currentCol][_currentRow];
        if(!(selected is UI_Buttons buttons))
        {
            selected.Excute();
        }

        CurrentSelected = selected;
        RectTransform selectedRect = selected.GetComponent<RectTransform>();
        RectTransform selectObjRect = SelectObj.GetComponent<RectTransform>();

        if (selectedRect != null && selectObjRect != null)
        {
            SelectObj.transform.SetParent(selected.transform, false);

            SelectObj.transform.SetAsLastSibling();

            selectObjRect.anchorMin = Vector2.zero; // (0, 0)
            selectObjRect.anchorMax = Vector2.one;  // (1, 1)
            selectObjRect.pivot = new Vector2(0.5f, 0.5f);

            selectObjRect.sizeDelta = Vector2.zero;
            selectObjRect.anchoredPosition = Vector2.zero;
        }
    }

    
    public override void ExecuteSelectedMenu()
    {
        CurrentSelected?.Excute();
    }

    public void ConfirmFunction()
    {
        SetUpAndDungeonLoad().Forget();
    }

    private async UniTaskVoid SetUpAndDungeonLoad()
    {
        await AddItemsAsync();
        Debug.Log("아이템 추가 완료");
        await LoadDungeonSceneAsync();
    }

    public async UniTask AddItemsAsync()
    {
        Dictionary<SlotType, string> selectedItem = itemSelect.selectItems;
        ItemManager itemManager = GameManager.instance.Get_ItemManager();
        PlayerEntity playerEntity = GameManager.instance.Get_PlayerEntity();
        if (itemManager == null)
        {
            Debug.Log("ItemManager Null");
            return;
        }
        
        List<string> items = new List<string>();
        foreach (string name in selectedItem.Values)
        {
            items.Add(name);
        }

        for(int i = 0; i<items.Count; i++)
        {
            ItemBase item = itemManager.ItemMake(items[i]);
            playerEntity.AddItem(item);
        }

        await UniTask.Yield();
    }

    private async UniTask LoadDungeonSceneAsync()
    {
        Debug.Log("던전 씬 비동기 로드 시작!");
        await SceneController.Instance.PrePostAwaits(null, Defines.Scenes.DungeonScene, CreateNewDungeon);
        Debug.Log("던전 씬 로드 완료. 던전 생성 시작.");
        
    }

    private async UniTask CreateNewDungeon()
    {
        await DungeonManager.instance.ChangeFloor(1);
    }
    public void AddConfirmPanel()
    {
        List<List<UI_Base>> data = confirmPanel.Get_GridData();
        Debug.Log("Call Add ConfirmPanel");
        for (int i = 0; i<data.Count; i++)
        {
            if (_grid.Contains(data[i]))
            {
                _grid.Remove(data[i]);
            }
            _grid.Add(data[i]);
            Debug.Log("Add ConfirmPanel");
        }
    }
    public void CloseButton()
    {
        InputManager.instance.CloseInputUI(this);
    }
    
    // [수정] 안전망: UI가 꺼질 때 고아 객체들 풀로 돌려보내기
    private void OnDisable()
    {
        foreach (var opt in previewOptions)
        {
            ModifierManager.instance.Return_ModifierOption(opt);
        }
        previewOptions.Clear();
    }

    public override void CloseUI()
    {
        //base.CloseUI();
    }
}
