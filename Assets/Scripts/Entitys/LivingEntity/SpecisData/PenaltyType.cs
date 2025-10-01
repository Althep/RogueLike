using UnityEngine;

[System.Serializable]
public class Penalty
{
    // �г�Ƽ ���� (Category ��������, Specific �������� ��)
    public Defines.PenaltyTargetType targetType;

    // � ī�װ��� �������� (Equipment, Consumable, Misc)
    public Defines.ItemCategory category;

    // ���� �׸� (Sword, Potion, Key ��) �� Specific�� ���� ���
    public int targetValue;

    // ����� �ؽ�Ʈ (UI ��� ���)
    public string description;

    // ȿ�� ���� (1.0 = �״��, 0.5 = ����, 0.0 = �Ұ�)
    public float modifierValue;

    // �ƿ� ��� ���� ����
    public bool restrict;

    


}