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
