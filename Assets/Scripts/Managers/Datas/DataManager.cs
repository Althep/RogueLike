using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
public class DataManager 
{
    Dictionary<string, ItemBase> itemDatas = new Dictionary<string, ItemBase>();
    Dictionary<string, MonsterEntity> monsterDatas = new Dictionary<string, MonsterEntity>();
    public CSVReader csvReader = new CSVReader();
    
    public StartDataManager startDataManager { get; private set; }
    ModifierManager modifierManager;
    public ItemDataManager itemDataManager;
    public MonsterDataManager monsterDataManager;
    public static DataManager instance;
    public MonsterManager monsterManager;
    public EffectDataManager effectDataManager;
    #region 어드레서블 데이터 이름
    #region 아이템
    string ConsumItem;
    string ConsumItemModifier;
    string Equipments;
    #endregion
    #region 스타팅 데이터
    string RaceTrait_Item;
    string RaceTrait_Stat;
    string StartItems;
    string JobStats;
    string BanedJob;
    #endregion
    #region 몬스터 데이터
    string MonsterSpawnData;
    string MonsterData;
    #endregion
    #endregion
    #region 데이터 매니저
    Dictionary<System.Type, string> tpyKey = new Dictionary<Type, string>();
    #endregion
    public async UniTask Init()
    {
        if(DataManager.instance == null)
        {
            DataManager.instance = new DataManager();
        }
        if(itemDataManager == null)
        {
            itemDataManager = await ItemDataManager.CreateAsync();
        }
        if(startDataManager == null)
        {
            startDataManager = await StartDataManager.CreateAsync();
        }
        if(modifierManager == null)
        {
            modifierManager = GameManager.instance.Get_ModifierManager();
        }
        if(monsterManager == null)
        {
            monsterManager = GameManager.instance.Get_MonsterManager();
        }
        if(effectDataManager == null)
        {
            effectDataManager = await EffectDataManager.CreateAsync();
        }
        if(monsterDataManager == null)
        {
            monsterDataManager = await MonsterDataManager.CreateAsync();
        }
        await itemDataManager.Init();
        await startDataManager.Init();
        await DataRead();
    }

    public async UniTask DataRead()
    {
        var handle = Addressables.LoadAssetsAsync<TextAsset>("GameData", null);
        IList<TextAsset> allAssets = await handle.ToUniTask() ;
        Debug.Log($"모든 에셋 수 : {allAssets.Count}");

        var modifierDataMgr = await ModifierDataManager.CreateAsync();
        var itemDataMgr = await ItemDataManager.CreateAsync();
        var startDataMgr = await StartDataManager.CreateAsync();
        var monsterDataMgr = await MonsterDataManager.CreateAsync();

        await effectDataManager.SetUp(allAssets.Where(a => a.name.StartsWith("Act_")).ToList());
        await modifierDataMgr.SetUp(allAssets.Where(a => a.name.StartsWith("Mod_")).ToList());
        await itemDataMgr.SetUp(allAssets.Where(a => a.name.StartsWith("Item_")).ToList());
        await startDataMgr.SetUp(allAssets.Where(a => a.name.StartsWith("Start_")).ToList());
        await monsterManager .SetUp(allAssets.Where(a => a.name.StartsWith("Monster_")).ToList());
        
        Debug.Log("모든 하위 매니저에게 데이터 배분 완료");
    }

    public Dictionary<string,ItemBase> Get_ItemData()
    {
        return itemDatas;
    }


    public Dictionary<string,ItemBase> Get_ItemDatass()
    {
        return itemDataManager.Get_ItemDatas();
    }

    public MonsterDataManager GetMonsterDataManager()
    {
        return monsterDataManager;
    }
    public async UniTask LoadCsvData(string address)
    {
        AsyncOperationHandle<TextAsset> handle = Addressables.LoadAssetAsync<TextAsset>(address);
        await handle.Task;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            TextAsset csvFile = handle.Result;
            Debug.Log($"CSV 로드 성공 ! 내용 {csvFile.text.Substring(0, 20)}...");
        }
        else
        {
            Debug.LogError($"CSV 로드 실패 : {address}");
        }
    }

    public async UniTask LoadSpriteToImage(string address, Image targetSprite)
    {
        AsyncOperationHandle<Sprite> handle = Addressables.LoadAssetAsync<Sprite>(address);
        await handle.Task;

        if(handle.Status == AsyncOperationStatus.Succeeded)
        {
            targetSprite.sprite = handle.Result;
            Debug.Log($"이미지 로드 성공 : {address}");
        }
        else
        {
            Debug.LogError($"이미지 로드 실패 {address}");
        }
    }

    
}

public struct WeightKeyPair
{
    public float cumulativeWeight;
    public string key;
}

public struct WeightTierPair
{
    public float weight;
    public int tier;
}
