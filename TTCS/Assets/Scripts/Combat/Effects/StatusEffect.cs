using TTCS.Combat.Entities;

namespace TTCS.Combat.Effects
{
    /// <summary>Thời điểm tick của status effect</summary>
    public enum TickTiming
    {
        StartTurn,   // Tick vào đầu lượt của entity bị ảnh hưởng
        EndTurn,     // Tick vào cuối lượt của entity bị ảnh hưởng
        OnHit,       // Tick khi đánh trúng (trigger từ ngoài)
        OnBeingHit,  // Tick khi bị đánh (trigger từ ngoài)
        Immediate    // Đã xử lý ngay lúc Apply, không cần tick
    }

    /// <summary>
    /// 🟢 Dev B - Status Effect Base Class
    /// Abstract base cho tất cả các status effect (buff / debuff / DoT / control).
    ///
    /// Stack rules:
    ///   - Mỗi con class override OnStack() để xử lý stacking phù hợp.
    ///   - Mặc định (base): refresh duration về giá trị của effect mới đến.
    ///
    /// Vòng đời:
    ///   OnApply → (OnTick mỗi trigger) → OnRemove
    ///
    /// Tạo effect mới:
    ///   1. Kế thừa StatusEffect
    ///   2. Set EffectId, TickTiming trong constructor
    ///   3. Implement OnApply / OnTick / OnRemove
    /// </summary>
    public abstract class StatusEffect
    {
        // ─── Core Properties ────────────────────────────────────────────

        /// <summary>ID duy nhất: bleed, burn, heal_regen, shield, stun...</summary>
        public string EffectId { get; protected set; }

        /// <summary>Số lượt còn lại. Khi về 0 → IsExpired = true → EffectComponent sẽ remove.</summary>
        public int Duration { get; set; }

        /// <summary>Cường độ effect: tuỳ context — damage per tick, heal amount, shield amount...</summary>
        public float Intensity { get; protected set; }

        /// <summary>Số stack hiện tại (dùng tuỳ loại effect)</summary>
        public int StackCount { get; protected set; } = 1;

        /// <summary>Thời điểm tick</summary>
        public TickTiming TickTiming { get; protected set; }

        /// <summary>Effect đã hết hạn chưa</summary>
        public bool IsExpired => Duration <= 0;

        // ─── Lifecycle Hooks ─────────────────────────────────────────────

        /// <summary>Được gọi khi effect lần đầu apply lên target</summary>
        public abstract void OnApply(CombatEntity target);

        /// <summary>Được gọi khi effect tick (theo TickTiming)</summary>
        public abstract void OnTick(CombatEntity target);

        /// <summary>Được gọi khi effect bị remove (hết hạn hoặc bị dispel)</summary>
        public abstract void OnRemove(CombatEntity target);

        /// <summary>
        /// Được gọi khi cùng loại effect được apply lại lên target đang có sẵn.
        /// Mặc định: refresh duration về duration của effect mới.
        /// Override để thay đổi stack logic (vd: stack count, stack value).
        /// </summary>
        public virtual void OnStack(StatusEffect incoming)
        {
            // Default: refresh duration
            Duration = incoming.Duration;
        }

        public override string ToString() =>
            $"[Effect:{EffectId}] dur={Duration} stacks={StackCount} intensity={Intensity}";
    }
}
