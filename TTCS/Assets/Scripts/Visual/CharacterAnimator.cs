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

        [Tooltip("Khoảng cách dừng trước mục tiêu cho đòn cận chiến")]
        [SerializeField] private float _meleeStopDistance = 0.9f;

        [Tooltip("Timeout chờ state Attack kết thúc trước khi quay về vị trí ban đầu")]
        [SerializeField] private float _attackEndWaitTimeout = 2.5f;

        [Tooltip("Khoảng chờ ngắn sau khi attack kết thúc trước khi quay về (giây)")]
        [SerializeField] private float _postAttackReturnDelay = 0.1f;

        [Header("Movement Root")]
        [Tooltip("Nếu bật, tween di chuyển sẽ chạy trên parent transform để tránh Animator ghi đè vị trí root.")]
        [SerializeField] private bool _useParentAsMotionRoot = true;

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
        private Coroutine _freeFrameCoroutine;

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

            // Ensure DOTween-driven movement is not overridden by Animator root motion.
            if (_animator != null)
                _animator.applyRootMotion = false;
        }

        private void OnDestroy()
        {
            _actionSequence?.Kill();
            DOTween.Kill(transform);
            if (_freeFrameCoroutine != null)
                StopCoroutine(_freeFrameCoroutine);
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
            var motionRoot = GetMotionRoot();
            Vector3 motionRootStartWorld = motionRoot.position;

            _originalLocalPos = transform.localPosition;
            _hitFrameNotified = false;
            _actionSequence?.Kill();

            // Lunge direction dựa trên facing (localScale.x âm = nhìn trái)
            float dir       = transform.localScale.x >= 0 ? 1f : -1f;
            Vector3 lungeWorldPos = motionRootStartWorld + new Vector3(dir * _lungeDistance, 0f, 0f);

            // DOTween: xử lý movement
            _actionSequence = DOTween.Sequence()
                .Append(motionRoot.DOMove(lungeWorldPos, _lungeSpeed).SetEase(Ease.OutQuint))
                .AppendInterval(0.05f)   // linger tại điểm attack
                .Append(motionRoot.DOMove(motionRootStartWorld, _returnSpeed).SetEase(Ease.InOutQuad))
                .OnComplete(() =>
                {
                    // Fallback: fire hit frame nếu Animation Event chưa kích hoạt
                    NotifyAttackHitFrame();
                    motionRoot.position = motionRootStartWorld;
                    OnAnimationComplete?.Invoke();
                });

            // Animator: play weapon animation clip
            _animator.SetTrigger(HashAttack);

            // Fallback timer: fire hit frame khi lunge xong (nếu chưa có Animation Event)
            
        }

        /// <summary>
        /// Đòn cận chiến: lao vào gần vị trí target rồi quay về vị trí ban đầu.
        /// </summary>
        public void PlayAttackMelee(Vector3 targetWorldPosition)
        {
            var motionRoot = GetMotionRoot();

            Debug.Log("LAO TỚI TẤN CÔNG " + targetWorldPosition + " | mover=" + motionRoot.name);
            _originalLocalPos = transform.localPosition;
            _hitFrameNotified = false;
            _actionSequence?.Kill();

            Vector3 startWorldPos = motionRoot.position;
            Vector3 towardTarget = targetWorldPosition - startWorldPos;

            if (towardTarget.sqrMagnitude < 0.0001f)
            {
                float fallbackDir = transform.localScale.x >= 0 ? 1f : -1f;
                towardTarget = new Vector3(fallbackDir, 0f, 0f);
            }

            Vector3 moveDir = towardTarget.normalized;
            Vector3 lungeWorldPos = targetWorldPosition - moveDir * _meleeStopDistance;
            lungeWorldPos.z = startWorldPos.z;

            float distance = Vector3.Distance(startWorldPos, lungeWorldPos);
            float lungeDuration = Mathf.Clamp(distance * 0.08f, _lungeSpeed, 0.3f);
            Debug.Log("Tấn công: " + distance + " " + lungeWorldPos + " " + lungeDuration);
            _actionSequence = DOTween.Sequence()
                .Append(motionRoot.DOMove(lungeWorldPos, lungeDuration).SetEase(Ease.OutQuint))
                .OnComplete(() =>
                {
                    _animator.SetTrigger(HashAttack);
                    StartCoroutine(ReturnAfterAttackFinished(motionRoot, startWorldPos));
                });

            
            
        }

        private IEnumerator ReturnAfterAttackFinished(Transform motionRoot, Vector3 startWorldPos)
        {
            float waitTimeout = Mathf.Min(_attackEndWaitTimeout, 1.2f);
            yield return WaitUntilAttackStateEnds(waitTimeout);
            if (_postAttackReturnDelay > 0f)
                yield return new WaitForSeconds(_postAttackReturnDelay);

            _actionSequence = DOTween.Sequence()
                .Append(motionRoot.DOMove(startWorldPos, _returnSpeed).SetEase(Ease.InOutQuad))
                .OnComplete(() =>
                {
                    NotifyAttackHitFrame();
                    motionRoot.position = startWorldPos;
                    OnAnimationComplete?.Invoke();
                });
        }

        private IEnumerator WaitUntilAttackStateEnds(float timeout)
        {
            if (_animator == null)
                yield break;

            float elapsed = 0f;
            bool enteredAttack = false;

            while (elapsed < timeout)
            {
                var state = _animator.GetCurrentAnimatorStateInfo(0);
                if (state.shortNameHash == HashAttack || state.IsName("Attack"))
                {
                    enteredAttack = true;
                    break;
                }

                elapsed += Time.deltaTime;
                yield return null;
            }

            if (!enteredAttack)
                yield break;

            while (elapsed < timeout)
            {
                var state = _animator.GetCurrentAnimatorStateInfo(0);
                bool inAttackState = state.shortNameHash == HashAttack || state.IsName("Attack");

                if (!inAttackState)
                    yield break;

                if (state.normalizedTime >= 1f && !_animator.IsInTransition(0))
                    yield break;

                elapsed += Time.deltaTime;
                yield return null;
            }
        }

        private Transform GetMotionRoot()
        {
            if (_useParentAsMotionRoot && transform.parent != null)
                return transform.parent;

            return transform;
        }

        /// <summary>
        /// Đòn tầm xa: đứng yên cast/attack, không di chuyển vị trí.
        /// </summary>
        public void PlayAttackInPlace()
        {
            _originalLocalPos = transform.localPosition;
            _hitFrameNotified = false;
            _actionSequence?.Kill();

            _actionSequence = DOTween.Sequence()
                .AppendInterval(_lungeSpeed + 0.12f)
                .OnComplete(() =>
                {
                    NotifyAttackHitFrame();
                    transform.localPosition = _originalLocalPos;
                    OnAnimationComplete?.Invoke();
                });

            _animator.SetTrigger(HashAttack);
            
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

        /// <summary>
        /// Free frame/hit-stop ngắn để tăng cảm giác impact theo từng đòn.
        /// </summary>
        public void ApplyFreeFrame(float duration)
        {
            if (duration <= 0f || _animator == null)
                return;

            if (_freeFrameCoroutine != null)
                StopCoroutine(_freeFrameCoroutine);

            _freeFrameCoroutine = StartCoroutine(FreeFrameCoroutine(duration));
        }

        private IEnumerator FreeFrameCoroutine(float duration)
        {
            float cachedAnimatorSpeed = _animator.speed;
            bool pausedSequence = _actionSequence != null && _actionSequence.IsActive() && _actionSequence.IsPlaying();

            _animator.speed = 0f;
            if (pausedSequence)
                _actionSequence.Pause();

            yield return new WaitForSecondsRealtime(duration);

            if (_animator != null)
                _animator.speed = cachedAnimatorSpeed <= 0f ? 1f : cachedAnimatorSpeed;

            if (pausedSequence && _actionSequence != null && _actionSequence.IsActive())
                _actionSequence.Play();

            _freeFrameCoroutine = null;
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
            _freeFrameCoroutine = null;
            transform.localPosition = _originalLocalPos;
            _view.ResetPartsColor();
            _view.SetAlpha(1f);
            // Animator tự quay về Idle state qua transitions
        }
    }
}
