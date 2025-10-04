using static Defines;
using UnityEngine.TextCore.Text;

public class ModifierContext
{
    public Character Target;  // �÷��̾� or ����
    public ActionType ActionType; // ����, ȸ��, ���� ��
    public StatType StatType; // ��, ��ø, ü�� ��
    public ModifierType ModifierType;
    public float BaseValue; // ���� ��
    public float ModifiedValue; // ������ ��
}