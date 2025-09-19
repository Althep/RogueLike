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
    public static bool TryConvertEnum<T>(Dictionary<string, string> data, string key, ref T target) where T : struct, Enum
    {
        if (data.ContainsKey(key) && data[key] != null)
        {

            if (Enum.TryParse(data[key], true, out target))
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
        return (T)values.GetValue(i); // object ¡æ T Ä³½ºÆÃ
    }
}