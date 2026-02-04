using UnityEngine;
using System;
using System.Collections.Generic;
public class PlayerSight : MonoBehaviour
{
    [SerializeField]float playerVision;
    PlayerEntity playerEntity;




    void Get_InSight()
    {
        if(playerEntity == null)
        {
            playerEntity = GameManager.instance.Get_PlayerObj().transform.GetComponent<PlayerEntity>();
        }
        Dictionary<Defines.StatType, float> stat = playerEntity.GetEntityStat(Defines.ModifierTriggerType.Passive);
        Collider2D[] hits = Physics2D.OverlapCircleAll(this.gameObject.transform.position, stat[Defines.StatType.Vision]);
        
        foreach(Collider2D col in hits)
        {
            GameObject go = col.transform.gameObject;

            TileEntity tile = Utils.GetOrAddComponent<TileEntity>(go);
            tile.GetFOW().ShotLayCast(this.gameObject);
        }
    }
}
