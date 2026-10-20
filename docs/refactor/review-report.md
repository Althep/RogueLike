# 모디파이어 풀링 리팩토링 검수 보고서

## 검수 1회차
- 판정: **수정 필요** (중간 1건, 가벼운 수정. 차단·높음 0건)
- 검수한 단계: 1, 2, 3, 4, 5, 6(일부: mutationOptions만), 7, 8, 9(일부), 10, CSV 철자 수정
- 기준: `docs/refactor/modifier-pool-spec.md`, `docs/refactor/implementation-log.md`, `git diff`/`git status`(커밋 안 된 22개 파일). 줄 번호는 워킹 트리 기준이다.
- 사용자 결정 반영: HEAD와의 동작 동일성은 기준에서 뺐다. H1(장비 해제 시 반환/소유권)은 결함이 아니라 보류 항목으로만 다룬다. 세이브는 valueId와 value 왕복을 기준으로 봤다. `Modifier.id`와 `originBuff.modifier.id`는 베이스 ID로 봤다. effectID는 보고만 한다. CSV 철자 수정 4종은 허용된 변경으로 봤다.
- 에이전트가 직접 확인할 수 없는 것: Unity 컴파일, 플레이, GC 측정. 아래 판단은 모두 정적 분석이다.

### 문제 목록

