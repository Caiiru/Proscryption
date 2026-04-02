using UnityEngine;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;
using System;

namespace proscryption
{
    [Serializable]
    public enum AppState { Initializing, MainMenu, Config, Playing, Paused, GameOver }
    public class AppManager : MonoBehaviour
    {
        public static AppManager Instance { get; private set; }
        public AppState CurrentState = AppState.Initializing;


        //LoadingScreen

        public float LoadingProgress { get; private set; } = 0f;



        void Awake()
        {
            DontDestroyOnLoad(this);
            Instance = this;
        }
        void Start()
        {
            Scene activeScene = SceneManager.GetActiveScene();

            if (activeScene.name == "Bootstrapper")
            {
                ChangeAppState(AppState.MainMenu).Forget();
                return;
            }

            if (activeScene.name == "MainMenuScreen")
            {
                CurrentState = AppState.MainMenu;
                return;
            }

            CurrentState = AppState.Playing;
        }

        public async UniTask ChangeAppState(AppState newState)
        {
            if (CurrentState == newState) return;
            CurrentState = newState;
            // Handle state transition logic here (e.g., load scenes, show UI, etc.)
            switch (newState)
            {
                case AppState.MainMenu:
                    SceneManager.LoadScene("MainMenuScreen"); // Example: Load main menu scene
                    // Load main menu scene or show main menu UI
                    break;
                case AppState.Playing:
                    SceneManager.LoadScene("LoadingScreen");
                    await LoadSceneAsync("GameScreen");
                    break;
                case AppState.Paused:
                    // Pause the game, show pause menu, etc.
                    break;
                case AppState.GameOver:
                    // Show game over screen, handle cleanup, etc.
                    break;
                default:
                    // Handle other states as needed
                    break;
            }
        }
        private async UniTask LoadSceneAsync(string sceneName)
        {
            // 1. Inicia o carregamento mas bloqueia a exibição (Ativação)
            var operation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            operation.allowSceneActivation = false;

            // 2. Loop de progresso (O progresso trava em 0.9 quando allowSceneActivation é false)
            while (operation.progress < 0.9f)
            {
                LoadingProgress = operation.progress;
                await UniTask.Yield();
            }

            // Forçamos a barra para 100% visualmente
            LoadingProgress = 1f;

            // 3. Pequeno delay de "respiro" para o jogador ver que terminou
            await UniTask.Delay(1000);

            // 4. AGORA permitimos que a cena finalize e apareça
            operation.allowSceneActivation = true;

            // Aguardamos o fim real da operação
            await operation.ToUniTask();

            // 5. Agora que a cena existe, podemos torná-la a principal
            Scene targetScene = SceneManager.GetSceneByName(sceneName);
            if (targetScene.IsValid())
            {
                SceneManager.SetActiveScene(targetScene);
                EnsureInitializer(targetScene);

                EventManager.OnGameLoaded += UnloadLoadingScreen;
            }

            // 6. Finalmente, descarregamos o loading e resetamos o progresso

        }
        private void UnloadLoadingScreen()
        {
            SceneManager.UnloadSceneAsync("LoadingScreen");
        }
        private void EnsureInitializer(Scene gameScene)
        {
            if (Initializer.Instance == null)
            {

                foreach (GameObject rootObject in gameScene.GetRootGameObjects())
                {
                    Initializer existingManager = rootObject.GetComponentInChildren<Initializer>(true);
                    if (existingManager != null)
                    {
                        return;
                    }
                }

                GameObject systemsRoot = new GameObject("[Initializer]");
                SceneManager.MoveGameObjectToScene(systemsRoot, gameScene);
                systemsRoot.AddComponent<Initializer>();
            }
            Initializer.Instance.Initialize();
        }
    }
}
