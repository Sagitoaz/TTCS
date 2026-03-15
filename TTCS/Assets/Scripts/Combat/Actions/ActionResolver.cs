using System.Collections.Generic;
using UnityEngine;
using TTCS.Combat.Effects;
using TTCS.Combat.Entities;
using TTCS.Combat.Stats;
using TTCS.Core.Events;
using TTCS.Core.Utilities;
using TTCS.Data;
using TTCS.Debugging;
using static TTCS.Debugging.DebugLogger;
using TTCS.UI.Combat; // TimingGrade
namespace TTCS.Combat.Actions
{
    /// <summary>
    /// 🟢 Dev B - Action Resolver
    /// Static — tính toán và áp dụng kết quả của một skill action.
    ///
    /// Được gọi từ SkillAction.Execute() sau khi SkillManager đã commit cost.
    ///
    /// Resolve pipeline cho mỗi target:
    ///   Type "attack"  → CalculateDamage → TakeDamage → apply effects
    ///   Type "heal"    → CalculateHeal → Heal → apply effects
    ///   Type "buff"    → only apply effects to ally
    ///   Type "debuff"  → only apply effects to enemy
    ///
    /// Publish events: ActionExecutedEvent, SkillCastEvent
    /// </summary>
    public static class ActionResolver
    {
        // ─── Main Resolve ─────────────────────────────────────────────────
        /// <summary>
        /// Resolve một skill: tính damage/heal, áp dụng effects lên tất cả target.
        /// </summary>
        public static void Resolve(
            CombatEntity actor,
            List<CombatEntity> targets,
            SkillDataModel skill,
            TimingGrade guard = TimingGrade.Miss)
        {
            if (actor == null || skill == null || targets == null) return;

            // Publish skill cast event
            var targetIds = GetTargetIds(targets);
            EventBus.Instance.Publish(new SkillCastEvent(actor.ID, skill.id, targetIds));

            float multiplier = ParseMultiplier(skill.damage?.formula);

            foreach (var target in targets)
            {
                if (target == null) continue;

                // Skip dead targets for attack/debuff skills
                bool isHostile = skill.type == "attack" || skill.type == "debuff";
                if (isHostile && target.IsDead) continue;

                switch (skill.type)
                {
                    case "attack":
                        ResolveAttack(actor, target, skill, multiplier, guard);
                        break;

                    case "heal":
                        ResolveHeal(actor, target, skill, multiplier);
                        break;

                    case "buff":
                    case "debuff":
                        ResolveEffectsOnly(actor, target, skill);
                        break;

                    default:
                        // Default: attack
                        ResolveAttack(actor, target, skill, multiplier, guard);
                        break;
                }
            }

            // Publish action executed event
            EventBus.Instance.Publish(new ActionExecutedEvent(actor.ID, skill.id, targetIds));

            Log($"ActionResolver: '{actor.ID}' used '{skill.id}' on {targets.Count} target(s)",
                LogCategory.Combat);
        }

        // ─── Attack Resolve ───────────────────────────────────────────────
        private static void ResolveAttack(
            CombatEntity actor,
            CombatEntity target,
            SkillDataModel skill,
            float multiplier,
            TimingGrade guard = TimingGrade.Miss)
        {
            var elementEnum = ParseElement(skill.damage?.element);

            var (damage, isCrit) = StatCalculator.CalculateDamage(
                actor, target, multiplier, elementEnum);

            if (target.IsPlayer)
            {
                float guardMultiplier = guard switch
                {
                    TimingGrade.Perfect => 0.2f,
                    TimingGrade.Good => 0.6f,
                    _ => 1.0f   // Miss
                };
                damage = Mathf.RoundToInt(damage * guardMultiplier);
                Log($"  → Guard: grade={guard}, multiplier={guardMultiplier:F1}x → final dmg={damage}",
                    LogCategory.Combat);
            }

            // Attacker timing bonus (player attacking)
            if (actor.IsPlayer)
            {
                float attackBonus = guard switch
                {
                    TimingGrade.Perfect => 1.2f,
                    TimingGrade.Good    => 1.0f,
                    _                   => 0.6f
                };
                damage = Mathf.RoundToInt(damage * attackBonus);
                Log($"  → Attack timing: grade={guard}, multiplier={attackBonus:F1}x → dmg={damage}",
                    LogCategory.Combat);
            }

            target.TakeDamage(damage, actor.ID);

            Log($"  → Attack: '{actor.ID}' → '{target.ID}' dmg={damage}{(isCrit ? " [CRIT]" : "")}",
                LogCategory.Combat);

            // Apply on-hit effects (if any + chance roll)
            if (skill.effects != null)
            {
                foreach (var eff in skill.effects)
                {
                    if (eff == null) continue;
                    // Only effect-type entries (not direct damage)
                    if (eff.type == "damage") continue;
                    TryApplyEffect(eff, actor, target);
                }
            }
        }

