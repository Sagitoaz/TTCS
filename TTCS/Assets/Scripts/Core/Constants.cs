using UnityEngine;

namespace TTCS.Core
{
    /// <summary>
    /// 💜 SHARED FILE - Coordinate before editing!
    /// Các hằng số dùng chung trong toàn bộ game
    /// </summary>
    public static class Constants
    {
        #region Game Config
        
        /// <summary>Phiên bản game</summary>
        public const string GAME_VERSION = "0.1.0-alpha";
        
        /// <summary>Tên hiển thị game</summary>
        public const string GAME_NAME = "TTCS";
        
        /// <summary>FPS mục tiêu</summary>
        public const int TARGET_FPS = 60;
        
        #endregion

        #region Combat Constants
        
        /// <summary>Số lượng slot trong đội hình tối đa</summary>
        public const int MAX_PARTY_SIZE = 4;
        
        /// <summary>Số lượng kỹ năng tối đa mỗi nhân vật</summary>
        public const int MAX_SKILLS_PER_CHARACTER = 4;
        
        /// <summary>Cơ số tính damage (100 = 100%)</summary>
        public const float DAMAGE_BASE = 100f;
        
        /// <summary>Tỷ lệ crit damage mặc định (150%)</summary>
        public const float CRIT_DAMAGE_MULTIPLIER = 1.5f;
        
        /// <summary>Giá trị SPD cơ bản để tính turn order</summary>
        public const int BASE_SPD_VALUE = 100;
        
        /// <summary>Turn cost mặc định cho hành động thường</summary>
        public const int DEFAULT_TURN_COST = 100;
        
        /// <summary>Turn cost cho item sử dụng</summary>
        public const int ITEM_TURN_COST = 50;
        
        /// <summary>Turn cost cho guard/defend</summary>
        public const int GUARD_TURN_COST = 80;
        
        #endregion

        #region Timing Windows (milliseconds)
        
        /// <summary>Khung thời gian Perfect timing (ms)</summary>
        public const int PERFECT_TIMING_WINDOW = 50;
        
        /// <summary>Khung thời gian Good timing (ms)</summary>
        public const int GOOD_TIMING_WINDOW = 100;
        
        /// <summary>Khung thời gian Normal timing (ms)</summary>
        public const int NORMAL_TIMING_WINDOW = 200;
        
        /// <summary>Multiplier damage khi Perfect</summary>
        public const float PERFECT_TIMING_MULTIPLIER = 1.5f;
        
        /// <summary>Multiplier damage khi Good</summary>
        public const float GOOD_TIMING_MULTIPLIER = 1.2f;
        
        /// <summary>Multiplier damage khi Normal</summary>
        public const float NORMAL_TIMING_MULTIPLIER = 1.0f;
        
        /// <summary>Guard reduction khi Perfect parry</summary>
        public const float PERFECT_GUARD_REDUCTION = 0.9f; // Giảm 90% damage
        
        /// <summary>Guard reduction khi Good parry</summary>
        public const float GOOD_GUARD_REDUCTION = 0.7f; // Giảm 70% damage
        
        /// <summary>Guard reduction khi Normal guard</summary>
        public const float NORMAL_GUARD_REDUCTION = 0.5f; // Giảm 50% damage
        
        #endregion

        #region Status Effect Constants
        
        /// <summary>Số lượng status effect tối đa trên 1 entity</summary>
        public const int MAX_STATUS_EFFECTS = 10;
        
        /// <summary>Burn damage % theo ATK của người gây</summary>
        public const float BURN_DAMAGE_PERCENT = 0.2f; // 20% ATK
        
        /// <summary>Poison damage % theo HP max của mục tiêu</summary>
        public const float POISON_DAMAGE_PERCENT = 0.05f; // 5% max HP
        
        /// <summary>Stun duration mặc định (turns)</summary>
        public const int STUN_DEFAULT_DURATION = 1;
        
        /// <summary>Weak duration mặc định (turns)</summary>
        public const int WEAK_DEFAULT_DURATION = 2;
        
        #endregion

        #region Resource Paths
        
        /// <summary>Path đến folder chứa Character data</summary>
        public const string CHARACTERS_DATA_PATH = "Data/Characters";
        
        /// <summary>Path đến folder chứa Skill data</summary>
        public const string SKILLS_DATA_PATH = "Data/Skills";
        
        /// <summary>Path đến folder chứa Enemy data</summary>
        public const string ENEMIES_DATA_PATH = "Data/Enemies";
        
        /// <summary>Path đến folder chứa Stage data</summary>
        public const string STAGES_DATA_PATH = "Data/Stages";
        
        /// <summary>Path đến folder chứa Item data</summary>
        public const string ITEMS_DATA_PATH = "Data/Items";
        
        /// <summary>Path đến VFX prefabs</summary>
        public const string VFX_PREFABS_PATH = "Prefabs/VFX";
        
        /// <summary>Path đến Audio clips</summary>
        public const string AUDIO_PATH = "Audio";
        
        #endregion

        #region Save System
        
        /// <summary>Tên file save chính</summary>
        public const string SAVE_FILE_NAME = "save.json";
        
        /// <summary>Prefix cho backup saves</summary>
        public const string BACKUP_SAVE_PREFIX = "save_backup_";
        
        /// <summary>Số lượng backup saves tối đa</summary>
        public const int MAX_BACKUP_SAVES = 3;
        
        /// <summary>Save version</summary>
        public const int SAVE_VERSION = 1;
        
        #endregion

        #region UI Constants
        
        /// <summary>Animation duration cho UI transitions (seconds)</summary>
        public const float UI_TRANSITION_DURATION = 0.3f;
        
        /// <summary>Fade duration cho screens (seconds)</summary>
        public const float SCREEN_FADE_DURATION = 0.5f;
        
        #endregion

        #region Debug
        
        /// <summary>Enable debug logs</summary>
        public const bool DEBUG_LOGS_ENABLED = true;
        
        /// <summary>Enable combat debug overlay</summary>
        public const bool DEBUG_COMBAT_OVERLAY = true;
        
        /// <summary>God mode (invincible player)</summary>
        public const bool DEBUG_GOD_MODE = false;
        
        #endregion

        #region Tags & Layers
        
        /// <summary>Tag cho player entities</summary>
        public const string TAG_PLAYER = "Player";
        
        /// <summary>Tag cho enemy entities</summary>
        public const string TAG_ENEMY = "Enemy";
        
        /// <summary>Layer name cho combat entities</summary>
        public const string LAYER_COMBAT = "Combat";
        
        #endregion
    }
}
