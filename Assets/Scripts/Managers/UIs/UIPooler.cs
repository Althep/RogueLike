using UnityEngine;
using System;
using System.Collections.Generic;
public class UIPooler : MonoBehaviour
{

    [SerializeField] GameObject confirmUIPrefab;
    [SerializeField] GameObject mainSelectPrefab;
    [SerializeField] GameObject itemSelectPrefab;

    Dictionary<UIDefines.UI_PrefabType, Queue<IPoolUI>> ui_pool = new Dictionary<UIDefines.UI_PrefabType, Queue<IPoolUI>>();
    Dictionary<UIDefines.UI_PrefabType, List<IPoolUI>> ui_active = new Dictionary<UIDefines.UI_PrefabType, List<IPoolUI>>();

    Dictionary<UIDefines.UI_PrefabType, GameObject> ui_Prefabs = new Dictionary<UIDefines.UI_PrefabType, GameObject>();

    private void Awake()
    {
        OnAwake();
    }

    void OnAwake()
    {
        Binding_Prefabs();
    }

    private void Start()
    {

    }

    void Binding_Prefabs()
    {
        if (!ui_Prefabs.ContainsKey(UIDefines.UI_PrefabType.Confirm) && confirmUIPrefab != null)
        {
            ui_Prefabs.Add(UIDefines.UI_PrefabType.Confirm, confirmUIPrefab);
        }
        if(!ui_Prefabs.ContainsKey(UIDefines.UI_PrefabType.MainSelect) && mainSelectPrefab != null)
        {
            ui_Prefabs.Add(UIDefines.UI_PrefabType.MainSelect, mainSelectPrefab);
        }
        if (!ui_Prefabs.ContainsKey(UIDefines.UI_PrefabType.ItemSelect) && itemSelectPrefab!= null)
        {
            ui_Prefabs.Add(UIDefines.UI_PrefabType.ItemSelect, itemSelectPrefab);
        }
        foreach (var key in ui_Prefabs.Keys)
        {
            if (!ui_pool.ContainsKey(key))
            {
                ui_pool.Add(key, new Queue<IPoolUI>());
            }

        }
        foreach (var key in ui_Prefabs.Keys)
        {
            if (!ui_active.ContainsKey(key))
            {
                ui_active.Add(key, new List<IPoolUI>());
            }

        }

    }
    public Dictionary<UIDefines.UI_PrefabType, Queue<IPoolUI>> Get_Pool()
    {
        return ui_pool;
    }
    public GameObject Get(UIDefines.UI_PrefabType type, GameObject parents)
    {
        if (ui_pool[type].Count > 0)
        {
            IPoolUI poolUI = ui_pool[type].Dequeue();
            GameObject go = poolUI.Get();
            go.SetActive(true);
            go.transform.SetParent(parents.transform);
            ui_active[type].Add(poolUI);
            return go;
        }
        else
        {
            GameObject go = CreateNew(type);
            IPoolUI poolUI = go.GetComponent<IPoolUI>();
            go.transform.SetParent(parents.transform);
            ui_active[type].Add(poolUI);
            return go;
        }

    }

    public void Return(UIDefines.UI_PrefabType type, IPoolUI ui)
    {
        if (ui_active[type].Contains(ui))
        {
            ui_active[type].Remove(ui);
        }
        if (!ui_pool[type].Contains(ui))
        {
            ui_pool[type].Enqueue(ui);
            ui.Return();
        }
    }

    public void ReturnAll(UIDefines.UI_PrefabType type)
    {
        if (!ui_active.ContainsKey(type))
        {
            return;
        }
        if (ui_active[type].Count < 1)
        {
            return;
        }
        foreach (var pool in ui_active[type])
        {
            ui_pool[type].Enqueue(pool);
            pool.Get().SetActive(false);
            //ui_active[type].Remove(pool);
        }
        ui_active[type].Clear();


    }

    public GameObject CreateNew(UIDefines.UI_PrefabType type)
    {

        GameObject go = null;

        switch (type)
        {
            case UIDefines.UI_PrefabType.Confirm:
                go = Instantiate(confirmUIPrefab);
                break;
            case UIDefines.UI_PrefabType.MainSelect:
                go = Instantiate(mainSelectPrefab);
                break;
            case UIDefines.UI_PrefabType.ItemSelect:
                go = Instantiate(itemSelectPrefab);
                break;
            default:
                break;
        }
        if (go == null)
        {
            Debug.Log("go is Null in ui_Pooler CreateNew");
            return new GameObject();
        }
        return go;
    }

}


