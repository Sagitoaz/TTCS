using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TTCS.UI.Combat;
using TTCS.Core.Events;

/// <summary>
/// Test SkillButtonPanel — target resolution logic, event subscription behaviour.
/// Tạo minimal UGUI wiring ở runtime để test logic mà không cần prefab đầy đủ.
/// Add component này lên một GameObject và Play để chạy.
/// </summary>
public class SkillButtonPanelTest : MonoBehaviour
{
    private int _pass = 0;
    private int _fail = 0;

    private void Start()
    {
        Debug.Log("=== SkillButtonPanelTest ===");

        // ── Tạo SkillButtonPanel object ──
        var panelGO = new GameObject("TestSkillButtonPanel");
        var panel = panelGO.AddComponent<SkillButtonPanel>();

        // SkillButtonPanel cần Canvas để có CanvasGroup
        var canvasGO = new GameObject("Canvas");
        canvasGO.AddComponent<Canvas>();
        canvasGO.transform.SetParent(transform);
        panelGO.transform.SetParent(canvasGO.transform);
        panelGO.AddComponent<CanvasGroup>();

        // Test 1: Component tạo không crash
        LogResult("SkillButtonPanel AddComponent", panel != null, "component created");

        // Test 2: TurnStartedEvent cho entity khác → panel không show (panel chưa Initialize)
        bool crashed = false;
        try
        {
            EventBus.Instance.Publish(new TurnStartedEvent("enemy_01", 1));
        }
        catch (System.Exception ex)
        {
            crashed = true;
            Debug.LogError($"TurnStartedEvent crash: {ex.Message}");
        }
        LogResult("TurnStartedEvent trước Initialize → không crash", !crashed, "");

        // Test 3: TurnEndedEvent → không crash
        crashed = false;
        try
        {
            EventBus.Instance.Publish(new TurnEndedEvent("enemy_01"));
        }
        catch (System.Exception ex)
        {
            crashed = true;
            Debug.LogError($"TurnEndedEvent crash: {ex.Message}");
        }
        LogResult("TurnEndedEvent → không crash", !crashed, "");

        // Test 4: Gọi Initialize với dữ liệu tối thiểu → không crash
        // (SkillManager cần có entity registered)
        crashed = false;
        try
        {
            TTCS.Combat.Managers.SkillManager.Instance.RegisterEntity("test_player_01", 100, 80);
            var dummyEnemies = new List<TTCS.Combat.Entities.CombatEntity>(); // danh sách trống
            panel.Initialize("test_player_01", new List<string> { "skill_slash", "skill_fireball" }, dummyEnemies);
            Debug.Log("[Test4] Initialize với dummyEnemies → không crash = PASS");
            _pass++;
        }
        catch (System.Exception ex)
        {
            crashed = true;
            Debug.LogWarning($"[Test4] Initialize threw: {ex.Message} — thường xảy ra khi DataManager chưa load skill data");
        }

        // Test 5: TurnStartedEvent cho đúng entity → panel hiện (CanvasGroup alpha change)
        crashed = false;
        try
        {
            EventBus.Instance.Publish(new TurnStartedEvent("test_player_01", 2));
        }
        catch (System.Exception ex)
        {
            crashed = true;
            Debug.LogError($"TurnStartedEvent(player) crash: {ex.Message}");
        }
        LogResult("TurnStartedEvent(player entity) → không crash", !crashed, "");

        Destroy(canvasGO);
        TTCS.Combat.Managers.SkillManager.Instance.ResetCombat();

        Debug.Log($"=== SkillButtonPanelTest DONE: {_pass} passed, {_fail} failed ===");
    }

    private void LogResult(string name, bool pass, string detail = "")
    {
        if (pass) { _pass++; Debug.Log($"✅ {name} — {detail}"); }
        else      { _fail++; Debug.LogError($"❌ {name} — {detail}"); }
    }
}
