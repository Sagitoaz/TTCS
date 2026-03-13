using UnityEngine;
using TTCS.Combat.Managers;

namespace TTCS.Visual
{
    /// <summary>
    /// 🟢 Dev B Sprint 2 - Character Visual Container
    ///
    /// Giữ references đến các SpriteRenderer parts theo style FEH (Fire Emblem Heroes).
    /// Không chứa logic animation — chỉ là data container + helper methods.
    ///
    /// SETUP ĐƠN GIẢN:
    ///   Tất cả SpriteRenderer con cháu được tự động collect qua GetComponentsInChildren.
    ///   Bạn không cần gán từng part trong Inspector — chỉ cần gán 2 anchor transforms.
    ///
    /// Part hierarchy (tạo trong Unity Editor — tên tự do):
    ///   CharacterRoot  (CharacterView + CharacterAnimator + Animator)
    ///   ├── [bất kỳ child nào có SpriteRenderer] — tự động nhận màu/alpha
    ///   ├── HeadAnchor  (Transform empty — PHẢI GÁN trong Inspector)
    ///   └── HitAnchor   (Transform empty — PHẢI GÁN trong Inspector)
    ///
    /// Xem hướng dẫn setup chi tiết tại:
    ///   aidlc-docs/construction/unity-setup/unit-devB-5-character-visual-setup.md
    /// </summary>
    public class CharacterView : MonoBehaviour, CombatBridge.ICharacterAnimatorBridge
    {
        // ─── Entity Binding ───────────────────────────────────────────────
        [Header("Entity Binding")]
        [Tooltip("Phải khớp với CombatEntity.ID — ActionAnimationController dùng để map")]
        public string EntityId;

        // ─── Anchor Points ────────────────────────────────────────────────
        [Header("Anchor Points")]
        [Tooltip("Điểm trên đầu nhân vật — vị trí spawn floating damage/heal text")]
        [SerializeField] private Transform _headAnchor;

        [Tooltip("Điểm giữa body — vị trí spawn VFX khi bị hit")]
        [SerializeField] private Transform _hitAnchor;

        // ─── Cached ───────────────────────────────────────────────────────
        private SpriteRenderer[]  _allRenderers;
        private CharacterAnimator _animator;

        /// <summary>CharacterAnimator cùng GameObject</summary>
        public CharacterAnimator Animator
        {
            get
            {
                if (_animator == null) _animator = GetComponent<CharacterAnimator>();
                return _animator;
            }
        }

        // ─── Lifecycle ────────────────────────────────────────────────────
        protected virtual void Awake()
        {
            // Tự động collect toàn bộ SpriteRenderer trong hierarchy — gọi 1 lần, cache lại
            _allRenderers = GetComponentsInChildren<SpriteRenderer>(includeInactive: true);
        }

        // ─── Public API ───────────────────────────────────────────────────

        /// <summary>
        /// Lật hướng nhìn của nhân vật.
        /// faceRight = true  → nhìn sang phải (player default)
        /// faceRight = false → nhìn sang trái  (enemy default)
        /// </summary>
        public void SetFacing(bool faceRight)
        {
            Vector3 s = transform.localScale;
            s.x = faceRight ? Mathf.Abs(s.x) : -Mathf.Abs(s.x);
            transform.localScale = s;
        }

        /// <summary>
        /// Bật/tắt highlight khi là current actor.
        /// FEH style: tint vàng nhạt khi active, trắng khi không.
        /// </summary>
        public void SetHighlight(bool active)
        {
            Color tint = active ? new Color(1f, 1f, 0.55f) : Color.white;
            SetAllPartsColor(tint);
        }

        /// <summary>Set alpha toàn bộ parts — dùng khi fade out chết</summary>
        public void SetAlpha(float alpha)
        {
            foreach (var sr in GetAllRenderers())
            {
                Color c = sr.color;
                c.a = alpha;
                sr.color = c;
            }
        }

        /// <summary>Set màu toàn bộ parts — dùng cho flash effects</summary>
        public void SetAllPartsColor(Color color)
        {
            foreach (var sr in GetAllRenderers())
                sr.color = color;
        }

        /// <summary>Reset màu toàn bộ parts về trắng (Color.white)</summary>
        public void ResetPartsColor()
        {
            SetAllPartsColor(Color.white);
        }

        // ─── Positions ────────────────────────────────────────────────────

        /// <summary>World position gốc của view root</summary>
        public Vector3 WorldPosition => transform.position;

        /// <summary>
        /// Vị trí spawn floating text (trên đầu nhân vật).
        /// Fallback về transform.position + 1.2 units nếu không có HeadAnchor.
        /// </summary>
        public Vector3 HeadAnchorPosition =>
            _headAnchor != null ? _headAnchor.position : transform.position + Vector3.up * 1.2f;

        /// <summary>
        /// Vị trí spawn VFX khi bị hit (giữa body).
        /// Fallback về transform.position + 0.5 units.
        /// </summary>
        public Vector3 HitAnchorPosition =>
            _hitAnchor != null ? _hitAnchor.position : transform.position + Vector3.up * 0.5f;

        // ─── Renderers ────────────────────────────────────────────────────

        /// <summary>Tất cả SpriteRenderer trong hierarchy (đã cache khi Awake)</summary>
        public SpriteRenderer[] GetAllRenderers() => _allRenderers;

        /// <summary>
        /// Buộc rebuild renderer cache — dùng khi thêm/bỏ parts ở runtime
        /// (thường không cần thiết)
        /// </summary>
        public void RebuildRendererCache()
        {
            _allRenderers = GetComponentsInChildren<SpriteRenderer>(includeInactive: true);
        }

        public void PlayAttack() => Animator?.PlayAttack();
        public void PlayHurt() => Animator?.PlayHurt();
        public void PlayDeath() => Animator?.PlayDeath();
        public void PlayVictory() => Animator?.PlayVictory();
        public Transform GetWorldTransform() => transform;
    }
}
