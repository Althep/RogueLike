using UnityEngine;

public class PlayerInputController : UI_InputUIBase
{
    GameControlls control;
    PlayerEntity playerEntity;

    private void Awake()
    {
        myInputType = Defines.InputType.Player;
        Set_PlayerEntity();
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

    public void OnMoveKey(Vector2Int dest)
    {
        playerEntity.TryMove(dest);

    }
    #region UI
    public void InventoryKey()
    {

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