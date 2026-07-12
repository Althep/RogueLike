using UnityEngine;
using System;
using System.Collections.Generic;
public class UI_Inventory : UI_GridSelect
{
    UI_InventorySlot[] slots;

    int _InventoryRow = 8;
    int _inventoryCol;

    InventoryData inventory;
    PlayerEntity playerEntity;

    UI_Base currentSelected;

    [SerializeField] GameObject inventoryConfirmObj;
    UI_InventoryConfirm inventoryConfirm;

    int selectedIndex = 0;

    ItemBase selectedItem;
    Action<ItemBase> OnSelectItem;
    Action<ItemBase> OnChangeSelection;
    [SerializeField] UI_ItemInfo itemInfo;
    private void Awake()
    {
        Init();
        this.gameObject.SetActive(false);
    }


    void Update()
    {
        
    }

    protected override void EnableFunc()
    {
        InputManager.instance.ChangeContext(GetInputType(), this, true);
        SetActive_ItemInfoPanel();
    }

    private void OnDisable()
    {
        inventoryConfirmObj.SetActive(false);
        if(selectedItem != null)
        {
            itemInfo.gameObject.SetActive(true);
        }
        
    }

    public void Init()
    {
        myInputType = Defines.InputType.GridUI;

        playerEntity = GameManager.instance.Get_PlayerEntity();
        inventory = playerEntity.GetInventory();
        playerEntity.Set_InventoryUI(this);
        if(SelectObj == null)
        {
            SelectObj = UIManager.instance.Get_PoolUI(UIDefines.UI_PrefabType.SelectView, this.gameObject);
        }
        if(inventoryConfirm == null)
        {
            inventoryConfirm = inventoryConfirmObj.transform.GetComponent<UI_InventoryConfirm>();
        }
        inventoryConfirmObj.SetActive(false);
        Set_InventorySlots();
        Set_Grid();
        Add_SelectItemEvents();
        Add_OnChangeSelectionEvent();
        Set_SelectedOBJ(new Vector2Int(0, 0));
        inventoryConfirm.Set_InventoryUI(this);

    }

    public void Add_SelectItemEvents()
    {
        OnSelectItem -= inventoryConfirm.Set_SelectedItem;
        OnSelectItem += inventoryConfirm.Set_SelectedItem;
    }
    public void Add_OnChangeSelectionEvent()
    {
        OnChangeSelection -= itemInfo.OnSelectItem;
        OnChangeSelection += itemInfo.OnSelectItem;
    }
    void Set_InventorySlots()
    {
        int inventoryMaxCount = inventory.maxInventory;
        slots = transform.GetComponentsInChildren<UI_InventorySlot>();
        int slotCount = slots.Length;
        int left = inventoryMaxCount - slots.Length;
        if (left>0)
        {
            for(int i = 0; i<left; i++)
            {
                GameObject go = UIManager.instance.Get_PoolUI(UIDefines.UI_PrefabType.InventorySlot, this.gameObject);
                UI_InventorySlot sc = go.transform.GetComponent<UI_InventorySlot>();
                slots[slotCount-1+i] = sc;
            }
        }
    }
    public void UpdateItemSlot(int index)
    {
        if(index<0 || index>=slots.Length)
        {
            Debug.Log("index Over");
        }
        ItemBase item = inventory.inventory[index];
        slots[index].UpdateSlot(item);
    }
    void Set_Grid()
    {
        if (_grid.Count!=0)
        {
            return;
        }
        ItemBase[] originData = inventory.inventory;
        int totalSlot = originData.Length;
        _inventoryCol = totalSlot / _InventoryRow;

        for(int col = 0; col<_inventoryCol; col++)
        {
            List<UI_Base> rowData = new List<UI_Base>();
            for(int row = 0; row<_InventoryRow; row++)
            {
                int index = col*_InventoryRow+row;
                rowData.Add(slots[index]);
                slots[index].Set_Index(index);
            }
            _grid.Add(rowData);
        }

    }
    public override void ChangeSelection(Vector2Int direction)
    {
        if (_grid == null || _grid.Count ==0)
            return;

        _currentRow += direction.x;
        _currentCol -= direction.y;

        if (_currentRow<0)
        {
            _currentRow = _InventoryRow-1;
            _currentCol--;
        }

        if (_currentRow>_InventoryRow-1)
        {
            _currentRow = 0;
            _currentCol++;
        }

        if (_currentCol > _inventoryCol - 1)
        {
            _currentCol = 0;
        }
        if (_currentCol < 0)
        {
            _currentCol = _inventoryCol - 1;
        }


        _currentCol = Mathf.Clamp(_currentCol, 0, _inventoryCol - 1);
        _currentRow = Mathf.Clamp(_currentRow, 0, _InventoryRow - 1);
        Debug.Log($"CurrentCol : {_currentCol} , CurrentRow{_currentRow}");
        currentSelected = _grid[_currentCol][_currentRow];

        selectedIndex = (_currentCol*_inventoryCol)+_currentRow;

        SelectObj.transform.SetParent(currentSelected.transform);
        SelectObj.transform.localPosition = Vector2.zero;

        selectedItem = inventory.inventory[selectedIndex];
        OnChangeSelection?.Invoke(selectedItem);
        SetActive_ItemInfoPanel();
    }


    public void Set_SelectedOBJ(Vector2Int pos)
    {
        currentSelected = _grid[pos.y][pos.x];

        // [핵심 1] worldPositionStays를 false로 설정하여 로컬 스케일/크기 왜곡 방지
        SelectObj.transform.SetParent(currentSelected.transform, false);
        SelectObj.transform.localPosition = Vector2.zero;

        RectTransform selectedRect = currentSelected.GetComponent<RectTransform>();
        RectTransform selectObjRect = SelectObj.GetComponent<RectTransform>();

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
    public void Set_ConfirmObj()
    {
        GameObject current = currentSelected.gameObject;
        inventoryConfirmObj.transform.SetParent(current.transform);
        inventoryConfirmObj.transform.localPosition = Vector2.zero;
        inventoryConfirmObj.SetActive(true);
    }
    public override void ExecuteSelectedMenu()
    {
        if(currentSelected == null)
        {
            return;
        }
        currentSelected.Excute();
        if (selectedItem != null)
        {
            InitInventoryConfirmPanel();
        }
    }

    public void InitInventoryConfirmPanel()
    {
        int index = selectedIndex;
        selectedItem = inventory.inventory[index]; // 선택 아이템 데이터
        OnSelectItem(selectedItem);//이벤트 실행
        inventoryConfirm.Set_SelectedItem(selectedItem);
        inventoryConfirmObj.SetActive(true);
    }

    public void SetActive_ItemInfoPanel()
    {
        if(selectedItem!= null)
        {
            itemInfo.gameObject.SetActive(true);
            
        }
        else
        {
            itemInfo.gameObject.SetActive(false);
        }
    }
}
