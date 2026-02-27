using System.Collections.Generic;
using TTCS.Core;
using TTCS.Debugging;

namespace TTCS.Core.Data
{
    /// <summary>
    /// 🔵 Dev A - Generic in-memory data cache.
    /// Tránh re-load cùng một file nhiều lần trong session.
    /// </summary>
    public class DataCache<T> where T : class
    {
        private readonly Dictionary<string, T> _cache = new Dictionary<string, T>();

        /// <summary>Số lượng entries trong cache</summary>
        public int Count => _cache.Count;

        /// <summary>Lưu data vào cache theo id</summary>
        public void Set(string id, T data)
        {
            if (string.IsNullOrEmpty(id) || data == null) return;

            _cache[id] = data;

            if (Constants.DEBUG_LOGS_ENABLED)
                DebugLogger.Log($"[DataCache] Cached: {typeof(T).Name} id='{id}'", DebugLogger.LogCategory.Data);
        }

        /// <summary>Lấy data từ cache. Trả về null nếu chưa có.</summary>
        public T Get(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            _cache.TryGetValue(id, out T result);
            return result;
        }

        /// <summary>Kiểm tra id đã có trong cache chưa</summary>
        public bool Has(string id)
        {
            if (string.IsNullOrEmpty(id)) return false;
            return _cache.ContainsKey(id);
        }

        /// <summary>Xóa toàn bộ cache</summary>
        public void Clear()
        {
            _cache.Clear();
            DebugLogger.Log($"[DataCache] Cleared cache for {typeof(T).Name}", DebugLogger.LogCategory.Data);
        }

        /// <summary>Trả về tất cả values trong cache</summary>
        public IReadOnlyCollection<T> GetAll()
        {
            return _cache.Values;
        }
    }
}
