using static UnityEditor.Progress;
using UnityEngine.TextCore.Text;
using static Defines;
public class ModifireSource
{
    public Character target;  // �÷��̾� or ����
    public ModifierTriggerType triggerType; // �ߵ�����
    public StatType statType; // ��, ��ø, ü�� ��
    public Item equippedItem; // ��� ������

    public float baseValue; // ���� ��
    public float modifiedValue; // ������ ��
}