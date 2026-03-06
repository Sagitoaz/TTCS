using UnityEngine;
using TTCS.Data;

namespace TTCS.Visual
{
    /// <summary>
    /// 🟢 Dev B Sprint 2 - Character View Factory
    ///
    /// Instantiate CharacterView và EnemyView từ data models.
    /// Load prefab từ Resources folder hoặc tạo placeholder nếu chưa có prefab.
    ///
    /// PREFAB LOCATION (phải đặt trong Resources/ để Load được):
    ///   Characters: Assets/Resources/Prefabs/Combat/Characters/{characterId}.prefab
    ///   Enemies:    Assets/Resources/Prefabs/Combat/Enemies/{enemyId}.prefab
    ///   Ví dụ: Assets/Resources/Prefabs/Combat/Characters/char_warrior.prefab
    ///
    /// FALLBACK:
    ///   Nếu prefab chưa tồn tại → tạo placeholder GameObject màu đặc trưng.
    ///   (Cyan = character, Đỏ = enemy)
    ///   Thay thế bằng prefab thực khi art sẵn sàng.
    ///
    /// GỌI TỪ: CombatSceneManager (Dev A) trong InitializeCombat()
    /// </summary>
    public static class CharacterViewFactory
    {
        private const string CharactersPath = "Prefabs/Combat/Characters/";
        private const string EnemiesPath    = "Prefabs/Combat/Enemies/";

        // ─── Character ────────────────────────────────────────────────────

        /// <summary>
        /// Tạo CharacterView từ CharacterDataModel (JSON-loaded runtime).
        /// Đặt tại parent nếu có, hoặc world root nếu không.
        /// </summary>
        public static CharacterView CreateCharacterView(CharacterDataModel model, Transform parent = null)
        {
            if (model == null)
            {
                Debug.LogWarning("[CharacterViewFactory] CharacterDataModel is null.");
                return null;
            }

            GameObject go = LoadOrCreatePlaceholder(CharactersPath + model.id, model.id, parent, new Color(0.4f, 0.8f, 1f));

            var view = go.GetComponent<CharacterView>();
            if (view == null)
                view = go.AddComponent<CharacterView>();

            view.EntityId = model.id;
            go.name       = $"CharView_{model.id}";

            // Player characters mặc định nhìn phải
            view.SetFacing(true);

            return view;
        }

        /// <summary>Overload dùng CharacterData ScriptableObject (Editor/prototyping)</summary>
        public static CharacterView CreateCharacterView(CharacterData data, Transform parent = null)
        {
            if (data == null)
            {
                Debug.LogWarning("[CharacterViewFactory] CharacterData is null.");
                return null;
            }

            GameObject go = LoadOrCreatePlaceholder(CharactersPath + data.id, data.id, parent, new Color(0.4f, 0.8f, 1f));

            var view = go.GetComponent<CharacterView>() ?? go.AddComponent<CharacterView>();
            view.EntityId = data.id;
            go.name       = $"CharView_{data.id}";
            view.SetFacing(true);

            return view;
        }

        // ─── Enemy ────────────────────────────────────────────────────────

        /// <summary>
        /// Tạo EnemyView từ EnemyDataModel (JSON-loaded runtime).
        /// Enemy mặc định nhìn trái (EnemyView.Awake() xử lý).
        /// </summary>
        public static EnemyView CreateEnemyView(EnemyDataModel model, Transform parent = null)
        {
            if (model == null)
            {
                Debug.LogWarning("[CharacterViewFactory] EnemyDataModel is null.");
                return null;
            }

            GameObject go = LoadOrCreatePlaceholder(EnemiesPath + model.id, model.id, parent, new Color(1f, 0.4f, 0.4f));

            var view = go.GetComponent<EnemyView>();
            if (view == null)
                view = go.AddComponent<EnemyView>();

            view.EntityId = model.id;
            go.name       = $"EnemyView_{model.id}";

            return view;
        }

        // ─── Internal ─────────────────────────────────────────────────────

        /// <summary>Load prefab từ Resources hoặc tạo placeholder</summary>
        private static GameObject LoadOrCreatePlaceholder(string resourcePath, string id, Transform parent, Color placeholderColor)
        {
            var prefab = Resources.Load<GameObject>(resourcePath);

            if (prefab != null)
            {
                return parent != null
                    ? Object.Instantiate(prefab, parent)
                    : Object.Instantiate(prefab);
            }

            // Prefab chưa có — tạo placeholder để dev tiếp tục mà không bị block
            Debug.LogWarning($"[CharacterViewFactory] Prefab không tìm thấy tại '{resourcePath}'. Dùng placeholder.");
            return CreatePlaceholder(id, parent, placeholderColor);
        }

        private static GameObject CreatePlaceholder(string id, Transform parent, Color color)
        {
            var root = new GameObject($"Placeholder_{id}");
            if (parent != null)
                root.transform.SetParent(parent, false);

            // Body sprite đơn giản (hình chữ nhật 32×64 px)
            var bodyGO = new GameObject("Body");
            bodyGO.transform.SetParent(root.transform, false);

            var sr = bodyGO.AddComponent<SpriteRenderer>();
            sr.sprite = CreateRectSprite(32, 64);
            sr.color  = color;
            sr.sortingLayerName = "Characters";

            // HeadAnchor placeholder
            var headAnchor = new GameObject("HeadAnchor");
            headAnchor.transform.SetParent(root.transform, false);
            headAnchor.transform.localPosition = new Vector3(0f, 1.1f, 0f);

            // HitAnchor placeholder
            var hitAnchor = new GameObject("HitAnchor");
            hitAnchor.transform.SetParent(root.transform, false);
            hitAnchor.transform.localPosition = new Vector3(0f, 0.5f, 0f);

            return root;
        }

        private static Sprite CreateRectSprite(int width, int height)
        {
            var tex    = new Texture2D(width, height, TextureFormat.RGBA32, false);
            var pixels = new Color[width * height];
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = Color.white;
            tex.SetPixels(pixels);
            tex.Apply();
            tex.filterMode = FilterMode.Point;

            return Sprite.Create(
                tex,
                new Rect(0, 0, width, height),
                new Vector2(0.5f, 0f),   // pivot tại chân nhân vật
                100f);
        }
    }
}
