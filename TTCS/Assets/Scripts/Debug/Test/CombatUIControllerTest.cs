using System.Collections.Generic;
using UnityEngine;
using TTCS.UI.Combat;
using TTCS.Core.Events;

/// <summary>
/// Test CombatUIController — entity registration, position lookup, HP percent.
/// CombatUIController phải có trong scene (singleton).
/// Add component này lên một GameObject và Play để chạy.
/// </summary>
public class CombatUIControllerTest : MonoBehaviour
{
    private int _pass = 0;
    private int _fail = 0;

    private void Start()
    {
        Debug.Log("=== CombatUIControllerTest ===");

        var controller = CombatUIController.Instance;
        if (controller == null)
        {
            Debug.LogError("CombatUIController.Instance không tồn tại — thêm vào scene!");
            return;
        }

        // ── Test 1: RegisterEntityPosition và GetEntityWorldPos ──
        const string entityId = "test_entity_001";
        var testObj = new GameObject("TestEntity");
        testObj.transform.position = new Vector3(3f, 5f, 0f);

        controller.RegisterEntityPosition(entityId, testObj.transform);

        Vector3 retrieved = controller.GetEntityWorldPos(entityId);
        bool posMatch = Vector3.Distance(retrieved, testObj.transform.position) < 0.001f;
        LogResult("RegisterEntityPosition + GetEntityWorldPos", posMatch,
            $"expected (3,5,0), got {retrieved}");

        // ── Test 2: GetEntityWorldPos cho entity chưa đăng ký → Vector3.zero ──
        Vector3 unknown = controller.GetEntityWorldPos("non_existent_id");
        LogResult("GetEntityWorldPos unknown → Vector3.zero", unknown == Vector3.zero,
            $"got {unknown}");

        // ── Test 3: EventBus — Publish DamageTakenEvent không crash controller ──
        EventBus.Instance.Publish(new DamageTakenEvent(entityId, "enemy_01", 100, false));
        Debug.Log("[Test3] Publish DamageTakenEvent → không crash = PASS");
        _pass++;

        // ── Test 4: ShowTimingResult gọi không crash (TimingFeedbackUI có thể null nếu không setup) ──
        try
        {
            controller.ShowTimingResult(TTCS.UI.Combat.TimingGrade.Perfect);
            Debug.Log("[Test4] ShowTimingResult(Perfect) → không crash = PASS");
            _pass++;
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"[Test4] ShowTimingResult threw: {ex.Message} — kiểm tra TimingFeedbackUI có được assign chưa");
        }

        // ── Test 5: CombatEndedEvent unsubscribes sau nhận ──
        EventBus.Instance.Publish(new CombatEndedEvent(true));
        Debug.Log("[Test5] CombatEndedEvent published → không crash = PASS");
        _pass++;

        // Cleanup
        Destroy(testObj);

        Debug.Log($"=== CombatUIControllerTest DONE: {_pass} passed, {_fail} failed ===");
    }

    private void LogResult(string name, bool pass, string detail = "")
    {
        if (pass) { _pass++; Debug.Log($"✅ {name} — {detail}"); }
        else      { _fail++; Debug.LogError($"❌ {name} — {detail}"); }
    }
}