| 번호 | 심각도 | 원인 | 항목 | 위치 | 설명 | 수정 방향 |
|---|---|---|---|---|---|---|
| 1 | 중간 | 구현 문제 | 범위 밖 수정 (데이터) | `Assets/Resources_moved/StringDatas/ModifierDatas/ModOption_JobStat.csv:1` | 사용자가 승인한 철자 수정 4종에 없는 **헤더 변경 `DeskKey` → `DescKey`** 가 들어 있다. 구현 로그 "CSV spelling fixes"에도 기록이 없다. 이 줄만 CRLF(`0d0a`)로 바뀌어 파일 안 줄바꿈이 섞였다(나머지 22줄은 LF). 파서가 `\r\n`/`\r`/`\n`로 나누고 `Trim()`하므로(`Utis/Utils.cs` TextAssetParse) 읽기는 정상이다. 이 변경으로 JobStat 행의 `ModifierValueData.descKey`와 `ModifierGroupData.descKey`가 처음으로 채워진다. 현재 `Modifier.descKey`를 읽는 UI가 없고 `AssembleOption`이 `option.descKey`를 세팅하지 않아(L6) 화면에 보이는 변화는 없다. | 되돌린다(`git checkout -- ModOption_JobStat.csv`). 사용자가 유지하기로 하면 구현 로그에 기록하고 1번째 줄의 줄바꿈을 LF로 맞춘다. |
| 2 | 낮음 | 스펙 문제 | 반환 경로 (옵션) | `Assets/Scripts/Managers/ModifierPooler.cs:199-231` | `ReturnModifierOption`에는 모디파이어 쪽(`:245`, `:252`)과 같은 중복·외부 반환 가드가 없다. `activeModifierOptions[id].Remove`가 실패해도 Enqueue한다. 이번 변경으로 옵션 반환 경로가 늘었다(`ItemBase.ReleaseOptions` `ItemBase.cs:89-102`, `ConsumableItem.cs:13`, `ItemManager.cs:316-320`, `ModifierOption.Return/Reset` `ModifierOption.cs:26-35`). 같은 옵션이 두 번 반환되면 두 소유자에게 같은 옵션이 대여되고, `AssembleOption`의 `myMods.Clear()`(`ModifierFactory.cs:110`)가 다른 소유자의 모디파이어를 지운다. 지금 코드에서 이중 반환이 실제로 일어나는 경로는 찾지 못했다(`ReleaseOptionList`가 `Clear()`해서 같은 아이템을 다시 해제해도 안전하다). 스펙 4단계는 모디파이어 반환만 다룬다. | 스펙에 옵션용 가드(`isInPool` 또는 active `Remove` 성공 검사)를 추가할지 정한다. |
| 3 | 낮음 | 스펙 문제 | 소유권 (6단계 범위 밖 선행 변경) | `ModifierController.cs:138-162`, `:330-335`, `:410` | 6단계가 보류인데 `mutationOptions` 등록·제거·초기화가 들어갔다(8단계 몬스터 변이 세이브에 필요하다는 이유, 구현 로그 Step 6). 위험: (a) 키가 optionKey라서 같은 키의 다른 옵션 객체는 등록되지 않아 세이브에서 빠진다. `RemoveMutate`는 객체가 달라도 키로 지운다. (b) `OnDead`가 `mutationOptions.Clear()`만 하고 옵션 객체는 반환하지 않는다(옵션 누수, 기존 6단계 범위). (c) `MonsterManager.ReturnAllObjects`가 `OnDead`를 거치지 않고 `LivingEntity.ResetData`(`LivingEntity.cs:316-334`)도 컨트롤러를 비우지 않는다. 그래서 풀에서 다시 나온 몬스터는 이전 변이와 `mutationOptions`를 그대로 들고 있고, 이제는 세이브에도 들어간다(Q12). 지금은 몬스터가 변이를 얻는 경로가 로드뿐이라(`Set_RaceData` 호출부 없음) 실제로 일어나지 않는다. | 스펙 6단계/Q12에서 `mutationOptions` 키(객체 참조 vs optionKey)와 풀 복귀 시 컨트롤러 초기화를 정한다. |
| 4 | 낮음 | 스펙 문제 | 세이브 (구 세이브) | `ItemManager.cs:290-296`, `MonsterManager.cs:360-366`, `ModifierOption.cs:68-112` | 구 세이브의 `ModifierOptionSaveData`에는 optionKey가 없다(이전 `SaveData`가 저장하지 않음). 그래서 `Get_ModifierOption(null)`이 null을 돌려주고 해당 옵션 전체가 경고와 함께 빠진다. 예외는 나지 않으므로 8단계 완료 조건("예외 없이 로드")은 충족한다. `RestoreSavedValues`의 baseID 폴백은 optionKey가 있을 때만 동작한다. 새 세이브는 영향이 없다. | 구 세이브 데이터 손실을 허용할지 스펙에 적는다(Q3 보완). |
| 5 | 낮음 | 스펙 문제 | 범위 밖 기존 결함 (참고) | `ConsumableItem.cs:12-14`, `ItemBase.cs:57-71`, `InventoryData.cs:66` | 줍기(`AddInventory`)는 항상 `Clone()`을 거친다. `CopyBaseProperties`가 `options`를 빈 리스트로 바꾸므로 인벤토리 소모품은 옵션이 없다. 이번 변경은 그 전에 `ItemMake`가 대여한 옵션을 반환해 누수만 막았고 결과(빈 options)는 같다. `EquipItem.Clone`(`EquipItem.cs:14-21`)도 같은 방식으로 base options와 바닥 아이템의 addOptions를 잃고, `ItemMake`가 대여한 옵션은 계속 샌다(9단계 범위 밖). 스펙 9단계의 "결과 options 구성이 같아야 함"이 이 기존 결함을 굳힌다. | 별도 이슈로 분리할지 스펙에 적는다. 이번 PR에서 고치지 않는다. |
| 6 | 낮음 | 구현 문제 | 기존 스타일/로그 | `ModifierDataManager.cs:261-271`, `ModifierPooler.cs:124` | 베이스가 없는 30행(Action_*)은 `ValidateValueBases`가 로드 때 한 번에 경고한다. 그런데 `RentBase`가 매번 `Get_ModifierBase`를 불러 대여 시도마다 LogError가 반복된다. 동작에는 문제가 없다. | 선택: `RentBase`에서 `HasModifierBase`로 먼저 걸러 로그를 한 번만 남긴다. |

차단: 없음. 높음: 없음.

