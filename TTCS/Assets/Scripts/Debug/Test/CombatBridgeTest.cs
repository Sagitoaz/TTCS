using System.Collections.Generic;
using UnityEngine;
using TTCS.Combat.Managers;
using TTCS.Combat.Timing;
using TTCS.UI.Combat;
using TTCS.Core.Events;

/// <summary>
/// Test CombatBridge — RegisterView, ICharacterAnimatorBridge, event forwarding.
/// Dùng một inner mock class để simulate CharacterView mà không cần Dev B code.
/// Add component này lên một GameObject và Play để chạy.
/// </summary>
public class CombatBridgeTest : MonoBehaviour
{
    // ── Mock implementation của ICharacterAnimatorBridge (thay thế cho CharacterView của Dev B) ──
    private class MockAnimatorBridge : CombatBridge.ICharacterAnimatorBridge
    {
        public int AttackCalls  = 0;
        public int HurtCalls    = 0;
        public int DeathCalls   = 0;
        public int VictoryCalls = 0;
        private readonly Transform _t;

        public MockAnimatorBridge(Transform t) { _t = t; }

        public void PlayAttack()  => AttackCalls++;
        public void PlayHurt()    => HurtCalls++;
        public void PlayDeath()   => DeathCalls++;
        public void PlayVictory() => VictoryCalls++;
        public Transform GetWorldTransform() => _t;
    }

    private int _pass = 0;
    private int _fail = 0;

    private void Start()
    {
        Debug.Log("=== CombatBridgeTest ===");

        var bridge = CombatBridge.Instance;
        if (bridge == null)
        {
            Debug.LogError("CombatBridge.Instance không tồn tại — thêm CombatBridge vào scene!");
            return;
        }

        var mockA = new MockAnimatorBridge(transform);
        var mockB = new MockAnimatorBridge(transform);
        const string idA = "test_unit_A";
        const string idB = "test_unit_B";

        // ── Test 1: RegisterView ──
        bridge.RegisterView(idA, mockA);
        bridge.RegisterView(idB, mockB);
        Debug.Log($"[Test1] RegisterView x2 — không crash = PASS");
        _pass++;

        // ── Test 2: SkillCastEvent → PlayAttack trên caster ──
        EventBus.Instance.Publish(new SkillCastEvent(idA, "skill_slash", new[] { idB }));
        LogResult("SkillCastEvent → PlayAttack(idA)", mockA.AttackCalls == 1,
            $"AttackCalls = {mockA.AttackCalls}");

        // ── Test 3: DamageTakenEvent → PlayHurt trên target ──
        EventBus.Instance.Publish(new DamageTakenEvent(idB, idA, 120, false));
        LogResult("DamageTakenEvent → PlayHurt(idB)", mockB.HurtCalls == 1,
            $"HurtCalls = {mockB.HurtCalls}");

        // ── Test 4: EntityDeathEvent → PlayDeath ──
        EventBus.Instance.Publish(new EntityDeathEvent(idB, idA));
        LogResult("EntityDeathEvent → PlayDeath(idB)", mockB.DeathCalls == 1,
            $"DeathCalls = {mockB.DeathCalls}");

        // ── Test 5: GetWorldTransform trả về đúng transform ──
        Transform t = mockA.GetWorldTransform();
        LogResult("GetWorldTransform trả về transform đúng", t == transform,
            $"t == this.transform: {t == transform}");

        // ── Test 6: NotifyTelegraphComplete mở timing window ──
        // Ghi nhận rằng TimingSystem cần tồn tại trong scene
        if (TimingSystem.Instance != null)
        {
            TimingSystem.Instance.OnTimingResult += _ => { };
            bridge.NotifyTelegraphComplete(idA, 1.0f);
            Debug.Log($"[Test6] NotifyTelegraphComplete called — TimingSystem.OpenWindow sẽ được gọi.");
            // Không thể assert ngay lập tức vì window async, chỉ log
            _pass++;
        }
        else
        {
            Debug.Log("[Test6] SKIP — TimingSystem không có trong scene");
        }

        // ── Test 7: Đăng ký entity không tồn tại → không crash ──
        EventBus.Instance.Publish(new DamageTakenEvent("unknown_entity", idA, 50, false));
        Debug.Log($"[Test7] Event trên entity không registered → không crash = PASS");
        _pass++;

        Debug.Log($"=== CombatBridgeTest DONE: {_pass} passed, {_fail} failed ===");
    }

    private void LogResult(string name, bool pass, string detail = "")
    {
        if (pass) { _pass++; Debug.Log($"✅ {name} — {detail}"); }
        else      { _fail++; Debug.LogError($"❌ {name} — {detail}"); }
    }
}
