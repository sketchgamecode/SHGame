using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using SHGame.Core;

namespace SHGame.UI
{
    /// <summary>
    /// Manages the main menu UI elements and interactions
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [Header("Menu Panels")]
        public GameObject mainPanel;
        public GameObject optionsPanel;
        public GameObject creditsPanel;
        public GameObject controlsPanel;
        
        [Header("UI Elements")]
        public Button startGameButton;
        public Button optionsButton;
        public Button creditsButton;
        public Button controlsButton;
        public Button quitButton;
        public Button backButton;
        
        [Header("Options")]
        public Slider masterVolumeSlider;
        public Slider bgmVolumeSlider;
        public Slider sfxVolumeSlider;
        public Slider ambientVolumeSlider;
        public Slider voiceVolumeSlider;
        
        [Header("Transitions")]
        public float transitionDuration = 0.5f;
        public CanvasGroup fadeCanvasGroup;
        
        [Header("Animation")]
        public bool animateMenuItems = true;
        public float itemAnimationDelay = 0.1f;
        public float itemAnimationDuration = 0.5f;
        
        [Header("Background")]
        public Image backgroundImage;
        public float backgroundParallaxAmount = 0.1f;
        public bool useBackgroundParallax = true;
        
        // Private state
        private GameObject currentPanel;
        private Coroutine fadeCoroutine;
        private Vector3 backgroundStartPosition;

        private void Awake()
        {
            // Initialize menu
            if (mainPanel != null)
            {
                currentPanel = mainPanel;
            }
            
            // Set up button listeners
            SetupButtonListeners();
            
            // Initialize fade
            if (fadeCanvasGroup != null)
            {
                fadeCanvasGroup.alpha = 1f;
            }
            
            // Save background start position
            if (backgroundImage != null)
            {
                backgroundStartPosition = backgroundImage.rectTransform.position;
            }
        }

        private void Start()
        {
            // Hide all panels except main panel
            HideAllPanels();
            if (currentPanel != null)
            {
                currentPanel.SetActive(true);
            }
            
            // Initialize sliders
            InitializeVolumeSliders();
            
            // Fade in
            StartCoroutine(FadeIn());
            
            // Animate menu items
            if (animateMenuItems)
            {
                AnimateMenuItems(mainPanel);
            }
            
            // Play menu music
            if (AudioManager.Instance != null)
            {
                // Already handled by GameManager's state change or AudioManager's scene specific settings
            }
        }
        
