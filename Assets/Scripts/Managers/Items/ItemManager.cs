using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using static Defines;
public class ItemManager :MonoBehaviour
{
    public static ItemManager instance;
    ItemRarityManager rarityManager;
    RandomOptionManager randomOptionManager;
    [SerializeField]ItemFactory itemFactory;

    MapManager mapManager;
    PoolManager poolManager;
    int itemGenPerTile = 70;
    public Dictionary<Vector2Int, List<ItemEntity>> fieldItems = new();
    private void Awake()
    {
        Init();
    }
    public async void Init()
    {
        if(instance == null)
        {
            instance = this;
        }
        if(itemFactory == null)
        {
            itemFactory =  new ItemFactory();
        }

        if(poolManager == null)
        {
            poolManager = GameManager.instance.Get_PoolManager();
        }
        if(mapManager == null)
        {
            mapManager = GameManager.instance.Get_MapManager();
        }
        if(rarityManager == null)
        {
            rarityManager = new ItemRarityManager();
            await rarityManager.Init();
        }
        if(randomOptionManager == null)
        {
            randomOptionManager = await RandomOptionManager.CreateAsync();
        }
    }
    #region ItemMake
    public GameObject MakeItem()
    {
        GameObject go = poolManager.ObjectPool(Defines.TileType.Item);
        ItemEntity entity = go.transform.GetComponent<ItemEntity>();
        entity.Set_ItemData(itemFactory.GetRandomItem());
        return go;
    }
    public GameObject MakeRandomItem(Vector2 pos)
    {
        GameObject go = poolManager.ObjectPool(Defines.TileType.Item);
        ItemEntity entity = go.GetComponent<ItemEntity>();
        ItemBase item = itemFactory.GetRandomItem();
        entity.Set_ItemData(item);
        entity.id = entity.GetItem().name;
        Vector3 targetPos = new Vector3(pos.x, pos.y, -1);
        go.transform.position = targetPos;
        
        Vector2Int keyPos = new Vector2Int((int)pos.x, (int)pos.y);
        if (!fieldItems.ContainsKey(keyPos))
        {
            fieldItems.Add(keyPos, new List<ItemEntity>());
        }
        mapManager.AddMapData(keyPos, entity);
        fieldItems[keyPos].Add(entity);
        if(item is EquipItem equip)
        {
            Set_EquipRarity(equip);
            Set_RandomOption(equip);
        }

        entity.ViewingOptions();

        return go;
    }

    public EquipItem Set_EquipRarity(EquipItem equip)
    {
        equip.rarity = rarityManager.SetItemRarity();

        return equip;
    }

    public void Set_RandomOption(EquipItem equip)
    {
        int optionCount = randomOptionManager.Get_OptionCount(equip.rarity);
        HashSet<string> excludeOptionNames = new HashSet<string>();
        List<AddOptionData> options = new List<AddOptionData>();

        for (int i = 0; i < optionCount; i++)
        {
            // 1. 일단 옵션을 하나 뽑아옵니다.
            AddOptionData rolledOption = randomOptionManager.GetRandomOption(equip.equipCategory, equip.itemSubType, equip.tier, excludeOptionNames);

            // 2. 방어 코드: 더 이상 뽑을 옵션이 없어서 null이 나왔다면 즉시 뽑기를 중단합니다.
            if (rolledOption == null) break;

            // 3. 정상적으로 뽑혔다면 리스트에 담습니다.
            options.Add(rolledOption);

            
            excludeOptionNames.Add(rolledOption.optionCategory);
        }

        for (int i = 0; i < options.Count; i++)
        {
            string targetName = options[i].optionKey;
            Debug.Log($"옵션 이름 {options[i].optionKey}");
            ModifierOption modOption = ModifierManager.instance.Get_ModifierOption(options[i].optionKey);
            if (modOption == null)
            {
                Debug.LogError($"?? 범인 발견! ModifierManager가 {options[i].optionKey} 의  '{targetName}'(을)를 찾지 못했습니다. CSV 오타나 풀(Pool) 등록 상태를 확인하세요!");
                continue; // 에러를 띄우고, 빈칸이 리스트에 들어가는 것을 막습니다.
            }
            List<string> modKeys = ModifierManager.instance.Get_ModOptionKeys(targetName);

            foreach(var key in modKeys)
            {
                Modifier mod = ModifierManager.instance.Get_Modifier(key);
                modOption.AddNotIncludeMod(mod);
            }
            equip.addOptions.Add(modOption);
            /*
            modi.value = options[i].value;
            modi.isMulti = options[i].isMulti;
            modi.id = options[i].optionName;
            equip.addOptions.Add(modi);*/
        }
    }


    public GameObject MakeLoadedItem(Vector2Int pos,ItemBase item)
    {
        GameObject go = poolManager.ObjectPool(Defines.TileType.Item);
        ItemEntity entity = go.GetComponent<ItemEntity>();
        entity.Set_ItemData(item);
        Vector3Int target = new Vector3Int(pos.x, pos.y, -1);
        go.transform.position = target;
        if (!fieldItems.ContainsKey(pos))
        {
            fieldItems.Add(pos, new List<ItemEntity>());
        }
        mapManager.AddMapData(pos, entity);
        fieldItems[pos].Add(entity);
        return go;
    }
    public async UniTask OnVisitNewFloor()
    {
        int tileCount = mapManager.GetEmptyPosList().Count;
        int itemCount = tileCount/itemGenPerTile;
        Debug.Log($"itemCount : {itemCount}");
        for(int i = 0; i < itemCount; i++)
        {
            Vector2 targetPos = mapManager.GetRandomTilePos();
            GameObject go = MakeRandomItem(targetPos);
            await Utils.WaitYield(i);
        }
    }
    public ItemBase ItemMake(string id)
    {
        return itemFactory.GetItemScript(id);
    }
    #endregion

    
    public ItemFactory Get_ItemFactory()
    {
        if(itemFactory == null)
        {
            itemFactory = new ItemFactory();
        }
        return itemFactory;
    }

    

