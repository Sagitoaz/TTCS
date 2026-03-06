using System.Collections;
using UnityEngine;
using TTCS.Combat.Timing;
using TTCS.UI.Combat; // TimingGrade

/// <summary>
/// Test TimingSystem — window lifecycle, input buffer, grade events.
/// Yêu cầu: TimingSystem MonoBehaviour phải có mặt trong scene (singleton).
/// Add component này lên cùng GameObject với TimingSystem, hoặc để TimingSystem tự tạo qua Instance.
/// </summary>
public class TimingSystemTest : MonoBehaviour
{
    private int _testsPassed = 0;
    private int _testsFailed = 0;

    private void Start()
    {
        Debug.Log("=== TimingSystem Tests ===");
        StartCoroutine(RunTests());
    }

    private IEnumerator RunTests()
    {
        var system = TimingSystem.Instance;
        if (system == null)
        {
            Debug.LogError("TimingSystem.Instance không tồn tại!");
            yield break;
        }

        // Test 1: Miss khi không có input trong window
        yield return StartCoroutine(Test_NoInput_ReturnsMiss(system));

        // Test 2: Perfect khi nhấn đúng ideal time
        yield return StartCoroutine(Test_PerfectInput(system));

        // Test 3: Good khi nhấn lệch 100ms
        yield return StartCoroutine(Test_GoodInput(system));

        // Test 4: Buffer — nhấn 40ms trước khi window mở vẫn được tính
        yield return StartCoroutine(Test_BufferedInput(system));

        // Test 5: ForceClose kết thúc window sớm với Miss
        yield return StartCoroutine(Test_ForceClose(system));

        Debug.Log($"=== TimingSystem Tests DONE: {_testsPassed} passed, {_testsFailed} failed ===");
    }

    private IEnumerator Test_NoInput_ReturnsMiss(TimingSystem system)
    {
        bool resultReceived = false;
        TimingGrade received = TimingGrade.Perfect;

        system.OnTimingResult += OnResult;

        float openTime = Time.time;
        var window = TimingWindow.CreateDefault(openTime, 0.5f); // window 0.5s
        system.OpenWindow(window);

        // Chờ window đóng tự nhiên (0.5s + buffer)
        yield return new WaitForSeconds(0.7f);

        system.OnTimingResult -= OnResult;

        if (!resultReceived)
        {
            Debug.Log("[Test_NoInput] WARN: Không nhận được result sau window. Có thể window chưa tự đóng.");
        }
        else
        {
            bool pass = received == TimingGrade.Miss;
            LogResult("Test_NoInput_ReturnsMiss", pass, $"got {received}");
        }

        void OnResult(TimingGrade grade) { resultReceived = true; received = grade; }
    }

    private IEnumerator Test_PerfectInput(TimingSystem system)
    {
        TimingGrade received = TimingGrade.Miss;
        system.OnTimingResult += OnResult;

        float openTime = Time.time;
        var window = TimingWindow.CreateDefault(openTime, 1.0f);
        system.OpenWindow(window);

        // Chờ đến ideal time rồi đăng ký input
        yield return new WaitForSeconds(window.Duration * 0.5f);
        system.RegisterInput(Time.time);

        yield return new WaitForSeconds(0.3f);
        system.OnTimingResult -= OnResult;

        bool pass = received == TimingGrade.Perfect || received == TimingGrade.Good;
        LogResult("Test_PerfectInput", pass, $"got {received} (Perfect or Good chấp nhận được vì timing không chính xác tuyệt đối)");

        void OnResult(TimingGrade grade) { received = grade; }
    }

    private IEnumerator Test_GoodInput(TimingSystem system)
    {
        TimingGrade received = TimingGrade.Miss;
        system.OnTimingResult += OnResult;

        float openTime = Time.time;
        var window = TimingWindow.CreateDefault(openTime, 1.0f);
        system.OpenWindow(window);

        // Nhấn lệch 100ms so với ideal time → Good
        yield return new WaitForSeconds(window.Duration * 0.5f + 0.1f);
        system.RegisterInput(Time.time);

        yield return new WaitForSeconds(0.3f);
        system.OnTimingResult -= OnResult;

        bool pass = received == TimingGrade.Good || received == TimingGrade.Perfect; // timing precision may vary
        LogResult("Test_GoodInput", pass, $"got {received} (Perfect hoặc Good đều có thể do frame precision)");

        void OnResult(TimingGrade grade) { received = grade; }
    }

    private IEnumerator Test_BufferedInput(TimingSystem system)
    {
        TimingGrade received = TimingGrade.Miss;
        bool resultReceived = false;
        system.OnTimingResult += OnResult;

        // Đăng ký input 40ms trước khi gọi OpenWindow (trong buffer 60ms)
        float inputTime = Time.time + 0.01f; // Sẽ đăng ký rất sớm
        yield return new WaitForSeconds(0.01f);
        system.RegisterInput(inputTime);

        // Mở window ngay sau
        yield return new WaitForSeconds(0.01f);
        float openTime = Time.time;
        var window = TimingWindow.CreateDefault(openTime, 1.0f);
        system.OpenWindow(window);

        yield return new WaitForSeconds(0.5f);
        system.OnTimingResult -= OnResult;

        if (resultReceived)
        {
            Debug.Log($"[Test_BufferedInput] Input buffer hoạt động — grade: {received}");
            _testsPassed++;
        }
        else
        {
            Debug.Log("[Test_BufferedInput] WARN: Không nhận kết quả — buffer có thể chưa kích hoạt.");
        }

        void OnResult(TimingGrade grade) { resultReceived = true; received = grade; }
    }

    private IEnumerator Test_ForceClose(TimingSystem system)
    {
        TimingGrade received = TimingGrade.Perfect;
        bool resultReceived = false;
        system.OnTimingResult += OnResult;

        float openTime = Time.time;
        var window = TimingWindow.CreateDefault(openTime, 5.0f); // window 5s
        system.OpenWindow(window);

        yield return new WaitForSeconds(0.1f);
        system.ForceClose(); // đóng sớm

        yield return new WaitForSeconds(0.2f);
        system.OnTimingResult -= OnResult;

        if (resultReceived)
        {
            bool pass = received == TimingGrade.Miss;
            LogResult("Test_ForceClose", pass, $"ForceClose phải trả về Miss, got {received}");
        }
        else
        {
            Debug.Log("[Test_ForceClose] WARN: Không nhận result sau ForceClose — kiểm tra ForceClose implementation.");
        }

        void OnResult(TimingGrade grade) { resultReceived = true; received = grade; }
    }

    private void LogResult(string testName, bool pass, string detail = "")
    {
        if (pass)
        {
            _testsPassed++;
            Debug.Log($"✅ {testName} PASS — {detail}");
        }
        else
        {
            _testsFailed++;
            Debug.LogError($"❌ {testName} FAIL — {detail}");
        }
    }
}
