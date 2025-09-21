using UnityEngine;
using System.Linq;
public class FogOfWar
{
    SpriteRenderer spriteRenderer;
    bool hasSeen = false;
    Defines.VisibleState myState = Defines.VisibleState.Hidden;
    GameObject myObject;

    public FogOfWar(GameObject go)
    {
        myObject = go;
        if(spriteRenderer == null)
        {
            spriteRenderer = Utils.GetOrAddComponent<SpriteRenderer>(myObject);
        }
    }

    public void ShotLayCast(GameObject target)
    {
        Vector2 direction = target.transform.position - myObject.transform.position;
        float dist = Vector2.Distance(myObject.transform.position, target.transform.position);
        RaycastHit2D hit = Physics2D.Raycast(myObject.transform.position, direction,dist);


        if(hit.collider !=null && hit.transform.CompareTag("Player"))
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
