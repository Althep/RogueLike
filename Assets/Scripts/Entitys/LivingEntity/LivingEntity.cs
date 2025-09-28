using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LivingEntity : MapEntity
{
    GameObject myObj;
    EntityData myData;

    Astar pathFinder;

    Vector2Int destination;
    

    Defines.MoveState moveState;

    [SerializeField]float animSpeed = 10f;
    public EntityData Get_MyData()
    {
        return myData;
    }

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
}
