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

            // 방향 이동량(Vector2Int.zero)을 0으로 넘겨주어, 위치값 변경 없이 현재(0,0) 위치로 UI만 갱신하도록 만듭니다.
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
    // 하위 패널에서 선택 시 호출
    public void OnRaceSelected(Races race)
    {
        jobSelect.RefreshContents(race);

    }

    public void OnJobSelected(Jobs job)
    {
        itemSelect.RefreshContents(job);
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
            // 1. [스케일 왜곡 방지] false를 넣어 로컬 트랜스폼 값을 깔끔하게 유지합니다.
            SelectObj.transform.SetParent(selected.transform, false);

            // 2. [가려짐 방지] 부모 객체 내에서 가장 마지막 순서로 보내 최상단에 렌더링되게 합니다.
            SelectObj.transform.SetAsLastSibling();

            selectObjRect.anchorMin = Vector2.zero; // (0, 0)
            selectObjRect.anchorMax = Vector2.one;  // (1, 1)
            selectObjRect.pivot = new Vector2(0.5f, 0.5f);

            // Stretch 앵커이므로 여백과 위치를 0으로 주면 부모와 100% 동일한 크기가 됩니다.
            selectObjRect.sizeDelta = Vector2.zero;
            selectObjRect.anchoredPosition = Vector2.zero;
        }
    }

    

    
    public override void ExecuteSelectedMenu()
    {
        CurrentSelected?.Excute();
    }

    // --- 씬 이동 및 생성 로직 ---
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
        //await AddItemsAsync();
        Debug.Log("던전씬 비동기 로드 시작!");
        await SceneController.Instance.PrePostAwaits(null, Defines.Scenes.DungeonScene, CreateNewDungeon);
        //DungeonManager.instance.ChangeFloor(1);
        Debug.Log("던전씬 로드 완료. 던전 생성 시작.");
        
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
    public override void CloseUI()
    {
        //base.CloseUI();
    }
}