using UnityEngine;
using UnityEngine.SceneManagement;
using System;

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

        [Header("Scene Management")]
        public string mainMenuScene = "MainMenu";
        public string firstLevelScene = "Level01_HorseYard";

        // Events for game state changes
        public static event Action<GameState> OnGameStateChanged;
        public static event Action OnGamePaused;
        public static event Action OnGameResumed;

        public enum GameState
        {
            MainMenu,
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
        }

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
            ChangeGameState(GameState.Playing);
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

        public void LoadScene(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogError("Scene name is null or empty");
                return;
            }

            Debug.Log($"Loading scene: {sceneName}");
            SceneManager.LoadScene(sceneName);
        }

        public void RestartCurrentLevel()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void ReturnToMainMenu()
        {
            Time.timeScale = 1f;
            ChangeGameState(GameState.MainMenu);
            LoadScene(mainMenuScene);
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus && currentGameState == GameState.Playing)
            {
                PauseGame();
            }
        }
    }
}