using UnityEngine;
using System.Collections.Generic;
public class ItemEntity : MapEntity
{

    ItemBase item;
    public string id;
    [SerializeField] List<string> myModifiers= new List<string>();
    [SerializeField] List<string> myAddOptions = new List<string>();
    [SerializeField] Defines.ItemRarity rarity;
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
        //ViewingOptions();
    }
    public void ViewingOptions()
    {
        for(int i = 0; i<item.options.Count; i++)
        {
            myModifiers.Add(item.options[i].id);
        }

        if(item is EquipItem equip)
        {
            for(int i = 0; i<equip.addOptions.Count; i++)
            {
                myAddOptions.Add(equip.addOptions[i].id);
            }
            rarity = equip.rarity;
        }
        
    }
    public override void Return()
    {
        base.Return();
        item = null;
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
