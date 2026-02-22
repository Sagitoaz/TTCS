namespace TTCS.Combat.Components
{
    /// <summary>
    /// 🟢 Dev B - Component Interface
    /// Interface cơ bản cho tất cả các component của combat entity
    /// </summary>
    public interface IEntityComponent
    {
        /// <summary>Entity ID mà component này thuộc về</summary>
        string EntityId { get; }

        /// <summary>Initialize component với entity ID</summary>
        void Initialize(string entityId);

        /// <summary>Reset component về trạng thái ban đầu</summary>
        void Reset();
    }
}
