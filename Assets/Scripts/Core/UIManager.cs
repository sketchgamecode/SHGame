using UnityEngine;
using UnityEngine.UI;
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

        private string informationLog = "";
        private Coroutine subtitleCoroutine;
        private Coroutine qteCoroutine;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                InitializeUI();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            // Subscribe to game manager events
            GameManager.OnGameStateChanged += OnGameStateChanged;
            GameManager.OnGamePaused += ShowPauseMenu;
            GameManager.OnGameResumed += HidePauseMenu;
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            GameManager.OnGameStateChanged -= OnGameStateChanged;
            GameManager.OnGamePaused -= ShowPauseMenu;
            GameManager.OnGameResumed -= HidePauseMenu;
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

        #region Game State UI

        private void OnGameStateChanged(GameManager.GameState newState)
        {
            switch (newState)
            {
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
        }

        #endregion
    }
}