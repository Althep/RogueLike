using UnityEngine;

public class ItemEntity : MapEntity
{

    public ItemBase item;
    public string id;
    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override MapEntity CopyToEmpty()
    {
        return new ItemEntity();
    }

    public override void ResetData()
    {
        item = null;
    }
}
