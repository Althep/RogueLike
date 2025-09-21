using UnityEngine;

public class TileEntity : MapEntity
{
    FogOfWar fow;



    private void Awake()
    {
        
    }


    void Init()
    {
        if(fow == null)
        {
            fow = new FogOfWar(this.gameObject);
        }
    }


    public FogOfWar Get_FOW()
    {
        return fow;
    }
}
