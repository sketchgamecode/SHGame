using UnityEngine;
using System.Collections;
using SHGame.Core;

namespace SHGame.Core
{
    /// <summary>
    /// Initializer script for the BootLoader scene
    /// Handles initialization of all persistent managers and loading the main menu
    /// </summary>
    public class BootLoaderInitializer : MonoBehaviour
    {
        [Header("Boot Settings")]
        public float initializationDelay = 1.5f;
        public string mainMenuSceneName = "MainMenu";
        public bool showDebugLogs = true;
        
        [Header("Required Managers")]
        public GameManager gameManager;
        public UIManager uiManager;
        public AudioManager audioManager;
        
        [Header("Splash Screen")]
        public CanvasGroup splashCanvasGroup;
        public float splashFadeInDuration = 0.5f;
        public float splashDisplayDuration = 2.0f;
        public float splashFadeOutDuration = 0.5f;

        private void Start()
        {
            LogDebug("BootLoader initialization started");
            
            // First check for required managers
            if (!ValidateManagers())
            {
                LogError("Required managers are missing! Check BootLoader scene setup.");
                return;
            }
            
            // Start initialization sequence
            StartCoroutine(InitializationSequence());
        }
        
        private bool ValidateManagers()
        {
            bool allValid = true;
            
            // Check GameManager
            if (gameManager == null)
            {
                gameManager = FindObjectOfType<GameManager>();
                if (gameManager == null)
                {
                    LogError("GameManager not found in scene!");
                    allValid = false;
                }
            }
            
            // Check UIManager
            if (uiManager == null)
            {
                uiManager = FindObjectOfType<UIManager>();
                if (uiManager == null)
                {
                    LogError("UIManager not found in scene!");
                    allValid = false;
                }
            }
            
            // Check AudioManager
            if (audioManager == null)
            {
                audioManager = FindObjectOfType<AudioManager>();
                if (audioManager == null)
                {
                    LogError("AudioManager not found in scene!");
                    allValid = false;
                }
            }
            
            return allValid;
        }
        
        private IEnumerator InitializationSequence()
        {
            // Handle splash screen if available
            if (splashCanvasGroup != null)
            {
                yield return StartCoroutine(ShowSplashScreen());
            }
            
            LogDebug("Waiting for all managers to initialize...");
            yield return new WaitForSeconds(initializationDelay);
            
            // Load saved settings
            LoadSavedSettings();
            
            // Additional initialization can be added here
            
            // Load main menu
            LogDebug("Loading main menu scene: " + mainMenuSceneName);
            if (gameManager != null)
            {
                gameManager.LoadScene(mainMenuSceneName);
            }
        }
        
        private IEnumerator ShowSplashScreen()
        {
            // Initialize alpha
            splashCanvasGroup.alpha = 0f;
            
            // Fade in
            float elapsed = 0f;
            while (elapsed < splashFadeInDuration)
            {
                elapsed += Time.deltaTime;
                splashCanvasGroup.alpha = Mathf.Clamp01(elapsed / splashFadeInDuration);
                yield return null;
            }
            splashCanvasGroup.alpha = 1f;
            
            // Display duration
            yield return new WaitForSeconds(splashDisplayDuration);
            
            // Fade out
            elapsed = 0f;
            while (elapsed < splashFadeOutDuration)
            {
                elapsed += Time.deltaTime;
                splashCanvasGroup.alpha = Mathf.Clamp01(1f - (elapsed / splashFadeOutDuration));
                yield return null;
            }
            splashCanvasGroup.alpha = 0f;
        }
        
        private void LoadSavedSettings()
        {
            // Load audio settings
            if (audioManager != null)
            {
                LogDebug("Loading saved audio settings");
                audioManager.LoadSavedVolumeSettings();
            }
            
            // Load other settings as needed
            // ...
        }
        
        #region Logging
        
        private void LogDebug(string message)
        {
            if (showDebugLogs)
            {
                Debug.Log("[BootLoader] " + message);
            }
        }
        
        private void LogError(string message)
        {
            Debug.LogError("[BootLoader] " + message);
        }
        
        #endregion
    }
}