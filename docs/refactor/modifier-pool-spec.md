# 모디파이어 풀링 리팩토링 스펙: 밸류 ID 기준에서 베이스 ID 기준으로

- 대상 브랜치: `ClaudeTest`
- 분석 기준: **워킹 트리**(커밋 전 변경 7개 파일 포함). 줄 번호는 모두 워킹 트리 기준이다. 과거 상태는 `git show <rev>:<path>` 기준으로 표기한다.
- 범위: 풀링, 모디파이어 생성/대여/반환 경로. 그 밖의 시스템은 풀링 변경의 영향을 직접 받는 경우에만 다룬다.
- 이 문서를 쓰면서 소스와 데이터는 수정하지 않았다.

---

## 1. 요약

1. 워킹 트리에서 리팩토링이 **절반쯤 적용된 상태**다. `ModifierPooler`는 이미 베이스 ID로 풀을 나누고(`Assets/Scripts/Managers/ModifierPooler.cs:66-111`), `AssembleOption`만 대여 후 `SetModifierValue`로 밸류를 주입한다(`ModifierFactory.cs:115-131`).
2. 하지만 다른 호출부 6곳(아이템 추가 옵션, 아이템/몬스터 로드, 버프 액션, 이펙트 복사)은 아직 밸류 ID나 "id 필드(=베이스 ID)"로 `GetModifier`를 부르고 밸류 주입 없이 그대로 쓴다.
3. 반환 쪽은 키 불일치(과거 버그 `cd4f4924`)가 `baseID` 필드로 해결됐다. 그러나 **중복 반환 방지가 없고**, **옵션이 아직 참조 중인 모디파이어를 컨트롤러가 반환**하는 구조가 남아 있다. 베이스 ID로 풀을 공유하면 이 문제가 "다른 밸류 값이 섞이는" 버그로 커진다.
4. 핵심 변경: (a) 밸류 ID 하나로 대여하는 API를 따로 두고 모든 호출부를 옮긴다, (b) Return에서 중복/외부 객체를 막는다, (c) 소유권을 하나로 정한다(옵션이 모디파이어를 소유), (d) 세이브에 valueId를 저장한다.
5. 심각도 높음 4건, 중간 6건, 낮음 6건. 구현 전에 답이 필요한 `[확인 필요]` 질문은 5건이다(9장 Q1~Q5).

---

## 2. 현재 풀 구조와 변경 후 구조 비교

### 2.1 이력: 세 가지 상태

| 시점 | 풀 키(대여) | 풀 키(반환) | 밸류 주입 | 근거 |
|---|---|---|---|---|
| `cd4f4924` ("모디파이어 풀링 버그 수정") | 인자 `id`(밸류 ID, `modifierDatas`가 행 ID로 저장됨) | `modifier.id`. 그런데 `SetData`가 `id = modData.modifierId`(베이스 ID)로 덮어씀. `Modifier.Return()`은 `Reset()`(id=null)부터 부른 뒤 반환 | `SetData(ModifierData)` | `git show cd4f4924:Assets/Scripts/Managers/ModifierPooler.cs` 66-112, 188-245줄 / 같은 rev의 `Modifier.cs` 48-56, 72-73줄 / `ModifierDataManager.cs` 143-145줄 |
| `HEAD`(c236b2fe "모디파이어 수정작업중") | 인자는 밸류 ID, 풀은 `modData.modifierId`(베이스 ID) | `modifier.baseID` | `SetData(modData)` + `Set_BaseModData` | `git show HEAD:Assets/Scripts/Managers/ModifierPooler.cs` 65-119, 190-210줄 |
| 워킹 트리(현재) | 인자 = **베이스 ID** | `modifier.baseID` | 호출부가 `SetModifierValue(ModifierValueData)`를 불러야 함 | `ModifierPooler.cs:66-111`, `:183-203` |

- `cd4f4924`의 버그는 대여 키(밸류 ID)와 반환 키(베이스 ID 또는 Reset 후 null)가 서로 달랐던 것이다. 그래서 객체가 대여되지 않는 풀로 흘러가거나 반환이 거부됐다. 아래 "기존 풀링 버그 재발"은 이 문제를 말한다.
- `HEAD`의 `ModifierFactory.AssembleOption`은 `modBase.SetData(modifierValueData)`를 부르는데 `SetData`의 시그니처는 `ModifierData`다. 그래서 HEAD는 컴파일이 안 됐을 가능성이 높다(`git diff`의 ModifierFactory.cs와 Modifier.cs 제거 줄). `[확인 필요]` Q1 참고.

### 2.2 현재(워킹 트리) 구조

| 항목 | 내용 | 근거 |
|---|---|---|
| 풀 클래스 | `ModifierPooler`(일반 클래스, `ModifierManager`가 소유) | `ModifierPooler.cs:4`, `ModifierManager.cs:54-58, 70-78` |
| 모디파이어 풀 저장 구조 | `Dictionary<string, Queue<IPoolScript>> inactiveModifiers`, `Dictionary<string, List<IPoolScript>> activeModifiers` | `ModifierPooler.cs:7-8` |
| 옵션 풀 저장 구조 | `inactiveModifierOptions`/`activeModifierOptions`, 키 = optionKey | `ModifierPooler.cs:10-11` |
| 모디파이어 풀 키 | `baseID`(`Mod_*.csv`의 `ID`) | `ModifierPooler.cs:85-101`, `:188` |
| 대여 시 초기화 | `mod.id = baseID; mod.baseID = baseID; mod.Set_BaseModData(baseMod)` | `ModifierPooler.cs:104-106` |
| 밸류 주입 | 풀은 하지 않음. `AssembleOption`만 `mod.SetModifierValue(optionData.modDatas[i])` 호출 | `ModifierFactory.cs:124-130` |
| 풀이 비었을 때 | `CreateNewBase`, 즉 `ModifierFactory.CreateNewModifier`가 `new XxxModifier()` (GC 할당) | `ModifierPooler.cs:95-98, 113-127`, `ModifierFactory.cs:28-59` |
| Prewarm | **없음** (`Prewarm` 류 메서드 없음, 모든 풀은 첫 대여 때 생성) | `ModifierPooler.cs` 전체 |
| 반환 | `activeModifiers[baseID].Remove` 후 `Reset()`, `inactiveModifiers[baseID].Enqueue` | `ModifierPooler.cs:183-203` |
| Reset이 baseID/id를 보존하는가 | 보존함(`//id = null;` 주석 처리, baseID는 건드리지 않음) | `Modifier.cs:66-76` |
| 옵션 반환 | 들고 있는 myMods를 모두 `ReturnModifier`로 반환한 뒤 Clear, 옵션은 inactive로 | `ModifierPooler.cs:149-181` |

### 2.3 밸류 ID와 베이스 ID의 관계(데이터)

