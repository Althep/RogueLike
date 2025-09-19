using System;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using UnityEngine;


public class SaveDataManager : MonoBehaviour
{
    public string saveDirectory;
    MapManager mapManager;
    MonsterManager monsterManager;
    ItemManager itemManager;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    List<string> fileNames = new List<string>();

    public void JsonSave<T>(T saveClass, string fileName)
    {
        if(saveDirectory == null)
        {
            saveDirectory = Application.persistentDataPath;
        }
        string jsonString = JsonUtility.ToJson(saveClass);
        string path = Path.Combine(saveDirectory, fileName);
        
    }

    void MakeSaveData(string filePath, string jsonData)
    {
        File.WriteAllText(filePath, jsonData);
        fileNames.Add(filePath);
    }

    public T LoadFile<T>(string targetName)
    {
        if(saveDirectory == null)
        {
            saveDirectory = Application.persistentDataPath;
        }
        string filePath = Path.Combine(saveDirectory, targetName);
        if (File.Exists(filePath))
        {
            string jsonString = File.ReadAllText(filePath);
            T loadedData = JsonUtility.FromJson<T>(jsonString);
            return loadedData;
        }
        else
        {
            Debug.Log("LoadFile Failled");
            return default;
        }
    }
}
