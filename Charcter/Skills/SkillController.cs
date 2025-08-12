using UnityEngine;
using System.Collections.Generic;
using static DesignEnums;

/// <summary>
/// 스킬을 실제로 사용하는 컨트롤러
/// </summary>
public class SkillController
{
    // 스킬 사용 메인 진입점
    BuffManager buffManager = BuffManager.Instance;
    static GameObject characterPrefab;

    public static void SkillUse(BattleCharacter user, SkillSO skill, List<BattleCharacter> allAllies, BattleEnemy enemy)
    {
        List<IDamagable> targets = GetTargets(user, skill.SkillTarget, allAllies, enemy);
        

        switch (skill.SkillRule)
        {
            case SkillRule.Cost:
                UseCostSkill(user, skill, targets);
                break;
            case SkillRule.SumOver:
                UseSumOverSkill(user, skill, targets);
                break;
            case SkillRule.UniqueSigniture:
                UseUniqueSignitureSkill(user, skill, targets);
                break;
            case SkillRule.DeckMaid:
                UseDeckMaidSkill(user, skill, targets);
                break;
            case SkillRule.TeamSignitureDeckMaid:
                UseTeamSignitureDeckMaidSkill(user, skill, targets);
                break;
            default:
                Debug.LogWarning($"SkillRule 미구현: {skill.SkillRule}");
                break;
        }
    }

    // 타겟 결정
    private static List<IDamagable> GetTargets(BattleCharacter user, SkillTarget targetType, List<BattleCharacter> allAllies, BattleEnemy enemy)
    {
        switch (targetType)
        {
            case SkillTarget.Self:
                return new List<IDamagable> { user };
            case SkillTarget.Enemy:
                return enemy != null ? new List<IDamagable> { enemy } : new List<IDamagable>();
            case SkillTarget.Team:
                return new List<IDamagable>(allAllies);
            default:
                return new List<IDamagable> { user };
        }
    }

    // 각 SkillRule별 처리 (예시)
    private static void UseCostSkill(BattleCharacter user, SkillSO skill, List<IDamagable> targets)
    {
        // 코스트 소모
        // 여기 왔다는 것 = ActiveSkill이므로 코스트가 존재함
        // skillSO가 ActiveSO 경우에만 코스트를 소모

        if (skill is ActiveSO activeSO)
        {
            BattleManager.Instance.SpendCost(activeSO.SkillCost);
        }

        if (skill.IsAttacking)
            user.GetBonusAttack = true; // 보너스 공격 가능 상태로 설정
        else
        {
            // 버프 적용 로직 등
            ApplyBuff(user, skill, targets);
        }
    }

    private static void UseSumOverSkill(BattleCharacter user, SkillSO skill, List<IDamagable> targets)
    {
        if (skill.IsAttacking)
           user.GetBonusAttack = true; // 보너스 공격 가능 상태로 설정
        else
        {
            // 버프 적용 로직 등
            ApplyBuff(user, skill, targets);
        }
    }

    private static void UseUniqueSignitureSkill(BattleCharacter user, SkillSO skill, List<IDamagable> targets)
    {
        if (skill.IsAttacking)
            user.GetBonusAttack = true; // 보너스 공격 가능 상태로 설정
        else
        {
            // 버프 적용 로직 등
            ApplyBuff(user, skill, targets);
        }
    }

    private static void UseDeckMaidSkill(BattleCharacter user, SkillSO skill, List<IDamagable> targets)
    {
        if (skill.IsAttacking)
            user.GetBonusAttack = true; // 보너스 공격 가능 상태로 설정
        else
        {
            // 버프 적용 로직 등
            ApplyBuff(user, skill, targets);
        }
    }

    private static void UseTeamSignitureDeckMaidSkill(BattleCharacter user, SkillSO skill, List<IDamagable> targets)
    {
        if (skill.IsAttacking)
            user.GetBonusAttack = true; // 보너스 공격 가능 상태로 설정
        else
        {
            // 버프 적용 로직 등
            ApplyBuff(user, skill, targets);
        }
    }