- 베이스: `Mod_*.csv`의 `ID` 열이다. `ModifierDataManager.ReadModifierBase`가 `ModifierType` 열로 타입별 템플릿을 만들어 `modifierBases[id]`에 넣는다(`ModifierDataManager.cs:45-78`).
- 밸류: `ModOption_*.csv`의 각 행이다. `ID` 열 = **밸류 ID**(`ModifierValueData.valueId`), `ModifierID` 열 = **베이스 ID**(`modifierId`), `OptionKey` 열 = 옵션 그룹 키(`ModifierDataClasses.cs:28-45`).
- 저장: `modifierOptionGroups[optionKey].modDatas`(List<ModifierValueData>)와 `optionIds[optionKey]`(밸류 ID 리스트)(`ModifierDataManager.cs:92-128`).
- **밸류 ID에서 ModifierValueData를 바로 찾는 인덱스가 없다.** 밸류 ID로 찾으려면 옵션 그룹을 거쳐야 한다(`ModifierDataManager.cs:137-145`).
- 관계는 N:1이다. 예: `ModOption_JobStat.csv`의 `Warrior_Str`, `Rogue_Str` 등 7행이 모두 `Job_Str` 베이스를 쓴다. 집계 결과(Mod_ 베이스 83개, ModOption 밸류 331행) 공유가 가장 많은 베이스는 `Weapon_Damage`(41개 밸류), `Weapon_Accurency`(27), `Weapon_Accurancy`(14), `Weapon_TwoHand`(12), `Weapon_AttackSpeed`(12)다.
- 한 옵션 안에서 같은 베이스를 두 번 쓰는 경우도 있다. 예: `Elf_LongSword_0`/`_1`은 둘 다 `Sword_Damage`이고 하나는 고정값, 하나는 배율이다(`ModOption_Item.csv:2-3`). 그래서 **베이스 ID만으로는 옵션 안의 모디파이어를 구분할 수 없다.**
- 데이터 무결성(집계 결과, 데이터는 수정하지 않음):
  - `ModifierID`가 어떤 `Mod_*.csv`에도 없는 밸류 행이 **73행** 있다. 예: `ModOption_Action.csv`의 `Action_*` 30행 전부, `Weapon_Accurancy`(AddOptions 14행), `Weapon_Accurency`(Stat 27행; 베이스는 `Weapon_Accuracy`), `Weapon_MagicAccuracy`, `Buff_LifeSteal`, `Buff_Telleport`. 현재 코드에서는 이 행들이 `Get_ModifierBase`에서 LogError와 null을 거쳐 조용히 건너뛰어진다(`ModifierPooler.cs:77-81`, `ModifierFactory.cs:124-128`).
  - **밸류 ID와 베이스 ID가 겹치는 경우가 1건** 있다. `ShortSword_Damage`는 밸류 ID이면서(`ModOption_Stat.csv:8`, 베이스는 `Weapon_Damage`=StatModifier) 베이스 ID이기도 하다(`Mod_ItemModifier.csv:12`, ItemModifier). 밸류 ID를 `GetModifier`에 넘기는 호출이 남아 있으면 **다른 타입의 풀에서 조용히 꺼내 온다.**

### 2.4 변경 후 구조(목표)

| 항목 | 현재 | 변경 후 |
|---|---|---|
| 풀 키 | baseID | baseID (그대로) |
| 공개 대여 API | `GetModifier(baseID)` 하나. 호출부마다 밸류를 주입하거나 빠뜨림 | `RentByValue(ModifierValueData)` / `RentByValueId(string valueId)`를 기본으로 쓴다. `GetModifier(baseID)`는 `internal`/private로 숨기거나 "밸류 없는 대여"임을 이름에 드러낸다(`RentBaseOnly`) |
| 대여 순서 | Dequeue, id/baseID 세팅, Set_BaseModData, (호출부가 할 수도 있는) SetModifierValue | Dequeue, **IsPooled 검사**, id/baseID 세팅, Set_BaseModData, SetModifierValue, active 등록. 모두 풀 안에서 한 번에 처리 |
| 밸류 조회 | 옵션 그룹을 거쳐서만 가능 | `ModifierDataManager.Get_ModifierValueData(valueId)` 인덱스 추가 |
| 인스턴스 식별 | `id`(= baseID) | `valueId`(밸류 단위), `baseID`(풀 단위), 객체 참조(소유 단위) |
| 반환 | 무조건 Enqueue | 키(baseID) 검증 + active 리스트에서 실제로 제거됐을 때만 Enqueue (중복/외부 반환 차단) |
| 소유권 | 옵션 myMods와 컨트롤러 리스트가 같은 객체를 들고, 컨트롤러가 반환 | 옵션(또는 단독 대여자)만 반환 권한을 가짐. 컨트롤러는 리스트에서 빼기만 함 |
| Prewarm | 없음 | 데이터 로드 직후 베이스별 개수 지정 Prewarm (5장) |
| 세이브 | `modifierId = id`(베이스), value | `valueId` 추가, 로드 시 valueId로 재조립 |

---

## 3. 밸류 필드 세팅 대조표

### 3.1 ModifierValueData 필드에서 Modifier로 (대여 + 주입 경로)

"생성 시 세팅"은 워킹 트리의 `GetModifier`(Set_BaseModData)와 `SetModifierValue`를 **둘 다 부르는 경로**(`AssembleOption`) 기준이다. 둘 중 하나만 부르는 경로는 3.3에서 다룬다.

| 필드 (ModifierValueData 기준) | 생성 시 세팅 여부 | Reset 시 초기화 여부 | 근거 |
|---|---|---|---|
| valueId → `Modifier.valueId` | 항상 세팅 | 예 (null) | `Modifier.cs:34`, `:69` |
| modifierId → `Modifier.baseID`/`id` | 풀이 baseID로 세팅 (밸류 데이터에서 복사하지 않음) | 아니오 (의도적으로 보존, 반환 키) | `ModifierPooler.cs:104-105`, `Modifier.cs:68` |
| optionKey | Modifier에 저장하지 않음 | 해당 없음 | `Modifier.cs:9-20` |
| value | 항상 세팅 | 예 (0) | `Modifier.cs:35`, `:75` |
| isMulti | 조건부: `hasIsMulti`일 때만. 아니면 Set_BaseModData의 베이스 값 | 예 (false) | `Modifier.cs:37-40`, `:50`, `:74`, `ModifierDataClasses.cs:33` |
| priority | 조건부: `hasPriority`일 때만. 아니면 베이스 값 | 예 (0) | `Modifier.cs:41-44`, `:52`, `:73`, `ModifierDataClasses.cs:35` |
| descKey | 항상 세팅 (열이 없으면 null) | 예 (null) | `Modifier.cs:36`, `:70` |
| keyOder | Modifier에 저장하지 않음 | 해당 없음 | `ModifierDataClasses.cs:37` |
| hasIsMulti / hasPriority | 플래그로만 쓰고 저장하지 않음 | 해당 없음 | `ModifierDataClasses.cs:19-20` |
| stringValue | Buff만: 비어 있지 않을 때 덮어씀. 아니면 Set_BaseModData의 베이스 값 | Buff/Action은 예, **기본 Modifier.Reset은 아니오** (단 대여 때 Set_BaseModData가 덮어써서 문제 없음) | `Modifier.cs:204-207`, `:49`, `:243`, `:388`, `:66-76` |
| effectName | Buff만: 비어 있지 않을 때 덮어씀. 그런데 `ModOption_Buff.csv` 헤더에 `effectName` 열이 없어 **사실상 항상 미세팅**. Set_BaseModData도 effectName을 복사하지 않음 | **Buff: 아니오** / Action: 예 | `Modifier.cs:208-211`, `:237-247`, `:387`, `ModOption_Buff.csv:1` |
| minDuration → `minTime` | Buff 항상 | 예 | `Modifier.cs:200`, `:241` |
| maxDuration → `maxTime` | Buff 항상 | 예 | `Modifier.cs:201`, `:242` |
| buffCategory | Buff 항상 | 예 | `Modifier.cs:202`, `:245` |
| buffType | Buff 항상 | **아니오** | `Modifier.cs:203`, `:237-247` |

### 3.2 베이스 템플릿 필드에서 Modifier로 (Set_BaseModData)

