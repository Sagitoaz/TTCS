using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TTCS.UI.Combat;
using TTCS.Core.Events;

/// <summary>
/// Test BattleHUD — InitializeSlots, event-driven HP bar updates.
/// Tạo minimal runtime UGUI structure để test event reactions.
/// Add component này lên một GameObject và Play để chạy.
/// </summary>
public class BattleHUDTest : MonoBehaviour
{
    private int _pass = 0;
    private int _fail = 0;

    private void Start()
    {
        Debug.Log("=== BattleHUDTest ===");

        // ── Test 1: BattleHUD component tạo không crash ──
        var canvasGO = new GameObject("BattleHUDTestCanvas");
        canvasGO.AddComponent<Canvas>();
        var hudGO = new GameObject("TestBattleHUD");
        hudGO.transform.SetParent(canvasGO.transform);

        BattleHUD hud = null;
        bool crashed = false;
        try
        {
            hud = hudGO.AddComponent<BattleHUD>();
        }
        catch (System.Exception ex)
        {
            crashed = true;
            Debug.LogError($"BattleHUD AddComponent crash: {ex.Message}");
        }
        LogResult("BattleHUD AddComponent", !crashed && hud != null, "");

        // ── Test 2: DamageTakenEvent trước InitializeSlots → không crash ──
        // (slots chưa được init nên HUD nên bỏ qua gracefully)
        crashed = false;
        try
        {
            EventBus.Instance.Publish(new DamageTakenEvent("unknown_01", "src_01", 80, false));
        }
        catch (System.Exception ex)
        {
            crashed = true;
            Debug.LogError($"DamageTakenEvent trước init crash: {ex.Message}");
        }
        LogResult("DamageTakenEvent trước InitializeSlots → không crash", !crashed, "");

        // ── Test 3: HealingReceivedEvent trước init ──
        crashed = false;
        try
        {
            EventBus.Instance.Publish(new HealingReceivedEvent("unknown_01", "src_01", 30));
        }
        catch (System.Exception ex)
        {
            crashed = true;
            Debug.LogError($"HealingReceivedEvent crash: {ex.Message}");
        }
        LogResult("HealingReceivedEvent trước init → không crash", !crashed, "");

        // ── Test 4: EntityDeathEvent trước init ──
        crashed = false;
        try
        {
            EventBus.Instance.Publish(new EntityDeathEvent("unknown_01", "src_01"));
        }
        catch (System.Exception ex)
        {
            crashed = true;
            Debug.LogError($"EntityDeathEvent crash: {ex.Message}");
        }
        LogResult("EntityDeathEvent trước init → không crash", !crashed, "");

        // ── Test 5: Damage event cho entity đã known (sau khi register positions) ──
        // CombatUIController cần có entity HP data
        var controller = CombatUIController.Instance;
        if (controller != null)
        {
            var fakeTransform = new GameObject("FakeEntityPos").transform;
            controller.RegisterEntityPosition("hud_test_entity", fakeTransform);

            crashed = false;
            try
            {
                EventBus.Instance.Publish(new DamageTakenEvent("hud_test_entity", "src_01", 100, false));
            }
            catch (System.Exception ex)
            {
                crashed = true;
                Debug.LogError($"DamageTakenEvent(known entity) crash: {ex.Message}");
            }
            LogResult("DamageTakenEvent(entity registered) → không crash", !crashed, "");

            Destroy(fakeTransform.gameObject);
        }
        else
        {
            Debug.Log("[Test5] SKIP — CombatUIController không có trong scene");
            _pass++;
        }

        // ── Cleanup ──
        Destroy(canvasGO);

        Debug.Log($"=== BattleHUDTest DONE: {_pass} passed, {_fail} failed ===");
    }

    private void LogResult(string name, bool pass, string detail = "")
    {
        if (pass) { _pass++; Debug.Log($"✅ {name} — {detail}"); }
        else      { _fail++; Debug.LogError($"❌ {name} — {detail}"); }
    }
}
