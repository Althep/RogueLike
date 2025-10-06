using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Defines;
public class LivingEntity : MapEntity
{
    GameObject myObj;
    EntityData myData;
    protected RaceData raceData;

    private Dictionary<ModifierTriggerType, ModifierContext> modifierContext = new Dictionary<ModifierTriggerType, ModifierContext>();

    Astar pathFinder;

    Vector2Int destination;
    Defines.MoveState moveState;

    [SerializeField]float animSpeed = 10f;

    ModifierManager modifierManager;

    public RaceData Get_RaceData()
    {
        return raceData;
    }
    public void Set_RaceData(RaceData race)
    {
        raceData = null;
        modifierContext.Clear();
        modifierManager.ResetModifiers();
        raceData = race;

        List<Modifier> modifiers = raceData.modifiers;

        for(int i = 0; i<modifiers.Count; i++)
        {
            ModifierTriggerType trigger = modifiers[i].triggerType;
            modifierManager.AddModifier(trigger, modifiers[i]);
            
        }


    }
    public EntityData Get_MyData()
    {
        return myData;
    }

    public void PlayerAction(ModifierTriggerType trigger)
    {
        modifierManager.ApplyModifiers(trigger, modifierContext[trigger]);
    }
    #region OnAttack
    public bool IsEvasion(LivingEntity target)
    {
        bool isEvasion = false;

        

        return isEvasion;
    }
    #region OnItemUse

    public void UseItem(ItemBase item)
    {
        
    }
    
    public bool IsRistricted(ItemBase item)
    {
        return raceData.IsRestricted(item, modifierContext[ModifierTriggerType.OnEquip]);
    }

    #endregion
    #endregion
    #region move
    protected virtual void Move_To(Vector2Int dest)
    {
        destination = dest;
        Vector2Int myPos = new Vector2Int((int)myObj.transform.position.x, (int)myObj.transform.position.y);
        Start_Move();
        GameManager.instance.EntityMove(myPos, dest,myType);
    }


    public void Start_Move()
    {
        if (moveState == Defines.MoveState.Idle)
        {
            moveState = Defines.MoveState.Move;
            StartCoroutine("Moving");
        }
    }
    public IEnumerator Moving()
    {
        if(myObj == null)
        {
            myObj = this.gameObject;
        }
        Vector2 myPos = myObj.transform.position;
        Vector2 moveDirection = destination - myPos;
        float maxDistance = SetMoveDistance(moveDirection);
        while (Vector2.Distance(destination, myObj.transform.position) >= 0.2f)
        {
            myObj.transform.Translate(moveDirection * Time.deltaTime * animSpeed);
            if (Vector2.Distance(destination, myObj.transform.position) >= maxDistance)
            {
                Debug.Log("Break!");
                break;
            }
            yield return null;
        }
        End_Move();
    }
    public void End_Move()
    {
        myObj.transform.position = new Vector2(destination.x, destination.y);
        moveState = Defines.MoveState.Idle;
    }
    float SetMoveDistance(Vector2 moveDirection)
    {
        float maxDixtance = 1.5f;
        if (Mathf.Abs(moveDirection.x) == Mathf.Abs(moveDirection.y))
        {
            maxDixtance = 1.4f;
        }
        else
        {
            maxDixtance = 1.1f;
        }
        return maxDixtance;
    }
    #endregion
}
