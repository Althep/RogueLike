using UnityEngine;
using System;
using System.Collections.Generic;
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

    
}