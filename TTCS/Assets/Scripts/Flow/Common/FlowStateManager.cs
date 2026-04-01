using System.Collections.Generic;

namespace TTCS.Flow
{
    /// <summary>
    /// Stores lightweight runtime navigation state for DevB flow scenes.
    /// </summary>
    public class FlowStateManager
    {
        public string CurrentScene { get; private set; }
        public Stack<string> NavHistory { get; } = new Stack<string>();

        public void NavigateTo(string sceneName)
        {
            if (!string.IsNullOrWhiteSpace(CurrentScene))
            {
                NavHistory.Push(CurrentScene);
            }

            CurrentScene = sceneName;
        }

        public string Back()
        {
            if (NavHistory.Count == 0)
            {
                return CurrentScene;
            }

            CurrentScene = NavHistory.Pop();
            return CurrentScene;
        }

        public bool TryEnterTutorial()
        {
            return UnityEngine.PlayerPrefs.GetInt("TutorialCompleted", 0) != 1;
        }
    }
}
