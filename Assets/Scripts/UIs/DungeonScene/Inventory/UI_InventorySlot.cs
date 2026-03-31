using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class UI_InventorySlot : UI_Buttons
{
    [SerializeField]int index;
    [SerializeField] GameObject IconGo;
    [SerializeField] GameObject ItemCountGo;
    [SerializeField] GameObject ItemNameGo;

    Image icon;
    TextMeshProUGUI itemCountText;
    TextMeshProUGUI itemNameText;
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
        if(itemNameText == null)
        {
            itemNameText = ItemNameGo.transform.GetComponent<TextMeshProUGUI>();
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
        Sprite sprite = SpriteManager.instance.GetSprite(consum.id);
        if(sprite != null)
        {
            icon.sprite = sprite;
        }
        
        itemCountText.text = consum.itemCount.ToString();
        itemNameText.text = consum.name.ToString();
        Debug.Log("ConsumUpdate");
    }

    public void SlotDataUpdate(EquipItem equip)
    {
        Sprite sprite = SpriteManager.instance.GetSprite(equip.id);
        if (sprite != null)
        {
            icon.sprite = sprite;
        }
        itemNameText.text = equip.name;
        Debug.Log("EquipUpdate");
    }

    public void SlotDataUpdate(MiscItem misc)
    {
        Sprite sprite = SpriteManager.instance.GetSprite(misc.id);
        if (sprite != null)
        {
            icon.sprite = sprite;
        }
        itemNameText.text = misc.name;
        Debug.Log("MiscUpdate");
    }
    public override void Excute()
    {
        Debug.Log($"InventorySlot Excute index : {index}");
    }
}