    public void AddStartItems(List<string> itemNames)
    {
        /*
        if(playerEntity == null)
        {
            playerEntity = GameManager.instance.Get_PlayerEntity();
        }
        InventoryData inventory = playerEntity.GetInventory();

        for(int i = 0; i < itemNames.Count; i++)
        {
            ItemBase item = itemFactory.GetItemScript(itemNames[i]);
            //inventory.AddinInventory(item);
        }
        */
    }

    public async UniTask OnFloorChange()
    {
        bool isVisitied = SaveDataManager.instance.IsSaved();
        itemFactory.OnFloorChange();
        if (SaveDataManager.instance.IsSaved())
        {
            await LoadDroppedItems();
        }
        else
        {
            Debug.Log("Visit New Floor");
            await OnVisitNewFloor();
        }
        //itemFactory.OnFloorChange();
    }

    public bool GroundCheck(Vector2Int pos)
    {
        if (fieldItems.ContainsKey(pos))
        {
            return true;
        }
        return false;
    }
    public List<ItemEntity> Get_GroundItems(Vector2Int pos)
    {
        if (fieldItems.ContainsKey(pos))
        {
            return fieldItems[pos];
        }
        return null;
    }
    #region Save&Load
    public List<ItemEntitySaveData> SaveAllItems()
    {
        List<ItemEntitySaveData> datas = new List<ItemEntitySaveData>();


        foreach(var pos in fieldItems.Keys)
        {
            List<ItemEntity> entitys = fieldItems[pos];

            for(int i = 0; i<entitys.Count; i++)
            {
                datas.Add(entitys[i].SaveEntity());
            }
        }

        return datas;
    }
    
    public async UniTask LoadDroppedItems()
    {
        List<ItemEntitySaveData> dropeedItems = SaveDataManager.instance.Get_FloorSaveData(DungeonManager.instance.Get_Floor()).droppedItems;

        for(int i = 0; i<dropeedItems.Count; i++)
        {
            Vector2Int pos = new Vector2Int(dropeedItems[i].x, dropeedItems[i].y);
            ItemBase itemSc = LoadItem(dropeedItems[i].itemData);
            MakeLoadedItem(pos, itemSc);

            await Utils.WaitYield(i);
        }
    }
    public ItemBase LoadItem(ItemSaveData saveData)
    {
        // 원본 아이템 생성
        ItemBase loadedItem = ItemMake(saveData.itemId);

        // 만약 잘못된 ID가 저장되어 생성에 실패했을 경우를 대비
        if (loadedItem == null)
        {
            Debug.LogWarning($"[LoadItem] 존재하지 않는 아이템 ID입니다: {saveData.itemId}");
            return null;
        }

        loadedItem.itemCount = saveData.itemCount;
        loadedItem.id = saveData.itemId;

        if (loadedItem is EquipItem equip)
        {
            List<ModifierOptionSaveData> addOptions = saveData.addOptionsData;

            // 방어 코드 1: 추가 옵션 데이터가 존재하는지 (Null 체크)
            if (addOptions != null && addOptions.Count > 0)
            {
                // 방어 코드 2: 장비 객체 내부에 옵션을 담을 리스트가 생성되어 있는지 확인
                if (equip.addOptions == null)
                {
                    equip.addOptions = new List<ModifierOption>();
                }

                foreach(var option in addOptions)
                {
                    ModifierOption newOption = ModifierManager.instance.Get_ModifierOption(option.optionKey);
                    foreach(var modSave in option.modifiers)
                    {
                        Modifier mod = ModifierManager.instance.Get_Modifier(modSave.modifierId);

                        if(mod != null)
                        {
                            mod.value = modSave.value;
                            newOption.AddNotIncludeMod(mod);
                        }
                    }
                    equip.addOptions.Add(newOption);
                }
                /*
                for (int i = 0; i < addOptions.Count; i++)
                {
                    // 앞서 만든 풀러를 통해 모디파이어 획득
                    Modifier modi = ModifierManager.instance.Get_Modifier(addOptions[i].modifierId);

                    // 방어 코드 3: 모디파이어를 정상적으로 가져왔는지 확인
                    if (modi != null)
                    {
                        modi.value = addOptions[i].value;
                        equip.addOptions.Add(modi);
                    }
                    else
                    {
                        Debug.LogWarning($"[LoadItem] 세이브된 모디파이어를 찾을 수 없습니다: {addOptions[i].modifierId}");
                    }
                }*/
            }
        }

        return loadedItem;
    }
    #endregion

    public void ReturnAllObjects()
    {
        foreach(var pos in fieldItems.Keys)
        {
            for(int i = 0; i<fieldItems[pos].Count; i++)
            {
                fieldItems[pos][i].Return();
            }
        }
        fieldItems.Clear();
    }
}
