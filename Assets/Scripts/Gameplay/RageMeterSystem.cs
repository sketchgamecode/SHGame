using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using SHGame.Core;

namespace SHGame.Gameplay
{
    /// <summary>
    /// Rage meter system that builds up as the player collects evidence
    /// Used in Level 3 (YuanYang Building) to track player's anger level
    /// </summary>
    public class RageMeterSystem : MonoBehaviour
    {
        [Header("UI References")]
        public Slider rageMeter;
        public Image fillImage;
        public TextMeshProUGUI rageText;
        public GameObject ragePanel;
        public GameObject revengeButton;
        public TextMeshProUGUI revengeButtonText;
        public TextMeshProUGUI evidenceCountText;
        
        [Header("Visual Effects")]
        public Color[] rageColors;
        public AnimationCurve pulseIntensityCurve;
        public float pulseDuration = 0.5f;
        public ParticleSystem rageParticles;
        public CanvasGroup screenVignetteEffect;
        
        [Header("Rage Settings")]
        public int requiredEvidenceForRevenge = 5;
        public float baseRagePerEvidence = 0.2f;
        public float rageDecayRate = 0.01f;
        public string[] rageTextsByLevel;
        
        [Header("Audio")]
        public AudioClip[] evidenceCollectedSounds;
        public AudioClip rageMilestoneSounds;
        public AudioClip revengeReadySound;
        public AudioClip heartbeatSound;
        
        // State
        private float currentRage = 0f;
        private int collectedEvidenceCount = 0;
        private bool revengeReady = false;
        private List<string> collectedEvidence = new List<string>();
        private Coroutine pulseCoroutine;
        private Coroutine heartbeatCoroutine;
        
        private void Awake()
        {
            // Initialize UI
            if (ragePanel != null)
                ragePanel.SetActive(true);
                
            if (revengeButton != null)
                revengeButton.SetActive(false);
                
            if (rageMeter != null)
                rageMeter.value = 0f;
                
            // Register button listener
            if (revengeButton != null && revengeButton.GetComponent<Button>() != null)
                revengeButton.GetComponent<Button>().onClick.AddListener(ExecuteRevenge);
        }
        
        private void Start()
        {
            UpdateUI();
            
            // Listen for information gathering events
            Utilities.GameEvents.OnInformationGathered += OnInformationGathered;
        }
        
        private void OnDestroy()
        {
            // Unsubscribe from events
            Utilities.GameEvents.OnInformationGathered -= OnInformationGathered;
        }
        
        private void Update()
        {
            // Apply rage decay if not at max
            if (currentRage > 0 && !revengeReady)
            {
                currentRage -= rageDecayRate * Time.deltaTime;
                currentRage = Mathf.Max(0f, currentRage);
                UpdateUI();
            }
        }
        
        private void OnInformationGathered(string information)
        {
            // Check if this is new evidence
            if (IsNewEvidence(information))
            {
                AddEvidence(information);
            }
        }
        
        private bool IsNewEvidence(string information)
        {
            // Perform basic check if this exact string has been collected
            if (collectedEvidence.Contains(information))
                return false;
                
            // More sophisticated checks could be added here
            // For example, checking for keywords or similar evidence
            
            return true;
        }
        
        private void AddEvidence(string evidence)
        {
            // Add to list
            collectedEvidence.Add(evidence);
            collectedEvidenceCount++;
            
            // Increase rage
            float rageIncrease = baseRagePerEvidence;
            
            // Apply multiplier for later evidence
            float multiplier = 1f + (collectedEvidenceCount * 0.1f);
            rageIncrease *= multiplier;
            
            AddRage(rageIncrease);
            
            // Log to console
            Debug.Log($"Evidence collected: {evidence} (Total: {collectedEvidenceCount}, Rage: {currentRage:F2})");
            
            // Play feedback
            PlayEvidenceCollectedFeedback();
            
            // Check if revenge is ready
            CheckRevengeReady();
        }
        
        private void AddRage(float amount)
        {
            float oldRage = currentRage;
            currentRage += amount;
            currentRage = Mathf.Clamp01(currentRage);
            
            // Check if we crossed a threshold
            CheckRageThresholds(oldRage, currentRage);
            
            // Update UI
            UpdateUI();
            
            // Start pulse effect
            if (pulseCoroutine != null)
                StopCoroutine(pulseCoroutine);
                
            pulseCoroutine = StartCoroutine(PulseEffect());
        }
        