### 중점 확인 결과

**1. 컴파일 오류 가능성 (정적 확인)**
- 삭제된 API의 남은 호출: `GetModifier(`, `Get_Modifier(`, `GetModifierData(`, `Modifier.SetData(ModifierData)`, `modifierDatas`, `Get_ModifierOptionKeys`를 `Assets` 전체 `*.cs`에서 찾았고 0건이다.
- 하위 클래스의 `override SetData`도 남아 있지 않다(`Modifier.cs` 전체).
- 새로 부르는 API는 모두 존재한다.
  - `Utils.TrySetValue` 반환형 bool(`Utis/Utils.cs:104`), `Utils.TryConvertEnum`(`:139`), `Defines.BuffCategory/BuffType`(`Utis/Defines.cs:145,151`). `ModifierDataClasses.cs`에 `using static Defines` 있음.
  - `ModifierFactory._instance` public static(`ModifierFactory.cs:10`), `ModifierManager.GetModifierPooler()`(`ModifierManager.cs:70`), `ItemEntity.GetItem()`(`ItemEntity.cs:35`).
  - `ItemBase.ReleaseOptions` virtual / `EquipItem` override / `protected static ReleaseOptionList` 시그니처가 맞다.
  - `ItemModifier.specificType`은 `Enum` 타입이라 null 대입이 가능하다(`Modifier.cs:411`).
- `#if UNITY_EDITOR || DEVELOPMENT_BUILD` 블록(`PoolStat`, `GetStat`)을 그 밖에서 참조하는 곳이 없고, `#else`에 빈 `DumpStats()`가 있다.
- `[Obsolete]`(error 아님) 사용처 3곳은 `#pragma warning disable 0618`로 감쌌다. 경고만 남는다.
- 인코딩: 바뀐 `.cs` 17개 모두 파일별 인코딩이 HEAD와 같다(CP949 파일은 CP949, UTF-8 파일은 UTF-8, 섞인 줄 없음). BOM 유무도 같고 줄바꿈도 파일마다 CRLF로 일관된다.
- 기존 위험(이번 변경과 무관): `Modifier.cs:6 using UnityEditor.Tilemaps;` 때문에 플레이어 빌드가 깨질 수 있다(L4).

**2. 반환 키 / Reset / isInPool**
- 반환 키를 Reset 전에 읽는다: `ModifierPooler.cs:239`(키 읽기) → `:258`(Reset). 빈 키는 LogError 후 return(`:240-244`).
- Reset은 id와 baseID를 지우지 않는다. 기본 Reset(`Modifier.cs:69-81`)과 하위 클래스 Reset(`:164`, `:242`, `:319`, `:390`, `:490`) 어디에도 id/baseID 대입이 없고, 경고 주석이 있다(`:68-69`).
- `Modifier.Return()`의 중복 Reset 제거(`Modifier.cs:95-99`) 확인.
- isInPool:
  - 생성 시 true(`ModifierPooler.cs:166`), 대여 시 false(`:139`, Dequeue 직후 다른 필드보다 먼저), 반환 시 true(`:259`).
  - 반환 진입 시 true면 경고 후 return(`:245-249`). active `Remove` 실패 시 Enqueue하지 않고 return(`:252-256`).
  - 템플릿이나 풀 밖에서 `new`한 객체는 baseID가 null이라 `:240`에서 걸린다.

