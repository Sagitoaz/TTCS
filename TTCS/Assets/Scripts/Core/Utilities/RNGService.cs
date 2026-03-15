using System;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace TTCS.Core.Utilities
{
    /// <summary>
    /// RNG service for deterministic combat and testing.
    /// Unity singleton MonoBehaviour (no direct new).
    /// </summary>
    public class RNGService : MonoBehaviour
    {
        private static RNGService _instance;

        public static RNGService Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<RNGService>();
                    if (_instance == null)
                    {
                        var go = new GameObject("[RNGService]");
                        _instance = go.AddComponent<RNGService>();
                        DontDestroyOnLoad(go);
                    }
                }

                return _instance;
            }
        }

        private System.Random _random;
        private int _currentSeed;

        public int CurrentSeed => _currentSeed;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            if (_random == null)
                InitializeWithTimestamp();
        }

        /// <summary>
        /// Initialize with timestamp seed (non-deterministic).
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
        /// Initialize with fixed seed (deterministic).
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

        public int Range(int min, int max)
        {
            EnsureInitialized();
            return _random.Next(min, max);
        }

        public int Range(int max)
        {
            EnsureInitialized();
            return _random.Next(max);
        }

        public float Value()
        {
            EnsureInitialized();
            return (float)_random.NextDouble();
        }

        public float Range(float min, float max)
        {
            EnsureInitialized();
            return min + (float)_random.NextDouble() * (max - min);
        }

        public bool RollChance(float chance)
        {
            return Value() < chance;
        }

        public bool RollChancePercent(int chancePercent)
        {
            return Range(0, 100) < chancePercent;
        }

        public bool RollCrit(float critRate)
        {
            return RollChance(critRate);
        }

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

        public T RandomElement<T>(T[] array)
        {
            if (array == null || array.Length == 0)
                throw new ArgumentException("Array is null or empty");

            return array[Range(array.Length)];
        }

        public T RandomElement<T>(System.Collections.Generic.List<T> list)
        {
            if (list == null || list.Count == 0)
                throw new ArgumentException("List is null or empty");

            return list[Range(list.Count)];
        }

        public int SaveState()
        {
            return _currentSeed;
        }

        public void RestoreState(int seed)
        {
            Initialize(seed);
        }

        private void EnsureInitialized()
        {
            if (_random == null)
                InitializeWithTimestamp();
        }
    }
}
