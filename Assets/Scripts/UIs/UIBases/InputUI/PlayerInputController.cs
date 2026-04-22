using UnityEngine;

public class PlayerInputController : UI_InputUIBase
{
    GameControlls control;
    PlayerEntity playerEntity;
    [SerializeField]TurnManager turnManager;
    private void Awake()
    {
        myInputType = Defines.InputType.Player;
        Set_PlayerEntity();
        Init();
    }

    public void Init()
    {
        if(turnManager == null)
        {
            turnManager = TurnManager.instance;
        }
    }
    private void OnEnable()
    {
        
    }
    public void Set_PlayerEntity()
    {
        if (playerEntity == null)
        {
            playerEntity = GameManager.instance.Get_PlayerEntity();
        }
    }
    public void Set_DefaultController()
    {
        InputManager.instance.ChangeContext(GetInputType(), this, true);
    }

    public async void OnMoveKey(Vector2Int dest)
    {
        // 1. 입력 무시: 이미 행동 중이거나 턴이 넘어간 상태라면 입력을 막습니다.
        if (!playerEntity.actable)
        {
            return;
        }

        // 2. 상태 잠금: 다중 입력을 막기 위해 로직 시작 직후 조작을 잠급니다.
        // (이 역할을 TryMove 내부가 아닌, 컨트롤러 단에서 해주는 것이 핵심입니다!)
        playerEntity.ChangeActable(false);

        // 3. 행동 결과 판단
        float actPoint = playerEntity.TryMove(dest);

        // 4. 결과에 따른 분기 처리
        if (actPoint > 0)
        {
            // 행동 성공: 턴 매니저로 제어권을 넘깁니다.
            // 턴 매니저의 동작이 모두 끝난 뒤에 턴 매니저 내부에서 ChangeActable(true)를 호출해 줍니다.
            if (turnManager == null)
                turnManager = TurnManager.instance;
            await turnManager.OnTurnEnd(actPoint);
        }
        else
        {
            // 행동 실패 (actPoint == 0): 벽에 부딪히는 등 턴을 소모하지 않은 경우입니다.
            // 턴 매니저로 넘어가지 않으므로, 컨트롤러가 직접 잠금을 해제하여 
            // 플레이어가 다른 방향키를 바로 다시 누를 수 있게 해줍니다.
            playerEntity.ChangeActable(true);
        }
    }
    #region UI
    public void InventoryKey()
    {
        UIManager.instance.Pop_Up_UI("Dungeon_InventoryButton");
    }

    public void SkillUIKey()
    {

    }

    public void CharactorUIKey()
    {

    }
    #endregion
    #region interact
    public void InteractKey()
    {

    }

    public void UseStairKey()
    {

    }

    public void GetItemKey()
    {
        playerEntity.TryGetItem();
    }
    #endregion
}