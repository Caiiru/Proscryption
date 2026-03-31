using proscryption;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class Bootstrapper
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Initialize()
    {
        // Ensure AppManager and GameManager are instantiated before any scene loads

        if (SceneManager.GetActiveScene().name == "Bootstrapper")
        {
            return;
        }
        if (AppManager.Instance == null)
        {
            new GameObject("AppManager").AddComponent<AppManager>();
        }
        if (GameManager.Instance == null)
        {
            new GameObject("GameManager").AddComponent<GameManager>();
        }
    }
}