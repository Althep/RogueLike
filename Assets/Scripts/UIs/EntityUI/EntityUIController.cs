using UnityEngine;

public class EntityUIController : MonoBehaviour
{
    [SerializeField]EntityHPBar myHPBar;
    [SerializeField] MonsterName monsterName;
    LivingEntity myEntity;

    public void InitUIs(int currentHp, int maxHp, string nameKey)
    {
        if(myHPBar!= null)
        {
            
            myHPBar.UpdateHPBar(currentHp,maxHp);
        }
        if(monsterName!= null)
        {
            monsterName.Set_MyKey(nameKey);
        }
    }

    public void UpdateUIs(int currentHp, int maxHp)
    {
        if(myHPBar != null)
        {
            myHPBar.UpdateHPBar(currentHp,maxHp);
        }
        
    }
}
