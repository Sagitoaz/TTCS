using System;

namespace TTCS.Meta.Progression
{
    [Serializable]
    public class LevelDataModel
    {
        public string id;
        public string chapterId;
        public string stageId;
        public int order;
        public int requiredStarsToUnlock;
    }
}
