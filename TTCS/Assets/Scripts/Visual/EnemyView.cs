using UnityEngine;

namespace TTCS.Visual
{
    /// <summary>
    /// 🟢 Dev B Sprint 2 - Enemy View
    ///
    /// Kế thừa CharacterView, thêm TelegraphVisual.
    /// Enemy mặc định nhìn sang trái (hướng về phía player bên phải).
    ///
    /// UNITY SETUP:
    ///   - Cùng part hierarchy với CharacterView
    ///   - Thêm child GameObject "Telegraph" có TelegraphVisual component
    ///   - Xem: aidlc-docs/construction/unity-setup/unit-devB-5-character-visual-setup.md
    /// </summary>
    public class EnemyView : CharacterView
    {
        [Header("Enemy Specific")]
        [Tooltip("TelegraphVisual component trên child GameObject 'Telegraph'")]
        [SerializeField] private TelegraphVisual _telegraph;

        protected override void Awake()
        {
            base.Awake();
            // Enemy luôn nhìn trái theo mặc định (hướng về player ở bên phải)
            SetFacing(false);
        }

        // ─── Properties ───────────────────────────────────────────────────

        /// <summary>TelegraphVisual — Dev A TimingSystem lắng nghe OnTelegraphComplete</summary>
        public TelegraphVisual Telegraph => _telegraph;

        // ─── Telegraph API ────────────────────────────────────────────────

        /// <summary>
        /// Hiển thị cảnh báo tấn công với duration chỉ định.
        /// ActionAnimationController gọi method này trước khi enemy thực hiện action.
        /// </summary>
        public void ShowTelegraph(float duration)
        {
            if (_telegraph != null)
                _telegraph.Play(duration);
        }

        /// <summary>Ẩn cảnh báo ngay lập tức</summary>
        public void HideTelegraph()
        {
            if (_telegraph != null)
                _telegraph.Stop();
        }

        /// <summary>True khi telegraph đang play</summary>
        public bool IsTelegraphActive => _telegraph != null && _telegraph.IsPlaying;
    }
}