| 필드 | 대여 시 세팅 | Reset 초기화 | 근거 |
|---|---|---|---|
| stat | 예 (Stat은 stringValue로 다시 파싱) | 예 (Accuracy) | `Modifier.cs:48`, `:166`, `:72` |
| triggerType | 예 | 예 (Passive) | `Modifier.cs:51`, `:71` |
| modifierType | 예 (생성자에서도 세팅) | 아니오 (같은 베이스면 같은 타입이라 무방) | `Modifier.cs:53`, `:119,187,269,416` |
| ItemModifier: itemTargetType / itemCategory / specificType / Equipable | **아니오** (템플릿에만 있음, 복사 안 함) | itemTargetType·itemCategory만 예, specificType·Equipable은 아니오 | `Modifier.cs:418-434`, `:483-488`, `:46-54` |
| Buff/Action: effectName (베이스 열 `EffectName`) | **아니오** | Buff 아니오 / Action 예 | `Modifier.cs:256-260`, `:393-397`, `:46-54` |
| DamageModifier: damageType / evokeRate | 아니오 (세팅하는 경로 자체가 없음) | 예 | `Modifier.cs:262-318` |
| ActionModifier: minValue / maxValue | 아니오 | 예 | `Modifier.cs:323-324`, `:389-390` |

이 누락은 리팩토링 전부터 있던 것이다(`HEAD`와 `cd4f4924`의 `SetData`도 해당 필드를 복사하지 않음). 고치면 게임 동작이 바뀌므로 이번 범위에서는 **그대로 유지**하고 Q7로 남긴다.

### 3.3 런타임에 바뀌는 필드

| 필드 | 누가 바꾸나 | Reset/대여 시 초기화 | 근거 |
|---|---|---|---|
| BuffModifier.duration | `EffectApplier.ApplySynchronized`가 세팅, `Apply`가 0으로 | Reset 예 | `EffectApplier.cs:18`, `Modifier.cs:194`, `:246` |
| BuffModifier.myAction | `SetMyAction` (호출부 없음) | Reset 예 | `Modifier.cs:248-254`, `:240` |
| ActionModifier.action | `SetAction` (호출부 없음) / `SetConsumableFlag`가 내부 플래그 변경 | Reset 예 | `Modifier.cs:356-368`, `:386`, `ConsumableItem.cs:22` |
| DamageModifier.canEvoke | `CanEvoke()`가 매번 다시 계산 | Reset 아니오 (매번 덮어써서 무방) | `Modifier.cs:266`, `:295-302` |
| value (로드 경로) | 세이브 값으로 덮어씀 | 해당 없음 | `ItemManager.cs:304`, `MonsterManager.cs:377` |
| 소스/소유자 참조 | 모디파이어 쪽에는 필드가 없음. 대신 `BuffInstance.modifier`, `StatBuff.modifier`, `ModifierOption.myMods`, 컨트롤러 리스트가 모디파이어를 가리킴 | BuffInstance.Reset은 참조 해제, StatBuff는 Reset 때만 | `Defines.cs:4-40`, `ModifierAction.cs:95-127` |

### 3.4 밸류 주입 없이 대여하는 경로 (값이 비거나 이전 밸류가 남는 지점)

| 호출 위치 | 넘기는 ID | 결과 |
|---|---|---|
| `ItemManager.cs:124-129` | `Get_ModOptionKeys`가 돌려준 **밸류 ID** (`ModifierDataManager.cs:88,126`) | 대부분 null이 나오고 `AddNotIncludeMod(null)`에서 NRE(`ModifierOption.cs:67`). `ShortSword_Damage`는 ItemModifier 풀에서 엉뚱하게 대여됨 |
| `ItemManager.cs:300-305` | 세이브의 `modifierId`(= `mod.id` = baseID, `Modifier.cs:80`) | value만 복원. valueId/descKey는 null, isMulti/priority는 베이스 값, Buff의 min/max는 0, buffType은 **이전 사용자 값이 남음** |
| `MonsterManager.cs:363` | 위와 같음 | 위와 같음 + value도 복원 안 함 |
| `MonsterManager.cs:375-379` | `BuffSaveData.modifierId`(= `modifier.id`, `ModifierController.cs:361`) | value만 복원 |
| `ModifierAction.cs:101` (`StatBuff.GetMyModifier`) | `effectID` (`EffectDataManager.cs:73`의 Act_ CSV `ID`) | 밸류 주입 없음. 베이스 ID가 아니면 null이 나와 `Add_Buff(null)`. `[확인 필요]` Q4 |
| `EffectDataManager.cs:199` | `originBuff.modifier.id` | 위와 같음. 원본 StatBuff.modifier가 어디서 세팅되는지 찾지 못함. `[확인 필요]` Q4 |

---

## 4. 식별 기준 변경 영향

모디파이어 인스턴스는 이제 `valueId`(`Modifier.cs:18`)와 `baseID`(`:17`)를 자체 필드로 가진다. `id`는 대여 때마다 baseID로 덮어써진다(`ModifierPooler.cs:104`). 따라서 **`id`로 비교하는 로직은 모두 "베이스 단위"로 동작한다.**

| 위치 | 현재 구분 방식 | 변경 후 문제 | 제안 | 근거 |
|---|---|---|---|---|
| `ModifierPooler.GetModifier/ReturnModifier` | baseID (풀 키) | 없음. 단 baseID가 null인 객체가 반환되면 `Dictionary.ContainsKey(null)`에서 ArgumentNullException | 반환 시 null/빈 baseID를 막고 경고 | `ModifierPooler.cs:188-199` |
| `ModifierOption.AddNotIncludeMod` | `m.id != mod.id` (= baseID). 조건도 뒤집혀 있음(다른 id가 하나라도 있으면 추가 안 함) | 같은 베이스·다른 밸류(`Elf_LongSword_0/1`)를 구분 못 함. null 인자에서 NRE | valueId 기준 "같은 valueId가 없으면 추가"로 바꾼다. 의도는 Q8로 확인 | `ModifierOption.cs:65-71` |
| `ModifierController.AddModifierInternal/RemoveModifiernInternal` | 객체 참조(`List.Add/Remove`) + `triggerType`으로 리스트 선택 | 참조 기준이라 문제 없음. 단 Reset 후 `triggerType=Passive`가 되므로 반환된 객체로 다시 Remove하면 엉뚱한 리스트를 뒤짐 | 반환은 소유자만 하도록 하고 Remove는 반환하지 않음 (7장 6단계) | `ModifierController.cs:88-109`, `:153-166`, `Modifier.cs:71` |
| `ActionModifier.Apply` (중복 갱신) | `BuffAction.effectName == this.effectName` | 풀링과 무관(effectName은 베이스 의미). 단 대여 때 effectName이 세팅되지 않음(3.2) | 유지 | `Modifier.cs:332-343` |
| `ModifierController.BuffSave` | `buffInstance.modifier.id` (= baseID) | 같은 베이스의 다른 밸류 버프(`Potion_StrengthBuff`/`Potion_Warrior_0` 모두 `Buff_Str`)를 로드할 때 구분 못 함 | `valueId`도 저장 | `ModifierController.cs:350-368`, `ModOption_Buff.csv:4,7` |
| `Modifier.SaveData` / `ModifierOption.SaveData` | `modifierId = id` (= baseID) | 로드 시 밸류 복원 불가. `ModifierOption.SaveData`는 `optionKey`를 저장하지 않고 `modifiers` 리스트를 초기화하지 않아 `Add`에서 NRE | `valueId` 필드 추가, optionKey 저장, 리스트 초기화 | `Modifier.cs:78-83`, `ModifierOption.cs:73-83`, `SaveStructs.cs:26-42` |
| `ModifierController.ApplyPassivesFrom` 로그 | `modifier.id` | 로그만. 영향 없음 | 필요하면 valueId로 출력 | `ModifierController.cs:202` |
| `PlayerEntity.Add_Mutation` 로그/UI | `mod.id`, `mod.stat` | 영향 없음 | 유지 | `PlayerEntity.cs:104-108` |
| `UI_ItemInfo` 표시 | `mod.value` (옵션 myMods 참조) | 옵션이 들고 있는 객체가 다른 곳에 재대여되면 **다른 밸류 값이 표시됨** (H1) | 소유권 정리 | `UI_ItemInfo.cs:62-65`, `:80-83` |

