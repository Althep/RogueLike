using UnityEngine;
using UnityEngine.EventSystems;
using static Defines;

public class ItemSelectCell : SelectSubCell // UI_Base를 상속받는 구조여야 함
{
    [SerializeField] string itemKey;
    private ItemSlotListCell myParents;
    private SlotType slot;

    public override void SetMyType<T>(T type, SelectSubPanel panel)
    {
        base.SetMyType(type, panel);
        if (type is SlotType s)
        {
            this.slot = s;
        }
    }

    public void SetMyKey(string key) => itemKey = key;
    public void SetMyParents(ItemSlotListCell parent) => this.myParents = parent;

    // --- 그리드 시스템의 "실행" 명령에 대응 ---
    public override void Excute()
    {
        base.Excute();
        ConfirmSelection();
    }

    // 마우스 클릭 시에도 동일하게 작동
    public override void ButtonFunction(PointerEventData pev)
    {
        ConfirmSelection();
    }

    private void ConfirmSelection()
    {
        if (selectPanel is ItemSelectPanel panel)
        {
            // 1. 패널 데이터 갱신
            panel.SelectItem(itemKey, slot);
            // 2. 부모 슬롯(Row) 비주얼 갱신 (선택 표시 등)
            myParents.Select(itemKey, this.gameObject);

            Debug.Log($"[ItemSelectCell] {itemKey} Confirmed");
        }
    }
}