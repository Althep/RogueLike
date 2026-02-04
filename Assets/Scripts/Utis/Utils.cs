using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using Cysharp.Threading.Tasks;
public static class Utils
{

    public static T GetOrAddComponent<T>(GameObject go) where T : UnityEngine.Component
    {
        T compoent = go.GetComponent<T>();
        if (compoent == null)
        {
            compoent = go.AddComponent<T>();
        }
        return compoent;
    }

    public static async UniTask WaitYield(int i)
    {
        if (i % 20 == 0)
        {
            await UniTask.Yield();
        }
    }
    public static List<Dictionary<string, object>> TextAssetParse(TextAsset asset)
    {
        var list = new List<Dictionary<string, object>>();
        if (asset == null) return list;

        string csvText = asset.text;
        // 1. 줄바꿈 단위로 분리 (윈도우/맥/리눅스 호환)
        string[] lines = csvText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length <= 1) return list;

        // 2. 헤더 추출 (첫 줄)
        string[] headers = lines[0].Split(',');

        // 3. 데이터 파싱 (둘째 줄부터)
        for (int i = 1; i < lines.Length; i++)
        {
            string[] values = lines[i].Split(',');
            if (values.Length != headers.Length) continue;

            var entry = new Dictionary<string, object>();
            for (int j = 0; j < headers.Length; j++)
            {
                // 공백 제거 후 저장
                string key = headers[j].Trim();
                string value = values[j].Trim();
                entry[key] = value;
            }
            list.Add(entry);
        }

        return list;
    }
    public static GameObject FindChild(GameObject go, bool recursive = false)
    {
        Transform transform = FindChild<Transform>(go, recursive);
        if (transform == null)
        {
            return null;
        }
        return transform.gameObject;
    }

    public static T FindChild<T>(GameObject go, bool recursive = false) where T : UnityEngine.Object
    {
        if (go == null)
        {
            return null;
        }

        if (recursive == false)
        {
            for (int i = 0; i < go.transform.childCount; i++)
            {
                Transform transform = go.transform.GetChild(i);
                T component = transform.GetComponent<T>();
                if (component != null)
                {
                    return component;
                }

            }
        }
        else
        {
            foreach (T component in go.GetComponentsInChildren<T>())
            {

                return component;

            }

        }
        return null;
    }

    public static bool TrySetValue<T>(Dictionary<string, object> data, string key, ref T target)
    {
        if (data.ContainsKey(key) && data[key] != null)
        {
            try
            {
                target = (T)Convert.ChangeType(data[key], typeof(T));
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to convert{key}: {ex.Message}");
                return false;
            }
        }
        return false;
    }

    public static bool TrySetValue<T>(Dictionary<string, string> data, string key, ref T target)
    {
        if (data.ContainsKey(key) && data[key] != null)
        {
            try
            {
                target = (T)Convert.ChangeType(data[key], typeof(T));
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to convert{key}: {ex.Message}");
                return false;
            }
        }
        return false;
    }
    public static bool TryConvertEnum<T>(Dictionary<string, object> data, string key, ref T target) where T : struct, Enum
    {
        if (data.ContainsKey(key) && data[key] != null)
        {

            if (Enum.TryParse(data[key].ToString(), true, out target))
            {
                return true;
            }

        }
        return false;
    }

    public static T ConvertToEnum<T>(string data, T defaultValue = default) where T : struct, Enum
    {
        // 1. 키가 존재하는지, 데이터가 null이 아닌지 확인

        // 2. Enum 파싱 시도 (대소문자 무시)
        if (Enum.TryParse<T>(data, true, out T result))
        {
            return result;
        }

        Debug.LogWarning($"[EnumConvert] '{data}'를 {typeof(T).Name}으로 변환할 수 없습니다. 기본값({defaultValue})을 반환합니다.");

        return defaultValue;
    }

    public static T Get_RandomType<T>() where T : Enum
    {
        Array values = Enum.GetValues(typeof(T));
        int i = UnityEngine.Random.Range(0, values.Length);
        return (T)values.GetValue(i); // object → T 캐스팅
    }

    public static T[] Get_Enums<T>(T type) where T : Enum
    {
        Array values = Enum.GetValues(typeof(T));
        return (T[])values;
    }

    public static bool StringToEnum<T>(string data, ref T target) where T : struct, Enum
    {
        if (Enum.TryParse<T>(data, true, out target))
        {
            return true;
        }
        else
        {
            return false;
        }
    }



    public static Enum Get_ItemSpecificType(string data)
    {
        Defines.EquipmentType equip = default;
        Defines.ConsumableType consum = default;
        Defines.MiscType misc = default;

        if (StringToEnum(data, ref equip))
            return equip;
        else if (StringToEnum(data, ref consum))
            return consum;
        else if (StringToEnum(data, ref misc))
            return misc;

        return null; // 아무 enum에도 해당하지 않을 경우
    }

    public static string GetRandomIDToWeight(List<WeightKeyPair> datas)
    {
        List<WeightKeyPair> sorted = datas
                                        .OrderBy(x => x.cumulativeWeight)
                                        .ToList();
        for (int i = 0; i < sorted.Count; i++)
        {
            Debug.Log($"Sorted Weight : {sorted[i].cumulativeWeight}" +
                $"SortedID : {sorted[i].key}");
        }
        float totalWeight = sorted[sorted.Count - 1].cumulativeWeight;
        string id = null;
        float targetWeight = UnityEngine.Random.Range(0, totalWeight);
        Debug.Log($"타겟  : {targetWeight}");
        for (int i = 0; i < sorted.Count; i++)
        {
            if (targetWeight <= sorted[i].cumulativeWeight)
            {
                id = sorted[i].key;
                Debug.Log($"선택된 무게 : {sorted[i].cumulativeWeight}");
                return id;
            }
        }
        Debug.Log("맞는 키 존재하지않음");
        return null;

    }

    public static int GetRandomTier(Dictionary<int, float> tiers)
    {
        var sorted = tiers.OrderBy(x => x.Value).ToList();

        for (int i = 0; i < sorted.Count; i++)
        {
            Debug.Log($"Sorted Weight : {sorted[i].Value}" +
                    $"SortedID : {sorted[i].Key}");
        }
        if (sorted.Count <= 1)
        {
            Debug.Log("Sorted Count 1 ");
            return 1;
        }
        float total = sorted[sorted.Count - 1].Value;
        float targetWeight = UnityEngine.Random.Range(0, total);
        Debug.Log($"Target : {targetWeight}");
        for (int i = 0; i < sorted.Count; i++)
        {
            if (targetWeight <= sorted[i].Value)
            {
                Debug.Log($"선택 티어 {sorted[i].Key}");
                return sorted[i].Key;
            }
        }

        Debug.Log("티어 설정 실패!");
        return 1;
    }

    public static void Shuffle<T>(this List<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = UnityEngine.Random.Range(0, n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    
}