**3. 대여 시 밸류 주입 누락 경로**
- 모든 대여는 `RentBase` 하나로 모인다(`ModifierPooler.cs:113`). 공개 진입점은 `RentByValue`/`RentByValueId`/`[Obsolete] RentBaseOnly` 셋뿐이다.
- 주입 경로: `AssembleOption`(`ModifierFactory.cs:124`), 몬스터 버프 로드(`MonsterManager.cs:380`). 장비·몬스터 옵션 로드는 옵션을 조립한 뒤 value만 덮어쓴다(`ItemManager.cs:290-298`, `MonsterManager.cs:361-368`).
- `RentBaseOnly` 사용처는 스펙 7단계 목록과 같다: `ModifierAction.cs:103`(StatBuff), `EffectDataManager.cs:200`(CopyEffect), `MonsterManager.cs:386`(valueId 없는 구 세이브 버프 폴백).
- 같은 베이스에서 다른 밸류를 연속 대여할 때 이전 값이 남는 경로는 찾지 못했다. Reset이 3.1/3.2 표의 5단계 대상(`stringValue`, Buff `effectName`/`buffType`, Item `specificType`/`Equipable`)을 모두 지운다(`Modifier.cs:75`, `:249-252`, `:495-496`). 대여 때 `Set_BaseModData`와 `SetModifierValue`가 나머지를 다시 채운다. `RentBaseOnly`로 나온 객체도 Reset된 기본값에서 시작하므로 이전 사용자의 값은 남지 않는다.
- `Set_BaseModData`의 복사 범위는 바뀌지 않았다(Q7 유지).

**4. 세이브/로드 왕복**
- 저장 쪽:
  - `ModifierSaveData`/`BuffSaveData`가 valueId(밸류)와 modifierId(베이스)를 함께 저장한다(`SaveStructs.cs:32-45`). value는 그대로 저장한다.
  - `Modifier.SaveData`(`Modifier.cs:85-90`), `ModifierOption.SaveData`(optionKey 저장 + 리스트 초기화, `ModifierOption.cs:115-127`), `BuffSave`(`ModifierController.cs:380-382`), `EquipItem.SaveData`(itemId/itemCount/리스트 초기화, `EquipItem.cs:34-47`).
  - 직렬화는 Newtonsoft(`SaveDataManager.cs:74`)라서 struct 필드를 추가해도 문제없다.
- 로드 쪽:
  - optionKey로 옵션을 다시 조립해 모든 필드를 데이터에서 채운 뒤, valueId가 같은 모디파이어에 value만 덮어쓴다(`ModifierOption.cs:68-112`). 추가 대여가 없어 개수가 늘지 않는다.
  - `ItemMake`는 addOptions를 굴리지 않으므로(`Set_RandomOption`은 `MakeRandomItem`에서만 호출, `ItemManager.cs:75-78`) 로드한 addOptions와 겹치지 않는다.
  - 새 세이브 기준으로 (valueId, value)가 세이브 전과 같게 복원된다고 판단한다.
- 남은 점(이번 변경과 무관한 기존 결함):
  - `StatBuff.Excute`의 이중 BuffInstance(Q11)가 있으면 같은 버프가 두 번 저장되고 두 번 로드된다.
  - 로드한 몬스터 버프는 `EventManager.AddBuff`에만 들어가고 컨트롤러의 buffs에는 들어가지 않는다(기존 코드와 같음).
  - 구 세이브는 문제 4 참고.

**5. Prewarm 호출 시점과 데이터 로드 순서**
- 호출 위치는 `DataManager.cs:107`이다. `Mod_`와 `ModOption_` SetUp 직후, `Item_` 앞이라 스펙 10단계와 같다. `ValidateValueBases`는 SetUp마다 불리지만 `Mod_`만 끝난 시점에는 `valueDatas.Count == 0`이라 바로 return한다(`ModifierDataManager.cs:226`).
- 초기화 순서 위험: `GameManager.Init`은 `dataManager.Init()`을 `modifierManager.Init()`보다 먼저 await한다(`GameManager.cs:46-55`). `ModifierManager.Awake`도 `Init().Forget()`을 부르므로, 이 시점에 `GetModifierPooler()`가 factory/dataManager가 null인 풀러를 먼저 만들 수 있고, 이후 `Init`은 풀러를 다시 세팅하지 않는다.
  - 구현은 이 경우를 대비해 `CreateNewBase`에서 `ModifierFactory._instance`로 폴백하고(`ModifierPooler.cs:160-163`), `PrewarmFromData`에서 dataManager를 주입한다(`:306-311`).
  - `ReadModifierBase`가 이미 `ModifierFactory._instance`에 의존하므로(`ModifierDataManager.cs:52-55`) Prewarm까지 왔다면 팩토리는 존재한다. `ModifierDataManager`는 싱글턴이라(`AsyncDataManager.cs:10-21`) 팩토리와 같은 인스턴스다.
  - 정적으로는 안전하다. 실제 순서는 Unity에서 확인해야 한다.
