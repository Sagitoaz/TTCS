using UnityEngine;

namespace TTCS.Flow.Tutorial
{
    /// <summary>
    /// Day 3 tutorial controller API from DevB plan.
    /// Keeps step state and delegates completion/skip to TutorialFlowLogic.
    /// </summary>
    public class TutorialController : MonoBehaviour
    {
        [SerializeField] private TutorialFlowLogic _tutorialFlowLogic;
        [SerializeField] private int _currentStepIndex;

        public IFlowController FlowController { get; set; }

        private void Awake()
        {
            if (_tutorialFlowLogic == null)
            {
                _tutorialFlowLogic = GetComponent<TutorialFlowLogic>();
            }
        }

        public void ShowStep(int stepIndex)
        {
            _currentStepIndex = Mathf.Max(0, stepIndex);
            Debug.Log($"[Tutorial] Show step: {_currentStepIndex}");
        }

        public void SkipTutorial()
        {
            if (_tutorialFlowLogic == null)
            {
                Debug.LogWarning("[Tutorial] TutorialFlowLogic is missing");
                return;
            }

            _tutorialFlowLogic.SkipTutorial();
        }

        public void CompleteTutorial()
        {
            if (_tutorialFlowLogic == null)
            {
                Debug.LogWarning("[Tutorial] TutorialFlowLogic is missing");
                return;
            }

            _tutorialFlowLogic.CompleteTutorial();
        }
    }
}