밸류 ID를 키로 쓰는 딕셔너리/캐시/룩업:

| 이름 | 키 | 값 | 근거 |
|---|---|---|---|
| `ModifierDataManager.optionIds` | optionKey | **밸류 ID 리스트** | `ModifierDataManager.cs:15`, `:120-128` |
| `ModifierGroupData.modDatas` | (리스트, valueId로 중복 검사) | ModifierValueData | `ModifierDataManager.cs:102`, `ModifierDataClasses.cs:50` |
| (삭제됨) `ModifierDataManager.modifierDatas` | 밸류 ID | ModifierData | `git show HEAD:...ModifierDataManager.cs` (워킹 트리에서 제거) |
| 그 밖의 풀/캐시 | 밸류 ID 키를 쓰는 곳 없음 | | `ModifierPooler.cs:7-11` |

---

## 5. 반환 경로 표

| 호출 위치 | 사용 키 | 변경 필요 여부 | 근거 |
|---|---|---|---|
| `ModifierPooler.ReturnModifier` | `modifier.baseID` | **필요**: null 가드, 중복 반환 차단(active에서 제거 성공 시에만 Enqueue), Reset보다 키를 먼저 읽는 순서 유지 | `ModifierPooler.cs:183-203` |
| `ModifierPooler.ReturnModifierOption` 안의 연쇄 반환 | 각 mod의 baseID | 불필요 (위 가드로 보호됨) | `ModifierPooler.cs:159-167` |
| `ModifierManager.Return_Modifier` | 그대로 넘김 | 불필요 | `ModifierManager.cs:91-94` |
| `Modifier.Return()` | `Reset()` 후 `Return_Modifier`. baseID는 Reset 뒤에도 남아 있어 현재는 안전 | **필요**: Reset 중복 호출 제거(풀이 Reset 담당). baseID를 지우는 Reset 변경 금지를 주석으로 고정 | `Modifier.cs:90-94` |
| `ModifierOption.Reset()` | 각 mod `Return()`, 리스트 Clear 안 함 | **필요**: 호출되면 myMods에 반환된 객체가 남음. 호출부 없음, 정리 대상 | `ModifierOption.cs:26-35` |
| `ModifierOption.Return()` | 각 mod `Return()`, Clear. **옵션 자신은 풀에 반환하지 않음** | **필요**: `Return_ModifierOption`으로 통일 | `ModifierOption.cs:37-45` |
| `ModifierController.RemoveModifiernInternal` | `Return_Modifier(modifier)`. `Remove` 성공 여부와 상관없이 호출 | **필요(높음)**: 옵션 소유 모디파이어를 반환함(H1), 중복 반환(H2) | `ModifierController.cs:153-166` |
| `ModifierController.OnDead` | buffs/mutation 리스트의 각 항목 `Return()` | **필요**: 같은 객체가 리스트에 여러 번 있으면 중복 반환(H2). 옵션 객체(`mutationOptions`)는 반환하지 않음 | `ModifierController.cs:371-391`, `PlayerEntity.cs:100-103` |
| `EventManager.CheckAndRemoveBuff` 만료 → `Remove_BuffModifier` → `RemoveModifiernInternal` | 위와 같음 | **필요**: 같은 modifier에 BuffInstance가 2개 생기는 경로가 있음(`StatBuff.Excute`) | `EventManager.cs:61-65, 96-132`, `ModifierAction.cs:110-111`, `LivingEntity.cs:78-89` |
| `SelectPanels.UpdateCheckerModifiers` / `OnDisable` | `Return_ModifierOption` | 불필요 | `SelectPanels.cs:111-115`, `:282-286` |
| 반환 누락: 필드 아이템 정리 (층 이동) | 없음 (`item = null`만) | **필요**: 아이템 옵션을 반환해야 함 | `ItemEntity.cs:63-70`, `ItemManager.cs:334-344` |
| 반환 누락: `ConsumableItem.Clone` | 없음. `ItemMake`가 대여한 옵션을 `CopyBaseProperties`가 새 리스트로 덮어써서 잃어버림 | **필요** | `ConsumableItem.cs:12-13`, `ItemBase.cs:71` |
| 반환 누락: `ItemManager.cs:128` 추가 대여 | 없음 (옵션에 안 들어가면 그대로 샘) | **필요**: 루프 제거 (Q6) | `ItemManager.cs:124-130`, `ModifierOption.cs:65-71` |
| 반환 누락: 로드 경로의 추가 대여 | 없음 | **필요** | `ItemManager.cs:297-308`, `MonsterManager.cs:358-367` |
| 반환 누락: 엔티티 사망 시 장비 옵션 | 장비는 반환하지 않음 (주석) | 의도 확인 (아이템이 드롭되면 소유가 아이템으로 넘어감) | `ModifierController.cs:390` |
| 반환 누락: 몬스터 `ReturnAllObjects` (층 이동) | `monsters[i].Return()`. `OnDead`를 거치지 않음 | **필요**: 살아 있는 몬스터의 mutation/buff 반환 경로 없음 `[확인 필요]` | `MonsterManager.cs:403-410`, `LivingEntity.cs:337-342` |
| 반환 후 참조 유지: `StatBuff.modifier` | 만료 뒤에도 필드가 남음 (`modifier == null`일 때만 다시 대여) | **필요**: 재사용 시 이미 반환된 객체를 다시 적용 | `ModifierAction.cs:99-112` |
| 반환 후 참조 유지: `ModifierOption.myMods` (장비 해제 후) | 컨트롤러가 반환해도 옵션은 계속 참조 | **필요(H1)** | `EquipSystem.cs:27-37` |

---

## 6. 발견된 문제

