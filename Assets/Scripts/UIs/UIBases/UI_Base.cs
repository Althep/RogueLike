using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class UI_Base : MonoBehaviour
{
    Dictionary<string, List<UnityEngine.Object>> _objects = new Dictionary<string, List<UnityEngine.Object>>();


    private void Start()
    {

    }

    protected void Bind<T>() where T : UnityEngine.Object
    {
        foreach (Transform child in transform.GetComponentsInChildren<Transform>(true))
        {
            string key = child.name;

            if (_objects.ContainsKey(key) == false)
                _objects[key] = new List<UnityEngine.Object>();

            if (typeof(T) == typeof(GameObject))
            {
                _objects[key].Add(child.gameObject);
            }
            else
            {
                T[] components = child.GetComponents<T>();
                if (components.Length == 0) continue;
                _objects[key].AddRange(components);
            }
        }
    }

    protected T Get<T>(string name) where T : UnityEngine.Object
    {
        List<UnityEngine.Object> objects = null;
        if (_objects.TryGetValue(name, out objects) == false)
        {
            return null;
        }
        foreach (T value in _objects[name])
        {
            return value as T;
        }
        return null;
    }

    protected TextMeshProUGUI GetTMP(string name) { return Get<TextMeshProUGUI>(name); }
    protected Button GetButton(string name) { return Get<Button>(name); }
    protected Image GetImage(string name) { return Get<Image>(name); }


    public static void AddUIEvent(GameObject go, Action<PointerEventData> action, Defines.UIEvents type)
    {

        UI_EventController evt = Utils.GetOrAddComponent<UI_EventController>(go);
        evt.OnClickHandler = null;
        switch (type)
        {
            case Defines.UIEvents.Click:
                evt.OnClickHandler -= action;
                evt.OnClickHandler += action;
                break;
            case Defines.UIEvents.DragStart:
                evt.OnBegineDragHandler -= action;
                evt.OnBegineDragHandler += action;
                break;
            case Defines.UIEvents.Drag:
                evt.OnDragHandler -= action;
                evt.OnDragHandler += action;
                break;
            case Defines.UIEvents.DragEnd:
                evt.OnDragEndHanlder -= action;
                evt.OnDragEndHanlder += action;
                break;
            default:
                break;
        }


    }
}
