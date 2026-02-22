using UnityEngine;

namespace TTCS.Debugging
{
    /// <summary>
    /// 🔵 Dev A - Debug Logger
    /// Centralized logging system với categories và colors
    /// </summary>
    public static class DebugLogger
    {
        public enum LogCategory
        {
            General,
            Combat,
            Events,
            Data,
            Save,
            UI,
            AI
        }

        // Colors cho từng category (Unity rich text)
        private static readonly string[] CategoryColors = new string[]
        {
            "#FFFFFF", // General - White
            "#FF6B6B", // Combat - Red
            "#4ECDC4", // Events - Cyan
            "#FFE66D", // Data - Yellow
            "#95E1D3", // Save - Green
            "#C7CEEA", // UI - Purple
            "#FFA07A"  // AI - Orange
        };

        /// <summary>
        /// Log message với category
        /// </summary>
        public static void Log(string message, LogCategory category = LogCategory.General)
        {
#pragma warning disable CS0162 // Unreachable code detected
            if (!TTCS.Core.Constants.DEBUG_LOGS_ENABLED)
            {
                return;
            }
#pragma warning restore CS0162

            string color = CategoryColors[(int)category];
            string formattedMessage = $"<color={color}>[{category}]</color> {message}";
            UnityEngine.Debug.Log(formattedMessage);
        }

        /// <summary>
        /// Log warning với category
        /// </summary>
        public static void LogWarning(string message, LogCategory category = LogCategory.General)
        {
#pragma warning disable CS0162 // Unreachable code detected
            if (!TTCS.Core.Constants.DEBUG_LOGS_ENABLED)
            {
                return;
            }
#pragma warning restore CS0162

            string color = CategoryColors[(int)category];
            string formattedMessage = $"<color={color}>[{category}]</color> {message}";
            UnityEngine.Debug.LogWarning(formattedMessage);
        }

        /// <summary>
        /// Log error với category
        /// </summary>
        public static void LogError(string message, LogCategory category = LogCategory.General)
        {
            string color = CategoryColors[(int)category];
            string formattedMessage = $"<color={color}>[{category}]</color> {message}";
            UnityEngine.Debug.LogError(formattedMessage);
        }

        /// <summary>
        /// Log combat event
        /// </summary>
        public static void LogCombat(string message)
        {
            Log(message, LogCategory.Combat);
        }

        /// <summary>
        /// Log event bus
        /// </summary>
        public static void LogEvent(string message)
        {
            Log(message, LogCategory.Events);
        }

        /// <summary>
        /// Log data loading/parsing
        /// </summary>
        public static void LogData(string message)
        {
            Log(message, LogCategory.Data);
        }

        /// <summary>
        /// Log save/load operations
        /// </summary>
        public static void LogSave(string message)
        {
            Log(message, LogCategory.Save);
        }

        /// <summary>
        /// Log UI events
        /// </summary>
        public static void LogUI(string message)
        {
            Log(message, LogCategory.UI);
        }

        /// <summary>
        /// Log AI decisions
        /// </summary>
        public static void LogAI(string message)
        {
            Log(message, LogCategory.AI);
        }
    }
}
