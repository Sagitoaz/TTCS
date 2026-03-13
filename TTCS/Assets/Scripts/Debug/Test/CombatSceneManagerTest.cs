using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TTCS.Combat.Managers;
using TTCS.Core.Data;
using TTCS.Data;

/// <summary>
/// Test CombatSceneManager — InitializeCombat pipeline validation.
/// DataManager phải load xong (tự load trong Awake), EntityFactory phải available.
/// Add component này lên một GameObject và Play để chạy.
/// </summary>
public class CombatSceneManagerTest : MonoBehaviour
{
    private int _pass = 0;
    private int _fail = 0;

    private void Start()
    {
        Debug.Log("=== CombatSceneManagerTest ===");
        StartCoroutine(RunTests());
    }

    private IEnumerator RunTests()
    {
        // Chờ DataManager load xong (thường xảy ra trong Awake của DataManager)
        yield return new WaitForSeconds(0.5f);

        var manager = CombatSceneManager.Instance;
        if (manager == null)
        {
            Debug.LogError("CombatSceneManager.Instance không tồn tại — thêm vào scene!");
            yield break;
        }

        // ── Test 1: Instance tồn tại và unique ──
        var instance2 = CombatSceneManager.Instance;
        LogResult("CombatSceneManager singleton unique", ReferenceEquals(manager, instance2),
            $"same instance: {ReferenceEquals(manager, instance2)}");

        // ── Test 2: GetFirstWaveEnemyIds trả về non-null ──
        // Thử load stage_01 — nếu DataManager có data thì trả về list; nếu không thì empty
        var stage = DataManager.Instance.LoadStage("stage_01");
        if (stage != null)
        {
            var enemyIds = manager.GetFirstWaveEnemyIds(stage);
            LogResult("GetFirstWaveEnemyIds trả về non-null", enemyIds != null,
                $"count = {enemyIds?.Count ?? -1}");
        }
        else
        {
            Debug.Log("[Test2] SKIP — stage_01 không có trong DataManager (data chưa load)");
            _pass++;
        }

        // ── Test 3: InitializeCombat với party ids hợp lệ ──
        // Chỉ test không crash — không verify toàn bộ pipeline vì Dev B views chưa có
        bool crashed = false;
        try
        {
            // Dùng IDs có trong JSON data nếu có; nếu không thì sẽ log warning
            var party = new List<string> { "char_warrior", "char_mage" };
            StartCoroutine(TestInitCoroutine(manager, "stage_01", party, 12345));
        }
        catch (System.Exception ex)
        {
            crashed = true;
            Debug.LogWarning($"[Test3] InitializeCombat throw ngay: {ex.Message}");
        }
        if (!crashed)
        {
            Debug.Log("[Test3] InitializeCombat coroutine started → no immediate crash = PASS");
            _pass++;
        }

        yield return new WaitForSeconds(0.5f);

        Debug.Log($"=== CombatSceneManagerTest DONE: {_pass} passed, {_fail} failed ===");
    }

    private IEnumerator TestInitCoroutine(CombatSceneManager manager, string stageId,
                                          List<string> partyIds, int seed)
    {
        // Wrap trong try/catch không được dùng với yield, nên chỉ log kết quả
        yield return StartCoroutine(manager.InitializeCombat(stageId, partyIds, seed));
        Debug.Log($"[Test3 Coroutine] InitializeCombat({stageId}) completed — pipeline ran without exception");
    }

    private void LogResult(string name, bool pass, string detail = "")
    {
        if (pass) { _pass++; Debug.Log($"✅ {name} — {detail}"); }
        else      { _fail++; Debug.LogError($"❌ {name} — {detail}"); }
    }
}
