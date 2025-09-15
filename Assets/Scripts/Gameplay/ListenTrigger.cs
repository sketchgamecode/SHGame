using UnityEngine;
using System.Collections;
using SHGame.Core;
using SHGame.Characters.Player;

namespace SHGame.Gameplay
{
    /// <summary>
    /// Trigger zone for listening to conversations and gathering intelligence
    /// Player must remain still and hidden to successfully listen
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class ListenTrigger : MonoBehaviour
    {
        [Header("Listening Settings")]
        public string triggerName = "Listening Point";
        public bool requiresStealth = true;
        public bool requiresStillness = true;
        public float listeningTime = 5f;
        public float movementThreshold = 0.1f;

        [Header("Dialogue Lines")]
        [TextArea(3, 5)]
        public string[] dialogueLines;
        public float timeBetweenLines = 2f;
        public bool playRandomly = false;

        [Header("Information Gathered")]
        [TextArea(2, 3)]
        public string[] informationToAdd;
        public bool addInformationOnComplete = true;

        [Header("Audio")]
        public AudioClip[] voiceClips;
        public bool playAudioWithDialogue = true;
        public float audioFadeDistance = 3f;

        [Header("Visual Feedback")]
        public GameObject listeningIndicator;
        public Color listeningColor = Color.yellow;
        public Color completeColor = Color.green;

        // State
        private bool playerInTrigger = false;
        private bool isListening = false;
        private bool hasCompleted = false;
        private float listeningProgress = 0f;
        private int currentDialogueIndex = 0;
        
        // Components and references
        private PlayerController playerController;
        private PlayerStealth playerStealth;
        private Coroutine listeningCoroutine;
        private SpriteRenderer indicatorRenderer;

        private void Awake()
        {
            // Ensure collider is set as trigger
            var col = GetComponent<Collider2D>();
            col.isTrigger = true;

            // Set up indicator
            if (listeningIndicator != null)
            {
                indicatorRenderer = listeningIndicator.GetComponent<SpriteRenderer>();
                listeningIndicator.SetActive(false);
            }
        }

        private void Update()
        {
            if (playerInTrigger && !hasCompleted)
            {
                CheckListeningConditions();
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                playerInTrigger = true;
                playerController = other.GetComponent<PlayerController>();
                playerStealth = other.GetComponent<PlayerStealth>();

                if (listeningIndicator != null)
                {
                    listeningIndicator.SetActive(true);
                }

                Debug.Log($"Player entered listening trigger: {triggerName}");
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                playerInTrigger = false;
                StopListening();

                if (listeningIndicator != null)
                {
                    listeningIndicator.SetActive(false);
                }

                Debug.Log($"Player exited listening trigger: {triggerName}");
            }
        }

        private void CheckListeningConditions()
        {
            bool canListen = true;

            // Check stealth requirement
            if (requiresStealth && playerStealth != null)
            {
                canListen = playerStealth.IsPlayerHidden();
            }

            // Check stillness requirement
            if (requiresStillness && playerController != null)
            {
                Vector2 velocity = playerController.GetPlayerVelocity();
                canListen = canListen && velocity.magnitude < movementThreshold;
            }

            if (canListen && !isListening)
            {
                StartListening();
            }
            else if (!canListen && isListening)
            {
                StopListening();
            }
        }

        private void StartListening()
        {
            if (hasCompleted || isListening) return;

            isListening = true;
            listeningProgress = 0f;
            currentDialogueIndex = 0;

            // Update visual indicator
            UpdateIndicatorColor();

            // Start listening coroutine
            listeningCoroutine = StartCoroutine(ListeningSequence());

            Debug.Log($"Started listening at: {triggerName}");
        }

        private void StopListening()
        {
            if (!isListening) return;

            isListening = false;

            if (listeningCoroutine != null)
            {
                StopCoroutine(listeningCoroutine);
                listeningCoroutine = null;
            }

            // Hide any current dialogue
            if (UIManager.Instance != null)
            {
                UIManager.Instance.HideSubtitle();
            }

            // Update visual indicator
            UpdateIndicatorColor();

            Debug.Log($"Stopped listening at: {triggerName}");
        }

        private IEnumerator ListeningSequence()
        {
            float elapsed = 0f;
            float nextDialogueTime = 0f;

            while (elapsed < listeningTime && isListening && !hasCompleted)
            {
                elapsed += Time.deltaTime;
                listeningProgress = elapsed / listeningTime;

                // Show dialogue lines at intervals
                if (elapsed >= nextDialogueTime && currentDialogueIndex < dialogueLines.Length)
                {
                    ShowCurrentDialogue();
                    
                    currentDialogueIndex++;
                    nextDialogueTime = elapsed + timeBetweenLines;
                }

                // Update visual feedback
                UpdateIndicatorColor();

                yield return null;
            }

            if (isListening && elapsed >= listeningTime)
            {
                CompleteListening();
            }
        }

        private void ShowCurrentDialogue()
        {
            if (currentDialogueIndex >= dialogueLines.Length) return;

            string dialogue = dialogueLines[currentDialogueIndex];
            
            // Show subtitle
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowSubtitle(dialogue);
            }

            // Play voice clip if available
            if (playAudioWithDialogue && voiceClips != null && currentDialogueIndex < voiceClips.Length)
            {
                PlayVoiceClip(voiceClips[currentDialogueIndex]);
            }

            Debug.Log($"Listening dialogue: {dialogue}");
        }

