using static UnityEditor.Progress;
using UnityEngine.TextCore.Text;
using static Defines;
public class ModifireSource
{
    public Character target;  // 플레이어 or 몬스터
    public ModifierTriggerType triggerType; // 발동조건
    public StatType statType; // 힘, 민첩, 체력 등
    public Item equippedItem; // 장비 아이템

    public float baseValue; // 원래 값
    public float modifiedValue; // 수정된 값
}