- GC:
  - 대여·반환 핫패스(`RentBase`, `ReturnModifier`, `AssembleOption` 루프)에 LINQ, 람다, 임시 컬렉션이 없다.
  - 문자열 연결은 실패·경고 경로에만 있다.
  - `EditorLog`는 `[Conditional("UNITY_EDITOR")]`라 빌드에서는 인자 평가까지 제거된다.
  - `Get_ModifierGroupKeys`의 기본 리스트 할당이 제거됐다(`ModifierDataManager.cs:158-165`).
  - 풀이 모자라면 `CreateNewBase`로 새로 할당하고, Prewarm 이후 첫 증가 때 경고를 1회 출력한다(`ModifierPooler.cs:171-175`). 스펙 10단계와 같다.
  - `PrewarmAll`의 인터페이스 foreach가 열거자를 박싱하지만 로드할 때 1회뿐이라 무방하다.

**6. 스펙 범위를 벗어난 변경의 위험**
- `AddNotIncludeMod`(`ModifierOption.cs:55-63`): 호출부가 0건이라 현재 위험은 없다. 조건을 valueId 기준 "없으면 추가"로 바로잡았고 null 가드가 있다. valueId가 null인 객체(`RentBaseOnly`)끼리는 같은 것으로 보고 두 번째를 넣지 않는다. 나중에 쓸 때 참고.
- `mutationOptions`: 문제 3.
- `ReleaseOptions`:
  - 장비는 의도적으로 제외했다(`ItemManager.cs:316-320`). 그래서 H1 객체(옵션이 아직 참조하는 반환된 모디파이어)를 새로 반환하는 경로는 생기지 않았다.
  - 줍기 후 `ItemEntity.Return()`이 item을 null로 만들어 바닥 정리 때 다시 해제되지 않는다(`ItemEntity.cs:64-66`). 같은 ItemEntity가 fieldItems에 두 번 남는 기존 문제가 있어도 `ReleaseOptionList`가 `Clear()`해서 두 번째 호출은 아무것도 하지 않는다.
  - 옵션 반환 가드 부재는 문제 2.
- `ModifierOption.Reset()` → `Return()` → 풀 위임(`ModifierOption.cs:26-35`): 호출부가 0건이다. 이제 `IPoolScript.Reset()`이 "풀에 반환"이라는 뜻이 됐다. 나중에 풀이 `option.Reset()`을 부르도록 바뀌면 무한 재귀가 생기므로 주의가 필요하다(낮음, 기록만).
- `ModifierManager.Get_ModOptionKeys`가 `Get_ModifierGroupKeys`로 바뀌었다. 호출부 0건이고 컴파일 수정 성격이다.

**7. CSV 철자 수정**
- 토큰만 바뀌었는지 HEAD 원본에 같은 치환을 적용해 `diff`로 비교했다. 4개 파일 모두 해당 토큰 외 차이가 없다.
  - `ModOption_Stat.csv`: `Weapon_Accurency` → `Weapon_Accuracy` 27행. 원본 비ASCII 4행(UTF-8, 예: `Executioner’s`)도 바이트 그대로다.
  - `ModOption_AddOptions.csv`: `Weapon_Accurancy` → `Weapon_Accuracy` 14행.
  - `Mod_StatModifier.csv:10`: `Weapon_MagicAccurancy` → `Weapon_MagicAccuracy`.
  - `Mod_BuffModifier.csv:18-19`: `Buff_LifeSeal` → `Buff_LifeSteal`, `Buff_Teleport` → `Buff_Telleport`.
