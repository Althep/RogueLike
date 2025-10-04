using static Defines;
using UnityEngine.TextCore.Text;

public class ModifierContext
{
    public Character Target;  // 플레이어 or 몬스터
    public ActionType ActionType; // 공격, 회피, 시전 등
    public StatType StatType; // 힘, 민첩, 체력 등
    public ModifierType ModifierType;
    public float BaseValue; // 원래 값
    public float ModifiedValue; // 수정된 값
}