| ID | 심각도 | 설명 | 근거 | 제안 |
|---|---|---|---|---|
| H1 | 높음 | **소유권 충돌.** 장비 해제 시 `RemoveEquipment(option)`이 옵션 안의 각 모디파이어를 풀에 반환하는데, `EquipItem.options/addOptions[*].myMods`는 계속 그 객체를 가리킨다. 베이스 ID 풀에서는 같은 베이스의 **다른 밸류**(예: Weapon_Damage를 쓰는 41개 밸류 중 하나)로 재대여되므로, 해제한 무기를 다시 끼거나 툴팁을 보면 다른 무기의 수치가 적용·표시된다. 밸류 ID 풀에서는 같은 밸류로 재세팅돼 잘 드러나지 않던 버그다. | `ModifierController.cs:111-117,153-166`, `EquipSystem.cs:27-37`, `UI_ItemInfo.cs:80-83`, `ModifierPooler.cs:95-106` | 컨트롤러 Remove는 리스트에서만 빼고 반환은 소유자(옵션/아이템)만 한다. `RemoveModifiernInternal(…, bool returnToPool)`를 두고 옵션 경유 제거는 false. Q2 |
| H2 | 높음 | **중복 반환 방지 없음.** `ReturnModifier`는 active 리스트에서 제거됐는지와 상관없이 Enqueue한다. 같은 객체가 큐에 두 번 들어가면 두 소유자에게 동시에 대여되어 값이 섞인다. 중복 반환 경로: (a) `PlayerEntity.Add_Mutation`이 옵션 전체를 myMods 개수만큼 반복 추가하고, `OnDead`가 리스트 항목마다 반환, (b) `StatBuff.Excute`가 `target.Add_Buff`(안에서 `EventManager.AddBuff`)와 `EventManager.AddBuff`를 둘 다 불러 BuffInstance 2개가 생기고 만료 때 두 번 Remove, (c) `RemoveModifiernInternal`이 `Remove` 실패해도 반환. | `ModifierPooler.cs:183-203`, `PlayerEntity.cs:98-111`, `ModifierController.cs:371-391`, `ModifierAction.cs:104-112`, `LivingEntity.cs:78-89`, `ModifierController.cs:159-165` | 풀에서 `activeModifiers[baseID].Remove(script)`가 false면 경고 후 무시. 선택: `Modifier.isInPool` 플래그로 O(1) 검사. (a), (b)의 중복 등록 자체는 별도 이슈로 보고 (Q11) |
| H3 | 높음 | **밸류 ID로 베이스 풀을 찾는 호출 잔존.** `ItemManager.MakeRandomItem` 계열이 `Get_ModOptionKeys`(밸류 ID 리스트)로 `Get_Modifier`를 부른다. 대부분 LogError와 null을 거쳐 `AddNotIncludeMod(null)`에서 NRE가 나고, `ShortSword_Damage`는 ItemModifier 풀에서 엉뚱한 타입을 꺼낸다. 게다가 `Get_ModifierOption`이 이미 같은 모디파이어를 조립했으므로 이 루프는 중복 대여다. | `ItemManager.cs:114-131`, `ModifierDataManager.cs:88,120-128,142-145`, `ModifierOption.cs:65-71`, `ModOption_Stat.csv:8`, `Mod_ItemModifier.csv:12` | 루프 제거 (Q6). `GetModifier` 공개 API를 없애고 `RentByValueId`로 대체해 컴파일 단계에서 남은 호출을 잡는다 |
| H4 | 높음 | **세이브/로드가 밸류 정보를 잃는다.** 세이브는 `id`(=baseID)와 value만 저장한다. 로드는 `Get_ModifierOption`으로 이미 조립된 옵션에 베이스만 대여한 모디파이어를 **추가로** 넣는다(중복 + valueId/isMulti/priority/descKey/min·maxTime 손실 + buffType 잔존). `ModifierOption.SaveData`와 `EquipItem.SaveData`는 리스트를 초기화하지 않아 NRE 가능성이 있다. | `Modifier.cs:78-83`, `ItemManager.cs:295-308`, `MonsterManager.cs:356-381`, `ModifierOption.cs:73-83`, `EquipItem.cs:28-40`, `SaveStructs.cs:26-48` | `valueId` 저장, 로드 시 옵션을 optionKey로 대여(조립)한 뒤 valueId가 맞는 항목에만 value를 덮어씀. 구버전 세이브는 baseID 폴백 (Q3) |
| M1 | 중간 | `ModifierID`가 없는 밸류 73행이 조용히 건너뛰어져 빈 옵션이 된다(Action 포션/스크롤 전부, 무기 명중 계열). `cd4f4924` 때는 밸류 ID로 대여해서 동작이 달랐다. | 2.3절 집계, `ModifierPooler.cs:77-81`, `ModifierFactory.cs:124-128` | 로드 시 누락 목록을 한 번에 경고로 출력. 데이터 수정 여부는 Q5 |
| M2 | 중간 | 밸류 주입 없이 대여하는 경로가 있다(3.4절). BuffModifier.Reset이 `buffType`, `effectName`을 지우지 않아 이전 밸류 값이 남는다. ItemModifier.Reset은 `specificType`, `Equipable`을 지우지 않는다. | `Modifier.cs:237-247`, `:483-488`, `ModifierAction.cs:101`, `EffectDataManager.cs:199` | Reset 보강 + 모든 대여를 `RentByValue` 계열로 |
| M3 | 중간 | `Set_BaseModData`가 타입별 베이스 필드를 복사하지 않아(3.2절) 풀에서 나온 ItemModifier의 대상/카테고리는 늘 기본값이고, Buff/Action의 effectName은 null이다. 리팩토링 전부터 있던 문제다. | `Modifier.cs:46-54,163-167`, `ModifierFactory.cs:28-33` | 이번 범위에서는 동작 유지. 수정은 Q7 승인 후 별도 작업 |
| M4 | 중간 | 풀 누수: 필드 아이템 정리, `ConsumableItem.Clone`, `ItemManager.cs:128`, 로드 추가 대여, `ModifierOption.Return()`의 옵션 미반환, 몬스터 일괄 정리. active 리스트가 계속 커지고 `List.Remove`가 O(n)이 된다. | 5장 반환 누락 행 | 소유자 반환 경로 추가 + 대여/반환 카운터 |
| M5 | 중간 | 풀 밖에서 만든 객체(`modifierBases` 템플릿, `GetCopyModifier`, `StatModifier(...)` 생성자)는 baseID가 null이라 반환 시 ArgumentNullException이 난다. | `ModifierDataManager.cs:74-76`, `ModifierFactory.cs:152-180`, `Modifier.cs:108-116`, `ModifierPooler.cs:188-199` | null 가드 + 경고, 템플릿은 절대 외부로 내보내지 않음 |
| M6 | 중간 | `AddNotIncludeMod`가 baseID로 비교하고 조건이 뒤집혀 있다. `BuffSave`도 baseID만 저장한다. | `ModifierOption.cs:65-71`, `ModifierController.cs:361` | valueId 기준으로 변경 (Q8) |
| L1 | 낮음 | `Get_ModifierGroupKeys`가 키가 있어도 `new List<string>()`을 매번 할당한다(기본값 인자를 먼저 평가). | `ModifierDataManager.cs:144` | static 빈 리스트 또는 TryGetValue |
| L2 | 낮음 | `AssembleOption`이 모디파이어마다 문자열 보간 `Debug.Log`를 남겨 GC 할당이 생긴다. | `ModifierFactory.cs:78,118` | `[Conditional]` 로그 래퍼 또는 제거 |
| L3 | 낮음 | `Modifier.Copy`가 baseID/valueId/descKey를 복사하지 않는다. `GetCopyModifier`는 호출부가 없다. | `Modifier.cs:55-64`, `ModifierFactory.cs:152` | 이번 범위에서는 사용 금지 주석만 |
| L4 | 낮음 | `using UnityEditor.Tilemaps;` 때문에 플레이어 빌드가 깨질 수 있다(범위 밖). | `Modifier.cs:6` | 별도 이슈 |
| L5 | 낮음 | CSV 헤더 오타/불일치: `ModOption_AddOptions`의 `IsMuti`(hasIsMulti=false라 베이스 값 사용), `Mod_BuffModifier`의 `StringKey`(코드는 `StringValue`를 읽음), `ModOption_Buff`에 `effectName` 열 없음. | `ModOption_AddOptions.csv:1`, `Mod_BuffModifier.csv:1`, `Modifier.cs:102`, `ModifierDataClasses.cs:40` | 데이터는 수정 금지. 현재 동작으로 기록만 |
| L6 | 낮음 | `AssembleOption`이 `option.descKey`를 세팅하지 않는다(그룹 데이터에는 있음). | `ModifierFactory.cs:76-131`, `ModifierDataClasses.cs:51` | 동작 유지. 별도 이슈 |

---

## 7. 변경 사항 명세 (구현 단계)

원칙: 단계마다 컴파일과 플레이가 되는 상태로 커밋한다. 게임 동작이 바뀌는 단계(6, 9)는 해당 `[확인 필요]`에 답이 나온 뒤에만 진행한다.

### 0단계. 기준선 측정 (코드 변경 없음)
- 대상: 없음. 8.4절 방식으로 리팩토링 전 GC 할당과 풀 크기를 기록한다.
- 완료 조건: 측정 구간 4곳의 GC Alloc 값과, 3개 층을 플레이한 뒤 `activeModifiers`/`inactiveModifiers` 베이스별 Count 스냅샷(디버거나 임시 로그)을 문서에 첨부한다.

