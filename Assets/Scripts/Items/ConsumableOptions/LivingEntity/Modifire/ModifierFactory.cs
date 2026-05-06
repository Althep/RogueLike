using UnityEngine;
using Cysharp.Threading.Tasks;
using static Defines;

public class ModifierFactory
{
    private ModifierDataManager modifierDataManager;
    public static ModifierFactory _instance;

    // 생성자
    private ModifierFactory() { }

    // 비동기 팩토리 생성 (게임 시작 시 ModifierManager에서 await로 호출하여 초기화를 완벽히 끝냅니다)
    public static async UniTask<ModifierFactory> CreateAsync()
    {
        if (_instance == null)
        {
            _instance = new ModifierFactory();
            // 팩토리가 생성될 때 데이터 매니저도 확실하게 세팅하고 대기합니다.
            _instance.modifierDataManager = await ModifierDataManager.CreateAsync();
        }
        return _instance;
    }

    // 풀러(Pooler)가 호출할 단일 진입점 (이전 대화에서 풀러가 호출하던 이름과 통일)
    public Modifier CreateNewInstance(string id)
    {
        // Init().Forget() 제거: 팩토리는 이미 생성 시점에 초기화가 끝났어야 정상입니다.

        Modifier targetTemplate = modifierDataManager.GetModifier(id);
        if (targetTemplate == null)
        {
            Debug.LogError($"[ModifierFactory] {id}에 해당하는 원본 데이터를 찾을 수 없습니다!");
            return null;
        }

        return GetCopyModifier(targetTemplate);
    }

    // 내부적으로 객체를 찍어내고 복사하는 로직
    private Modifier GetCopyModifier(Modifier template)
    {
        // 풀러 참조(modifierPooler = ModifierManager...) 삭제 

        switch (template.modifierType)
        {
            case ModifierType.StatModifier:
                StatModifier newStat = new StatModifier();
                // 패턴 매칭으로 바로 캐스팅하여 Copy 진행
                if (template is StatModifier statTemplate)
                {
                    statTemplate.Copy(newStat);
                }
                return newStat;

            case ModifierType.BuffModifier:
                BuffModifier newBuff = new BuffModifier();
                if (template is BuffModifier buffTemplate)
                {
                    buffTemplate.Copy(newBuff);
                }
                return newBuff;

            case ModifierType.DamageModifier:
                DamageModifier newDamage = new DamageModifier();
                if (template is DamageModifier dmgTemplate)
                {
                    dmgTemplate.Copy(newDamage);
                }
                return newDamage;

            case ModifierType.ActionModifier:
                ActionModifier newAction = new ActionModifier();
                if (template is ActionModifier actionTemplate)
                {
                    actionTemplate.Copy(newAction);
                }
                return newAction;

            default:
                Debug.LogError($"[ModifierFactory] 정의되지 않은 ModifierType입니다: {template.modifierType}");
                return null;
        }
    }
}