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
        Init();
        if(SelectObj == null)
        {
            SelectObj = UIManager.instance.Get_PoolUI(UIDefines.UI_PrefabType.SelectView, this.gameObject);
        }
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
        
        SelectObj.transform.SetParent(selected.transform);
        SelectObj.transform.localPosition = Vector2.zero;
        CurrentSelected = selected;
        RectTransform selectedRect = selected.GetComponent<RectTransform>();
        RectTransform selectObjRect = SelectObj.GetComponent<RectTransform>();

        if (selectedRect != null && selectObjRect != null)
        {
            // 앵커를 중앙으로 맞추고 크기를 동일하게 설정
            selectObjRect.anchorMin = new Vector2(0.5f, 0.5f);
            selectObjRect.anchorMax = new Vector2(0.5f, 0.5f);
            selectObjRect.pivot = new Vector2(0.5f, 0.5f);

            // 대상의 실제 가로, 세로 크기를 복사
            selectObjRect.sizeDelta = selectedRect.rect.size;
        }

        Debug.Log($"{selected.gameObject.name}");
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
        await SceneController.Instance.PrePostAwaits(null, Defines.Scenes.DungeonScene, GameManager.instance.GetDungeonManager().GenerateDungeon);
        Debug.Log("던전씬 로드 완료. 던전 생성 시작.");
        
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