### 1단계. 밸류 인덱스 추가 — `ModifierDataManager.cs`
- 추가: `private Dictionary<string, ModifierValueData> valueDatas`. `ReadAllModifierGroups`에서 `optionGroup.AddModifierData` 직후 `valueDatas[modValueData.valueId] = modValueData`(중복이면 LogError).
- 추가: `public ModifierValueData Get_ModifierValueData(string valueId)` (없으면 null, 경고 1회).
- 추가: `SetUp` 마지막(ModOption 로드 후)에 `ValidateValueBases()`. 모든 valueData의 `modifierId`가 `modifierBases`에 있는지 확인하고 누락 목록을 **한 번에** LogWarning. 밸류 ID와 베이스 ID가 겹치면(`ShortSword_Damage`) LogWarning.
- 선택(동작 동일): `Get_ModifierGroupKeys`의 기본값 할당 제거(L1).
- 기대 동작: 기존 조회 결과는 그대로. 새 인덱스로 331개 밸류를 조회할 수 있다.
- 완료 조건: 데이터 로드 후 `valueDatas.Count`가 ModOption 전체 행 수(중복 제외)와 같다. 누락 73건과 충돌 1건이 경고 한 줄씩 묶여 출력된다.

### 2단계. 대여 API 정리 — `ModifierPooler.cs`, `ModifierManager.cs`
- `GetModifier(string baseID)`를 `private Modifier RentBase(string baseID)`로 바꾼다(내용은 현재 66-111줄 그대로).
- 추가: `public Modifier RentByValue(ModifierValueData data)`: null 검사, `RentBase(data.modifierId)`, `mod.SetModifierValue(data)`, 반환.
- 추가: `public Modifier RentByValueId(string valueId)`: `Get_ModifierValueData`를 거쳐 `RentByValue`.
- 임시 호환: `public Modifier RentBaseOnly(string baseID)`는 3.4절 경로 중 Q4 답을 기다리는 곳에서만 쓴다. `[Obsolete]`로 표시.
- `ModifierManager.Get_Modifier(string)`는 제거하고 `Get_ModifierByValueId(string)`로 대체한다. 컴파일 에러로 남은 호출부를 모두 드러내는 것이 목적이다.
- 기대 동작: 대여된 모든 객체는 반환 직전까지 `baseID`, `valueId`가 채워져 있다.
- 완료 조건: 프로젝트 전체에서 `GetModifier(` / `Get_Modifier(` 호출이 0건(Obsolete 경로 제외). `RentByValue`로 같은 베이스·다른 밸류를 연속 대여하면 8.1 테스트를 통과한다.

### 3단계. AssembleOption 전환 — `ModifierFactory.cs`
- `AssembleOption` 115-131줄: `modifierPooler.RentByValue(optionData.modDatas[i])` 한 줄로 바꾼다(SetModifierValue 직접 호출 제거).
- 선택(동작 동일): 반복 Debug.Log를 `#if UNITY_EDITOR`나 Conditional 래퍼로 감싼다(L2).
- 기대 동작: 옵션 조립 결과(타입, 값, isMulti, priority, descKey)가 2단계 전과 같다.
- 완료 조건: 캐릭터 생성 화면에서 종족/직업 옵션 프리뷰 수치, 무기 툴팁 수치가 0단계 스크린샷과 같다.

### 4단계. 반환 하드닝 — `ModifierPooler.cs`, `Modifier.cs`
- `ReturnModifier`:
  1. `modifier == null`이면 return.
  2. `string key = modifier.baseID;`를 **Reset보다 먼저** 읽는다. null/빈 값이면 LogError(타입명 포함) 후 return (M5).
  3. `activeModifiers.TryGetValue(key, out list)`가 실패하거나 `!list.Remove(script)`이면 LogWarning("중복 또는 외부 반환") 후 **return (Enqueue 금지)** (H2).
  4. `modifier.Reset()` 후 `inactiveModifiers[key].Enqueue(script)`.
- `Modifier.Return()`: `Reset()` 호출을 지우고 `Return_Modifier(this)`만 남긴다(풀이 Reset 담당).
- `Modifier.Reset()` 위에 "baseID/id는 반환 키이므로 Reset에서 지우지 말 것" 주석을 단다(`cd4f4924` 재발 방지).
- 선택: `internal bool isInPool`을 두고 RentBase에서 false, Return에서 true. Return 진입 시 true면 경고 후 return. `List.Remove`의 O(n) 비용을 줄이려면 active를 `HashSet<IPoolScript>`로 바꾼다(동작 동일).
- 기대 동작: 같은 객체를 두 번 반환해도 큐에는 한 번만 들어간다.
- 완료 조건: 8.3 누수 검사에서 경고 로그 없이 대여 수 = 반환 수 + active 수.

### 5단계. Reset 보강 — `Modifier.cs`
- `BuffModifier.Reset`: `effectName = null; buffType = default;` 추가.
- `ItemModifier.Reset`: `specificType = null; Equipable = false;` 추가.
- 기본 `Modifier.Reset`: `stringValue = null;` 추가(대여 때 Set_BaseModData가 다시 세팅하므로 동작은 같음).
- 기대 동작: 풀에 들어간 객체에 이전 밸류의 흔적이 남지 않는다. 대여 경로는 Set_BaseModData와 SetModifierValue로 모든 값을 다시 채우므로 게임 동작은 같다(3.1, 3.2 표 기준).
- 완료 조건: 8.1 테스트의 "Reset 직후 필드 기본값" 검사를 통과한다.

### 6단계. 소유권 정리 (H1, H2) — `ModifierController.cs` `[확인 필요 Q2 승인 후]`
- `RemoveModifiernInternal(targetDict, modifier, bool returnToPool)`로 바꾼다. `Remove`가 true일 때만 dirty 처리와 이벤트를 발생시키고, `returnToPool && removed`일 때만 `Return_Modifier`.
- `RemoveModifierOptionInternal`은 `returnToPool:false`로 호출한다(옵션이 소유). 단독 모디파이어 Remove(`RemoveBuff(Modifier)` 등)는 true를 유지한다.
- `OnDead`: 리스트를 돌며 반환하기 전에 `HashSet<Modifier>`로 중복을 걸러 객체당 한 번만 반환한다. 옵션 소유 모디파이어는 반환하지 않고 `mutationOptions`/`buffOptions`의 옵션을 `Return_ModifierOption`으로 반환한다. 옵션 사전이 실제로 채워지지 않는 문제(`AddMutation(ModifierOption)`이 사전에 넣지 않음, `ModifierController.cs:138`)는 이 단계에서 함께 채운다.
- 기대 동작: 장비를 해제했다가 다시 끼면 원래 수치가 다시 적용된다. 버프 만료나 중복 Remove가 있어도 풀은 깨지지 않는다.
- 완료 조건: 8.2 시나리오 2, 3, 4 통과.

### 7단계. 호출부 이전 — 아이템/몬스터/액션
- `ItemManager.cs:124-130`: 추가 대여 루프를 지운다(`Get_ModifierOption`이 이미 조립함). `[확인 필요 Q6]`
- `ItemManager.LoadItem` 295-309줄: `Get_ModifierOption(optionKey)`로 조립된 옵션을 받고, 저장된 각 `modSave`에 대해 `valueId`가 같은 `newOption.myMods` 항목을 찾아 `value`만 덮어쓴다. 추가 대여는 하지 않는다. `valueId`가 없는 구 세이브는 `baseID`가 같은 첫 항목에 덮어쓴다(`[확인 필요 Q3]`).
- `MonsterManager.cs:358-367`: 위와 같은 방식. `:375-379` 버프: `RentByValueId(buffData.valueId)` 후 value 덮어쓰기. 구 세이브는 `RentBaseOnly(modifierId)`로 폴백.
- `ModifierAction.cs:101`, `EffectDataManager.cs:199`: `[확인 필요 Q4]` 답에 따라 `RentByValueId` 또는 `RentBaseOnly`. 답이 나오기 전에는 `RentBaseOnly`로 현재 동작을 유지한다.
- 기대 동작: 로드 후 옵션의 모디파이어 개수와 값이 세이브 전과 같다.
- 완료 조건: 프로젝트에서 `[Obsolete] RentBaseOnly` 사용처가 Q4 대상 2곳(+구 세이브 폴백)뿐이다. 8.2 시나리오 5 통과.

