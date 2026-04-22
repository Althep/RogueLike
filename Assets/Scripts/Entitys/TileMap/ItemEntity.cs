using UnityEngine;

public class ItemEntity : MapEntity
{

    ItemBase item;
    public string id;

    void Start()
    {
        SetSpriteRender();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetSpriteRender()
    {
        if(spriteRenderer == null)
        {
            spriteRenderer = this.transform.GetComponentInChildren<SpriteRenderer>();
        }
    }
    public override MapEntity CopyToEmpty()
    {
        return new ItemEntity();
    }

    public override void ResetData()
    {
        item = null;
    }

    public ItemBase GetItem()
    {
        return item;
    }

    public void Set_ItemData(ItemBase item)
    {
        this.item = item;
        spriteRenderer.sprite = SpriteManager.instance.Get_Sprite(item.name);
    }

    public override void Return()
    {
        base.Return();
        Vector2 currentPos = transform.position;
        Vector2Int mykey = new Vector2Int(Mathf.RoundToInt(currentPos.x), Mathf.RoundToInt(currentPos.y));
        MapManager.instance.RemoveMapEntity(mykey,this);
    }

    public ItemEntitySaveData SaveEntity()
    {
        ItemEntitySaveData saveData = new ItemEntitySaveData();
        ItemSaveData myItemSave = item.SaveData();
        saveData.x = Mathf.RoundToInt(transform.position.x);
        saveData.y = Mathf.RoundToInt(transform.position.y);
        saveData.itemData = myItemSave;
        return saveData;
    }
}
