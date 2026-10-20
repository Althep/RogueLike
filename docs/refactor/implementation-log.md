# Implementation Log (modifier-pool-spec)

Common: no Unity editor here, so nothing was compiled. All steps need "compile check in the Unity editor".
User decisions (override the spec): see task message (Q1..Q6). H1 (ownership change in ModifierController.RemoveModifiernInternal) is left to the user.

## Step 0: baseline measurement
- Status: skipped (needs Unity Profiler / play session)
- Modified files: none
- Remaining: user measures GC Alloc and per-base pool counts. `ModifierPooler.DumpStats()` (editor/dev build) was added in step 10 to help.

## Step 1: value index (ModifierDataManager)
- Status: done
- Modified files: Assets/Scripts/Items/ConsumableOptions/LivingEntity/Modifire/ModifierDataManager.cs
- Summary: `valueDatas` index, `Get_ModifierValueData`, `HasModifierBase`, `ValidateValueBases` (one aggregated warning for missing bases and for valueId==baseId), `Get_ModifierGroupKeys` no longer allocates (L1).
- Completion check: awk over the CSVs gives 331 value rows, no duplicate valueId, 30 rows without base (all Action_*), 1 valueId/baseId overlap (ShortSword_Damage). Runtime check (`valueDatas.Count`, warning lines) needs play.
- Remaining: play check of the log lines.
- Spec difference: spec said 73 missing rows; actual count was 74 before the CSV spelling fixes (44 fixed, 30 left).

## Step 2: rent API (ModifierPooler, ModifierManager)
- Status: done
- Modified files: Managers/ModifierPooler.cs, ModifierManager.cs
- Summary: `GetModifier` -> private `RentBase`; public `RentByValue`, `RentByValueId`, `[Obsolete] RentBaseOnly`; `ModifierManager.Get_Modifier` -> `Get_ModifierByValueId`.
- Completion check: grep `GetModifier(` / `Get_Modifier(` in Assets/Scripts = 0 hits. `RentBaseOnly` users: ModifierAction.cs (StatBuff.GetMyModifier), EffectDataManager.cs (CopyEffect), MonsterManager.cs (old-save buff fallback).
- Remaining: run spec 8.1 test.

## Step 3: AssembleOption
- Status: done
- Modified files: ModifierFactory.cs
- Summary: loop uses `RentByValue` only; removed the commented-out old loop; per-modifier Debug.Log removed, the one remaining log is a `[Conditional("UNITY_EDITOR")]` helper (L2).
- Completion check: code review. Preview/tooltip numbers need play check.

## Step 4: return hardening
- Status: done
- Modified files: ModifierPooler.cs, Modifier.cs
- Summary: ReturnModifier reads baseID before Reset, null/empty key -> LogError + return, `isInPool` flag + active-list Remove check (duplicate/foreign return -> warning, no Enqueue). `Modifier.Return()` no longer calls Reset. Reset has a "never clear id/baseID" comment.
- Completion check: code review (grep: Reset() does not touch id/baseID).

## Step 5: Reset reinforcement
- Status: done
- Modified files: Modifier.cs
- Summary: base Reset clears stringValue; BuffModifier.Reset clears effectName, buffType; ItemModifier.Reset clears specificType, Equipable.

## Step 6: ownership (H1/H2)
- Status: held by user (H1). Nothing changed in RemoveModifiernInternal / RemoveModifierOptionInternal / OnDead dedup.
- Done anyway (needed for step 8 save of mutations): `AddMutation(ModifierOption)` records the option in `mutationOptions`, `RemoveMutate(ModifierOption)`, `Reset_Mutations`, `OnDead` clear it. File: ModifierController.cs.
- H2 duplicate returns are blocked in the pool (step 4).

## Step 7: call sites
- Status: done
- Modified files: ItemManager.cs, MonsterManager.cs, ModifierAction.cs, EffectDataManager.cs, ModifierOption.cs
- Summary: removed the extra-rent loop in `Set_RandomOption` (H3); `LoadItem` and monster load assemble the option and call `ModifierOption.RestoreSavedValues` (no extra rent, H4); buff load uses `Get_ModifierByValueId` (old save: `RentBaseOnly`). StatBuff/CopyEffect use `RentBaseOnly` (behavior kept). `AddNotIncludeMod` now compares valueId and handles null (no callers left).
- Completion check: grep.

## Step 8: save data
- Status: done
- Modified files: SaveStructs.cs, Modifier.cs, ModifierOption.cs, ModifierController.cs, EquipItem.cs
- Summary: `valueId` in ModifierSaveData/BuffSaveData; Modifier.SaveData saves valueId + baseID; ModifierOption.SaveData saves optionKey and initializes list; BuffSave saves valueId/baseID; EquipItem.SaveData saves itemId, itemCount, initializes addOptionsData.
- Remaining: save -> load play test. Old saves: RestoreSavedValues falls back to baseID, but old saves have no optionKey so they cannot be assembled (warning, skipped).

## Step 9: leak paths
- Status: partial
- Done: `ItemBase.ReleaseOptions` (+ EquipItem override incl. addOptions), `ConsumableItem.Clone` releases options rented by ItemMake, `ModifierOption.Return/Reset` delegate to `Return_ModifierOption`, `ItemManager.ReturnAllObjects` releases non-equipment floor items.
- Not done: equipment floor items (depends on H1 ownership), `MonsterManager.ReturnAllObjects` (Q12 unanswered).

## Step 10: Prewarm and stats
- Status: done
- Modified files: ModifierPooler.cs, ModifierDataManager.cs (`Get_PrewarmPlan`), DataManager.cs (call after ModOption load)
- Summary: `Prewarm`, `PrewarmAll`, `PrewarmFromData`; sizes 16 shared / 4 Buff_* / 8 others / 0 unused; counters + `DumpStats()` in editor/dev build; one warning when the pool grows after prewarm.
- Remaining: measure peaks and tune sizes (Q10).

## CSV spelling fixes (decision 6)
- ModOption_Stat.csv: ModifierID `Weapon_Accurency` -> `Weapon_Accuracy` (27 rows)
- ModOption_AddOptions.csv: `Weapon_Accurancy` -> `Weapon_Accuracy` (14 rows)
- Mod_StatModifier.csv: base ID `Weapon_MagicAccurancy` -> `Weapon_MagicAccuracy` (1 row; the value row already used the enum spelling)
- Mod_BuffModifier.csv: base ID `Buff_Teleport` -> `Buff_Telleport` (project-wide spelling, enum ActionEffectType.Telleport)
- Mod_BuffModifier.csv: base ID `Buff_LifeSeal` -> `Buff_LifeSteal` (value row, EffectName and StringValue all say LifeSteal)
- Remaining unresolved (not spelling): 30 rows in ModOption_Action.csv (`Action_*`), no base exists; skipped with the aggregated warning.
