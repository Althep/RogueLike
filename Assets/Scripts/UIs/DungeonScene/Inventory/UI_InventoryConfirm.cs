using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class UI_InventoryConfirm : UI_VirticalSelect
{
    [SerializeField] GameObject usePanel;
    [SerializeField] GameObject throwPanel;
    [SerializeField] GameObject dropPanel;
    [SerializeField] GameObject cancelPanel;

    UI_Buttons useButton;
    UI_Buttons throwButton;
    UI_Buttons dropButton;
    UI_Buttons cancelButton;

    UI_Inventory inventoryUI;

    ItemBase selectedItem;

    PlayerEntity playerEntity;
    private void Awake()
    {

        Init();
        
        
    }

    private void Init()
    {
        myInputType = Defines.InputType.VirticalUI;
        
        if (InputManager.instance!=null)
        {
            Set_Input();
        }
        if (useButton == null)
        {
            useButton = usePanel.transform.GetComponent<UI_Buttons>();
            // UseItem 함수를 버튼 액션으로 등록
            useButton.SetMyAction(UseItem);
        }

        if (throwButton == null)
        {
            throwButton = throwPanel.transform.GetComponent<UI_Buttons>();
            // ThrowItem 함수를 버튼 액션으로 등록
            throwButton.SetMyAction(ThrowItem);
        }

        if (dropButton == null)
        {
            dropButton = dropPanel.transform.GetComponent<UI_Buttons>();
            // DropItem 함수를 버튼 액션으로 등록
            dropButton.SetMyAction(DropItem);
        }

        if (cancelButton == null)
        {
            cancelButton = cancelPanel.transform.GetComponent<UI_Buttons>();
            // CancelAction 함수를 버튼 액션으로 등록
            cancelButton.SetMyAction(CancelAction);
        }

        if(playerEntity == null)
        {
            playerEntity = PlayerController.instance.Get_PlayerEntity();
        }
        SetButtons();
        if (selectPanel == null)
        {
            selectPanel = UIManager.instance.Get_PoolUI(UIDefines.UI_PrefabType.SelectView,usePanel);
        }
        UIManager.instance.Set_ObjSize(usePanel, selectPanel);
        if (inventoryUI == null)
        {
            inventoryUI = GameObject.Find("InventoryPanel").transform.GetComponent<UI_Inventory>();
        }
    }
    private void OnEnable()
    {
        InputManager.instance.ChangeContext(Defines.InputType.VirticalUI, this, true);
        Set_Select(0);
    }
    void Start()
    {

    }

    void Update()
    {

    }

    public void Set_SelectedItem(ItemBase item)
    {
        selectedItem = item;
    }

    public void Set_InventoryUI(UI_Inventory inventory)
    {
        inventoryUI = inventory;
    }
    
    #region ButtonFunctions
    private void UseItem()
    {
        // selectedItem이 할당되지 않은 상태에서 버튼이 눌릴 경우를 대비해 Null 체크를 해줍니다.
        if (selectedItem != null)
        {
            Debug.Log($"아이템 사용: {selectedItem.name}");
            selectedItem.OnUse(playerEntity);
        }
        else
        {
            Debug.LogWarning("선택된 아이템이 없습니다!");
        }
        InputManager.instance.CloseInputUI(this);
    }

    private void ThrowItem()
    {
        Debug.Log("아이템을 멀리 던집니다. (임시 기능)");
        InputManager.instance.CloseInputUI(this);
        InputManager.instance.CloseInputUI(inventoryUI);
    }

    private void DropItem()
    {
        Debug.Log("아이템을 바닥에 내려놓습니다. (임시 기능)");
        InputManager.instance.CloseInputUI(this);
        InputManager.instance.CloseInputUI(inventoryUI);
    }

    private void CancelAction()
    {
        Debug.Log("행동을 취소하고 창을 닫습니다. (임시 기능)");
        InputManager.instance.CloseInputUI(this);

    }
    #endregion
}