- 참조 확인:
  - 옛 철자는 `Assets`의 `*.cs`/`*.csv`/`*.json`/`*.asset`/`*.txt`에서 0건이다.
  - 새 철자의 베이스와 밸류가 짝을 이룬다: `ModOption_Stat.csv:18` → `Weapon_MagicAccuracy`, `ModOption_Buff.csv:23` → `Buff_LifeSteal`, `ModOption_Buff.csv:29` → `Buff_Telleport`, `Mod_StatModifier.csv:4` `Weapon_Accuracy`.
  - 재집계 결과 베이스가 없는 밸류 행은 `ModOption_Action.csv`의 Action_* 행만 남는다.
- 인코딩·줄바꿈: 4개 파일은 HEAD와 같다(ASCII/UTF-8, LF, BOM 없음). `ModOption_JobStat.csv`만 문제 1(범위 밖 헤더 변경 + 1줄 CRLF).
- 참고(바꾸지 않은 것): `Mod_BuffModifier.csv`의 `StringKey` 열 값은 여전히 `LifeSeal`/`Teleport`다. 코드는 `StringValue` 열을 읽으므로(`Modifier.cs:105`) 영향은 없다(L5).

### H1 보류 항목 (결함 아님)
- 장비 해제 시 `RemoveModifierOptionInternal` → `RemoveModifiernInternal`이 옵션 소유 모디파이어를 풀에 반환하는 구조(`ModifierController.cs:112-117`, `:168-180`)는 바뀌지 않았다. 사용자가 직접 처리한다.
- **이번 변경이 H1을 악화시켰는가: 아니다.** 근거:
  - 중복 반환 시 이중 Enqueue가 막혔다(`ModifierPooler.cs:245-256`). 이전에는 해제 후 다시 반환하면 같은 객체가 큐에 두 번 들어가 두 대여자에게 동시에 나갈 수 있었다.
  - `Modifier.Return()`이 반환 거부 때도 Reset하던 동작이 없어졌다.
  - 새 옵션 반환 경로(`ReleaseOptions`)는 장비를 제외한다. `EquipItem.Clone`은 건드리지 않았다.
- 남는 위험(H1 그대로): 해제로 반환된 객체가 다른 소유자에게 재대여된 뒤 원래 옵션이 반환되거나 재해제되면 가드를 통과해 다른 소유자의 객체가 Reset된다. Reset이 이제 `descKey`/`stringValue`도 지우지만, 이전에도 value/stat이 0/기본값이 됐으므로 체감 차이는 없다. `StatBuff.modifier`가 만료 뒤에도 반환된 객체를 계속 참조하는 문제(스펙 5장)도 같은 소유권 문제로 남아 있다.

### effectID 보고 (삭제하지 않음, 사용자 결정대로)
- `StatBuff.GetMyModifier`(`ModifierAction.cs:99-106`)는 `RentBaseOnly(effectID)`를 쓴다. effectID는 Act_ CSV의 `ID`다(`EffectDataManager.cs:70-72`).
- 그런데 `Assets`에 `Act_*` 파일이 없다(Glob 0건). 그래서 이 경로는 지금 데이터로는 실행되지 않는다고 본다.
- 실행된다면 다음 위험이 있다:
  - Act_ ID가 베이스 ID가 아니면 null이 나와 `Add_Buff(null)`가 된다.
  - 다른 타입의 베이스와 이름이 겹치면 `(BuffModifier)` 캐스트에서 InvalidCastException이 난다.
  - value가 주입되지 않아 0으로 적용된다.
- `EffectDataManager.CopyEffect`(`:197-202`)는 `originBuff.modifier.id`(=베이스 ID)로 `RentBaseOnly`한다. 키는 맞지만 원본 value가 복사되지 않는다. `originBuff.modifier`가 null이면 NRE가 난다(기존과 같음).

