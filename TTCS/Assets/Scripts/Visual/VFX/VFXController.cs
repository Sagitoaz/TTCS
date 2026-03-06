using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TTCS.Core.Events;
using TTCS.Combat.Managers;

namespace TTCS.Visual.VFX
{
    /// <summary>
    /// 🟢 Dev B Sprint 2 - VFX Controller (Object Pool)
    ///
    /// Quản lý ParticleSystem effects trong combat với object pooling.
    /// FEH-style VFX: colorful particle bursts theo element/damage type.
    ///
    /// DAMAGE TYPE → VFX MAPPING:
    ///   "Physical"  → trắng/xám (sword slash)
    ///   "Fire"      → cam/đỏ (flame burst)
    ///   "Ice"       → xanh lam (ice shards)
    ///   "Lightning" → vàng/trắng (spark)
    ///   "Dark"      → tím (shadow)
    ///   "Light"     → vàng/trắng sáng
    ///   "Heal"      → xanh lá (restore orbs)
    ///   "Death"     → tím/trắng (dissolve)
    ///
    /// SETUP TRONG UNITY:
    ///   1. Đặt VFXController trên một GameObject trong CombatScene
    ///   2. Tạo ParticleSystem prefabs — đặt trong Assets/Prefabs/VFX/Combat/
    ///   3. Gán prefabs vào các serialized fields trong Inspector
    ///   Chi tiết: aidlc-docs/construction/unity-setup/unit-devB-5-character-visual-setup.md
    /// </summary>
    public class VFXController : MonoBehaviour
    {
        // ─── Singleton ────────────────────────────────────────────────────
        private static VFXController _instance;
        public  static VFXController Instance => _instance;

        private void Awake()
        {
            if (_instance != null && _instance != this) { Destroy(gameObject); return; }
            _instance = this;
        }

        private void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }

        // ─── VFX Prefabs ──────────────────────────────────────────────────
        [Header("Hit VFX Prefabs — gán trong Inspector (Assets/Prefabs/VFX/Combat/)")]
        [SerializeField] private ParticleSystem _hitPhysicalPrefab;
        [SerializeField] private ParticleSystem _hitFirePrefab;
        [SerializeField] private ParticleSystem _hitIcePrefab;
        [SerializeField] private ParticleSystem _hitLightningPrefab;
        [SerializeField] private ParticleSystem _hitDarkPrefab;
        [SerializeField] private ParticleSystem _hitLightPrefab;
        [SerializeField] private ParticleSystem _hitHealPrefab;
        [SerializeField] private ParticleSystem _deathPrefab;

        [Header("Pool Settings")]
        [Tooltip("Số lượng instance pre-warm cho mỗi loại VFX")]
        [SerializeField] private int _poolSizePerType = 4;

        // ─── Pool ─────────────────────────────────────────────────────────
        private Dictionary<string, Queue<ParticleSystem>> _pools;
        private Dictionary<string, ParticleSystem>        _prefabMap;

        // ─── Lifecycle ────────────────────────────────────────────────────
        private void Start()
        {
            BuildPrefabMap();
            InitPools();
            SubscribeEvents();
        }

        private void OnDisable()
        {
            UnsubscribeEvents();
        }

        // ─── Init ─────────────────────────────────────────────────────────
        private void BuildPrefabMap()
        {
            _prefabMap = new Dictionary<string, ParticleSystem>
            {
                { "Physical",  _hitPhysicalPrefab  },
                { "Fire",      _hitFirePrefab       },
                { "Ice",       _hitIcePrefab        },
                { "Lightning", _hitLightningPrefab  },
                { "Dark",      _hitDarkPrefab       },
                { "Light",     _hitLightPrefab      },
                { "Heal",      _hitHealPrefab       },
                { "Death",     _deathPrefab         },
            };
        }

        private void InitPools()
        {
            _pools = new Dictionary<string, Queue<ParticleSystem>>();
            foreach (var kvp in _prefabMap)
                PrewarmPool(kvp.Key, kvp.Value);
        }

        private void PrewarmPool(string key, ParticleSystem prefab)
        {
            if (prefab == null) return;
            var queue = new Queue<ParticleSystem>();
            for (int i = 0; i < _poolSizePerType; i++)
            {
                var ps = Instantiate(prefab, transform);
                ps.gameObject.SetActive(false);
                ps.gameObject.name = $"VFX_{key}_{i}";
                queue.Enqueue(ps);
            }
            _pools[key] = queue;
        }

