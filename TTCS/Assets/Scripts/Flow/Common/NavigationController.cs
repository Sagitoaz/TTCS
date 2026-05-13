using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TTCS.Flow
{
    /// <summary>
    /// Scene loading helper used by flow to keep scene transitions consistent.
    /// </summary>
    public class NavigationController : MonoBehaviour
    {
        public async Task LoadScene(string sceneName)
        {
            if (string.IsNullOrWhiteSpace(sceneName))
            {
                return;
            }

            if (!Application.CanStreamedLevelBeLoaded(sceneName))
            {
                Debug.LogWarning($"[Flow] Scene '{sceneName}' cannot be loaded (not in build settings?)");
                return;
            }

            // Prefer global transition overlay if available.
            var flow = FlowController.Instance;
            if (flow != null)
            {
                var transition = flow.GetComponent<SceneTransitionController>();
                if (transition != null)
                {
                    transition.LoadScene(sceneName);

                    // Wait until the active scene changes to the requested one.
                    // We avoid waiting on an AsyncOperation here because the transition controller owns it.
                    while (SceneManager.GetActiveScene().name != sceneName)
                    {
                        await Task.Yield();
                    }

                    return;
                }
            }

            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
            if (operation == null)
            {
                return;
            }

            while (!operation.isDone)
            {
                await Task.Yield();
            }
        }

        public async Task UnloadScene(string sceneName)
        {
            Scene scene = SceneManager.GetSceneByName(sceneName);
            if (!scene.IsValid() || !scene.isLoaded)
            {
                return;
            }

            AsyncOperation operation = SceneManager.UnloadSceneAsync(sceneName);
            if (operation == null)
            {
                return;
            }

            while (!operation.isDone)
            {
                await Task.Yield();
            }
        }
    }
}
