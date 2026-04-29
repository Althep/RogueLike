using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class SaveTestButton : UI_Base
{
    [SerializeField]Button button;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private void Awake()
    {
        AddUIEvent(this.gameObject, AddButtonFunc, Defines.UIEvents.Click);
    }
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddButtonFunc(PointerEventData pve)
    {
        button = GetComponent<Button>();
        SaveDataManager.instance.FloorSave();
        Debug.Log("SaveTest!");
    }
}
