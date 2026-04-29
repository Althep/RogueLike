using System;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

public class SaveDataManager : MonoBehaviour
{
    public string saveDirectory;
    MapManager mapManager;
    MonsterManager monsterManager;
    ItemManager itemManager;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public static SaveDataManager instance;
    
    List<string> fileNames = new List<string>();

    Dictionary<int, FloorSaveData> floorSaveDatas = new Dictionary<int, FloorSaveData>();
    private void Awake()
    {
        Init();
    }

    public void Init()
    {
        saveDirectory = Path.Combine(Application.persistentDataPath, "Saves");

        if (!Directory.Exists(saveDirectory))
        {
            Directory.CreateDirectory(saveDirectory);
        }

        if(SaveDataManager.instance == null)
        {
            SaveDataManager.instance = this;
        }
    }
    public string GetSavePath(string fileName)
    {
        // 1. 기본 영구 저장 경로에 "Saves"라는 하위 폴더 경로를 합침
        string directoryPath = Path.Combine(Application.persistentDataPath, "Saves");

        // 2. 만약 Saves 폴더가 없다면 새로 생성 (최초 실행 시 필요)
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        // 3. 폴더 경로와 파일 이름(예: "slot_1.json")을 합쳐서 최종 반환
        return Path.Combine(directoryPath, fileName);
    }


    void MakeSaveData(string name, string jsonData)
    {
        string savePath = Path.Combine(saveDirectory, name); 
        File.WriteAllText(savePath, jsonData);
        fileNames.Add(savePath);
        Debug.Log($"{savePath} 세이브 완료");
    }

    public void SaveAllFloorDatas()
    {
        FloorSaveData saveData = new FloorSaveData();
        saveData.mapData = FloorSave();
        saveData.monsterDatas = MonsterSave();
        saveData.droppedItems = ItemSave();
        int floor = DungeonManager.instance.Get_Floor();
        string name = $"floorSaveData_{floor}.json";
        string jsonData = JsonConvert.SerializeObject(saveData,Formatting.Indented);
        floorSaveDatas[floor]= saveData;
        MakeSaveData(name, jsonData);
    }

    public MapSaveData FloorSave()
    {
        MapSaveData tileSave = MapManager.instance.TileSave();
        return tileSave;
    }

    public List<LivingEntitySaveData> MonsterSave()
    {
        List<LivingEntitySaveData> datas = MonsterManager.Instance.SaveAllMonster();
        return datas;
    }

    public List<ItemEntitySaveData> ItemSave()
    {
        List<ItemEntitySaveData> datas = ItemManager.instance.SaveAllItems();
        return datas;
    }

    public ModifierSaveData Get_ModifierSaveStruct(string jsonData)
    {
        ModifierSaveData saveData =JsonConvert.DeserializeObject<ModifierSaveData>(jsonData);
        return saveData;

    }

    public bool FloorSaveFileExistTest(int floor)
    {
        string filePath = Path.Combine(saveDirectory, $"floorSaveData_{floor}");
        return File.Exists(filePath);
    }
    public FloorSaveData LoadFloorData(int floor)
    {
        string filePath = Path.Combine(saveDirectory, $"floorSaveData_{floor}");
        string jsonText = File.ReadAllText(filePath);
        FloorSaveData floorSaveData = JsonConvert.DeserializeObject<FloorSaveData>(jsonText);
        return floorSaveData;
    }


    public T LoadJsonData<T>(string fileName)
    {
        // 기기별 안전한 저장 경로(persistentDataPath)와 파일 이름을 합칩니다.
        string filePath = Path.Combine(Application.persistentDataPath, fileName);

        // 파일이 존재하는지 먼저 검사합니다.
        if (!File.Exists(filePath))
        {
            Debug.LogWarning($"[SaveLoadManager] {fileName} 파일이 없습니다. 처음 시작이거나 데이터가 지워졌습니다.");

            // 파일이 없으면 해당 타입의 기본값(클래스라면 null, 리스트라면 null 등)을 반환합니다.
            return default(T);
        }

        try
        {
            // 파일에서 JSON 문자열을 전체 읽어옵니다.
            string jsonText = File.ReadAllText(filePath);

            // JsonConvert를 이용해 문자열을 T 타입의 객체로 역직렬화합니다.
            T loadedData = JsonConvert.DeserializeObject<T>(jsonText);

            Debug.Log($"[SaveLoadManager] {fileName} 로드 완료!");
            return loadedData;
        }
        catch (System.Exception e)
        {
            // JSON 형식이 깨졌거나 타입이 안 맞을 경우를 대비한 예외 처리입니다.
            Debug.LogError($"[SaveLoadManager] {fileName} 로드 중 오류 발생: {e.Message}");
            return default(T);
        }
    }

    public FloorSaveData Get_FloorSaveData(int floor)
    {
        return floorSaveDatas[floor];
    }

    public bool IsSaved()
    {
        int floor = DungeonManager.instance.Get_Floor();
        if (floorSaveDatas.ContainsKey(floor))
        {
            if (floorSaveDatas[floor] != null)
            {
                return true;
            }
        }

        return false;
    }
}