        private void PlayVoiceClip(AudioClip clip)
        {
            if (clip == null || AudioManager.Instance == null) return;

            // Calculate volume based on distance (for immersion)
            float distance = Vector2.Distance(transform.position, playerController.transform.position);
            float volume = Mathf.Clamp01(1f - (distance / audioFadeDistance));

            AudioManager.Instance.PlaySFX(clip, volume);
        }

        private void CompleteListening()
        {
            hasCompleted = true;
            isListening = false;
            listeningProgress = 1f;

            // Add information to player's log
            if (addInformationOnComplete && informationToAdd != null)
            {
                foreach (string info in informationToAdd)
                {
                    if (UIManager.Instance != null)
                    {
                        UIManager.Instance.AddToInformationLog(info);
                    }
                }
            }

            // Update visual indicator
            UpdateIndicatorColor();

            // Show completion message
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowSubtitle($"[完整听取了对话内容]");
            }

            Debug.Log($"Completed listening at: {triggerName}");

            // Trigger any completion events
            OnListeningComplete();
        }

        private void UpdateIndicatorColor()
        {
            if (indicatorRenderer == null) return;

            if (hasCompleted)
            {
                indicatorRenderer.color = completeColor;
            }
            else if (isListening)
            {
                // Interpolate color based on progress
                Color progressColor = Color.Lerp(listeningColor, completeColor, listeningProgress);
                indicatorRenderer.color = progressColor;
            }
            else
            {
                indicatorRenderer.color = listeningColor;
            }
        }

        protected virtual void OnListeningComplete()
        {
            // Override in derived classes for specific completion behavior
            // Could trigger next story sequence, unlock doors, etc.
        }

        #region Public Methods

        public void SetListeningEnabled(bool enabled)
        {
            this.enabled = enabled;
            
            if (!enabled)
            {
                StopListening();
            }
        }

        public bool HasPlayerCompleted()
        {
            return hasCompleted;
        }

        public float GetListeningProgress()
        {
            return listeningProgress;
        }

        public bool IsPlayerListening()
        {
            return isListening;
        }

        public void ResetListeningTrigger()
        {
            hasCompleted = false;
            isListening = false;
            listeningProgress = 0f;
            currentDialogueIndex = 0;
            
            if (listeningCoroutine != null)
            {
                StopCoroutine(listeningCoroutine);
                listeningCoroutine = null;
            }
            
            UpdateIndicatorColor();
        }

        /// <summary>
        /// Manually add a dialogue line (for dynamic conversations)
        /// </summary>
        public void AddDialogueLine(string dialogue)
        {
            if (dialogueLines == null)
            {
                dialogueLines = new string[] { dialogue };
            }
            else
            {
                var newLines = new string[dialogueLines.Length + 1];
                dialogueLines.CopyTo(newLines, 0);
                newLines[dialogueLines.Length] = dialogue;
                dialogueLines = newLines;
            }
        }

        #endregion

        #region Debug

        private void OnDrawGizmos()
        {
            // Draw trigger bounds
            Gizmos.color = hasCompleted ? completeColor : listeningColor;
            
            var col = GetComponent<Collider2D>();
            if (col != null)
            {
                Gizmos.DrawWireCube(transform.position, col.bounds.size);
            }

            // Draw audio fade range
            if (audioFadeDistance > 0)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(transform.position, audioFadeDistance);
            }
        }

        private void OnDrawGizmosSelected()
        {
            // Draw detailed information
            #if UNITY_EDITOR
            string info = $"{triggerName}\n";
            info += $"Completed: {hasCompleted}\n";
            info += $"Listening: {isListening}\n";
            info += $"Progress: {listeningProgress:F2}";
            
            UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, info);
            
            // Draw dialogue count
            if (dialogueLines != null)
            {
                UnityEditor.Handles.Label(transform.position + Vector3.down * 1f, 
                    $"Dialogue Lines: {dialogueLines.Length}");
            }
            #endif
        }

        #endregion
    }
}