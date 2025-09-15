using UnityEngine;
using System.Collections.Generic;
using SHGame.Core;
using SHGame.Interaction;

namespace SHGame.Characters.Player
{
    /// <summary>
    /// Handles player interaction with environment objects and NPCs
    /// Detects nearby interactables and manages interaction prompts
    /// </summary>
    public class PlayerInteraction : MonoBehaviour
    {
        [Header("Interaction Settings")]
        public float interactionRange = 2f;
        public LayerMask interactableLayerMask = -1;
        public bool showDebugRays = true;

        [Header("Detection")]
        public Transform interactionPoint;
        public float detectionAngle = 45f;
        public int raycastCount = 5;

        // Current interaction state
        private IInteractable currentInteractable;
        private List<IInteractable> nearbyInteractables = new List<IInteractable>();
        
        // Components
        private PlayerController playerController;

        private void Awake()
        {
            playerController = GetComponent<PlayerController>();
            
            // Create interaction point if not assigned
            if (interactionPoint == null)
            {
                GameObject point = new GameObject("InteractionPoint");
                point.transform.SetParent(transform);
                point.transform.localPosition = Vector3.zero;
                interactionPoint = point.transform;
            }
        }

        private void Update()
        {
            DetectInteractables();
            UpdateInteractionPrompt();
        }

        private void DetectInteractables()
        {
            nearbyInteractables.Clear();
            IInteractable closestInteractable = null;
            float closestDistance = float.MaxValue;

            // Perform multiple raycasts in a cone to detect interactables
            Vector2 playerPosition = interactionPoint.position;
            Vector2 forward = playerController != null && !playerController.transform.localScale.x.Equals(1f) ? Vector2.left : Vector2.right;

            for (int i = 0; i < raycastCount; i++)
            {
                float angle = -detectionAngle / 2f + (detectionAngle / (raycastCount - 1)) * i;
                Vector2 direction = Quaternion.Euler(0, 0, angle) * forward;
                
                RaycastHit2D hit = Physics2D.Raycast(playerPosition, direction, interactionRange, interactableLayerMask);
                
                if (showDebugRays)
                {
                    Debug.DrawRay(playerPosition, direction * interactionRange, hit.collider != null ? Color.green : Color.red);
                }

                if (hit.collider != null)
                {
                    IInteractable interactable = hit.collider.GetComponent<IInteractable>();
                    if (interactable != null && !nearbyInteractables.Contains(interactable))
                    {
                        nearbyInteractables.Add(interactable);
                        
                        float distance = hit.distance;
                        if (distance < closestDistance)
                        {
                            closestDistance = distance;
                            closestInteractable = interactable;
                        }
                    }
                }
            }

            // Also check for trigger-based interactions
            Collider2D[] colliders = Physics2D.OverlapCircleAll(playerPosition, interactionRange, interactableLayerMask);
            foreach (Collider2D col in colliders)
            {
                IInteractable interactable = col.GetComponent<IInteractable>();
                if (interactable != null && !nearbyInteractables.Contains(interactable))
                {
                    nearbyInteractables.Add(interactable);
                    
                    float distance = Vector2.Distance(playerPosition, col.transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestInteractable = interactable;
                    }
                }
            }

            // Update current interactable
            if (currentInteractable != closestInteractable)
            {
                if (currentInteractable != null)
                {
                    currentInteractable.OnPlayerExitRange();
                }

                currentInteractable = closestInteractable;

                if (currentInteractable != null)
                {
                    currentInteractable.OnPlayerEnterRange();
                }
            }
        }

        private void UpdateInteractionPrompt()
        {
            if (UIManager.Instance == null) return;

            if (currentInteractable != null && currentInteractable.CanInteract())
            {
                // Show interaction prompt
                string promptText = currentInteractable.GetInteractionPrompt();
                UIManager.Instance.ShowInteractionPrompt(promptText);
            }
            else
            {
                // Hide interaction prompt
                UIManager.Instance.HideInteractionPrompt();
            }
        }

        public void PerformInteraction()
        {
            if (currentInteractable == null || !currentInteractable.CanInteract())
                return;

            // Play interaction animation if available
            if (playerController != null)
            {
                playerController.PlayInteractionAnimation();
            }

            // Perform the interaction
            currentInteractable.Interact();

            Debug.Log($"Interacted with: {currentInteractable.GetType().Name}");
        }

        #region Public Methods

        public IInteractable GetCurrentInteractable()
        {
            return currentInteractable;
        }

        public List<IInteractable> GetNearbyInteractables()
        {
            return new List<IInteractable>(nearbyInteractables);
        }

        public bool HasInteractableInRange()
        {
            return currentInteractable != null && currentInteractable.CanInteract();
        }

        public void SetInteractionEnabled(bool enabled)
        {
            this.enabled = enabled;
            
            if (!enabled && UIManager.Instance != null)
            {
                UIManager.Instance.HideInteractionPrompt();
            }
        }

        #endregion

        #region Gizmos

        private void OnDrawGizmos()
        {
            if (interactionPoint == null) return;

            // Draw interaction range
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(interactionPoint.position, interactionRange);

            // Draw detection cone
            Vector2 forward = transform.localScale.x > 0 ? Vector2.right : Vector2.left;
            Vector2 leftRay = Quaternion.Euler(0, 0, detectionAngle / 2f) * forward;
            Vector2 rightRay = Quaternion.Euler(0, 0, -detectionAngle / 2f) * forward;

            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(interactionPoint.position, leftRay * interactionRange);
            Gizmos.DrawRay(interactionPoint.position, rightRay * interactionRange);

            // Highlight current interactable
            if (currentInteractable != null)
            {
                MonoBehaviour interactableMB = currentInteractable as MonoBehaviour;
                if (interactableMB != null)
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawWireSphere(interactableMB.transform.position, 0.5f);
                    Gizmos.DrawLine(interactionPoint.position, interactableMB.transform.position);
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            // Draw all nearby interactables
            Gizmos.color = Color.cyan;
            foreach (IInteractable interactable in nearbyInteractables)
            {
                MonoBehaviour interactableMB = interactable as MonoBehaviour;
                if (interactableMB != null)
                {
                    Gizmos.DrawWireCube(interactableMB.transform.position, Vector3.one * 0.3f);
                }
            }
        }

        #endregion
    }
}