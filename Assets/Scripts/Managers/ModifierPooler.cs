using UnityEngine;
using System.Collections.Generic;

public class ModifierPooler
{
    // ModifierType을 제거하고 id(문자열)만 키값으로 사용하는 단일 딕셔너리로 최적화
    public Dictionary<string, Queue<IPoolScript>> inactiveModifiers = new Dictionary<string, Queue<IPoolScript>>();
    public Dictionary<string, List<IPoolScript>> activeModifiers = new Dictionary<string, List<IPoolScript>>();

    private ModifierManager modifierManager;
    private ModifierFactory modifierFactory;

    public void Set_ModifierManager(ModifierManager MM, ModifierFactory MF)
    {
        modifierManager = MM;
        modifierFactory = MF;
    }

    public Modifier GetModifier(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            Debug.LogWarning("[ModifierPooler] 유효하지 않은 ID입니다!");
            return null;
        }

        // 1. 비활성 큐와 활성 리스트가 없으면 초기화
        if (!inactiveModifiers.ContainsKey(id))
        {
            inactiveModifiers[id] = new Queue<IPoolScript>();
        }
        if (!activeModifiers.ContainsKey(id))
        {
            activeModifiers[id] = new List<IPoolScript>();
        }

        // 2. 큐에 남은 객체가 없으면 새로 생성해서 큐에 보충
        if (inactiveModifiers[id].Count == 0)
        {
            CreateNew(id);
        }

        // 3. 큐에서 하나 꺼내서 활성 리스트로 이동
        IPoolScript value = inactiveModifiers[id].Dequeue();
        activeModifiers[id].Add(value);

        return value as Modifier;
    }

    public IPoolScript CreateNew(string id)
    {
        if (modifierFactory == null)
        {
            modifierFactory = modifierManager.GetModifierFactory();
        }

        // 핵심: 풀러는 switch문으로 타입을 구분하지 않습니다. 
        // 팩토리에게 "이 ID에 맞는 완성된 모디파이어 객체를 하나 만들어줘"라고 지시만 합니다.
        Modifier newModifier = modifierFactory.CreateNewInstance(id);

        if (newModifier == null)
        {
            Debug.LogError($"[ModifierPooler] 팩토리에서 {id} 생성에 실패했습니다.");
            return null;
        }

        // 비활성 큐가 없으면 초기화 후 삽입
        if (!inactiveModifiers.ContainsKey(id))
        {
            inactiveModifiers[id] = new Queue<IPoolScript>();
        }
        inactiveModifiers[id].Enqueue(newModifier);

        return newModifier;
    }

    public void ReturnModifier(IPoolScript script)
    {
        Modifier modifier = script as Modifier;
        if (modifier == null) return;

        string id = modifier.id;

        // 1. 활성 리스트에서 제거
        if (activeModifiers.ContainsKey(id))
        {
            activeModifiers[id].Remove(script);
        }

        // 2. 비활성 큐로 반환 (큐가 없으면 방어적으로 생성)
        if (!inactiveModifiers.ContainsKey(id))
        {
            inactiveModifiers[id] = new Queue<IPoolScript>();
        }
        inactiveModifiers[id].Enqueue(script);
    }
}