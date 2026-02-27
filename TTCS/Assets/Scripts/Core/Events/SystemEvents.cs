namespace TTCS.Core.Events
{
    /// <summary>
    /// 🔵 Dev A - System-level Events
    /// Các event liên quan đến hệ thống: data loading, save/load, scene transitions, v.v.
    /// </summary>

    #region Data Events

    /// <summary>Event khi DataManager load xong toàn bộ dữ liệu</summary>
    public class DataLoadedEvent : GameEvent
    {
        public int CharacterCount { get; private set; }
        public int SkillCount     { get; private set; }
        public int EnemyCount     { get; private set; }
        public int StageCount     { get; private set; }

        public DataLoadedEvent(int characterCount, int skillCount, int enemyCount, int stageCount)
        {
            CharacterCount = characterCount;
            SkillCount     = skillCount;
            EnemyCount     = enemyCount;
            StageCount     = stageCount;
        }
    }

    #endregion

    #region Save Events

    /// <summary>Event khi game được save thành công</summary>
    public class GameSavedEvent : GameEvent
    {
        public int SlotIndex { get; private set; }

        public GameSavedEvent(int slotIndex)
        {
            SlotIndex = slotIndex;
        }
    }

    /// <summary>Event khi game được load thành công</summary>
    public class GameLoadedEvent : GameEvent
    {
        public int SlotIndex { get; private set; }

        public GameLoadedEvent(int slotIndex)
        {
            SlotIndex = slotIndex;
        }
    }

    #endregion
}
