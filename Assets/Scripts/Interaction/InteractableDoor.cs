using UnityEngine;
using System.Collections;

namespace SHGame.Interaction
{
    /// <summary>
    /// Door interaction component for Wu Song game
    /// Handles door pushing mechanics that can alert NPCs
    /// </summary>
    public class InteractableDoor : InteractableBase
    {
        [Header("Door Settings")]
        public bool isLocked = false;
        public bool requiresKey = false;
        public string requiredKeyName = "";
        
        [Header("Door Behavior")]
        public float openDuration = 1f;
        public float closeDuration = 0.5f;
        public AnimationCurve openCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        
        [Header("Alert System")]
        public bool alertsNearbyNPCs = true;
        public float alertRadius = 5f;
        public LayerMask npcLayerMask = 1 << 8; // Assuming NPCs are on layer 8
        public int maxPushAttempts = 2;
        
        [Header("Visual")]
        public Transform doorTransform;
        public Vector3 closedPosition;
        public Vector3 openPosition;
        
        // State
        private bool isOpen = false;
        private bool isAnimating = false;
        private int pushCount = 0;
        private Coroutine doorAnimationCoroutine;

        protected override void Start()
        {
            base.Start();
            
            // Store initial position as closed position if not set
            if (doorTransform != null && closedPosition == Vector3.zero)
            {
                closedPosition = doorTransform.localPosition;
                openPosition = closedPosition + Vector3.right * 1f; // Default open position
            }

            // Set initial interaction prompt
            UpdateInteractionPrompt();
        }

        protected override void PerformInteraction()
        {
            if (isAnimating) return;

            if (isLocked)
            {
                HandleLockedDoor();
                return;
            }

            if (isOpen)
            {
                CloseDoor();
            }
            else
            {
                OpenDoor();
            }
        }

        protected override bool CanInteractInternal()
        {
            return !isAnimating;
        }

        private void HandleLockedDoor()
        {
            pushCount++;
            
            ShowSubtitle("门被推了推，发出响声...");
            
            // Alert nearby NPCs based on push count
            if (alertsNearbyNPCs)
            {
                AlertNearbyNPCs(pushCount);
            }

            // Check if this is a special scripted door (like for 后槽 sequence)
            if (pushCount >= maxPushAttempts)
            {
                // Trigger special behavior - NPC comes to investigate
                TriggerNPCInvestigation();
            }

            // Add to information log
            if (pushCount == 1)
            {
                AddInformation("推门发出了响声，里面有人抱怨");
            }
            else if (pushCount == maxPushAttempts)
            {
                AddInformation("连续推门激怒了里面的人，他要出来查看");
            }
        }

        private void OpenDoor()
        {
            if (doorAnimationCoroutine != null)
            {
                StopCoroutine(doorAnimationCoroutine);
            }
            
            doorAnimationCoroutine = StartCoroutine(AnimateDoor(openPosition, openDuration));
            isOpen = true;
            
            ShowSubtitle("门开了...");
            UpdateInteractionPrompt();
        }

        private void CloseDoor()
        {
            if (doorAnimationCoroutine != null)
            {
                StopCoroutine(doorAnimationCoroutine);
            }
            
            doorAnimationCoroutine = StartCoroutine(AnimateDoor(closedPosition, closeDuration));
            isOpen = false;
            
            UpdateInteractionPrompt();
        }

        private IEnumerator AnimateDoor(Vector3 targetPosition, float duration)
        {
            if (doorTransform == null) yield break;

            isAnimating = true;
            Vector3 startPosition = doorTransform.localPosition;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / duration;
                float curveValue = openCurve.Evaluate(progress);
                
                doorTransform.localPosition = Vector3.Lerp(startPosition, targetPosition, curveValue);
                yield return null;
            }

            doorTransform.localPosition = targetPosition;
            isAnimating = false;
        }

        private void AlertNearbyNPCs(int alertLevel)
        {
            Collider2D[] nearbyColliders = Physics2D.OverlapCircleAll(transform.position, alertRadius, npcLayerMask);
            
            foreach (Collider2D col in nearbyColliders)
            {
                // Try to find NPC controller component
                var npcController = col.GetComponent<Characters.NPC.NPCController>();
                if (npcController != null)
                {
                    // Alert the NPC based on alert level
                    switch (alertLevel)
                    {
                        case 1:
                            // Mild alert - NPC becomes aware but doesn't move
                            ShowSubtitle($"{col.name}: 老爷方才睡，你要偷我衣裳也早些哩！");
                            break;
                        case 2:
                            // Strong alert - NPC comes to investigate
                            ShowSubtitle($"{col.name}: 是甚么人在此推门？");
                            npcController.InvestigatePosition(transform.position);
                            break;
                    }
                }
            }
        }

        private void TriggerNPCInvestigation()
        {
            // This is specifically for the 后槽 sequence in Level 1
            Collider2D[] nearbyColliders = Physics2D.OverlapCircleAll(transform.position, alertRadius, npcLayerMask);
            
            foreach (Collider2D col in nearbyColliders)
            {
                var scriptedNPC = col.GetComponent<Characters.NPC.NPCScripted>();
                if (scriptedNPC != null)
                {
                    // Trigger the "angry investigation" sequence
                    scriptedNPC.TriggerAngryInvestigation(transform.position);
                    
                    // Unlock the door for the QTE sequence
                    isLocked = false;
                    UpdateInteractionPrompt();
                    
                    break;
                }
            }
        }

        private void UpdateInteractionPrompt()
        {
            if (isLocked)
            {
                interactionPrompt = $"推门 ({pushCount}/{maxPushAttempts})";
            }
            else if (isOpen)
            {
                interactionPrompt = "关门";
            }
            else
            {
                interactionPrompt = "开门";
            }
        }

        #region Public Methods

        public void SetLocked(bool locked)
        {
            isLocked = locked;
            UpdateInteractionPrompt();
        }

        public void SetOpen(bool open, bool animate = true)
        {
            if (isOpen == open) return;

            if (animate)
            {
                if (open)
                    OpenDoor();
                else
                    CloseDoor();
            }
            else
            {
                isOpen = open;
                if (doorTransform != null)
                {
                    doorTransform.localPosition = open ? openPosition : closedPosition;
                }
                UpdateInteractionPrompt();
            }
        }

        public bool IsOpen()
        {
            return isOpen;
        }

        public bool IsLocked()
        {
            return isLocked;
        }

        public int GetPushCount()
        {
            return pushCount;
        }

        public void ResetPushCount()
        {
            pushCount = 0;
            UpdateInteractionPrompt();
        }

        #endregion

        #region Gizmos

        protected override void OnDrawGizmos()
        {
            base.OnDrawGizmos();

            // Draw alert radius
            if (alertsNearbyNPCs)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(transform.position, alertRadius);
            }

            // Draw door positions
            if (doorTransform != null)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireCube(transform.TransformPoint(closedPosition), Vector3.one * 0.5f);
                
                Gizmos.color = Color.green;
                Gizmos.DrawWireCube(transform.TransformPoint(openPosition), Vector3.one * 0.5f);
                
                Gizmos.color = Color.white;
                Gizmos.DrawLine(transform.TransformPoint(closedPosition), transform.TransformPoint(openPosition));
            }
        }

        protected override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();

            // Draw push count indicator
            #if UNITY_EDITOR
            if (isLocked)
            {
                UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, 
                    $"Push Count: {pushCount}/{maxPushAttempts}");
            }
            #endif
        }

        #endregion
    }
}