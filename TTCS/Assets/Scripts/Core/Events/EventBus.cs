using System;
using System.Collections.Generic;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace TTCS.Core.Events
{
    /// <summary>
    /// 🔵 Dev A - Event system
    /// Central event bus cho toàn bộ game
    /// Sử dụng để decouple các systems với nhau
    /// </summary>
    public class EventBus : MonoBehaviour
    {
        private static EventBus _instance;
        public static EventBus Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("[EventBus]");
                    _instance = go.AddComponent<EventBus>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        // Dictionary lưu các subscriber cho từng loại event
        private readonly Dictionary<Type, List<Delegate>> _subscribers = new Dictionary<Type, List<Delegate>>();

        // Lock để thread-safe (nếu cần)
        private readonly object _lock = new object();

        /// <summary>
        /// Subscribe vào một loại event
        /// </summary>
        /// <typeparam name="T">Loại event cần subscribe</typeparam>
        /// <param name="handler">Callback khi event được raise</param>
        public void Subscribe<T>(Action<T> handler) where T : GameEvent
        {
            lock (_lock)
            {
                Type eventType = typeof(T);

                if (!_subscribers.ContainsKey(eventType))
                {
                    _subscribers[eventType] = new List<Delegate>();
                }

                _subscribers[eventType].Add(handler);

                if (Constants.DEBUG_LOGS_ENABLED)
                {
                    Debug.Log($"[EventBus] Subscribed to {eventType.Name}. Total subscribers: {_subscribers[eventType].Count}");
                }
            }
        }

        /// <summary>
        /// Unsubscribe khỏi một loại event
        /// </summary>
        /// <typeparam name="T">Loại event</typeparam>
        /// <param name="handler">Handler đã subscribe trước đó</param>
        public void Unsubscribe<T>(Action<T> handler) where T : GameEvent
        {
            lock (_lock)
            {
                Type eventType = typeof(T);

                if (_subscribers.ContainsKey(eventType))
                {
                    _subscribers[eventType].Remove(handler);

                    if (_subscribers[eventType].Count == 0)
                    {
                        _subscribers.Remove(eventType);
                    }

                    if (Constants.DEBUG_LOGS_ENABLED)
                    {
                        Debug.Log($"[EventBus] Unsubscribed from {eventType.Name}");
                    }
                }
            }
        }

        /// <summary>
        /// Publish một event đến tất cả subscribers
        /// </summary>
        /// <typeparam name="T">Loại event</typeparam>
        /// <param name="eventData">Dữ liệu event</param>
        public void Publish<T>(T eventData) where T : GameEvent
        {
            lock (_lock)
            {
                Type eventType = typeof(T);

                if (_subscribers.ContainsKey(eventType))
                {
                    var handlers = _subscribers[eventType];

                    if (Constants.DEBUG_LOGS_ENABLED)
                    {
                        Debug.Log($"[EventBus] Publishing {eventType.Name} to {handlers.Count} subscribers");
                    }

                    // Copy list để tránh modify during iteration
                    var handlersCopy = new List<Delegate>(handlers);

                    foreach (var handler in handlersCopy)
                    {
                        try
                        {
                            (handler as Action<T>)?.Invoke(eventData);
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"[EventBus] Error invoking handler for {eventType.Name}: {ex.Message}\n{ex.StackTrace}");
                        }
                    }
                }
                else
                {
                    if (Constants.DEBUG_LOGS_ENABLED)
                    {
                        Debug.LogWarning($"[EventBus] No subscribers for {eventType.Name}");
                    }
                }
            }
        }

        /// <summary>
        /// Clear tất cả subscribers (dùng khi cleanup hoặc scene transition)
        /// </summary>
        public void ClearAll()
        {
            lock (_lock)
            {
                _subscribers.Clear();
                Debug.Log("[EventBus] Cleared all subscribers");
            }
        }

        /// <summary>
        /// Clear subscribers của một loại event cụ thể
        /// </summary>
        public void Clear<T>() where T : GameEvent
        {
            lock (_lock)
            {
                Type eventType = typeof(T);
                if (_subscribers.ContainsKey(eventType))
                {
                    _subscribers.Remove(eventType);
                    Debug.Log($"[EventBus] Cleared subscribers for {eventType.Name}");
                }
            }
        }

        /// <summary>
        /// Đếm số subscribers cho một loại event
        /// </summary>
        public int GetSubscriberCount<T>() where T : GameEvent
        {
            lock (_lock)
            {
                Type eventType = typeof(T);
                return _subscribers.ContainsKey(eventType) ? _subscribers[eventType].Count : 0;
            }
        }
    }
}