### 전달 사항
- **implementer에게**: 문제 1, 6.
  - 1: `ModOption_JobStat.csv` 1행 헤더 변경(`DeskKey` → `DescKey`)을 되돌리거나, 사용자가 유지를 승인하면 구현 로그에 기록하고 그 줄의 줄바꿈을 LF로 맞춘다.
  - 6(선택): `RentBase`에서 베이스 누락 LogError가 대여마다 반복되지 않게 한다.
- **spec-writer에게**: 문제 2, 3, 4, 5.
  - 2: `ReturnModifierOption`에 중복·외부 반환 가드를 넣을지 4단계 또는 9단계에 명시한다.
  - 3: 6단계/Q12에서 `mutationOptions`의 키 기준(optionKey vs 객체), `OnDead`에서 옵션 객체 반환, 풀 복귀 몬스터의 컨트롤러 초기화를 정한다.
  - 4: optionKey가 없는 구 세이브 옵션의 손실 허용 여부를 Q3에 보완한다.
  - 5: `Clone()`(소모품·장비)이 옵션을 잃는 기존 결함을 별도 이슈로 분리할지 적는다.

### 완료 조건 재확인

| 단계 | 조건 | 결과 | 확인 방법 |
|---|---|---|---|
| 1 | 밸류 인덱스, 조회 API, 누락/충돌 경고 1줄씩 | 코드 충족 / 런타임 미확인 | `ModifierDataManager.cs:17`, `:124-131`, `:168-182`, `:224-256`. CSV 재집계: 밸류 331행, 베이스 누락은 Action_* 행만 |
| 2 | `GetModifier(`/`Get_Modifier(` 0건, `RentBaseOnly`는 목록과 일치 | 충족 | grep 0건. RentBaseOnly 3곳 = 스펙 7단계 목록 |
| 3 | AssembleOption이 `RentByValue`만 사용 | 코드 충족 / 수치 비교는 플레이 필요 | `ModifierFactory.cs:115-131` |
| 4 | Reset 전 키 읽기, null 가드, active 제거 성공 시에만 Enqueue, Return의 Reset 제거 | 충족 | `ModifierPooler.cs:239-268`, `Modifier.cs:95-99` |
| 5 | Reset 보강 | 충족 | `Modifier.cs:75`, `:249-252`, `:495-496` |
| 6 | 소유권 정리 | **보류 (H1, 사용자 처리)** | mutationOptions만 선행 변경(문제 3) |
| 7 | 로드 시 추가 대여 없음, 추가 대여 루프 제거 | 충족 | `ItemManager.cs:114-130`(루프 삭제), `:290-298`, `MonsterManager.cs:358-394` |
| 8 | valueId 저장, optionKey 저장, 리스트 초기화, 구 세이브 폴백 | 충족 (구 세이브 옵션 손실은 문제 4) | `SaveStructs.cs:32-45`, `Modifier.cs:85-90`, `ModifierOption.cs:68-127`, `EquipItem.cs:34-47` |
| 9 | 누수 경로 차단 | **일부** (장비 바닥 아이템은 H1, 몬스터 정리는 Q12로 미완) | `ItemBase.cs:86-102`, `ConsumableItem.cs:13`, `ItemManager.cs:316-320` |
| 10 | Prewarm, 계측, 성장 경고 | 코드 충족 / 측정 필요 | `ModifierPooler.cs:271-357`, `DataManager.cs:107`, `ModifierDataManager.cs:190-221` |
| 0, 8.1~8.4 | 기준선과 테스트 결과 첨부 | 미충족 (Unity 필요) | 구현 로그 Step 0 skipped |
| 유니티 규칙 | `.meta`/`ProjectSettings/`/`Packages/` 무수정, 새 스크립트 없음 | 충족 | `git status`: 수정 22개는 모두 `Assets/Scripts`와 `ModifierDatas/*.csv`. 추적 안 된 것은 `.claude/`, `docs/`뿐 |

