using UnityEngine;

public class TileEntity : MapEntity
{
    public GameObject upperObj;
    private void Awake()
    {
        
    }


    void Init()
    {

    }
    public override MapEntity CopyToEmpty()
    {
        return new TileEntity();
    }

    public override void ResetData()
    {
        upperObj = null;
    }

}
