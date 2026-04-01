using UnityEngine;

namespace TTCS.Flow.Tutorial
{
    /// <summary>
    /// Manages tutorial scene flow for first-time players.
    /// Dev B - Ngày 2-3 Sprint 03.
    /// Listens to: scene loaded, skip button, tutorial completed.
    /// </summary>
    public class TutorialFlowLogic : MonoBehaviour
    {
        [SerializeField] private float _tutorialDuration = 15f; // Tutorial lasts ~15s
        
        private float _elapsedTime = 0f;
        private bool _tutorialCompleted = false;

        private void Start()
        {
            Debug.Log("[Tutorial] Tutorial scene started");
            // Mark save as tutorial started
            PlayerPrefs.SetInt("TutorialStarted", 1);
        }

        private void Update()
        {
            if (_tutorialCompleted)
                return;

            _elapsedTime += Time.deltaTime;

            if (_elapsedTime >= _tutorialDuration)
            {
                CompleteTutorial();
            }
        }

        /// <summary>
        /// Called when player completes tutorial (auto or manually).
        /// </summary>
        public void CompleteTutorial()
        {
            if (_tutorialCompleted)
                return;

            _tutorialCompleted = true;
            Debug.Log("[Tutorial] Tutorial completed");

            // Save tutorial completion state
            PlayerPrefs.SetInt("TutorialCompleted", 1);
            PlayerPrefs.Save();

            // After a short delay, go to main menu
            Invoke(nameof(GoToMainMenu), 1f);
        }

        /// <summary>
        /// Player skips tutorial.
        /// </summary>
        public void SkipTutorial()
        {
            Debug.Log("[Tutorial] Tutorial skipped by player");
            CompleteTutorial();
        }

        private void GoToMainMenu()
        {
            Debug.Log("[Tutorial] Transitioning to main menu");
            FlowController.Instance.OpenMainMenu();
        }
    }
}
