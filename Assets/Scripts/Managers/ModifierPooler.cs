using UnityEngine;
using System.Collections.Generic;

public class ModifierPooler
{
    // ModifierType을 제거하고 id(문자열)만 키값으로 사용하는 단일 딕셔너리로 최적화
    public Dictionary<string, Queue<IPoolScript>> inactiveModifiers = new Dictionary<string, Queue<IPoolScript>>();
    public Dictionary<string, List<IPoolScript>> activeModifiers = new Dictionary<string, List<IPoolScript>>();

    public Dictionary<string, Queue<IPoolScript>> inactiveModifierOptions = new();
    public Dictionary<string, List<IPoolScript>> activeModifierOptions = new();

    private ModifierManager modifierManager;
    private ModifierFactory modifierFactory;
    private ModifierDataManager modifierDataManager;
    public void Set_ModifierManager(ModifierManager MM, ModifierFactory MF,ModifierDataManager MD)
    {
        modifierManager = MM;
        modifierFactory = MF;
        modifierDataManager = MD;
    }

    public ModifierOption GetModifierOption(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            Debug.LogWarning("[ModifierPooler] 유효하지 않은 ID");
            return null;
        }
        if (!inactiveModifierOptions.ContainsKey(id))
        {
            inactiveModifierOptions[id] = new Queue<IPoolScript>();
        }
        if (!activeModifierOptions.ContainsKey(id))
        {
            activeModifierOptions[id] = new List<IPoolScript>();
        }
        if (inactiveModifierOptions[id].Count == 0)
        {
            CreateNewOption(id);
        }

        IPoolScript value = inactiveModifierOptions[id].Dequeue();
        activeModifierOptions[id].Add(value);

        return value as ModifierOption;
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

    IPoolScript CreateNew(string id)
    {
        Modifier newMod = modifierFactory.CreateNewModifier(id); //새로운 모디파이어 생성
        ModifierData modData = modifierDataManager.GetModifierData(id); //데이터 가져오기
        Defines.ModifierType modType = modData.modifierType; 
        
        newMod.SetData(modData); // 데이터셋팅
        return newMod;
    }

    IPoolScript CreateNewOption(string id)
    {
        if(modifierFactory == null)
        {
            modifierFactory = modifierManager.GetModifierFactory();
        }

        ModifierOption newOption = modifierFactory.CreatNewOption(id);

        if(newOption == null)
        {
            Debug.Log($"[ModifierPooler] 팩토리에서 {id} 생성 실패!");
            return null;
        }
        if (inactiveModifierOptions.ContainsKey(id))
        {
            inactiveModifierOptions[id] = new Queue<IPoolScript>();
        }
        inactiveModifierOptions[id].Enqueue(newOption);
        return newOption;
    }

    public void ReturnModifierOption(IPoolScript script)
    {
        ModifierOption modOption = script as ModifierOption;
        if (modOption == null) return;

        string id = modOption.optionKey;

        if (activeModifierOptions.ContainsKey(id))
        {
            activeModifierOptions[id].Remove(script);
        }
        if (!inactiveModifierOptions.ContainsKey(id))
        {
            inactiveModifierOptions[id] = new Queue<IPoolScript>();
        }
        inactiveModifierOptions[id].Enqueue(script);
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