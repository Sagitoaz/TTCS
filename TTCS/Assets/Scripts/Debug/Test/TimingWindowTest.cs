using UnityEngine;
using TTCS.Combat.Timing;
using TTCS.UI.Combat; // TimingGrade

/// <summary>
/// Test TimingWindow data class — pure logic, không cần Unity UI.
/// Add component này lên một GameObject bất kỳ rồi Play là chạy.
/// </summary>
public class TimingWindowTest : MonoBehaviour
{
    private void Start()
    {
        Debug.Log("=== TimingWindow Tests ===");

        // 1. Tạo window tại t=0, duration=2s
        float openTime = 0f;
        float duration = 2f;
        TimingWindow window = TimingWindow.CreateDefault(openTime, duration);

        Debug.Log($"IdealTime = {window.IdealTime} (expected {openTime + duration * 0.5f})");
        Debug.Assert(Mathf.Approximately(window.IdealTime, openTime + duration * 0.5f),
            "IdealTime phải là giữa window");

        // 2. Perfect — nhấn đúng ideal time
        TimingGrade perfect = window.EvaluateInput(window.IdealTime);
        Debug.Log($"Input tại IdealTime => {perfect} (expected Perfect)");
        Debug.Assert(perfect == TimingGrade.Perfect, "Nhấn đúng IdealTime phải là Perfect");

        // 3. Perfect — nhấn cách 30ms (trong ngưỡng 50ms)
        TimingGrade perfectNear = window.EvaluateInput(window.IdealTime + 0.030f);
        Debug.Log($"Input tại IdealTime+30ms => {perfectNear} (expected Perfect)");
        Debug.Assert(perfectNear == TimingGrade.Perfect, "30ms offset phải là Perfect");

        // 4. Good — nhấn cách 80ms (50ms < 80ms < 150ms)
        TimingGrade good = window.EvaluateInput(window.IdealTime + 0.080f);
        Debug.Log($"Input tại IdealTime+80ms => {good} (expected Good)");
        Debug.Assert(good == TimingGrade.Good, "80ms offset phải là Good");

        // 5. Miss — nhấn cách 200ms (vượt ngưỡng 150ms)
        TimingGrade miss = window.EvaluateInput(window.IdealTime + 0.200f);
        Debug.Log($"Input tại IdealTime+200ms => {miss} (expected Miss)");
        Debug.Assert(miss == TimingGrade.Miss, "200ms offset phải là Miss");

        // 6. Miss — nhấn trước khi window mở (t = -0.5)
        TimingGrade early = window.EvaluateInput(openTime - 0.5f);
        Debug.Log($"Input trước window (t=-0.5) => {early} (expected Miss)");
        Debug.Assert(early == TimingGrade.Miss, "Input trước window phải là Miss");

        // 7. Miss — nhấn sau khi window đóng
        TimingGrade late = window.EvaluateInput(openTime + duration + 0.1f);
        Debug.Log($"Input sau window đóng => {late} (expected Miss)");
        Debug.Assert(late == TimingGrade.Miss, "Input sau window đóng phải là Miss");

        // 8. Kiểm tra PerfectThreshold và GoodThreshold
        Debug.Log($"PerfectThreshold = {window.PerfectThreshold}s (expected 0.05)");
        Debug.Log($"GoodThreshold    = {window.GoodThreshold}s (expected 0.15)");
        Debug.Assert(Mathf.Approximately(window.PerfectThreshold, 0.05f), "PerfectThreshold phải là 50ms");
        Debug.Assert(Mathf.Approximately(window.GoodThreshold, 0.15f), "GoodThreshold phải là 150ms");

        // 9. Edge case — nhấn đúng ranh giới Perfect/Good (exacty 50ms)
        TimingGrade boundary = window.EvaluateInput(window.IdealTime + 0.05f);
        Debug.Log($"Input tại đúng ranh giới Perfect (50ms) => {boundary}");
        // Có thể là Perfect hoặc Good tuỳ implementation (<= hay <), chỉ log, không assert cứng

        Debug.Log("✅ TimingWindow tests PASSED");
    }
}