        private void CheckRageThresholds(float oldValue, float newValue)
        {
            // Check if we crossed any 0.25 thresholds
            for (float threshold = 0.25f; threshold <= 1f; threshold += 0.25f)
            {
                if (oldValue < threshold && newValue >= threshold)
                {
                    OnRageThresholdReached(threshold);
                    break;
                }
            }
        }
        
        private void OnRageThresholdReached(float threshold)
        {
            // Play milestone sound
            if (AudioManager.Instance != null && rageMilestoneSounds != null)
            {
                AudioManager.Instance.PlaySFX(rageMilestoneSounds);
            }
            
            // Increase particle emission
            if (rageParticles != null)
            {
                var emission = rageParticles.emission;
                emission.rateOverTime = threshold * 30f; // Scale emission rate with rage
            }
            
            // Increase vignette intensity
            if (screenVignetteEffect != null)
            {
                screenVignetteEffect.alpha = threshold * 0.8f;
            }
            
            // Add feedback through UI manager
            if (UIManager.Instance != null)
            {
                int thresholdIndex = Mathf.FloorToInt(threshold * 4) - 1; // 0.25->0, 0.5->1, etc.
                if (thresholdIndex >= 0 && thresholdIndex < rageTextsByLevel.Length)
                {
                    UIManager.Instance.ShowSubtitle(rageTextsByLevel[thresholdIndex]);
                }
            }
            
            // If it's the max threshold, start heartbeat
            if (threshold >= 0.75f && heartbeatCoroutine == null)
            {
                heartbeatCoroutine = StartCoroutine(HeartbeatSound());
            }
        }
        
        private void CheckRevengeReady()
        {
            if (collectedEvidenceCount >= requiredEvidenceForRevenge && !revengeReady)
            {
                revengeReady = true;
                
                // Show revenge button
                if (revengeButton != null)
                {
                    revengeButton.SetActive(true);
                    
                    // Add animation if needed
                    StartCoroutine(PulseRevengeButton());
                }
                
                // Play revenge ready sound
                if (AudioManager.Instance != null && revengeReadySound != null)
                {
                    AudioManager.Instance.PlaySFX(revengeReadySound);
                }
                
                // Show subtitle
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.ShowSubtitle("怒火中烧！武松已按捺不住，随时准备复仇！");
                }
                
                // Notify level manager
                if (LevelManager.Instance != null)
                {
                    LevelManager.Instance.ForceCompleteObjective("收集所有罪证，激发怒火");
                }
                
                // Max out rage meter
                currentRage = 1f;
                UpdateUI();
                
                // Start heartbeat if not already started
                if (heartbeatCoroutine == null)
                {
                    heartbeatCoroutine = StartCoroutine(HeartbeatSound());
                }
            }
        }
        
        private void UpdateUI()
        {
            // Update rage meter
            if (rageMeter != null)
            {
                rageMeter.value = currentRage;
            }
            
            // Update fill color based on rage level
            if (fillImage != null && rageColors.Length >= 2)
            {
                int colorIndex = Mathf.FloorToInt(currentRage * (rageColors.Length - 1));
                colorIndex = Mathf.Clamp(colorIndex, 0, rageColors.Length - 2);
                
                float t = (currentRage * (rageColors.Length - 1)) - colorIndex;
                fillImage.color = Color.Lerp(rageColors[colorIndex], rageColors[colorIndex + 1], t);
            }
            
            // Update rage text
            if (rageText != null)
            {
                if (revengeReady)
                {
                    rageText.text = "怒火燃烧！";
                    rageText.color = Color.red;
                }
                else
                {
                    rageText.text = GetRageText();
                    
                    // Change color based on rage level
                    rageText.color = Color.Lerp(Color.white, Color.red, currentRage);
                }
            }
            
            // Update evidence count
            if (evidenceCountText != null)
            {
                evidenceCountText.text = $"罪证: {collectedEvidenceCount}/{requiredEvidenceForRevenge}";
            }
        }
        
        private string GetRageText()
        {
            if (currentRage < 0.25f)
                return "心事重重";
            else if (currentRage < 0.5f)
                return "怒火渐起";
            else if (currentRage < 0.75f)
                return "怒火中烧";
            else
                return "燃烧的愤怒";
        }
        
        private void PlayEvidenceCollectedFeedback()
        {
            // Play sound
            if (AudioManager.Instance != null && evidenceCollectedSounds != null && evidenceCollectedSounds.Length > 0)
            {
                int index = Random.Range(0, evidenceCollectedSounds.Length);
                AudioManager.Instance.PlaySFX(evidenceCollectedSounds[index]);
            }
            
            // Show UI feedback (handled by UIManager through GameEvents)
        }
        
