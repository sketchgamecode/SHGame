using UnityEngine;
using System;
using SHGame.Core;

namespace SHGame.Interaction
{
    /// <summary>
    /// Base class for all interactable objects
    /// Provides common functionality and events for interactions
    /// </summary>
    public abstract class InteractableBase : MonoBehaviour, IInteractable
    {
        [Header("Interaction Settings")]
        public InteractionType interactionType = InteractionType.Generic;
        public string interactionPrompt = "Interact";
        public bool isInteractable = true;
        public bool requiresStealth = false;
        public bool consumeOnUse = false;

        [Header("Audio")]
        public AudioClip interactionSound;
        public bool playDefaultSound = true;

        [Header("Visual Feedback")]
        public GameObject highlightEffect;
        public bool showHighlightOnHover = true;

        // Events
        public static event Action<InteractableBase> OnInteractionStarted;
        public static event Action<InteractableBase> OnInteractionCompleted;

        // State
        protected bool playerInRange = false;
        protected bool hasBeenUsed = false;

        protected virtual void Start()
        {
            // Initialize highlight effect
            if (highlightEffect != null)
            {
                highlightEffect.SetActive(false);
            }
        }

        #region IInteractable Implementation

        public virtual void OnPlayerEnterRange()
        {
            playerInRange = true;
            
            if (showHighlightOnHover && highlightEffect != null && CanInteract())
            {
                highlightEffect.SetActive(true);
            }

            OnPlayerEnterRangeInternal();
        }

        public virtual void OnPlayerExitRange()
        {
            playerInRange = false;
            
            if (highlightEffect != null)
            {
                highlightEffect.SetActive(false);
            }

            OnPlayerExitRangeInternal();
        }

        public virtual void Interact()
        {
            if (!CanInteract()) return;

            OnInteractionStarted?.Invoke(this);

            // Play interaction sound
            PlayInteractionSound();

            // Perform the actual interaction
            PerformInteraction();

            // Mark as used if consumable
            if (consumeOnUse)
            {
                hasBeenUsed = true;
                isInteractable = false;
            }

            OnInteractionCompleted?.Invoke(this);
        }

        public virtual bool CanInteract()
        {
            if (!isInteractable || hasBeenUsed) return false;

            // Check stealth requirement
            if (requiresStealth)
            {
                // Find player stealth component
                var playerStealth = FindObjectOfType<Characters.Player.PlayerStealth>();
                if (playerStealth != null && !playerStealth.IsPlayerHidden())
                {
                    return false;
                }
            }

            return CanInteractInternal();
        }

        public virtual string GetInteractionPrompt()
        {
            return interactionPrompt;
        }

        public virtual InteractionType GetInteractionType()
        {
            return interactionType;
        }

        #endregion

        #region Abstract and Virtual Methods

        /// <summary>
        /// Override this method to implement specific interaction logic
        /// </summary>
        protected abstract void PerformInteraction();

        /// <summary>
        /// Override this method for additional interaction conditions
        /// </summary>
        /// <returns>True if interaction is allowed</returns>
        protected virtual bool CanInteractInternal()
        {
            return true;
        }

        /// <summary>
        /// Called when player enters range - override for custom behavior
        /// </summary>
        protected virtual void OnPlayerEnterRangeInternal() { }

        /// <summary>
        /// Called when player exits range - override for custom behavior
        /// </summary>
        protected virtual void OnPlayerExitRangeInternal() { }

        #endregion

        #region Utility Methods

        protected virtual void PlayInteractionSound()
        {
            if (AudioManager.Instance == null) return;

            if (interactionSound != null)
            {
                AudioManager.Instance.PlaySFX(interactionSound);
            }
            else if (playDefaultSound)
            {
                // Play default interaction sound based on type
                PlayDefaultSoundForType();
            }
        }

        private void PlayDefaultSoundForType()
        {
            switch (interactionType)
            {
                case InteractionType.Door:
                    AudioManager.Instance.PlayDoorCreak();
                    break;
                case InteractionType.Light:
                    AudioManager.Instance.PlayLightExtinguish();
                    break;
                case InteractionType.Draggable:
                    AudioManager.Instance.PlayBodyDrag();
                    break;
                default:
                    // Generic interaction sound
                    break;
            }
        }

        /// <summary>
        /// Add information to the player's log
        /// </summary>
        /// <param name="information">Information to add</param>
        protected void AddInformation(string information)
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.AddToInformationLog(information);
            }
        }

        /// <summary>
        /// Show subtitle text
        /// </summary>
        /// <param name="text">Text to display</param>
        protected void ShowSubtitle(string text)
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowSubtitle(text);
            }
        }

        #endregion

        #region Public Utility Methods

        public void SetInteractionEnabled(bool enabled)
        {
            isInteractable = enabled;
        }

        public void SetInteractionPrompt(string prompt)
        {
            interactionPrompt = prompt;
        }

        public bool IsPlayerInRange()
        {
            return playerInRange;
        }

        public bool HasBeenUsed()
        {
            return hasBeenUsed;
        }

        public void ResetInteractable()
        {
            hasBeenUsed = false;
            isInteractable = true;
        }

        #endregion

        #region Debug

        protected virtual void OnDrawGizmos()
        {
            // Draw interaction indicator
            Gizmos.color = isInteractable ? Color.green : Color.gray;
            Gizmos.DrawWireSphere(transform.position, 0.5f);

            // Draw interaction type icon
            Vector3 iconPosition = transform.position + Vector3.up * 1f;
            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(iconPosition, Vector3.one * 0.2f);
        }

        protected virtual void OnDrawGizmosSelected()
        {
            // Draw detailed interaction info
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 1f);
            
            // Show interaction prompt in scene view
            #if UNITY_EDITOR
            UnityEditor.Handles.Label(transform.position + Vector3.up * 1.5f, 
                $"{interactionType}: {interactionPrompt}");
            #endif
        }

        #endregion
    }
}