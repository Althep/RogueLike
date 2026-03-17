using System.Collections.Generic;
using UnityEditor.ShaderGraph;
using UnityEngine;
using UnityEngine.InputSystem;
public class Input_Player : IInputController
{
    GameControlls control;
    PlayerInputController target;
    private readonly Vector2Int[] _dirVectors = new Vector2Int[]
    {
        new Vector2Int(0,1), // index 0
        new Vector2Int(-1,1), // 1
        new Vector2Int(1,1), //2
        new Vector2Int(0,0), //3
        new Vector2Int(1,0),//4
        new Vector2Int(-1,0),//5
        new Vector2Int(-1,-1),//6
        new Vector2Int(0,-1),//7
        new Vector2Int(1,-1)//8
    };

    public void Enter(object target)
    {
        control.Player.Enable();
        this.target = target as PlayerInputController;
    }

    public void Exit()
    {
        control.Player.Disable();
    }

    public void OnUpdate()
    {

    }

    public void SetUp(GameControlls controls, InputManager manager)
    {
        controls.Player.MoveN.performed += ctx => OnMoveKey(_dirVectors[0]);
        controls.Player.MoveNW.performed += ctx => OnMoveKey(_dirVectors[1]);
        controls.Player.MoveNE.performed += ctx => OnMoveKey(_dirVectors[2]);
        controls.Player.MoveSelf.performed += ctx => OnMoveKey(_dirVectors[3]);
        controls.Player.MoveE.performed += ctx => OnMoveKey(_dirVectors[4]);
        controls.Player.MoveW.performed += ctx => OnMoveKey(_dirVectors[5]);
        controls.Player.MoveSW.performed += ctx => OnMoveKey(_dirVectors[6]);
        controls.Player.MoveS.performed+= ctx => OnMoveKey(_dirVectors[7]);
        controls.Player.MoveSE.performed+= ctx => OnMoveKey(_dirVectors[8]);

        controls.Player.Inventory.performed += ctx => InventoryKey();
        controls.Player.OpenSkil.performed += ctx => SkillUIKey();
        controls.Player.OpenCharactor.performed += ctx => CharactorUIKey();
        #region interacts
        controls.Player.Interact.performed += ctx => InteractKey();
        controls.Player.UseStair.performed += ctx => UseStairKey();
        controls.Player.GetItem.performed += ctx => GetItemKey();
        #endregion

        control = controls;
    }

    private void OnMoveKey(Vector2Int driection)
    {
        target.OnMoveKey(driection);
    }
    #region UI
    private void InventoryKey()
    {
        target.InventoryKey();
    }

    private void SkillUIKey()
    {
        target.SkillUIKey();
    }

    private void CharactorUIKey()
    {
        target.CharactorUIKey();
    }
    #endregion
    #region interact
    private void InteractKey()
    {
        target.InteractKey();
    }

    private void UseStairKey()
    {
        target.UseStairKey();
    }

    private void GetItemKey()
    {
        target.GetItemKey();
    }

    #endregion
}
