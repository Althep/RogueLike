using static Defines;
using UnityEngine.TextCore.Text;
using System.Collections.Generic;
public class ModifierContext
{
    public ModifierTriggerType ActionType; // ����, ȸ��, ���� ��
    public Dictionary<StatType,int> stats; // ��, ��ø, ü�� ��
    public ModifierType ModifierType;
    public float BaseValue; // ���� ��
    public float ModifiedValue; // ������ ��
}