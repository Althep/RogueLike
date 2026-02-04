using UnityEngine;

public abstract class EntityData 
{
    public void Return()
    {

    }

    public abstract MapEntity CopyToEmpty();
    public abstract void ResetData();
    public string GetMyType()
    {
        return this.GetType().ToString();
    }
}
