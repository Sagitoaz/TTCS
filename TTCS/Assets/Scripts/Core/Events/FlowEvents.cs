namespace TTCS.Core.Events
{
    /// <summary>
    /// Raised when tutorial is completed or skipped.
    /// </summary>
    public class TutorialCompletedEvent : GameEvent
    {
        public bool Skipped { get; private set; }

        public TutorialCompletedEvent(bool skipped)
        {
            Skipped = skipped;
        }
    }

    /// <summary>
    /// Raised when the player enters a level from level select.
    /// </summary>
    public class LevelEnteredEvent : GameEvent
    {
        public string LevelId { get; private set; }

        public LevelEnteredEvent(string levelId)
        {
            LevelId = levelId;
        }
    }

    /// <summary>
    /// Raised when flow receives combat result payload.
    /// </summary>
    public class CombatResultReceivedEvent : GameEvent
    {
        public string LevelId { get; private set; }
        public bool Victory { get; private set; }
        public int Stars { get; private set; }
        public int Score { get; private set; }

        public CombatResultReceivedEvent(string levelId, bool victory, int stars, int score)
        {
            LevelId = levelId;
            Victory = victory;
            Stars = stars;
            Score = score;
        }
    }
}
