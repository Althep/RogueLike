using UnityEngine;
using System.Linq;
public class FogOfWar
{
    SpriteRenderer spriteRenderer;
    bool hasSeen = false;
    Defines.VisibleState myState = Defines.VisibleState.Hidden;
    GameObject myObject;

    FogOfWar(GameObject go)
    {
        myObject = go;
    }

    public void ShotLayCast(GameObject target)
    {
        
        RaycastHit2D hit = Physics2D.Raycast(myObject.transform.position, target.transform.position);

        if(hit.transform.gameObject.tag == "Player")
        {
            myState = Defines.VisibleState.Visible;
            hasSeen = true;
        }
        else
        {
            if (hasSeen)
            {
                myState = Defines.VisibleState.Discovered;
            }
            else
            {
                myState = Defines.VisibleState.Hidden;
            }
        }
        OnChangeState();
    }


    void OnChangeState()
    {
        switch (myState)
        {
            case Defines.VisibleState.Visible:
                spriteRenderer.enabled = true;
                spriteRenderer.color = Color.white;
                break;
            case Defines.VisibleState.Discovered:
                if(myObject.tag == "Monster")
                {
                    spriteRenderer.enabled = false;
                }
                else
                {
                    spriteRenderer.enabled = true;
                    spriteRenderer.color = Color.gray;
                }
                break;
            case Defines.VisibleState.Hidden:
                spriteRenderer.enabled = false;
                break;
            default:
                break;
        }
    }

}
