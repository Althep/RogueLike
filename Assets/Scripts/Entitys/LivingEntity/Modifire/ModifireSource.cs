using static UnityEditor.Progress;
using UnityEngine.TextCore.Text;
using static Defines;
public class ModifireSource
{
    public Character Target;  // �÷��̾� or ����
    public ActionType ActionType; // ����, ȸ��, ���� ��
    public StatType StatType; // ��, ��ø, ü�� ��
    public Item EquippedItem; // ��� ������

    public float BaseValue; // ���� ��
    public float ModifiedValue; // ������ ��
}