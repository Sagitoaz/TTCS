using UnityEngine;
using UnityEngine.InputSystem;
using TTCS.Debugging;
using static TTCS.Debugging.DebugLogger;

namespace TTCS.Combat.Timing
{
    /// <summary>
    /// 🔵 Dev A - Timing Input Handler
    /// Lắng nghe player input (Space / Guard button) và forward đến TimingSystem.
    /// Hỗ trợ cả Input System mới và Input cũ (fallback).
    ///
    /// Setup trong Unity:
    ///   Đặt component này trên cùng GameObject với TimingSystem.
    ///   Nếu dùng New Input System, đảm bảo InputSystem_Actions asset có action "Guard".
    /// </summary>
    public class TimingInputHandler : MonoBehaviour
    {
        // ─── Inspector ────────────────────────────────────────────────────
        [Header("Input (New Input System)")]
        [Tooltip("Action name để tìm trong InputSystem_Actions. Mặc định: 'Guard'")]
        [SerializeField] private string _actionName = "Guard";

        [Header("Fallback (New Input System Keyboard)")]
        [Tooltip("Key fallback nếu New Input System không tìm thấy action")]
        [SerializeField] private Key _fallbackKey = Key.Space;

        // ─── Runtime ──────────────────────────────────────────────────────
        private InputAction _guardAction;
        private bool        _useNewInputSystem;

        // ──────────────────────────────────────────────────────────────────
        #region Unity Lifecycle

        private void Awake()
        {
            // Thử tìm action "Guard" trong InputSystem
            var inputActions = FindFirstObjectByType<PlayerInput>();
            if (inputActions != null)
            {
                _guardAction = inputActions.actions.FindAction(_actionName, throwIfNotFound: false);
                if (_guardAction != null)
                {
                    _useNewInputSystem = true;
                    Log($"TimingInputHandler: Using New Input System action '{_actionName}'.", LogCategory.UI);
                }
                else
                {
                    LogWarning($"TimingInputHandler: Action '{_actionName}' không tìm thấy — dùng fallback key {_fallbackKey}.", LogCategory.UI);
                }
            }
            else
            {
                Log($"TimingInputHandler: Không có PlayerInput — dùng fallback key {_fallbackKey}.", LogCategory.UI);
            }
        }

        private void OnEnable()
        {
            if (_useNewInputSystem && _guardAction != null)
                _guardAction.performed += OnGuardPerformed;
        }

        private void OnDisable()
        {
            if (_useNewInputSystem && _guardAction != null)
                _guardAction.performed -= OnGuardPerformed;
        }

        private void Update()
        {
            // Fallback: dùng New Input System Keyboard khi không có PlayerInput
            if (!_useNewInputSystem)
            {
                var keyboard = Keyboard.current;
                if (keyboard != null && _fallbackKey != Key.None && keyboard[_fallbackKey].wasPressedThisFrame)
                    RegisterInput();
            }
        }

        #endregion

        // ──────────────────────────────────────────────────────────────────
        #region Input Handling

        private void OnGuardPerformed(InputAction.CallbackContext ctx)
        {
            RegisterInput();
        }

        private void RegisterInput()
        {
            if (TimingSystem.Instance == null) return;

            // Tránh dính input từ UI khác (vd: confirm target bằng Space)
            // chỉ forward khi timing window đang mở.
            if (!TimingSystem.Instance.IsWindowActive) return;

            TimingSystem.Instance.RegisterInput(Time.time);
        }

        #endregion
    }
}