        // ─── EventBus ─────────────────────────────────────────────────────
        private void SubscribeEvents()
        {
            EventBus.Instance.Subscribe<DamageTakenEvent>(OnDamageTaken);
            EventBus.Instance.Subscribe<EntityDeathEvent>(OnEntityDeath);
            EventBus.Instance.Subscribe<HealingReceivedEvent>(OnHealingReceived);
        }

        private void UnsubscribeEvents()
        {
            if (EventBus.Instance == null) return;
            EventBus.Instance.Unsubscribe<DamageTakenEvent>(OnDamageTaken);
            EventBus.Instance.Unsubscribe<EntityDeathEvent>(OnEntityDeath);
            EventBus.Instance.Unsubscribe<HealingReceivedEvent>(OnHealingReceived);
        }

        private void OnDamageTaken(DamageTakenEvent e)
        {
            Vector3 pos = GetEntityPosition(e.TargetId);
            PlayHitVFX(pos, e.DamageType ?? "Physical");
        }

        private void OnEntityDeath(EntityDeathEvent e)
        {
            Vector3 pos = GetEntityPosition(e.EntityId);
            PlayDeathVFX(pos);
        }

        private void OnHealingReceived(HealingReceivedEvent e)
        {
            Vector3 pos = GetEntityPosition(e.TargetId);
            PlayHitVFX(pos, "Heal");
        }

        // ─── Public API ───────────────────────────────────────────────────

        /// <summary>
        /// Play hit VFX tại worldPosition theo loại damage.
        /// damageType: "Physical", "Fire", "Ice", "Lightning", "Dark", "Light", "Heal"
        /// </summary>
        public void PlayHitVFX(Vector3 worldPosition, string damageType = "Physical")
        {
            string key = MapDamageTypeToKey(damageType);
            PlayFromPool(key, worldPosition);
        }

        /// <summary>Play death dissolve VFX</summary>
        public void PlayDeathVFX(Vector3 worldPosition)
        {
            PlayFromPool("Death", worldPosition);
        }

        /// <summary>
        /// Skill-specific VFX (placeholder — mở rộng per-skillId sau).
        /// Hiện tại fallback về hit effect theo element.
        /// </summary>
        public void PlaySkillVFX(string skillId, Vector3 worldPosition, string element = "Physical")
        {
            PlayHitVFX(worldPosition, element);
        }

        // ─── Pool Operations ──────────────────────────────────────────────
        private void PlayFromPool(string key, Vector3 worldPosition)
        {
            if (!_pools.TryGetValue(key, out var queue))
            {
                Debug.LogWarning($"[VFXController] Pool key '{key}' không tồn tại. Kiểm tra prefab assignments.");
                return;
            }

            if (queue.Count == 0)
            {
                // Pool cạn — cố tái tạo nếu có prefab
                if (_prefabMap.TryGetValue(key, out var prefab) && prefab != null)
                    PrewarmPool(key, prefab);
                else
                {
                    Debug.LogWarning($"[VFXController] Pool '{key}' empty và không có prefab để recreate.");
                    return;
                }
            }

            var ps = queue.Dequeue();
            ps.transform.position = worldPosition;
            ps.gameObject.SetActive(true);
            ps.Play();

            float returnDelay = ps.main.duration + ps.main.startLifetime.constantMax + 0.1f;
            StartCoroutine(ReturnToPool(ps, returnDelay, key));
        }

        private IEnumerator ReturnToPool(ParticleSystem ps, float delay, string key)
        {
            yield return new WaitForSeconds(delay);
            if (ps == null) yield break;
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ps.gameObject.SetActive(false);
            if (_pools.TryGetValue(key, out var queue))
                queue.Enqueue(ps);
        }

        // ─── Helpers ──────────────────────────────────────────────────────
        private static string MapDamageTypeToKey(string damageType)
        {
            if (string.IsNullOrEmpty(damageType)) return "Physical";
            return damageType switch
            {
                "Fire"      => "Fire",
                "Ice"       => "Ice",
                "Lightning" => "Lightning",
                "Dark"      => "Dark",
                "Light"     => "Light",
                "Heal"      => "Heal",
                _           => "Physical"
            };
        }

        private static Vector3 GetEntityPosition(string entityId)
        {
            if (ActionAnimationController.Instance == null) return Vector3.zero;
            var view = ActionAnimationController.Instance.GetViewForEntity(entityId);
            return view != null ? view.HitAnchorPosition : Vector3.zero;
        }
    }
}