        private IEnumerator PulseEffect()
        {
            if (fillImage == null) yield break;
            
            Color originalColor = fillImage.color;
            Color targetColor = Color.white;
            
            float elapsed = 0f;
            while (elapsed < pulseDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / pulseDuration;
                float intensity = pulseIntensityCurve.Evaluate(t);
                
                fillImage.color = Color.Lerp(originalColor, targetColor, intensity);
                
                yield return null;
            }
            
            fillImage.color = originalColor;
        }
        
        private IEnumerator PulseRevengeButton()
        {
            if (revengeButton == null) yield break;
            
            Button button = revengeButton.GetComponent<Button>();
            if (button == null) yield break;
            
            while (revengeReady)
            {
                // Pulse scale
                float time = Time.time;
                float scale = 1f + 0.1f * Mathf.Sin(time * 3f);
                
                revengeButton.transform.localScale = new Vector3(scale, scale, 1f);
                
                // Pulse text color if available
                if (revengeButtonText != null)
                {
                    float colorIntensity = 0.7f + 0.3f * Mathf.Sin(time * 5f);
                    revengeButtonText.color = new Color(1f, colorIntensity * 0.3f, colorIntensity * 0.3f);
                }
                
                yield return null;
            }
            
            // Reset scale when done
            revengeButton.transform.localScale = Vector3.one;
        }
        
        private IEnumerator HeartbeatSound()
        {
            if (AudioManager.Instance == null || heartbeatSound == null) yield break;
            
            while (currentRage >= 0.75f || revengeReady)
            {
                // Calculate heartbeat rate based on rage
                float interval = Mathf.Lerp(1.2f, 0.6f, currentRage);
                
                // Play heartbeat sound
                AudioManager.Instance.PlaySFX(heartbeatSound, Mathf.Lerp(0.8f, 1.2f, currentRage));
                
                // Wait for interval
                yield return new WaitForSeconds(interval);
            }
            
            heartbeatCoroutine = null;
        }
        
        private void ExecuteRevenge()
        {
            if (!revengeReady) return;
            
            // Trigger revenge sequence
            Debug.Log("Revenge sequence triggered!");
            
            // Notify level manager or game manager
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.ForceCompleteObjective("按下复仇按钮");
            }
            
            // Hide revenge button
            if (revengeButton != null)
            {
                revengeButton.SetActive(false);
            }
            
            // Play a dramatic sound
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayMusicForMood(AudioManager.MusicMood.Action);
            }
            
            // Show dramatic subtitle
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowSubtitle("武松再也无法忍受，决定立刻血洗鸳鸯楼！");
            }
            
            // Signal game to transition to final confrontation
            Utilities.GameEvents.TriggerLevelCompleted();
        }
        
        #region Public Interface
        
        public void AddRageEvidence(string evidence, float rageAmount)
        {
            if (IsNewEvidence(evidence))
            {
                collectedEvidence.Add(evidence);
                collectedEvidenceCount++;
                AddRage(rageAmount);
                
                // Play feedback
                PlayEvidenceCollectedFeedback();
                
                // Check if revenge is ready
                CheckRevengeReady();
            }
        }
        
        public float GetCurrentRage()
        {
            return currentRage;
        }
        
        public int GetEvidenceCount()
        {
            return collectedEvidenceCount;
        }
        
        public bool IsRevengeReady()
        {
            return revengeReady;
        }
        
        public void ResetRage()
        {
            currentRage = 0f;
            collectedEvidenceCount = 0;
            collectedEvidence.Clear();
            revengeReady = false;
            
            if (revengeButton != null)
                revengeButton.SetActive(false);
                
            UpdateUI();
            
            // Stop coroutines
            if (pulseCoroutine != null)
            {
                StopCoroutine(pulseCoroutine);
                pulseCoroutine = null;
            }
            
            if (heartbeatCoroutine != null)
            {
                StopCoroutine(heartbeatCoroutine);
                heartbeatCoroutine = null;
            }
            
            // Reset particles
            if (rageParticles != null)
            {
                var emission = rageParticles.emission;
                emission.rateOverTime = 0f;
            }
            
            // Reset vignette
            if (screenVignetteEffect != null)
            {
                screenVignetteEffect.alpha = 0f;
            }
        }
        
        #endregion
    }
}