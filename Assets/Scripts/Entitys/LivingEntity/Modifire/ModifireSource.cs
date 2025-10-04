using static UnityEditor.Progress;
using UnityEngine.TextCore.Text;
using static Defines;
public class ModifireSource
{
    public Character Target;  // 플레이어 or 몬스터
    public ActionType ActionType; // 공격, 회피, 시전 등
    public StatType StatType; // 힘, 민첩, 체력 등
    public Item EquippedItem; // 장비 아이템

    public float BaseValue; // 원래 값
    public float ModifiedValue; // 수정된 값
}