        // ─── Heal Resolve ─────────────────────────────────────────────────
        private static void ResolveHeal(
            CombatEntity actor,
            CombatEntity target,
            SkillDataModel skill,
            float multiplier
            )
        {
            int healAmt = StatCalculator.CalculateHeal(actor, multiplier);
            target.Heal(healAmt, actor.ID);

            Log($"  → Heal: '{actor.ID}' → '{target.ID}' heal={healAmt}", LogCategory.Combat);

            // Apply additional effects (e.g. HoT)
            if (skill.effects != null)
            {
                foreach (var eff in skill.effects)
                {
                    if (eff == null || eff.type == "damage") continue;
                    TryApplyEffect(eff, actor, target);
                }
            }
        }

        // ─── Effects Only ─────────────────────────────────────────────────
        private static void ResolveEffectsOnly(
            CombatEntity actor,
            CombatEntity target,
            SkillDataModel skill)
        {
            if (skill.effects == null) return;

            foreach (var eff in skill.effects)
            {
                if (eff == null) continue;
                TryApplyEffect(eff, actor, target);
            }
        }

        // ─── Effect Application ───────────────────────────────────────────
        /// <summary>Apply một SkillEffect lên target nếu chance roll thành công</summary>
        private static void TryApplyEffect(
            SkillEffect skillEffect,
            CombatEntity actor,
            CombatEntity target)
        {
            float chance = skillEffect.chance <= 0f ? 1f : skillEffect.chance;

            if (!RNGService.Instance.RollChance(chance)) return;

            StatusEffect effect = BuildStatusEffect(skillEffect, actor);
            if (effect == null) return;

            target.ApplyEffect(effect);

            Log($"  → Effect '{skillEffect.type}' applied to '{target.ID}' ({chance:P0} chance)",
                LogCategory.Combat);
        }

        /// <summary>Tạo StatusEffect từ SkillEffect definition</summary>
        private static StatusEffect BuildStatusEffect(SkillEffect eff, CombatEntity actor)
        {
            int duration = eff.duration > 0 ? eff.duration : 2;
            float intensity = TryParseFloat(eff.value, 30f);

            return eff.type switch
            {
                "bleed" => new BleedEffect(intensity, duration),
                "burn" => new BurnEffect(intensity, duration),
                "heal_regen"
                    or "heal" => new HealEffect(intensity, duration),
                "shield" => new ShieldEffect(intensity, duration),
                "stun" => new StunEffect(duration),
                _ => null
            };
        }

        // ─── Helpers ──────────────────────────────────────────────────────
        /// <summary>
        /// Phân tích multiplier từ formula string.
        /// Hỗ trợ: "ATK * 1.5", "1.5", "ATK*2.0"
        /// Mặc định: 1.0
        /// </summary>
        private static float ParseMultiplier(string formula)
        {
            if (string.IsNullOrEmpty(formula)) return 1.0f;

            // Pattern: "ATK * X" or "ATK*X"
            var parts = formula.Replace(" ", "").Split('*');
            if (parts.Length == 2 && float.TryParse(parts[1], out float mult))
                return mult;

            // Try plain float
            if (float.TryParse(formula, out float directMult))
                return directMult;

            return 1.0f;
        }

        private static float TryParseFloat(string value, float defaultVal)
        {
            return float.TryParse(value, out float f) ? f : defaultVal;
        }

        private static Element ParseElement(string elementStr)
        {
            if (string.IsNullOrEmpty(elementStr)) return Element.Physical;

            return elementStr.ToLower() switch
            {
                "fire" => Element.Fire,
                "ice" => Element.Ice,
                "lightning" => Element.Lightning,
                "dark" => Element.Dark,
                "light" => Element.Light,
                _ => Element.Physical
            };
        }

        private static string[] GetTargetIds(List<CombatEntity> targets)
        {
            if (targets == null) return new string[0];
            var ids = new string[targets.Count];
            for (int i = 0; i < targets.Count; i++)
                ids[i] = targets[i]?.ID ?? "";
            return ids;
        }
    }
}
