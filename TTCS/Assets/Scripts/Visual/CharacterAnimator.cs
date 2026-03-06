using System;
using System.Collections;
using UnityEngine;
using DG.Tweening;

namespace TTCS.Visual
{
    /// <summary>
    /// 🟢 Dev B Sprint 2 - Character Animation Controller (FEH Style)
    ///
    /// CÁCH HOẠT ĐỘNG:
    ///   - Unity Animator: chứa TẤT CẢ clips (Idle, Attack, Hurt, Death, Victory, SkillCast)
    ///     tạo bằng Record Mode trong Editor — xem guide unit-devB-5
    ///   - DOTween: CHỈ dùng cho combat feel (lunge root transform, hurt shake, death fade)
    ///     KHÔNG dùng cho Idle — Idle hoàn toàn do Animator clip xử lý
    ///   - Animation Events: clip "Attack" cần có Animation Event gọi "NotifyAttackHitFrame"
    ///     tại frame weapon chạm mục tiêu. Có fallback timer nếu event chưa setup.
    ///
    /// ANIMATOR PARAMETERS cần tạo:
    ///   Attack   (Trigger) — play attack clip
    ///   Hurt     (Trigger) — play hurt clip
    ///   Death    (Trigger) — play death clip
    ///   Victory  (Trigger) — play victory clip
    ///   SkillCast (Trigger)— play skill cast clip
    ///
    /// ANIMATION EVENT (thêm vào clip Attack):
    ///   Function: NotifyAttackHitFrame  (không có parameter)
    ///   Đặt tại frame khi weapon tiếp xúc mục tiêu (thường frame 8-12 của attack clip)
    /// </summary>
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(CharacterView))]
    public class CharacterAnimator : MonoBehaviour
    {
        // ─── Events ───────────────────────────────────────────────────────

        /// <summary>
        /// Invoke khi weapon frame hit — ActionAnimationController dùng để
        /// trigger hurt trên targets và spawn VFX đúng thời điểm.
        /// Kích hoạt từ Animation Event trong clip "Attack" hoặc fallback timer.
        /// </summary>
        public event Action OnAttackHitFrame;

        /// <summary>Invoke khi action animation hoàn thành (return to idle)</summary>
        public event Action OnAnimationComplete;

        // ─── Inspector ────────────────────────────────────────────────────
        [Header("FEH Combat Feel — Lunge")]
        [Tooltip("Khoảng lunge về phía trước khi tấn công (units)")]
        [SerializeField] private float _lungeDistance = 0.5f;

        [Tooltip("Thời gian lunge đến vị trí attack (giây)")]
        [SerializeField] private float _lungeSpeed    = 0.12f;

        [Tooltip("Thời gian quay lại vị trí ban đầu (giây)")]
        [SerializeField] private float _returnSpeed   = 0.22f;

        [Header("Hurt Flash")]
        [SerializeField] private float _whiteFlashDuration = 0.07f;
        [SerializeField] private float _redTintDuration    = 0.10f;

        // ─── Components ───────────────────────────────────────────────────
        private Animator      _animator;
        private CharacterView _view;

        // ─── State ────────────────────────────────────────────────────────
        private Vector3  _originalLocalPos;
        private Sequence _actionSequence;
        private bool     _hitFrameNotified;

        // ─── Animator Parameter Hashes (nhanh hơn string lookup) ─────────
        private static readonly int HashAttack   = Animator.StringToHash("Attack");
        private static readonly int HashHurt     = Animator.StringToHash("Hurt");
        private static readonly int HashDeath    = Animator.StringToHash("Death");
        private static readonly int HashVictory  = Animator.StringToHash("Victory");
        private static readonly int HashSkill    = Animator.StringToHash("SkillCast");

        // ─── Lifecycle ────────────────────────────────────────────────────
        private void Awake()
        {
            _animator         = GetComponent<Animator>();
            _view             = GetComponent<CharacterView>();
            _originalLocalPos = transform.localPosition;
        }

        private void OnDestroy()
        {
            _actionSequence?.Kill();
            DOTween.Kill(transform);
        }

        // ─── ATTACK ───────────────────────────────────────────────────────

        /// <summary>
        /// Attack animation (FEH style):
        ///   1. Lunge về phía kẻ địch (DOTween)
        ///   2. Hit frame → OnAttackHitFrame event (từ Animation Event hoặc fallback)
        ///   3. Quay trở lại vị trí gốc (DOTween)
        ///   4. Resume idle bob
        ///
        /// Animator clip "Attack" chạy song song để tay/vũ khí có animation.
        /// </summary>
        public void PlayAttack()
        {
            _hitFrameNotified = false;
            _actionSequence?.Kill();

            // Lunge direction dựa trên facing (localScale.x âm = nhìn trái)
            float dir       = transform.localScale.x >= 0 ? 1f : -1f;
            Vector3 lungePos = _originalLocalPos + new Vector3(dir * _lungeDistance, 0f, 0f);

            // DOTween: xử lý movement
            _actionSequence = DOTween.Sequence()
                .Append(transform.DOLocalMove(lungePos, _lungeSpeed).SetEase(Ease.OutQuint))
                .AppendInterval(0.05f)   // linger tại điểm attack
                .Append(transform.DOLocalMove(_originalLocalPos, _returnSpeed).SetEase(Ease.InOutQuad))
                .OnComplete(() =>
                {
                    // Fallback: fire hit frame nếu Animation Event chưa kích hoạt
                    NotifyAttackHitFrame();
                    transform.localPosition = _originalLocalPos;
                    OnAnimationComplete?.Invoke();
                });

            // Animator: play weapon animation clip
            _animator.SetTrigger(HashAttack);

            // Fallback timer: fire hit frame khi lunge xong (nếu chưa có Animation Event)
            StartCoroutine(HitFrameFallback(_lungeSpeed + 0.03f));
        }

        /// <summary>
        /// Gọi từ Animation Event trong clip "Attack" TẠI FRAME HIT.
        /// Cũng được gọi từ fallback timer nếu Animation Event chưa setup.
        /// Guard: chỉ fire một lần duy nhất mỗi attack.
        /// </summary>
        public void NotifyAttackHitFrame()
        {
            if (_hitFrameNotified) return;
            _hitFrameNotified = true;
            OnAttackHitFrame?.Invoke();
        }

        private IEnumerator HitFrameFallback(float delay)
        {
            yield return new WaitForSeconds(delay);
            NotifyAttackHitFrame();
        }

        // ─── HURT ─────────────────────────────────────────────────────────

        /// <summary>
        /// Hurt animation (FEH signature):
        ///   - White flash → red tint → white (3 giai đoạn)
        ///   - DOShakePosition cho cảm giác impact
        ///   - Chạy song song với idle, không block turn flow
        /// </summary>
        public void PlayHurt()
        {
            _animator.SetTrigger(HashHurt);
            StartCoroutine(HurtFlashCoroutine());
            transform.DOShakePosition(0.28f, 0.07f, 18, 90f, false, true);
        }

        private IEnumerator HurtFlashCoroutine()
        {
            // Giai đoạn 1: trắng flash (FEH hit signature)
            _view.SetAllPartsColor(Color.white);
            yield return new WaitForSeconds(_whiteFlashDuration);

            // Giai đoạn 2: đỏ nhạt (damage indicator)
            _view.SetAllPartsColor(new Color(1f, 0.45f, 0.45f));
            yield return new WaitForSeconds(_redTintDuration);

            // Giai đoạn 3: trở lại normal
            _view.ResetPartsColor();
        }

        // ─── DEATH ────────────────────────────────────────────────────────

        /// <summary>
        /// Death animation (FEH style):
        ///   - Animator clip "Death" (ngã ra sau)
        ///   - DOTween: rơi xuống nhẹ + fade alpha về 0
        ///   - Sau khi xong: tắt GameObject
        /// </summary>
        public void PlayDeath()
        {
            _actionSequence?.Kill();

            _animator.SetTrigger(HashDeath);

            _actionSequence = DOTween.Sequence()
                .AppendInterval(0.12f)   // chờ animator bắt đầu
                .Append(transform.DOLocalMoveY(_originalLocalPos.y - 0.18f, 0.35f)
                    .SetEase(Ease.InQuad))
                .Join(DOTween.To(
                    () => 1f,
                    v  => _view.SetAlpha(v),
                    0f,
                    0.4f).SetEase(Ease.InQuad))
                .OnComplete(() =>
                {
                    gameObject.SetActive(false);
                    OnAnimationComplete?.Invoke();
                });
        }

        // ─── VICTORY ──────────────────────────────────────────────────────

        /// <summary>Victory pose — nhảy nhẹ lên sau khi thắng trận (FEH style)</summary>
        public void PlayVictory()
        {
            _animator.SetTrigger(HashVictory);

            _actionSequence?.Kill();
            _actionSequence = DOTween.Sequence()
                .Append(transform.DOLocalMoveY(_originalLocalPos.y + 0.22f, 0.16f)
                    .SetEase(Ease.OutQuad))
                .Append(transform.DOLocalMoveY(_originalLocalPos.y, 0.24f)
                    .SetEase(Ease.InBounce));
        }

        // ─── SKILL CAST ───────────────────────────────────────────────────

        /// <summary>
        /// Skill cast animation. Hiện tại play generic "SkillCast" trigger.
        /// skillId reserved để mở rộng per-skill animation sau.
        /// </summary>
        public void PlaySkillCast(string skillId = "")
        {
            _animator.SetTrigger(HashSkill);
        }

        // ─── STOP ─────────────────────────────────────────────────────────

        /// <summary>
        /// Dừng tất cả animations, reset về trạng thái idle.
        /// Gọi khi reset combat hoặc xử lý lỗi.
        /// </summary>
        public void StopAll()
        {
            _actionSequence?.Kill();
            StopAllCoroutines();
            DOTween.Kill(transform);
            transform.localPosition = _originalLocalPos;
            _view.ResetPartsColor();
            _view.SetAlpha(1f);
            // Animator tự quay về Idle state qua transitions
        }
    }
}
