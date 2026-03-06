namespace TTCS.Combat.Timing
{
    /// <summary>
    /// 🔵 Dev A - Timing Window
    /// Data class mô tả một cửa sổ timing input.
    /// Tạo qua factory methods để đảm bảo threshold hợp lệ.
    /// </summary>
    public class TimingWindow
    {
        // ─── Properties ───────────────────────────────────────────────────
        /// <summary>Thời điểm window mở (game time, giây)</summary>
        public float OpenTime { get; private set; }

        /// <summary>Thời lượng window (giây)</summary>
        public float Duration { get; private set; }

        /// <summary>Thời điểm window đóng (game time, giây)</summary>
        public float CloseTime => OpenTime + Duration;

        /// <summary>Ngưỡng Perfect (giây từ điểm lý tưởng, mặc định 0.05s = 50ms)</summary>
        public float PerfectThreshold { get; private set; }

        /// <summary>Ngưỡng Good (giây từ điểm lý tưởng, mặc định 0.15s = 150ms)</summary>
        public float GoodThreshold { get; private set; }

        /// <summary>Điểm lý tưởng trong window (mặc định = trung tâm)</summary>
        public float IdealTime => OpenTime + Duration * 0.5f;

        // ─── Constructor ──────────────────────────────────────────────────
        /// <summary>
        /// Tạo timing window.
        /// </summary>
        /// <param name="openTime">Game time khi window mở (Time.time)</param>
        /// <param name="duration">Thời lượng window (giây)</param>
        /// <param name="perfectThreshold">Offset từ điểm lý tưởng để đạt Perfect (giây)</param>
        /// <param name="goodThreshold">Offset từ điểm lý tưởng để đạt Good (giây)</param>
        public TimingWindow(float openTime, float duration,
                            float perfectThreshold = 0.05f,
                            float goodThreshold    = 0.15f)
        {
            OpenTime         = openTime;
            Duration         = duration;
            PerfectThreshold = perfectThreshold;
            GoodThreshold    = goodThreshold;
        }

        // ─── Factory ──────────────────────────────────────────────────────
        /// <summary>Tạo window với giá trị từ Constants.</summary>
        public static TimingWindow CreateDefault(float openTime, float duration)
        {
            return new TimingWindow(
                openTime,
                duration,
                TTCS.Core.Constants.PERFECT_TIMING_WINDOW * 0.001f,   // ms → s
                TTCS.Core.Constants.GOOD_TIMING_WINDOW    * 0.001f
            );
        }

        // ─── Grade Calculation ────────────────────────────────────────────
        /// <summary>
        /// Tính TimingGrade dựa trên thời điểm input so với điểm lý tưởng.
        /// </summary>
        /// <param name="inputTime">Game time khi input xảy ra (Time.time)</param>
        /// <returns>Perfect / Good / Miss</returns>
        public TTCS.UI.Combat.TimingGrade EvaluateInput(float inputTime)
        {
            float offset = UnityEngine.Mathf.Abs(inputTime - IdealTime);

            if (offset <= PerfectThreshold)
                return TTCS.UI.Combat.TimingGrade.Perfect;
            if (offset <= GoodThreshold)
                return TTCS.UI.Combat.TimingGrade.Good;
            return TTCS.UI.Combat.TimingGrade.Miss;
        }
    }
}
