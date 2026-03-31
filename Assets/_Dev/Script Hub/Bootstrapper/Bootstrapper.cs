using proscryption;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class Bootstrapper
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Initialize()
    {
        // Ensure AppManager exists before any non-bootstrap scene starts.
        // Game-specific systems are initialized later by GameEntryPoint.
        if (SceneManager.GetActiveScene().name == "Bootstrapper")
        {
            return;
        }

        if (AppManager.Instance == null)
        {
            new GameObject("AppManager").AddComponent<AppManager>();
        }
    }
}