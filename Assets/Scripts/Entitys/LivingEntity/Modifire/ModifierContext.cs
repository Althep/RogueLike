using static Defines;
using UnityEngine.TextCore.Text;
using System.Collections.Generic;
public class ModifierContext
{
    public ModifierTriggerType triggerType; // �ߵ�����
    public Dictionary<StatType,int> stats; // �� �ɷ�ġ
    public Dictionary<StatType, float> multifle;
    public ModifierType ModifierType;
    public float BaseValue; // ���� ��
    public float ModifiedValue; // ������ ��

    
}