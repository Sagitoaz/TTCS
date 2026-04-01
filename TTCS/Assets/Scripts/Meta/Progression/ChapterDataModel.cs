using System;
using System.Collections.Generic;

namespace TTCS.Meta.Progression
{
    [Serializable]
    public class ChapterDataModel
    {
        public string id;
        public string nameKey;
        public int order;
        public string unlockRule;
        public int requiredTotalStars;
        public List<string> levelIds = new List<string>();
    }
}
