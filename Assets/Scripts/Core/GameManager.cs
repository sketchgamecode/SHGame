using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;

namespace SHGame.Core
{
    /// <summary>
    /// Core game state management and scene loading system
    /// Implements singleton pattern for global access
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Game Settings")]
        public bool isPaused = false;
        public GameState currentGameState = GameState.MainMenu;
        public bool debugMode = false;

        [Header("Scene Management")]
        public string bootLoaderScene = "BootLoader";
        public string mainMenuScene = "MainMenu";
        public string firstLevelScene = "Level01_HorseYard";
        public bool useAsyncLoading = true;

        [Header("Loading Screen")]
        public float minimumLoadingTime = 0.5f;
        public float loadingFadeTime = 0.5f;

        // Events for game state changes
        public static event Action<GameState> OnGameStateChanged;
        public static event Action OnGamePaused;
        public static event Action OnGameResumed;
        public static event Action<string> OnBeforeSceneLoad;
        public static event Action<string> OnAfterSceneLoad;

        // State
        private bool isLoading = false;
        private Coroutine loadingCoroutine;

        public enum GameState
        {
            MainMenu,
            Loading,
            Playing,
            Paused,
            GameOver,
            Victory
        }

        private void Awake()
        {
            // Singleton pattern implementation
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeGame();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void Update()
        {
            HandleInput();
        }

        private void InitializeGame()
        {
            // Set initial game settings
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 1;
            
            Debug.Log("GameManager initialized successfully");
        }

        private void HandleInput()
        {
            // Handle pause input
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (currentGameState == GameState.Playing)
                {
                    PauseGame();
                }
                else if (currentGameState == GameState.Paused)
                {
                    ResumeGame();
                }
            }

            // Debug mode toggle
            if (Input.GetKeyDown(KeyCode.F1))
            {
                debugMode = !debugMode;
                Debug.Log($"Debug Mode: {(debugMode ? "Enabled" : "Disabled")}");
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Debug.Log($"Scene loaded: {scene.name}");
            OnAfterSceneLoad?.Invoke(scene.name);
            
            // Set appropriate game state based on scene
            SetGameStateForScene(scene.name);
        }

        private void SetGameStateForScene(string sceneName)
        {
            if (sceneName == mainMenuScene)
            {
                ChangeGameState(GameState.MainMenu);
            }
            else if (sceneName != bootLoaderScene)
            {
                // Any non-menu, non-bootloader scene is a gameplay scene
                ChangeGameState(GameState.Playing);
            }
        }

        #region Game State Management

        public void ChangeGameState(GameState newState)
        {
            if (currentGameState == newState) return;

            GameState previousState = currentGameState;
            currentGameState = newState;

            Debug.Log($"Game state changed from {previousState} to {newState}");
            OnGameStateChanged?.Invoke(newState);
        }

        public void StartGame()
        {
            ChangeGameState(GameState.Loading);
            LoadScene(firstLevelScene);
        }

        public void PauseGame()
        {
            if (currentGameState != GameState.Playing) return;

            isPaused = true;
            Time.timeScale = 0f;
            ChangeGameState(GameState.Paused);
            OnGamePaused?.Invoke();
        }

        public void ResumeGame()
        {
            if (currentGameState != GameState.Paused) return;

            isPaused = false;
            Time.timeScale = 1f;
            ChangeGameState(GameState.Playing);
            OnGameResumed?.Invoke();
        }

        public void GameOver()
        {
            ChangeGameState(GameState.GameOver);
            Time.timeScale = 0f;
        }

        public void Victory()
        {
            ChangeGameState(GameState.Victory);
            Time.timeScale = 0f;
        }

        #endregion

        #region Scene Loading

        public void LoadScene(string sceneName, bool showLoadingScreen = true)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogError("Scene name is null or empty");
                return;
            }

            // Prevent multiple simultaneous loads
            if (isLoading)
            {
                Debug.LogWarning("Already loading a scene, request ignored");
                return;
            }

            // Notify before loading
            OnBeforeSceneLoad?.Invoke(sceneName);
            
            Debug.Log($"Loading scene: {sceneName}");

            if (useAsyncLoading && showLoadingScreen)
            {
                // Use coroutine for async loading with loading screen
                if (loadingCoroutine != null)
                {
                    StopCoroutine(loadingCoroutine);
                }
                
                loadingCoroutine = StartCoroutine(LoadSceneAsync(sceneName));
            }
            else
            {
                // Use synchronous loading (simpler but may cause frame drops)
                SceneManager.LoadScene(sceneName);
            }
        }

        private IEnumerator LoadSceneAsync(string sceneName)
        {
            isLoading = true;
            ChangeGameState(GameState.Loading);
            
            // Show loading screen
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowLoadingScreen();
            }
            
            // Track loading start time
            float startTime = Time.realtimeSinceStartup;
            
            // Start async loading operation
            AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName);
            asyncOperation.allowSceneActivation = false;
            
            // Wait until the load is nearly complete
            while (asyncOperation.progress < 0.9f)
            {
                // Update loading progress
                float progress = asyncOperation.progress / 0.9f;
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.UpdateLoadingProgress(progress);
                }
                
                yield return null;
            }
            
            // Make sure we show the loading screen for at least the minimum time
            float elapsedTime = Time.realtimeSinceStartup - startTime;
            float remainingTime = minimumLoadingTime - elapsedTime;
            
            if (remainingTime > 0)
            {
                // Complete the progress bar while we wait
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.UpdateLoadingProgress(1.0f);
                }
                
                yield return new WaitForSecondsRealtime(remainingTime);
            }
            
            // Allow the scene to activate
            asyncOperation.allowSceneActivation = true;
            
            // Wait for activation to complete
            while (!asyncOperation.isDone)
            {
                yield return null;
            }
            
            // Clean up
            isLoading = false;
            loadingCoroutine = null;
        }

        public void RestartCurrentLevel()
        {
            Time.timeScale = 1f;
            LoadScene(SceneManager.GetActiveScene().name);
        }

        public void ReturnToMainMenu()
        {
            Time.timeScale = 1f;
            ChangeGameState(GameState.Loading);
            LoadScene(mainMenuScene);
        }

        public string GetCurrentSceneName()
        {
            return SceneManager.GetActiveScene().name;
        }

        public bool IsSceneLoaded(string sceneName)
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                if (SceneManager.GetSceneAt(i).name == sceneName)
                {
                    return true;
                }
            }
            
            return false;
        }

        #endregion

        #region System Events

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus && currentGameState == GameState.Playing)
            {
                PauseGame();
            }
        }

        private void OnApplicationQuit()
        {
            // Perform any cleanup before application quits
            Debug.Log("Application is quitting, performing cleanup...");
        }

        #endregion

        #region Debug Methods

        public void ToggleDebugMode()
        {
            debugMode = !debugMode;
            Debug.Log($"Debug Mode: {(debugMode ? "Enabled" : "Disabled")}");
        }

        public bool IsDebugModeEnabled()
        {
            return debugMode;
        }

        public void JumpToScene(string sceneName)
        {
            if (debugMode)
            {
                LoadScene(sceneName);
            }
            else
            {
                Debug.LogWarning("Debug mode must be enabled to use JumpToScene");
            }
        }

        #endregion
    }
}