        private void Update()
        {
            // Handle background parallax
            if (useBackgroundParallax && backgroundImage != null)
            {
                UpdateBackgroundParallax();
            }
            
            // Handle back button
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (currentPanel != mainPanel && backButton != null)
                {
                    OnBackButtonClicked();
                }
            }
        }
        
        private void SetupButtonListeners()
        {
            // Main menu buttons
            if (startGameButton != null)
                startGameButton.onClick.AddListener(OnStartGameButtonClicked);
                
            if (optionsButton != null)
                optionsButton.onClick.AddListener(OnOptionsButtonClicked);
                
            if (creditsButton != null)
                creditsButton.onClick.AddListener(OnCreditsButtonClicked);
                
            if (controlsButton != null)
                controlsButton.onClick.AddListener(OnControlsButtonClicked);
                
            if (quitButton != null)
                quitButton.onClick.AddListener(OnQuitButtonClicked);
                
            if (backButton != null)
                backButton.onClick.AddListener(OnBackButtonClicked);
                
            // Volume sliders
            if (masterVolumeSlider != null)
                masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
                
            if (bgmVolumeSlider != null)
                bgmVolumeSlider.onValueChanged.AddListener(OnBGMVolumeChanged);
                
            if (sfxVolumeSlider != null)
                sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
                
            if (ambientVolumeSlider != null)
                ambientVolumeSlider.onValueChanged.AddListener(OnAmbientVolumeChanged);
                
            if (voiceVolumeSlider != null)
                voiceVolumeSlider.onValueChanged.AddListener(OnVoiceVolumeChanged);
        }
        
        private void InitializeVolumeSliders()
        {
            if (AudioManager.Instance == null) return;
            
            // Set slider values from AudioManager
            if (masterVolumeSlider != null)
                masterVolumeSlider.value = AudioManager.Instance.masterVolume;
                
            if (bgmVolumeSlider != null)
                bgmVolumeSlider.value = AudioManager.Instance.bgmVolume;
                
            if (sfxVolumeSlider != null)
                sfxVolumeSlider.value = AudioManager.Instance.sfxVolume;
                
            if (ambientVolumeSlider != null)
                ambientVolumeSlider.value = AudioManager.Instance.ambientVolume;
                
            if (voiceVolumeSlider != null)
                voiceVolumeSlider.value = AudioManager.Instance.voiceVolume;
        }
        
        private void HideAllPanels()
        {
            if (mainPanel != null) mainPanel.SetActive(false);
            if (optionsPanel != null) optionsPanel.SetActive(false);
            if (creditsPanel != null) creditsPanel.SetActive(false);
            if (controlsPanel != null) controlsPanel.SetActive(false);
        }
        
        private IEnumerator FadeIn()
        {
            if (fadeCanvasGroup == null) yield break;
            
            fadeCanvasGroup.alpha = 1f;
            
            float elapsed = 0f;
            while (elapsed < transitionDuration)
            {
                elapsed += Time.deltaTime;
                fadeCanvasGroup.alpha = 1f - (elapsed / transitionDuration);
                yield return null;
            }
            
            fadeCanvasGroup.alpha = 0f;
        }
        
        private IEnumerator FadeOut(System.Action onComplete = null)
        {
            if (fadeCanvasGroup == null)
            {
                onComplete?.Invoke();
                yield break;
            }
            
            fadeCanvasGroup.alpha = 0f;
            
            float elapsed = 0f;
            while (elapsed < transitionDuration)
            {
                elapsed += Time.deltaTime;
                fadeCanvasGroup.alpha = elapsed / transitionDuration;
                yield return null;
            }
            
            fadeCanvasGroup.alpha = 1f;
            
            onComplete?.Invoke();
        }
        
        private void SwitchPanel(GameObject newPanel)
        {
            if (newPanel == null || newPanel == currentPanel) return;
            
            // Hide current panel
            if (currentPanel != null)
            {
                currentPanel.SetActive(false);
            }
            
            // Show new panel
            newPanel.SetActive(true);
            currentPanel = newPanel;
            
            // Animate items
            if (animateMenuItems)
            {
                AnimateMenuItems(newPanel);
            }
            
            // Play sound
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayButtonClick();
            }
        }
        
        private void AnimateMenuItems(GameObject panel)
        {
            if (panel == null) return;
            
            // Find all buttons, texts, and sliders
            Component[] items = panel.GetComponentsInChildren<RectTransform>();
            
            // Start all items off-screen
            foreach (var item in items)
            {
                // Skip the panel itself
                if (item.transform == panel.transform) continue;
                
                // Skip items that aren't direct children of the panel
                if (item.transform.parent != panel.transform) continue;
                
                // Set initial position
                RectTransform rt = item as RectTransform;
                if (rt != null)
                {
                    rt.localScale = Vector3.zero;
                    StartCoroutine(AnimateItem(rt, items.Length));
                }
            }
        }
        
        private IEnumerator AnimateItem(RectTransform item, int totalItems)
        {
            // Random delay based on position in hierarchy
            int siblingIndex = item.GetSiblingIndex();
            float delay = siblingIndex * itemAnimationDelay;
            
            yield return new WaitForSeconds(delay);
            
            // Animate scale
            float elapsed = 0f;
            while (elapsed < itemAnimationDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / itemAnimationDuration;
                
                // Ease in/out
                t = Mathf.SmoothStep(0f, 1f, t);
                
                item.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, t);
                yield return null;
            }
            
            item.localScale = Vector3.one;
        }
        
        private void UpdateBackgroundParallax()
        {
            // Get mouse position in screen space (0-1)
            Vector2 mousePos = new Vector2(
                Input.mousePosition.x / Screen.width,
                Input.mousePosition.y / Screen.height);
                
            // Center around 0.5
            mousePos -= new Vector2(0.5f, 0.5f);
            
            // Apply parallax
            Vector3 newPos = backgroundStartPosition + new Vector3(
                mousePos.x * backgroundParallaxAmount,
                mousePos.y * backgroundParallaxAmount,
                0f);
                
            // Apply to background
            backgroundImage.rectTransform.position = Vector3.Lerp(
                backgroundImage.rectTransform.position,
                newPos,
                Time.deltaTime * 2f);
        }
        
        #region Button Handlers
        
        public void OnStartGameButtonClicked()
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayButtonClick();
            }
            
            // Start fade out and then load game
            if (fadeCanvasGroup != null)
            {
                StartCoroutine(FadeOut(() => {
                    if (GameManager.Instance != null)
                    {
                        GameManager.Instance.StartGame();
                    }
                }));
            }
            else
            {
                // No fade, just start game
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.StartGame();
                }
            }
        }
        
        public void OnOptionsButtonClicked()
        {
            SwitchPanel(optionsPanel);
        }
        
        public void OnCreditsButtonClicked()
        {
            SwitchPanel(creditsPanel);
        }
        
        public void OnControlsButtonClicked()
        {
            SwitchPanel(controlsPanel);
        }
        
        public void OnQuitButtonClicked()
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayButtonClick();
            }
            
            // Fade out and quit
            if (fadeCanvasGroup != null)
            {
                StartCoroutine(FadeOut(() => {
                    Application.Quit();
                    #if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
                    #endif
                }));
            }
            else
            {
                // No fade, just quit
                Application.Quit();
                #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
                #endif
            }
        }
        
        public void OnBackButtonClicked()
        {
            SwitchPanel(mainPanel);
        }
        
        #endregion
        
        #region Volume Slider Handlers
        
        public void OnMasterVolumeChanged(float value)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetMasterVolume(value);
            }
        }
        
        public void OnBGMVolumeChanged(float value)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetBGMVolume(value);
            }
        }
        
        public void OnSFXVolumeChanged(float value)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetSFXVolume(value);
                
                // Play a test sound when adjusting
                AudioManager.Instance.PlayButtonClick();
            }
        }
        
        public void OnAmbientVolumeChanged(float value)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetAmbientVolume(value);
            }
        }
        
        public void OnVoiceVolumeChanged(float value)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetVoiceVolume(value);
            }
        }
        
        #endregion
    }
}