using System;
using System.Collections;
using UnityEngine;
using TTCS.Debugging;
using TTCS.UI.Combat;
using static TTCS.Debugging.DebugLogger;

namespace TTCS.Combat.Timing
{
    /// <summary>
    /// 🔵 Dev A - Timing System
    /// MonoBehaviour Singleton quản lý vòng đời của một timing window.
    ///
    /// Flow:
    ///   CombatFlowController gọi OpenWindow(window) trước khi enemy attack resolve.
    ///   TimingInputHandler gọi RegisterInput(Time.time) khi player nhấn.
    ///   Window tự đóng sau Duration → trả về TimingGrade qua OnTimingResult event.
    ///
    /// Setup trong Unity:
    ///   Đặt TimingSystem trên cùng GameObject với CombatFlowController (hoặc riêng).
    ///   Đảm bảo TimingInputHandler cũng tồn tại trong scene.
    /// </summary>
    public class TimingSystem : MonoBehaviour
    {
        // ─── Singleton ────────────────────────────────────────────────────
        private static TimingSystem _instance;
        public  static TimingSystem Instance => _instance;

        private void Awake()
        {
            if (_instance != null && _instance != this) { Destroy(gameObject); return; }
            _instance = this;
        }

        private void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }

        // ─── Events ───────────────────────────────────────────────────────
        /// <summary>Fired khi window kết thúc — trả về grade của lần input (hoặc Miss nếu không input).</summary>
        public event Action<TimingGrade> OnTimingResult;
        /// <summary>Fired ngay khi timing window mở, truyền vào duration (giây).</summary>
        public event Action<float> OnWindowOpened;
        /// <summary>Fired khi timing window đóng (tự đóng hoặc force close).</summary>
        public event Action OnWindowClosed;

        // ─── State ────────────────────────────────────────────────────────
        private TimingWindow _activeWindow;
        private float?       _inputTime;         // Time.time khi input đến (null nếu chưa input)
        private bool         _windowActive;
        private Coroutine    _windowCoroutine;

        // ─── Input Buffer ─────────────────────────────────────────────────
        /// <summary>Lưu input đến 60ms trước khi window mở.</summary>
        private float? _bufferedInputTime;
        private const float InputBufferSeconds = 0.06f;

        // ──────────────────────────────────────────────────────────────────
        #region Public API

        /// <summary>
        /// Mở timing window. Input hợp lệ trong suốt Duration.
        /// Nếu đã có buffered input trong vòng 60ms, áp dụng ngay.
        /// </summary>
        public void OpenWindow(TimingWindow window)
        {
            if (_windowActive)
            {
                LogWarning("TimingSystem: Window đang mở — ForceClose trước.", LogCategory.Combat);
                ForceClose();
            }

            _activeWindow = window;
            _inputTime    = null;

            // Áp dụng buffered input nếu còn trong thời hạn buffer
            if (_bufferedInputTime.HasValue)
            {
                float bufferAge = Time.time - _bufferedInputTime.Value;
                if (bufferAge <= InputBufferSeconds)
                {
                    Log($"TimingSystem: Applying buffered input (age={bufferAge*1000:F0}ms).", LogCategory.Combat);
                    _inputTime = _bufferedInputTime.Value;
                }
                _bufferedInputTime = null;
            }

            _windowActive    = true;

            // Nếu đã có input buffer hợp lệ thì trả kết quả ngay, không chạy countdown UI.
            if (_inputTime.HasValue)
            {
                TimingGrade bufferedGrade = _activeWindow != null
                    ? _activeWindow.EvaluateInput(_inputTime.Value)
                    : TimingGrade.Miss;
                FinishWindow(bufferedGrade);
                return;
            }

            _windowCoroutine = StartCoroutine(WindowLifecycle(window));
            OnWindowOpened?.Invoke(window.Duration);
        }

        /// <summary>
        /// Đăng ký input từ player. Gọi từ TimingInputHandler.
        /// Input buffer được giữ 60ms để dùng khi window mở sau đó.
        /// </summary>
        public void RegisterInput(float inputGameTime)
        {
            if (_windowActive && !_inputTime.HasValue)
            {
                _inputTime = inputGameTime;
                Log($"TimingSystem: Input registered at t={inputGameTime:F3}s.", LogCategory.Combat);

                // Yêu cầu mới: nhận input xong thì đóng window và trả kết quả ngay.
                if (_windowCoroutine != null)
                {
                    StopCoroutine(_windowCoroutine);
                    _windowCoroutine = null;
                }

                TimingGrade immediateGrade = _activeWindow != null
                    ? _activeWindow.EvaluateInput(_inputTime.Value)
                    : TimingGrade.Miss;

                FinishWindow(immediateGrade);
            }
            else if (!_windowActive)
            {
                // Không nhận input khi chưa mở window để tránh dính phím từ UI khác.
                Log($"TimingSystem: Ignored input (no active window).", LogCategory.Combat);
            }
        }

        /// <summary>Đóng window ngay và trả Miss.</summary>
        public void ForceClose()
        {
            if (_windowCoroutine != null)
            {
                StopCoroutine(_windowCoroutine);
                _windowCoroutine = null;
            }
            FinishWindow(TimingGrade.Miss);
        }

        /// <summary>True khi có window đang mở.</summary>
        public bool IsWindowActive => _windowActive;

        #endregion

        // ──────────────────────────────────────────────────────────────────
        #region Window Lifecycle

        private IEnumerator WindowLifecycle(TimingWindow window)
        {
            float elapsed = 0f;
            

            while (elapsed < window.Duration)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            // Window kết thúc — evaluate
            TimingGrade grade;
           
            
            if (_inputTime.HasValue)
            {
                grade = window.EvaluateInput(_inputTime.Value);
            }
            else
            {
                grade = TimingGrade.Miss;   // Không input → Miss
            }
             

            FinishWindow(grade);
        }

        private void FinishWindow(TimingGrade grade)
        {
            _windowCoroutine = null;
            _windowActive    = false;
            _activeWindow    = null;
            _inputTime       = null;

            Log($"TimingSystem: Window closed → {grade}.", LogCategory.Combat);

            // Hiển thị feedback UI
            CombatUIController.Instance?.ShowTimingResult(grade);

            OnTimingResult?.Invoke(grade);
            OnWindowClosed?.Invoke();
        }

        #endregion
    }
}