    // Skill의 BuffID에 따른 버프 적용
    private static void ApplyBuff(BattleCharacter user, SkillSO skill, List<IDamagable> targets)
    {
        // 애니메이션 처리
        characterPrefab = BattleManager.Instance.PartyData.Characters[BattleManager.Instance.PartyData.DefaultCharacters.IndexOf(user)].Prefab;
        characterPrefab.GetComponent<SpawnedCharacter>().SkillAttack();

        // BuffDataLoader 인스턴스 생성 (리소스에서 버프 데이터 로드)
        BuffDataLoader buffLoader = new BuffDataLoader();

        // SkillSO의 BuffID, BuffID_2 참조
        string[] buffIDs = new string[2];
        int skillLevel = 1;
        if (skill is ActiveSO activeSO && activeSO.BuffIDs != null)
        {
            buffIDs = activeSO.BuffIDs;
            skillLevel = user.SkillLevelA;
        }
        else if (skill is PassiveSO passiveSO && passiveSO.BuffIDs != null)
        {
            buffIDs = passiveSO.BuffIDs;
            skillLevel = user.SkillLevelB;
        }
        else if (!string.IsNullOrEmpty(skill.SkillDescription))
        {
            buffIDs[0] = skill.SkillDescription;
        }
        else
        {
            buffIDs[0] = null;
            buffIDs[1] = null;
        }

        // 각 대상에게 버프/디버프 적용
        foreach (var target in targets)
        {
            for (int i = 0; i < buffIDs.Length; i++)
            {
                if (string.IsNullOrEmpty(buffIDs[i]) || buffIDs[i] == "None") continue;
                BuffData buff = buffLoader.ItemsList.Find(b => b.CharID == buffIDs[i]);
                if (buff == null) continue;

                // 확률/값 누적 적용 (레벨별)
                int probability = skill.BuffProbability + skill.PlusBuffProbability * (skillLevel - 1);
                float value = skill.BuffValue + skill.PlusBuffValue * (skillLevel - 1);

                if (Random.Range(0, 100) >= probability) continue;

                // 턴/스택 적용
                int turn = skill.BuffTurn;
                int stack = buff.Stack;

                // 버프/디버프 효과 적용 (스탯 변화)
                if (target is BattleCharacter bc)
                {
                    switch (buff.BuffCategory)
                    {
                        case BuffCategory.ATK:
                            bc.CurrentATK += Mathf.RoundToInt(bc.CurrentATK * value);
                            break;
                        case BuffCategory.DEF:
                            bc.CurrentDEF += Mathf.RoundToInt(bc.CurrentDEF * value);
                            break;
                        case BuffCategory.HP:
                            bc.CurrentHP += Mathf.RoundToInt(bc.CurrentHP * value);
                            if (bc.CurrentHP > bc.RegularHP) bc.CurrentHP = bc.RegularHP;
                            break;
                        case BuffCategory.CritC:
                            bc.CurrentCritChance += bc.CurrentCritChance * value;
                            break;
                        case BuffCategory.CritD:
                            bc.CurrentCritDamage += bc.CurrentCritDamage * value;
                            break;
                        case BuffCategory.Pene:
                            bc.CurrentPenetration += bc.CurrentPenetration * value;
                            break;
                        case BuffCategory.Heal:
                            bc.Heal(Mathf.RoundToInt(bc.RegularHP * value));
                            break;
                        case BuffCategory.Cost:
                            BattleManager.Instance.GetCost(Mathf.RoundToInt(value));
                            break;
                        case BuffCategory.Shield:
                            // 보호막 로직 (예시)
                            // bc.AddShield(Mathf.RoundToInt(bc.RegularHP * value));
                            break;
                        default:
                            break;
                    }
                    Debug.Log($"{user.CharNameKr}이(가) {bc.CharNameKr}에게 {buff.NameKr} 버프/디버프({value}, {turn}턴, {stack}스택)를 적용");
                }
                else if (target is BattleEnemy be)
                {
                    switch (buff.BuffCategory)
                    {
                        case BuffCategory.ATK:
                            be.DebuffAtk += value;
                            break;
                        case BuffCategory.DEF:
                            be.DebuffDef += value;
                            break;
                        case BuffCategory.HP:
                            be.Heal(Mathf.RoundToInt(be.MaxHP * value));
                            break;
                        default:
                            break;
                    }
                    Debug.Log($"{user.CharNameKr}이(가) 적에게 {buff.NameKr} 디버프({value}, {turn}턴, {stack}스택)를 적용");
                }
            }
        }
        Debug.Log($"{user.CharNameKr}이(가) {skill.SkillNameKr} 스킬을 사용하여 버프/디버프를 적용했습니다.");
    }


    // 현재 스킬이 적용되는 중인지 확인하는 메서드 (턴 감소 포함)
    public void SkillCheck()
    {



    }



}
