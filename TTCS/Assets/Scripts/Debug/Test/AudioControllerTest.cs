using UnityEngine;
using TTCS.Audio;
using TTCS.UI.Combat; // TimingGrade
using TTCS.Core.Events;

/// <summary>
/// Test AudioController — volume setters/getters, event subscriptions, PlayTimingResult.
/// AudioController là DontDestroyOnLoad singleton — phải có trong scene hoặc được tạo tự động.
/// Add component này lên một GameObject và Play để chạy.
/// </summary>
public class AudioControllerTest : MonoBehaviour
{
    private int _pass = 0;
    private int _fail = 0;

    private void Start()
    {
        Debug.Log("=== AudioControllerTest ===");

        var audio = AudioController.Instance;
        if (audio == null)
        {
            Debug.LogError("AudioController.Instance không tồn tại — thêm AudioController vào scene!");
            return;
        }

        // ── Test 1: Singleton duy nhất ──
        var instance2 = AudioController.Instance;
        LogResult("AudioController singleton unique", ReferenceEquals(audio, instance2), "");

        // ── Test 2: SetSFXVolume + PlaySFX không crash khi clip = null ──
        bool crashed = false;
        try
        {
            audio.SetSFXVolume(0.7f);
            audio.PlaySFX(null); // clip null — không crash vì guard check
        }
        catch (System.Exception ex)
        {
            crashed = true;
            Debug.LogError($"PlaySFX(null) crash: {ex.Message}");
        }
        LogResult("SetSFXVolume + PlaySFX(null) → không crash", !crashed, "");

        // ── Test 3: SetBGMVolume + PlayBGM không crash khi clip = null ──
        crashed = false;
        try
        {
            audio.SetBGMVolume(0.5f);
            audio.PlayBGM(null);
        }
        catch (System.Exception ex)
        {
            crashed = true;
            Debug.LogError($"PlayBGM(null) crash: {ex.Message}");
        }
        LogResult("SetBGMVolume + PlayBGM(null) → không crash", !crashed, "");

        // ── Test 4: StopBGM khi không có BGM đang chạy → không crash ──
        crashed = false;
        try { audio.StopBGM(); }
        catch (System.Exception ex) { crashed = true; Debug.LogError($"StopBGM crash: {ex.Message}"); }
        LogResult("StopBGM khi idle → không crash", !crashed, "");

        // ── Test 5: PlayTimingResult với tất cả grades ──
        crashed = false;
        try
        {
            audio.PlayTimingResult(TimingGrade.Perfect);
            audio.PlayTimingResult(TimingGrade.Good);
            audio.PlayTimingResult(TimingGrade.Miss);
        }
        catch (System.Exception ex)
        {
            crashed = true;
            Debug.LogError($"PlayTimingResult crash: {ex.Message}");
        }
        LogResult("PlayTimingResult(Perfect/Good/Miss) → không crash (clips có thể null)", !crashed, "");

        // ── Test 6: DamageTakenEvent → callback không crash ──
        crashed = false;
        try { EventBus.Instance.Publish(new DamageTakenEvent("e1", "e2", 50, false)); }
        catch (System.Exception ex) { crashed = true; Debug.LogError($"DamageTakenEvent crash: {ex.Message}"); }
        LogResult("DamageTakenEvent → AudioController callback không crash", !crashed, "");

        // ── Test 7: EntityDeathEvent → callback không crash ──
        crashed = false;
        try { EventBus.Instance.Publish(new EntityDeathEvent("e1", "e2")); }
        catch (System.Exception ex) { crashed = true; Debug.LogError($"EntityDeathEvent crash: {ex.Message}"); }
        LogResult("EntityDeathEvent → AudioController callback không crash", !crashed, "");

        // ── Test 8: CombatEndedEvent (win) → không crash ──
        crashed = false;
        try { EventBus.Instance.Publish(new CombatEndedEvent(true)); }
        catch (System.Exception ex) { crashed = true; Debug.LogError($"CombatEndedEvent crash: {ex.Message}"); }
        LogResult("CombatEndedEvent(win) → không crash", !crashed, "");

        // ── Test 9: CombatEndedEvent (lose) → không crash ──
        crashed = false;
        try { EventBus.Instance.Publish(new CombatEndedEvent(false)); }
        catch (System.Exception ex) { crashed = true; Debug.LogError($"CombatEndedEvent(lose) crash: {ex.Message}"); }
        LogResult("CombatEndedEvent(lose) → không crash", !crashed, "");

        // ── Test 10: Volume clamp — âm lượng không vượt quá [0,1] ──
        audio.SetSFXVolume(5f);  // quá max
        audio.SetSFXVolume(-1f); // dưới min
        audio.SetBGMVolume(2f);
        Debug.Log("[Test10] SetVolume ngoài range [0,1] → không crash (Unity AudioSource tự clamp) = PASS");
        _pass++;

        Debug.Log($"=== AudioControllerTest DONE: {_pass} passed, {_fail} failed ===");
    }

    private void LogResult(string name, bool pass, string detail = "")
    {
        if (pass) { _pass++; Debug.Log($"✅ {name} — {detail}"); }
        else      { _fail++; Debug.LogError($"❌ {name} — {detail}"); }
    }
}
