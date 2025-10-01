using UnityEngine;

[System.Serializable]
public class Penalty
{
    // 패널티 종류 (Category 제한인지, Specific 제한인지 등)
    public Defines.PenaltyTargetType targetType;

    // 어떤 카테고리에 적용할지 (Equipment, Consumable, Misc)
    public Defines.ItemCategory category;

    // 세부 항목 (Sword, Potion, Key 등) → Specific일 때만 사용
    public int targetValue;

    // 설명용 텍스트 (UI 등에서 출력)
    public string description;

    // 효과 배율 (1.0 = 그대로, 0.5 = 절반, 0.0 = 불가)
    public float modifierValue;

    // 아예 사용 금지 여부
    public bool restrict;

    


}