### 사용자 확인 필요 (Unity에서 확인할 항목)
1. **컴파일**: 에디터 Console 에러 0건. CS0618 경고는 `#pragma`로 막았으므로 남으면 안 된다.
2. **데이터 로드 로그**: `value index size 331`(행 수와 일치), `value rows without base modifier ... 30`(Action_*만), `valueId equals a baseId 1: ShortSword_Damage` 각 1줄. `pool grew after prewarm` 경고가 로드 직후에 나오지 않는지 확인. 나오면 Prewarm 전에 풀러가 다른 인스턴스로 만들어졌을 수 있다.
3. **초기화 순서**: `ModifierManager.Init`과 `DataManager.DataRead`의 실행 순서와 관계없이 NRE가 없는지 확인한다(게임 시작 3회).
4. **캐릭터 생성 화면**: 종족/직업을 10번 바꾸며 프리뷰 수치가 CSV와 같은지, 풀 경고(`duplicate ... return ignored`)가 나오는지 확인한다.
5. **같은 베이스·다른 밸류 (스펙 8.1)**: `RentByValueId("Warrior_Str")`/`("Rogue_Str")`, `Potion_StrengthBuff` → `Potion_Warrior_0`, `Elf_LongSword_1` → `_0` 순서로 대여·반환하며 값이 섞이지 않는지 확인한다(개발 콘솔이나 임시 테스트).
6. **무기 툴팁**: 명중 계열(`Weapon_Accuracy`)이 철자 수정 덕분에 이제 실제로 적용·표시되는지 확인한다. 이전에는 베이스를 찾지 못해 빠졌으므로 **이 부분은 게임 수치가 달라지는 것이 정상**이다. `Weapon_MagicAccuracy`, `Buff_LifeSteal`(흡혈 포션), `Buff_Telleport`(순간이동 스크롤)도 이제 조립된다. 해당 아이템을 사용해 동작을 확인한다.
7. **층 이동**: 같은 층을 5번 왕복한 뒤 `ModifierPooler.DumpStats()`(에디터/개발 빌드)에 `INVARIANT BROKEN`이 없는지, 소모품 베이스의 active 수가 늘지 않는지 확인한다. 장비와 몬스터 쪽 증가는 H1/Q12 보류 때문이며 예상된 결과다.
8. **세이브 → 로드**: 장비 addOptions, 몬스터 버프의 (valueId, value)가 세이브 전과 같고 모디파이어 개수가 늘지 않는지 확인한다. 세이브 JSON에 `valueId`와 `modifierId`가 모두 들어 있는지도 본다.
9. **GC 측정 (스펙 8.4)**: Prewarm 이후 층 생성·전투·장비 교체 구간에서 `CreateNewModifierToType` 할당이 0인지 확인한다.
10. 장비 해제 → 재장착 수치(H1)는 보류 항목이라 이번 확인 대상이 아니다.

### 커밋해도 되는지
- **조건부 가능.** 차단·높음 결함은 없다. 목표 구조(베이스 ID 풀 + 대여 시 밸류 주입, Reset 전 키 읽기, 중복 반환 가드, valueId 세이브, Prewarm)는 스펙대로 구현됐다.
- 커밋 전에 할 것:
  1. 문제 1(`ModOption_JobStat.csv` 헤더) 정리: 되돌리기, 또는 승인 후 로그 기록과 줄바꿈 정리.
  2. Unity 에디터 컴파일 에러 0건 확인.
  3. 위 확인 항목 2, 4, 6을 최소한 한 번 플레이로 확인.
- 커밋 범위: `.claude/`는 에이전트 설정이므로 함께 커밋할지 따로 판단한다. `docs/refactor/`는 함께 커밋해도 된다.
- H1과 9단계 미완(장비·몬스터 누수)은 커밋 메시지나 PR 설명에 "보류"로 남기는 것을 권한다.
