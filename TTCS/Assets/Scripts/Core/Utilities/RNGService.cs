using System;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace TTCS.Core.Utilities
{
    /// <summary>
    /// 🔵 Dev A - RNG Service
    /// Service quản lý random number generation với seed
    /// Dùng để đảm bảo reproducible combat và testing
    /// </summary>
    public class RNGService
    {
        private static RNGService _instance;
        public static RNGService Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new RNGService();
                }
                return _instance;
            }
        }

        private System.Random _random;
        private int _currentSeed;

        /// <summary>Seed hiện tại đang sử dụng</summary>
        public int CurrentSeed => _currentSeed;

        /// <summary>
        /// Constructor mặc định - sử dụng timestamp làm seed
        /// </summary>
        public RNGService()
        {
            InitializeWithTimestamp();
        }

        /// <summary>
        /// Constructor với seed cụ thể
        /// </summary>
        public RNGService(int seed)
        {
            Initialize(seed);
        }

        /// <summary>
        /// Khởi tạo với timestamp (random thật sự)
        /// </summary>
        public void InitializeWithTimestamp()
        {
            _currentSeed = Environment.TickCount;
            _random = new System.Random(_currentSeed);
            
            if (Constants.DEBUG_LOGS_ENABLED)
            {
                Debug.Log($"[RNGService] Initialized with timestamp seed: {_currentSeed}");
            }
        }

        /// <summary>
        /// Khởi tạo với seed cố định (dùng cho testing hoặc replay)
        /// </summary>
        public void Initialize(int seed)
        {
            _currentSeed = seed;
            _random = new System.Random(seed);
            
            if (Constants.DEBUG_LOGS_ENABLED)
            {
                Debug.Log($"[RNGService] Initialized with fixed seed: {seed}");
            }
        }

        /// <summary>
        /// Trả về số nguyên random trong khoảng [min, max)
        /// </summary>
        public int Range(int min, int max)
        {
            return _random.Next(min, max);
        }

        /// <summary>
        /// Trả về số nguyên random trong khoảng [0, max)
        /// </summary>
        public int Range(int max)
        {
            return _random.Next(max);
        }

        /// <summary>
        /// Trả về float random trong khoảng [0.0, 1.0)
        /// </summary>
        public float Value()
        {
            return (float)_random.NextDouble();
        }

        /// <summary>
        /// Trả về float random trong khoảng [min, max)
        /// </summary>
        public float Range(float min, float max)
        {
            return min + (float)_random.NextDouble() * (max - min);
        }

        /// <summary>
        /// Roll xem có trigger được chance không (0-100%)
        /// </summary>
        /// <param name="chance">Xác suất từ 0.0 đến 1.0</param>
        /// <returns>True nếu roll thành công</returns>
        public bool RollChance(float chance)
        {
            return Value() < chance;
        }

        /// <summary>
        /// Roll xem có trigger được chance không (0-100)
        /// </summary>
        /// <param name="chancePercent">Xác suất từ 0 đến 100</param>
        /// <returns>True nếu roll thành công</returns>
        public bool RollChancePercent(int chancePercent)
        {
            return Range(0, 100) < chancePercent;
        }

        /// <summary>
        /// Roll crit với tỷ lệ cho trước
        /// </summary>
        /// <param name="critRate">Crit rate từ 0.0 đến 1.0</param>
        /// <returns>True nếu crit</returns>
        public bool RollCrit(float critRate)
        {
            return RollChance(critRate);
        }

        /// <summary>
        /// Shuffle một array (Fisher-Yates algorithm)
        /// </summary>
        public void Shuffle<T>(T[] array)
        {
            int n = array.Length;
            for (int i = n - 1; i > 0; i--)
            {
                int j = Range(i + 1);
                T temp = array[i];
                array[i] = array[j];
                array[j] = temp;
            }
        }

        /// <summary>
        /// Random một phần tử từ array
        /// </summary>
        public T RandomElement<T>(T[] array)
        {
            if (array == null || array.Length == 0)
            {
                throw new ArgumentException("Array is null or empty");
            }
            return array[Range(array.Length)];
        }

        /// <summary>
        /// Random một phần tử từ list
        /// </summary>
        public T RandomElement<T>(System.Collections.Generic.List<T> list)
        {
            if (list == null || list.Count == 0)
            {
                throw new ArgumentException("List is null or empty");
            }
            return list[Range(list.Count)];
        }

        /// <summary>
        /// Save seed để có thể replay
        /// </summary>
        public int SaveState()
        {
            return _currentSeed;
        }

        /// <summary>
        /// Restore seed từ state đã save
        /// </summary>
        public void RestoreState(int seed)
        {
            Initialize(seed);
        }
    }
}
