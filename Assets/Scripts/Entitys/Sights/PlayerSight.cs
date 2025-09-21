using UnityEngine;
using System;
using System.Collections.Generic;
public class PlayerSight : MonoBehaviour
{
    [SerializeField]float playerVision;





    void Get_InSight()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(this.gameObject.transform.position, playerVision);
        
        foreach(Collider2D col in hits)
        {
            GameObject go = col.transform.gameObject;

            TileEntity tile = Utils.GetOrAddComponent<TileEntity>(go);
            tile.Get_FOW().ShotLayCast(this.gameObject);
        }
    }
}