### 8단계. 세이브 데이터 — `SaveStructs.cs`, `Modifier.cs`, `ModifierOption.cs`, `ModifierController.cs`, `EquipItem.cs` `[확인 필요 Q3]`
- `ModifierSaveData`, `BuffSaveData`에 `public string valueId;`를 추가한다(기존 `modifierId`는 유지, 의미는 baseID).
- `Modifier.SaveData`: `valueId = valueId, modifierId = baseID`.
- `ModifierOption.SaveData`: `optionKey = optionKey`, `modifiers = new List<ModifierSaveData>()` 초기화.
- `ModifierController.BuffSave`: `valueId` 저장.
- `EquipItem.SaveData`: `addOptionsData` 리스트를 초기화하고 itemId/itemCount를 저장한다(현재 주석 처리된 블록과 같은 의도).
- 완료 조건: 장비/몬스터/버프를 세이브한 뒤 로드하면 8.2 시나리오 5 통과. 구버전 세이브 파일도 예외 없이 로드된다.

### 9단계. 누수 경로 차단 — `ItemEntity.cs`, `ItemManager.cs`, `ConsumableItem.cs`, `ModifierOption.cs`, `MonsterManager.cs`
- 아이템 폐기 API `ItemBase.ReleaseOptions()`를 추가한다. options/addOptions의 각 옵션을 `Return_ModifierOption` 후 Clear. 아이템이 **완전히 사라질 때만** 호출한다(층 이동 시 `ItemManager.ReturnAllObjects`, 소모품 소진, 몬스터 정리). 인벤토리로 옮겨질 때는 호출하지 않는다.
- `ConsumableItem.Clone`: `CopyBaseProperties`가 options를 덮어쓰기 전에 받아 둔 옵션을 반환하거나, `CopyBaseProperties` 호출 순서를 바꾼다(결과 options 구성이 같아야 함).
- `ModifierOption.Return()`/`Reset()`: `Return_ModifierOption(this)`으로 위임하거나 제거한다(호출부 없음 확인 후).
- `MonsterManager.ReturnAllObjects`: 몬스터 mutation/buff 옵션 반환 경로를 추가한다. `[확인 필요 Q12]`
- 완료 조건: 같은 층을 5회 왕복한 뒤에도 베이스별 active Count가 늘지 않는다(8.3).

### 10단계. Prewarm과 계측 — `ModifierPooler.cs`, 초기화 지점
- 추가: `public void Prewarm(string baseID, int count)`, `public void PrewarmAll(IReadOnlyDictionary<string,int> plan)`. inactive 큐에 `CreateNewBase` 결과를 넣고 Queue/List는 `new Queue<IPoolScript>(count)`로 미리 할당한다.
- 호출 지점: `DataManager.DataRead`에서 ModOption 로드가 끝난 직후(`DataManager.cs:106` 다음). 로딩 화면 안이라 할당 비용을 숨길 수 있다.
- 크기 산정(초안, Q10 답으로 보정):
  - 1차: 3개 층 플레이 후 측정한 베이스별 **최대 동시 active 수 x 1.25**(올림).
  - 측정 전 기본값: 밸류 공유가 많은 베이스(`Weapon_Damage`, `Weapon_AttackSpeed`, `Weapon_TwoHand`, `Armor_Def`, `Job_*`, `Race_*`)는 16, 나머지 8, `Buff_*` 4, 데이터에 등장하지 않는 베이스는 0. 근거: 풀 키 하나를 같은 베이스의 모든 밸류가 공유하므로 기존 밸류 ID 풀보다 키 개수는 줄고(331개에서 최대 83개로) 키당 수요는 커진다.
- 계측: `#if UNITY_EDITOR || DEVELOPMENT_BUILD`에서 베이스별 `rentCount`, `returnCount`, `createCount`, `peakActive`를 기록하고 `DumpStats()`로 출력한다.
- 풀이 모자랄 때: 지금처럼 `CreateNewBase`로 새로 할당한다(게임은 멈추지 않음). 이때 `createCount`를 올리고, Prewarm이 끝난 뒤 생성이 일어나면 경고를 1회 출력한다.
- 완료 조건: 8.4 측정 구간에서 Prewarm 이후 `createCount` 증가가 0이거나, 0이 아니면 해당 베이스 목록이 보고된다.

---

## 8. 검증 계획

### 8.1 같은 베이스 ID, 다른 밸류 ID 연속 대여·반환 (값 섞임 검사)
PlayMode 테스트나 개발용 콘솔 명령으로 수행한다(`ModifierFactory`는 private 생성자와 async 초기화를 쓰므로 데이터 로드가 끝난 씬에서 실행).
1. `a = RentByValueId("Warrior_Str")`, `b = RentByValueId("Rogue_Str")` (둘 다 `Job_Str`). `a.valueId/value/descKey`가 각 CSV 행과 같은지, `a != b`(참조)인지 확인한다.
2. `Return(a)` 후 `c = RentByValueId("Rogue_Str")`. `ReferenceEquals(c, a)`가 true여도 `c.value == 1`, `c.valueId == "Rogue_Str"`이어야 한다.
3. Buff: `RentByValueId("Potion_StrengthBuff")` → 반환 → `RentByValueId("Potion_Warrior_0")` (둘 다 `Buff_Str`). value 3→2, minTime/maxTime, buffCategory, buffType, descKey가 모두 두 번째 행 값인지 확인한다.
4. isMulti/priority 덮어쓰기: `Elf_LongSword_1`(IsMulti TRUE, Priority 100) 대여·반환 뒤 `Elf_LongSword_0` 대여 → isMulti false, priority 50.
5. 열이 없는 경우: `ModOption_JobStat` 행(IsMulti/Priority 열 없음)은 isMulti/priority가 `Job_*` 베이스 값과 같아야 한다.
6. Reset 직후(풀 안 객체)의 valueId/descKey/value/effectName/buffType/specificType가 기본값인지 리플렉션으로 확인한다.

### 8.2 중첩·갱신·제거가 밸류 ID 기준으로 동작하는지
1. 같은 베이스·다른 밸류 2개가 들어 있는 옵션(`Elf_LongSword`)을 장착 → `equips[OnEquip]`에 2개, 서로 다른 valueId인지 확인.
2. 장착 → 해제 → 재장착: 스탯 컨텍스트 값이 처음 장착 때와 같아야 한다(H1 회귀).
3. 무기 A 해제 직후 다른 무기 B 생성(같은 `Weapon_Damage` 베이스 대여 유도) → A의 툴팁 수치가 바뀌지 않는지(H1).
4. `StatBuff` 포션 사용 → 만료까지 턴 진행 → 풀 경고 로그 0건, 해당 베이스 active Count가 사용 전과 같은지(H2).
5. 세이브 → 로드: 장비 addOptions, 몬스터 mutation, 몬스터 버프의 (valueId, value, isMulti, priority) 목록이 세이브 전과 같은지. 모디파이어 개수가 늘지 않는지(H4).
6. `AddNotIncludeMod`: 같은 valueId는 한 번만, 다른 valueId는 추가되는지(Q8 결정에 따름).

