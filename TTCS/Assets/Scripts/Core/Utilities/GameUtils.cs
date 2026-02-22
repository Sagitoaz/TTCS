using UnityEngine;

namespace TTCS.Core.Utilities
{
    /// <summary>
    /// Utility class cho các hàm helper chung
    /// </summary>
    public static class GameUtils
    {
        /// <summary>
        /// Clamp value trong khoảng min-max
        /// </summary>
        public static int Clamp(int value, int min, int max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        /// <summary>
        /// Clamp value trong khoảng min-max
        /// </summary>
        public static float Clamp(float value, float min, float max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        /// <summary>
        /// Clamp value trong khoảng 0-1
        /// </summary>
        public static float Clamp01(float value)
        {
            return Clamp(value, 0f, 1f);
        }

        /// <summary>
        /// Linear interpolation
        /// </summary>
        public static float Lerp(float a, float b, float t)
        {
            return a + (b - a) * Clamp01(t);
        }

        /// <summary>
        /// Tính phần trăm (a/b * 100)
        /// </summary>
        public static float Percentage(float value, float total)
        {
            if (total == 0) return 0;
            return (value / total) * 100f;
        }

        /// <summary>
        /// Format số thành string với separators (1000 -> "1,000")
        /// </summary>
        public static string FormatNumber(int number)
        {
            return number.ToString("N0");
        }

        /// <summary>
        /// Format thời gian (seconds) thành MM:SS
        /// </summary>
        public static string FormatTime(float seconds)
        {
            int minutes = Mathf.FloorToInt(seconds / 60f);
            int secs = Mathf.FloorToInt(seconds % 60f);
            return $"{minutes:00}:{secs:00}";
        }

        /// <summary>
        /// Parse string thành int, trả về defaultValue nếu fail
        /// </summary>
        public static int ParseInt(string value, int defaultValue = 0)
        {
            if (int.TryParse(value, out int result))
            {
                return result;
            }
            return defaultValue;
        }

        /// <summary>
        /// Parse string thành float, trả về defaultValue nếu fail
        /// </summary>
        public static float ParseFloat(string value, float defaultValue = 0f)
        {
            if (float.TryParse(value, out float result))
            {
                return result;
            }
            return defaultValue;
        }

        /// <summary>
        /// Generate random ID (dùng GUID)
        /// </summary>
        public static string GenerateId()
        {
            return System.Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Generate random ID ngắn (8 ký tự)
        /// </summary>
        public static string GenerateShortId()
        {
            return System.Guid.NewGuid().ToString().Substring(0, 8);
        }

        /// <summary>
        /// Safe null check cho UnityEngine.Object
        /// </summary>
        public static bool IsNull(UnityEngine.Object obj)
        {
            return obj == null || obj.Equals(null);
        }

        /// <summary>
        /// Deep copy một object bằng JSON serialization
        /// </summary>
        public static T DeepCopy<T>(T source)
        {
            string json = JsonUtility.ToJson(source);
            return JsonUtility.FromJson<T>(json);
        }
    }
}
