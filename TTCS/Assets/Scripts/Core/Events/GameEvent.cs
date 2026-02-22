namespace TTCS.Core.Events
{
    /// <summary>
    /// Base class cho tất cả game events
    /// Sử dụng pattern này để type-safe event handling
    /// </summary>
    public abstract class GameEvent
    {
        /// <summary>Timestamp khi event được tạo</summary>
        public float Timestamp { get; private set; }

        protected GameEvent()
        {
            Timestamp = UnityEngine.Time.time;
        }
    }
}