### 8.3 대여 수와 반환 수 일치 (풀 누수 검사)
- 불변식(베이스별): `createCount == inactive.Count + active.Count`, `rentCount - returnCount == active.Count`.
- 시나리오: 캐릭터 생성 화면에서 종족/직업을 10회 바꾸고 → 시작 → 같은 층을 5회 왕복 → 몬스터 5마리 처치 → 장비 교체 10회 → 세이브/로드 → 메인 메뉴 복귀.
- 판정: 층 왕복마다 베이스별 active Count가 단조 증가하지 않는다. 중복/외부 반환 경고 0건. 메인 메뉴 복귀 후 active 합계는 "플레이어가 들고 있는 아이템/변이 옵션 수"와 같다.

### 8.4 GC 할당 측정 구간과 비교 방법
- 측정 구간(`Profiler.BeginSample`/`ProfilerMarker` 또는 `GC.GetAllocatedBytesForCurrentThread()` 차이):
  1. 데이터 로드 + Prewarm (`DataManager.DataRead`)
  2. 층 생성 (`ItemManager.OnFloorChange`/`OnVisitNewFloor`, 몬스터 생성·로드)
  3. 캐릭터 생성 (`SelectPanels.Create_Character`, 프리뷰 갱신 `UpdateCheckerModifiers`)
  4. 전투 턴 (공격 1회 `GetUpdatedContext` + 버프 적용·만료 `EventManager.ProcessPassedTurn`)
  5. 장비 교체 (`EquipSystem.ItemEquip`)
- 방법: Development Build + Deep Profile 끄고 같은 시드(`Random.InitState` 고정, 가능하면)로 0단계(전)와 10단계(후)를 각각 3회 측정한 중앙값을 비교한다. Debug.Log의 할당은 측정을 왜곡하므로 비교할 때는 로그를 끄거나 양쪽 조건을 같게 맞춘다.
- 기대: 2~5 구간에서 `ModifierFactory.CreateNewModifierToType`의 할당 0(Prewarm 이후). `Get_ModifierGroupKeys`의 기본 리스트 할당 제거. 1 구간은 Prewarm 때문에 늘어나는 것이 정상.

### 8.5 Reviewer 검수 체크리스트
- [ ] `GetModifier(`/`Get_Modifier(` 공개 호출이 남아 있지 않다(Obsolete `RentBaseOnly` 사용처는 목록과 일치).
- [ ] 모든 대여 경로가 `RentByValue` 계열을 거쳐 valueId가 채워진다(Q4 대상 제외).
- [ ] `ReturnModifier`가 Reset **전에** baseID를 읽고, Reset은 baseID/id를 지우지 않는다(`cd4f4924` 재발 방지).
- [ ] `ReturnModifier`는 active에서 실제로 제거됐을 때만 Enqueue한다.
- [ ] baseID가 null이면 예외가 아니라 경고를 내고 return한다.
- [ ] `Modifier.Return()`에서 중복 Reset이 제거됐다.
- [ ] Reset이 3.1/3.2 표의 "아니오" 항목 중 5단계 대상을 모두 초기화한다.
- [ ] `RemoveModifierOptionInternal` 경로는 풀에 반환하지 않는다. 단독 모디파이어 경로만 반환한다.
- [ ] `OnDead`가 같은 객체를 두 번 반환하지 않는다.
- [ ] 로드 경로가 옵션을 조립한 뒤 모디파이어를 추가 대여하지 않는다.
- [ ] 세이브 구조체에 valueId가 추가됐고 구 세이브 폴백이 있다.
- [ ] `Set_BaseModData`의 복사 범위는 이번 PR에서 바뀌지 않았다(Q7이 승인되지 않은 경우).
- [ ] 데이터 CSV 파일은 바뀌지 않았다.
- [ ] 8.1~8.3 결과 첨부, 8.4 전후 수치 첨부.

---

## 9. `[확인 필요]` 질문 목록

**구현 전에 반드시 답이 필요한 것**

- **Q1.** 사용자가 "동작 확인"한 기준 빌드는 어느 상태인가? (a) `cd4f4924`, (b) `HEAD`(SetData 시그니처 불일치로 컴파일되지 않았을 가능성이 높음), (c) 현재 워킹 트리. "동작 동일"의 기준이 이 답에 따라 달라진다. 특히 M1(빈 옵션 73행)은 (a)와 (c)에서 동작이 다르다.
- **Q2.** 장비 해제 시 옵션 모디파이어를 풀에 반환하는 현재 동작(H1)을 "옵션이 소유하고 컨트롤러는 반환하지 않음"으로 바꿔도 되는가? 재장착 시 수치가 복구되는 쪽으로 동작이 바뀐다(현재는 0 또는 다른 밸류 값).
- **Q3.** 세이브 호환: 기존 세이브 파일(`modifierId`=baseID, valueId 없음)을 계속 읽어야 하는가? 세이브 구조체에 `valueId` 필드를 추가해도 되는가? 현재 세이브/로드 경로가 실제로 쓰이는가(`EquipItem.SaveData`, `ModifierOption.SaveData`는 NRE 가능성 있음)?
- **Q4.** `StatBuff.effectID`(`ModifierAction.cs:101`)와 `EffectDataManager`의 `originBuff.modifier.id`(`:199`)는 베이스 ID인가, 밸류 ID인가, Act_ 전용 ID인가? 어떤 밸류 데이터(value, min/maxDuration)를 주입해야 하나? 원본 `StatBuff.modifier`는 어디서 세팅되나(찾지 못함)?
- **Q5.** `ModifierID`가 베이스에 없는 73행(Action 30행, `Weapon_Accurency`/`Weapon_Accurancy` 철자 차이 등)은 데이터 오류인가, 미구현인가? 이번 리팩토링에서는 데이터를 고치지 않고 "건너뜀 + 경고" 동작을 유지하면 되는가?

**구현 중에 답해도 되는 것**

- **Q6.** `ItemManager.cs:124-130`의 추가 대여 루프는 무엇을 의도했나? `Get_ModifierOption`이 이미 같은 모디파이어를 조립하므로 지워도 되는가? (`RandomOptionManager.SetUp`이 주석 처리돼 있어(`DataManager.cs:112`) 지금은 이 경로에 도달하지 않을 수 있다.)
- **Q7.** `Set_BaseModData`가 ItemModifier의 대상/카테고리/세부 타입, Buff/Action의 effectName을 복사하지 않는 문제(M3)를 이번 범위에서 고칠 것인가? 고치면 아이템 제한·버프 동작이 바뀐다.
- **Q8.** `AddNotIncludeMod`의 의도는 "같은 valueId가 없으면 추가"인가? 현재 조건은 반대로 동작한다.
- **Q9.** `ModOption_AddOptions.csv`의 `IsMuti` 오타 때문에 AddOptions의 isMulti는 베이스 값이 쓰인다. 이것이 의도한 동작으로 고정해도 되는가?
- **Q10.** 층당 생성되는 아이템 수(`itemGenPerTile`), 몬스터 수, 몬스터당 평균 mutation/장비 옵션 수는? Prewarm 크기 산정에 필요하다.
- **Q11.** `PlayerEntity.Add_Mutation`이 myMods 개수만큼 옵션 전체를 반복 추가하는 것(`PlayerEntity.cs:100-103`)과 `StatBuff.Excute`의 이중 버프 등록(`ModifierAction.cs:110-111`)은 의도인가? 풀링 쪽 가드(4단계)로 피해는 막지만 스탯 중복 적용은 그대로 남는다.
- **Q12.** 층 이동 때 `MonsterManager.ReturnAllObjects`로 정리되는 몬스터의 mutation/buff 모디파이어는 누가 반환해야 하나? `EventManager.entityBuffs`에 남은 BuffInstance 정리도 함께 해야 하나?
