using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

public class UI_InventorySlot : UI_Buttons
{
    [SerializeField]int index;
    [SerializeField] GameObject IconGo;
    [SerializeField] GameObject ItemCountGo;
    [SerializeField] GameObject ItemNameGo;

    Image icon;
    TextMeshProUGUI itemCountText;
    UI_StringKeyController itemNameController;
    private void Awake()
    {
        Init();
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void Init()
    {
        if(icon == null)
        {
            icon = IconGo.transform.GetComponent<Image>();
        }
        if(itemCountText == null)
        {
            itemCountText = ItemCountGo.transform.GetComponent<TextMeshProUGUI>();
        }
        if(itemNameController == null)
        {
            itemNameController = ItemNameGo.transform.GetComponent<UI_StringKeyController>();
        }
        AllChangeActive(false);
        
    }

    public void Set_Index(int index)
    {
        this.index = index;
    }

    public int Get_Index()
    {
        return index;
    }
    public void UpdateSlot(ItemBase item)
    {
        if(item == null)
        {
            Debug.Log("Item Null");
            AllChangeActive(false);
            return;
        }
        if(item is ConsumableItem consum)
        {
            SlotDataUpdate(consum);
            AllChangeActive(true);
            return;
        }
        if(item is EquipItem equip)
        {
            SlotDataUpdate(equip);
            OtherChangeActive(true);
            return;
        }
        if(item is MiscItem misc)
        {
            SlotDataUpdate(misc);
            OtherChangeActive(true);
            return;
        }
        
    }

    public void AllChangeActive(bool active)
    {
        Debug.Log($"All change Active {active}");
        IconGo.SetActive(active);
        ItemCountGo.SetActive(active);
        ItemNameGo.SetActive(active);
    }

    public void OtherChangeActive(bool active)
    {
        Debug.Log($"change Active {active}");
        IconGo.SetActive(active);
        ItemNameGo.SetActive(active);
        ItemCountGo.SetActive(false);
    }

    public void SlotDataUpdate(ConsumableItem consum)
    {
        Sprite sprite = SpriteManager.instance.Get_Sprite(consum.id);
        if(sprite != null)
        {
            icon.sprite = sprite;
        }
        
        itemCountText.text = consum.itemCount.ToString();
        itemNameController.Set_MyKey(consum.name);
        Debug.Log("ConsumUpdate");
    }

    public void SlotDataUpdate(EquipItem equip)
    {
        Sprite sprite = SpriteManager.instance.Get_Sprite(equip.id);
        if (sprite != null)
        {
            icon.sprite = sprite;
        }
        itemNameController.Set_MyKey(equip.name);
        Debug.Log("EquipUpdate");
    }

    public void SlotDataUpdate(MiscItem misc)
    {
        Sprite sprite = SpriteManager.instance.Get_Sprite(misc.id);
        if (sprite != null)
        {
            icon.sprite = sprite;
        }
        itemNameController.Set_MyKey(misc.name);
        Debug.Log("MiscUpdate");
    }
    public override void Excute()
    {
        Debug.Log($"InventorySlot Excute index : {index}");
    }
}
