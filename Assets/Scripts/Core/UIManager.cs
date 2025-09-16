using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

namespace SHGame.Core
{
    /// <summary>
    /// Manages all UI elements including dialogue, interaction prompts, and stealth indicators
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("Dialogue System")]
        public GameObject subtitlePanel;
        public TextMeshProUGUI subtitleText;
        public float subtitleDisplayTime = 3f;

        [Header("Interaction System")]
        public GameObject interactionPrompt;
        public TextMeshProUGUI interactionText;
        public KeyCode interactionKey = KeyCode.Space;

        [Header("Stealth Indicators")]
        public GameObject stealthIndicator;
        public Image stealthStatusImage;
        public Color hiddenColor = Color.green;
        public Color exposedColor = Color.red;

        [Header("Information Log")]
        public GameObject informationPanel;
        public TextMeshProUGUI informationLogText;
        public ScrollRect informationScrollRect;

        [Header("QTE System")]
        public GameObject qtePanel;
        public TextMeshProUGUI qtePromptText;
        public Image qteProgressBar;
        public float qteTimeLimit = 3f;

        [Header("Game UI")]
        public GameObject pauseMenu;
        public GameObject gameOverPanel;
        public GameObject victoryPanel;
        
        [Header("Loading Screen")]
        public GameObject loadingScreen;
        public Slider loadingBar;
        public TextMeshProUGUI loadingText;
        public Image loadingBackgroundImage;
        public Sprite[] loadingBackgrounds;

        [Header("General UI Settings")]
        public bool persistAcrossScenes = true;
        public Canvas mainCanvas;
        public bool autoAdjustCanvasScaler = true;

        // State
        private string informationLog = "";
        private Coroutine subtitleCoroutine;
        private Coroutine qteCoroutine;
        private Coroutine loadingFadeCoroutine;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                
                // Make persistent if needed
                if (persistAcrossScenes)
                {
                    DontDestroyOnLoad(this.gameObject);
                }
                
                InitializeUI();
            }
            else
            {
                Destroy(this.gameObject);
            }
        }

        private void Start()
        {
            // Subscribe to game manager events
            if (GameManager.Instance != null)
            {
                GameManager.OnGameStateChanged += OnGameStateChanged;
                GameManager.OnGamePaused += ShowPauseMenu;
                GameManager.OnGameResumed += HidePauseMenu;
                GameManager.OnBeforeSceneLoad += OnBeforeSceneLoad;
                GameManager.OnAfterSceneLoad += OnAfterSceneLoad;
            }
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (GameManager.Instance != null)
            {
                GameManager.OnGameStateChanged -= OnGameStateChanged;
                GameManager.OnGamePaused -= ShowPauseMenu;
                GameManager.OnGameResumed -= HidePauseMenu;
                GameManager.OnBeforeSceneLoad -= OnBeforeSceneLoad;
                GameManager.OnAfterSceneLoad -= OnAfterSceneLoad;
            }
        }

        private void InitializeUI()
        {
            // Hide all panels initially
            HideAllPanels();
            
            // Show stealth indicator if available
            if (stealthIndicator != null)
            {
                stealthIndicator.SetActive(true);
            }
            
            // Adjust canvas scaler if needed
            if (autoAdjustCanvasScaler && mainCanvas != null)
            {
                var canvasScaler = mainCanvas.GetComponent<CanvasScaler>();
                if (canvasScaler != null)
                {
                    // Set reference resolution based on current screen
                    float screenAspect = (float)Screen.width / Screen.height;
                    if (screenAspect > 1.7f) // Widescreen
                    {
                        canvasScaler.referenceResolution = new Vector2(1920, 1080);
                    }
                    else // Standard
                    {
                        canvasScaler.referenceResolution = new Vector2(1600, 900);
                    }
                }
            }

            Debug.Log("UIManager initialized successfully");
        }

        private void HideAllPanels()
        {
            if (subtitlePanel != null) subtitlePanel.SetActive(false);
            if (interactionPrompt != null) interactionPrompt.SetActive(false);
            if (informationPanel != null) informationPanel.SetActive(false);
            if (qtePanel != null) qtePanel.SetActive(false);
            if (pauseMenu != null) pauseMenu.SetActive(false);
            if (gameOverPanel != null) gameOverPanel.SetActive(false);
            if (victoryPanel != null) victoryPanel.SetActive(false);
            if (loadingScreen != null) loadingScreen.SetActive(false);
        }

        #region Dialogue System

        public void ShowSubtitle(string text)
        {
            if (subtitlePanel == null || subtitleText == null) return;

            if (subtitleCoroutine != null)
            {
                StopCoroutine(subtitleCoroutine);
            }

            subtitlePanel.SetActive(true);
            subtitleText.text = text;
            
            subtitleCoroutine = StartCoroutine(HideSubtitleAfterTime());
        }

        public void HideSubtitle()
        {
            if (subtitlePanel != null)
            {
                subtitlePanel.SetActive(false);
            }

            if (subtitleCoroutine != null)
            {
                StopCoroutine(subtitleCoroutine);
                subtitleCoroutine = null;
            }
        }

        private IEnumerator HideSubtitleAfterTime()
        {
            yield return new WaitForSeconds(subtitleDisplayTime);
            HideSubtitle();
        }

        #endregion

        #region Interaction System

        public void ShowInteractionPrompt(string promptText)
        {
            if (interactionPrompt == null || interactionText == null) return;

            interactionPrompt.SetActive(true);
            interactionText.text = $"{promptText} [{interactionKey}]";
        }

        public void HideInteractionPrompt()
        {
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(false);
            }
        }

        #endregion

        #region Stealth System UI

        public void UpdateStealthStatus(bool isHidden)
        {
            if (stealthStatusImage == null) return;

            stealthStatusImage.color = isHidden ? hiddenColor : exposedColor;
            
            // Optional: Add visual feedback
            if (isHidden)
            {
                // Player is hidden - maybe add a subtle glow effect
            }
            else
            {
                // Player is exposed - maybe add warning effects
            }
        }

        #endregion

        #region Information Log System

        public void AddToInformationLog(string information)
        {
            informationLog += $"• {information}\n";
            
            if (informationLogText != null)
            {
                informationLogText.text = informationLog;
            }

            // Auto-scroll to bottom
            if (informationScrollRect != null)
            {
                Canvas.ForceUpdateCanvases();
                informationScrollRect.verticalNormalizedPosition = 0f;
            }

            Debug.Log($"Added to information log: {information}");
        }

        public void ShowInformationPanel()
        {
            if (informationPanel != null)
            {
                informationPanel.SetActive(true);
            }
        }

        public void HideInformationPanel()
        {
            if (informationPanel != null)
            {
                informationPanel.SetActive(false);
            }
        }

        public void ClearInformationLog()
        {
            informationLog = "";
            if (informationLogText != null)
            {
                informationLogText.text = "";
            }
        }

        #endregion

        #region QTE System

        public void ShowQTEPrompt(string promptText, KeyCode requiredKey, System.Action<bool> onComplete)
        {
            if (qtePanel == null) return;

            qtePanel.SetActive(true);
            qtePromptText.text = $"{promptText} - Press [{requiredKey}]!";
            
            if (qteCoroutine != null)
            {
                StopCoroutine(qteCoroutine);
            }
            
            qteCoroutine = StartCoroutine(QTETimer(requiredKey, onComplete));
        }

        private IEnumerator QTETimer(KeyCode requiredKey, System.Action<bool> onComplete)
        {
            float timeRemaining = qteTimeLimit;
            bool qteSuccess = false;

            while (timeRemaining > 0 && !qteSuccess)
            {
                if (Input.GetKeyDown(requiredKey))
                {
                    qteSuccess = true;
                    break;
                }

                timeRemaining -= Time.deltaTime;
                
                // Update progress bar
                if (qteProgressBar != null)
                {
                    qteProgressBar.fillAmount = timeRemaining / qteTimeLimit;
                }

                yield return null;
            }

            HideQTEPrompt();
            onComplete?.Invoke(qteSuccess);
        }

        public void HideQTEPrompt()
        {
            if (qtePanel != null)
            {
                qtePanel.SetActive(false);
            }
        }

        #endregion

        #region Loading Screen

        public void ShowLoadingScreen()
        {
            if (loadingScreen == null) return;
            
            // Stop any previous fade coroutine
            if (loadingFadeCoroutine != null)
            {
                StopCoroutine(loadingFadeCoroutine);
            }
            
            // Reset loading bar
            if (loadingBar != null)
            {
                loadingBar.value = 0f;
            }
            
            // Set loading text
            if (loadingText != null)
            {
                loadingText.text = "正在加载...";
            }
            
            // Set random background if available
            if (loadingBackgroundImage != null && loadingBackgrounds != null && loadingBackgrounds.Length > 0)
            {
                int randomIndex = Random.Range(0, loadingBackgrounds.Length);
                loadingBackgroundImage.sprite = loadingBackgrounds[randomIndex];
            }
            
            // Show loading screen
            loadingScreen.SetActive(true);
            
            // Fade in loading screen
            loadingFadeCoroutine = StartCoroutine(FadeLoadingScreen(true));
        }

        public void HideLoadingScreen()
        {
            if (loadingScreen == null) return;
            
            // Stop any previous fade coroutine
            if (loadingFadeCoroutine != null)
            {
                StopCoroutine(loadingFadeCoroutine);
            }
            
            // Fade out loading screen
            loadingFadeCoroutine = StartCoroutine(FadeLoadingScreen(false));
        }

        public void UpdateLoadingProgress(float progress)
        {
            if (loadingBar == null) return;
            
            // Update loading bar
            loadingBar.value = progress;
            
            // Update loading text if needed
            if (loadingText != null)
            {
                int percent = Mathf.RoundToInt(progress * 100);
                loadingText.text = $"正在加载... {percent}%";
            }
        }

        private IEnumerator FadeLoadingScreen(bool fadeIn)
        {
            CanvasGroup canvasGroup = loadingScreen.GetComponent<CanvasGroup>();
            
            // Add CanvasGroup if it doesn't exist
            if (canvasGroup == null)
            {
                canvasGroup = loadingScreen.AddComponent<CanvasGroup>();
            }
            
            float startAlpha = fadeIn ? 0f : 1f;
            float targetAlpha = fadeIn ? 1f : 0f;
            float duration = 0.5f;
            float elapsedTime = 0f;
            
            canvasGroup.alpha = startAlpha;
            
            while (elapsedTime < duration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / duration);
                yield return null;
            }
            
            canvasGroup.alpha = targetAlpha;
            
            // Hide loading screen when fade out is complete
            if (!fadeIn)
            {
                loadingScreen.SetActive(false);
            }
        }

        #endregion

        #region Scene Management Events

        private void OnBeforeSceneLoad(string sceneName)
        {
            // Hide all UI except for loading screen
            HideAllPanels();
            
            // Show loading screen
            ShowLoadingScreen();
        }

        private void OnAfterSceneLoad(string sceneName)
        {
            // Hide loading screen
            HideLoadingScreen();
            
            // Find and register scene-specific UI
            FindSceneSpecificUI();
        }

        private void FindSceneSpecificUI()
        {
            // Find UI elements tagged with "LevelUI"
            GameObject levelUI = GameObject.FindGameObjectWithTag("LevelUI");
            if (levelUI != null)
            {
                Debug.Log($"Found level-specific UI: {levelUI.name}");
                
                // Could integrate level-specific UI here
                // For example, connect to objective display, etc.
            }
        }

        #endregion

        #region Game State UI

        private void OnGameStateChanged(GameManager.GameState newState)
        {
            switch (newState)
            {
                case GameManager.GameState.Loading:
                    // Show loading screen (handled by scene load events)
                    break;
                case GameManager.GameState.GameOver:
                    ShowGameOverPanel();
                    break;
                case GameManager.GameState.Victory:
                    ShowVictoryPanel();
                    break;
                case GameManager.GameState.Playing:
                    HideAllMenus();
                    break;
            }
        }

        private void ShowPauseMenu()
        {
            if (pauseMenu != null)
            {
                pauseMenu.SetActive(true);
            }
        }

        private void HidePauseMenu()
        {
            if (pauseMenu != null)
            {
                pauseMenu.SetActive(false);
            }
        }

        private void ShowGameOverPanel()
        {
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);
            }
        }

        private void ShowVictoryPanel()
        {
            if (victoryPanel != null)
            {
                victoryPanel.SetActive(true);
            }
        }

        private void HideAllMenus()
        {
            if (pauseMenu != null) pauseMenu.SetActive(false);
            if (gameOverPanel != null) gameOverPanel.SetActive(false);
            if (victoryPanel != null) victoryPanel.SetActive(false);
        }

        #endregion

        #region Public UI Button Methods

        public void OnResumeButtonClicked()
        {
            GameManager.Instance?.ResumeGame();
        }

        public void OnRestartButtonClicked()
        {
            GameManager.Instance?.RestartCurrentLevel();
        }

        public void OnMainMenuButtonClicked()
        {
            GameManager.Instance?.ReturnToMainMenu();
        }

        public void OnQuitButtonClicked()
        {
            Application.Quit();
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #endif
        }
        
        public void OnCloseInformationButtonClicked()
        {
            HideInformationPanel();
        }

        #endregion
    }
}