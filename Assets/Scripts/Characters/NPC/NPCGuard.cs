using UnityEngine;
using System.Collections;
using SHGame.Core;
using SHGame.Characters.Player;

namespace SHGame.Characters.NPC
{
    /// <summary>
    /// Specialized NPC class for guards with enhanced patrol and chase behaviors
    /// </summary>
    public class NPCGuard : NPCController
    {
        [Header("Guard Settings")]
        public GuardType guardType = GuardType.Patroller;
        public bool detectsNoise = true;
        public float noiseDetectionRadius = 8f;
        
        [Header("Alert System")]
        public float suspicionThreshold = 3f;
        public float suspicionDecayRate = 0.5f;
        public float maxSuspicionLevel = 10f;
        public float alertOtherGuardsRadius = 10f;
        public LayerMask guardLayerMask = 1 << 8; // Assuming guards are on layer 8
        
        [Header("Patrol Behavior")]
        public bool useRandomPatrol = false;
        public float chanceToChangeDirection = 0.1f;
        public float standGuardTime = 5f; // Time to stand at a position for stationary guards
        
        [Header("Search Behavior")]
        public int maxSearchAttempts = 3;
        public float searchRadius = 8f;
        public float timePerSearchLocation = 4f;
        
        [Header("Visual Feedback")]
        public GameObject alertIndicator;
        public GameObject suspicionIndicator;
        public SpriteRenderer visionConeRenderer;
        public Color normalVisionColor = Color.white;
        public Color suspiciousVisionColor = Color.yellow;
        public Color alertVisionColor = Color.red;

        // State
        private float currentSuspicionLevel = 0f;
        private Vector3 lastKnownPlayerPosition;
        private Vector3 currentSearchLocation;
        private int searchAttemptsRemaining;
        private float searchLocationTimer;
        private bool isSearching = false;
        private bool isStandingGuard = false;
        private float standingGuardTimer = 0f;

        public enum GuardType
        {
            Patroller,    // Moves between patrol points
            Stationary,   // Stands in one place, can turn around
            Sleeper       // Initially sleeping, wakes up when alerted
        }

        protected override void Start()
        {
            base.Start();
            
            // Initialize guard based on type
            switch (guardType)
            {
                case GuardType.Patroller:
                    if (patrolPoints != null && patrolPoints.Length > 0)
                    {
                        ChangeState(NPCState.Patrol);
                    }
                    else
                    {
                        Debug.LogWarning($"Guard {name} is set as Patroller but has no patrol points");
                        ChangeState(NPCState.Idle);
                    }
                    break;
                    
                case GuardType.Stationary:
                    ChangeState(NPCState.Idle);
                    isStandingGuard = true;
                    break;
                    
                case GuardType.Sleeper:
                    ChangeState(NPCState.Sleeping);
                    break;
            }
            
            // Initialize visual indicators
            if (alertIndicator != null)
                alertIndicator.SetActive(false);
                
            if (suspicionIndicator != null)
                suspicionIndicator.SetActive(false);
                
            // Initialize vision cone color
            UpdateVisionConeColor();
        }

        protected override void Update()
        {
            base.Update();
            
            // Update suspicion level
            if (currentSuspicionLevel > 0)
            {
                currentSuspicionLevel -= suspicionDecayRate * Time.deltaTime;
                currentSuspicionLevel = Mathf.Max(0, currentSuspicionLevel);
                
                // Update visuals based on suspicion
                UpdateVisionConeColor();
                
                if (suspicionIndicator != null)
                {
                    suspicionIndicator.SetActive(currentSuspicionLevel > 0);
                    // Could also scale the indicator based on suspicion level
                }
            }
            
            // Handle stationary guard behavior
            if (isStandingGuard && currentState == NPCState.Idle)
            {
                UpdateStandingGuard();
            }
            
            // Check for noise if enabled
            if (detectsNoise && currentState != NPCState.Dead && currentState != NPCState.Sleeping)
            {
                CheckForNoise();
            }
        }

        #region Guard-Specific State Updates

        protected override void UpdatePatrolState()
        {
            if (useRandomPatrol && Random.value < chanceToChangeDirection * Time.deltaTime)
            {
                // Randomly change patrol direction
                currentPatrolIndex = Random.Range(0, patrolPoints.Length);
            }
            
            base.UpdatePatrolState();
        }

        protected override void UpdateInvestigateState()
        {
            if (isSearching)
            {
                // Handle the search behavior
                UpdateSearchBehavior();
            }
            else
            {
                base.UpdateInvestigateState();
            }
        }

        protected override void UpdateChaseState()
        {
            base.UpdateChaseState();
            
            // Check if close enough to capture player
            if (playerDetected && playerTransform != null)
            {
                float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
                if (distanceToPlayer < 0.5f) // Capture distance
                {
                    // Capture player - this could trigger game over
                    CapturePlayer();
                }
            }
        }

        private void UpdateStandingGuard()
        {
            standingGuardTimer += Time.deltaTime;
            
            if (standingGuardTimer > standGuardTime)
            {
                standingGuardTimer = 0f;
                
                // Flip direction periodically
                Vector3 newScale = transform.localScale;
                newScale.x *= -1;
                transform.localScale = newScale;
            }
        }

        private void UpdateSearchBehavior()
        {
            if (searchAttemptsRemaining <= 0)
            {
                // Give up search and return to normal behavior
                CompletedSearch();
                return;
            }
            
            // Move toward current search location
            float distanceToTarget = Vector2.Distance(transform.position, currentSearchLocation);

            if (distanceToTarget < 0.5f)
            {
                // Reached search location, look around
                rb.linearVelocity = Vector2.zero;
                
                searchLocationTimer += Time.deltaTime;
                if (searchLocationTimer >= timePerSearchLocation)
                {
                    // Move to next search location
                    searchLocationTimer = 0f;
                    searchAttemptsRemaining--;
                    
                    if (searchAttemptsRemaining > 0)
                    {
                        // Choose a new search location
                        ChooseNewSearchLocation();
                    }
                }
            }
            else
            {
                // Move toward search location
                Vector2 direction = (currentSearchLocation - transform.position).normalized;
                rb.linearVelocity = direction * investigateSpeed;
                FaceDirection(direction);
            }
        }

        #endregion

        #region Guard-Specific Event Handlers

        protected override void OnPlayerDetected()
        {
            base.OnPlayerDetected();
            
            // Alert nearby guards
            AlertNearbyGuards();
            
            // Set alert indicator
            if (alertIndicator != null)
            {
                alertIndicator.SetActive(true);
            }
            
            // Max out suspicion level
            currentSuspicionLevel = maxSuspicionLevel;
            
            // Update vision cone
            UpdateVisionConeColor();
        }

        protected override void OnExitState(NPCState state)
        {
            base.OnExitState(state);
            
            if (state == NPCState.Investigate && isSearching)
            {
                // Clean up search when leaving investigate state
                isSearching = false;
                searchAttemptsRemaining = 0;
            }
        }

        protected override void OnEnterState(NPCState state)
        {
            base.OnEnterState(state);
            
            if (state == NPCState.Investigate)
            {
                // Initialize search if entering investigate state
                if (!isSearching)
                {
                    StartSearch();
                }
            }
            else if (state == NPCState.Chase)
            {
                // Clear search state if entering chase
                isSearching = false;
            }
        }

        #endregion

        #region Guard-Specific Methods

        private void CheckForNoise()
        {
            // Check for player making noise
            if (playerTransform != null)
            {
                var playerController = playerTransform.GetComponent<PlayerController>();
                if (playerController != null && playerController.IsPlayerMoving())
                {
                    float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
                    
                    // Only hear if within radius and not already detecting player
                    if (distanceToPlayer < noiseDetectionRadius && !playerDetected)
                    {
                        // Check if player is moving loud enough
                        var playerStealth = playerTransform.GetComponent<PlayerStealth>();
                        
                        // If player is not in stealth mode or is moving too fast
                        if (playerStealth == null || !playerStealth.IsPlayerHidden() || 
                            playerController.GetPlayerVelocity().magnitude > 3f) // Adjust threshold as needed
                        {
                            // Increase suspicion
                            float noiseFactor = 1f - (distanceToPlayer / noiseDetectionRadius);
                            IncreaseSuspicion(noiseFactor * Time.deltaTime * 2f);
                            
                            // If suspicion is high enough, investigate the noise
                            if (currentSuspicionLevel >= suspicionThreshold && 
                                (currentState == NPCState.Idle || currentState == NPCState.Patrol))
                            {
                                lastKnownPlayerPosition = playerTransform.position;
                                InvestigatePosition(lastKnownPlayerPosition);
                            }
                        }
                    }
                }
            }
        }

        private void IncreaseSuspicion(float amount)
        {
            currentSuspicionLevel += amount;
            currentSuspicionLevel = Mathf.Min(currentSuspicionLevel, maxSuspicionLevel);
            
            // Update visual feedback
            UpdateVisionConeColor();
            
            if (suspicionIndicator != null)
            {
                suspicionIndicator.SetActive(currentSuspicionLevel > 0);
                // Could also scale indicator based on suspicion level
            }
        }

        private void AlertNearbyGuards()
        {
            if (playerTransform == null) return;
            
            Collider2D[] nearbyGuards = Physics2D.OverlapCircleAll(transform.position, alertOtherGuardsRadius, guardLayerMask);
            
            foreach (var guardCollider in nearbyGuards)
            {
                if (guardCollider.gameObject == gameObject) continue;
                
                NPCGuard guard = guardCollider.GetComponent<NPCGuard>();
                if (guard != null && guard.currentState != NPCState.Dead)
                {
                    // Wake up sleeping guards
                    if (guard.currentState == NPCState.Sleeping)
                    {
                        guard.WakeUp();
                    }
                    
                    // Alert the guard to player's position
                    guard.ReceiveAlert(playerTransform.position);
                }
            }
        }

        private void ReceiveAlert(Vector3 alertPosition)
        {
            // Set suspicion to max
            currentSuspicionLevel = maxSuspicionLevel;
            
            // Update last known player position
            lastKnownPlayerPosition = alertPosition;
            
            // Change state based on current state
            if (currentState != NPCState.Chase && currentState != NPCState.Dead)
            {
                InvestigatePosition(alertPosition);
            }
            
            // Update visual feedback
            if (alertIndicator != null)
            {
                alertIndicator.SetActive(true);
            }
            
            UpdateVisionConeColor();
        }

        private void UpdateVisionConeColor()
        {
            if (visionConeRenderer == null) return;
            
            if (playerDetected)
            {
                visionConeRenderer.color = alertVisionColor;
            }
            else if (currentSuspicionLevel > 0)
            {
                // Interpolate between normal and suspicious based on suspicion level
                float t = currentSuspicionLevel / suspicionThreshold;
                visionConeRenderer.color = Color.Lerp(normalVisionColor, suspiciousVisionColor, t);
            }
            else
            {
                visionConeRenderer.color = normalVisionColor;
            }
        }

        private void CapturePlayer()
        {
            // This would trigger game over or capture sequence
            if (GameManager.Instance != null)
            {
                GameManager.Instance.GameOver();
            }
            
            Debug.Log("Player captured by guard!");
        }

        private void StartSearch()
        {
            isSearching = true;
            searchAttemptsRemaining = maxSearchAttempts;
            searchLocationTimer = 0f;
            
            // Initial search location is the investigation target
            currentSearchLocation = investigationTarget;
            
            Debug.Log($"Guard {name} started searching");
        }

        private void ChooseNewSearchLocation()
        {
            // Choose a random point within search radius of last known position
            Vector2 randomOffset = Random.insideUnitCircle * searchRadius;
            currentSearchLocation = lastKnownPlayerPosition + new Vector3(randomOffset.x, randomOffset.y, 0);
            
            // Ensure the location is navigable (could add more checks here)
            Debug.Log($"Guard {name} chose new search location: {currentSearchLocation}");
        }

        private void CompletedSearch()
        {
            isSearching = false;
            
            // Return to normal patrol or idle
            if (patrolPoints != null && patrolPoints.Length > 0)
            {
                ChangeState(NPCState.Patrol);
            }
            else
            {
                ChangeState(NPCState.Idle);
            }
            
            // Clear alert indicator
            if (alertIndicator != null)
            {
                alertIndicator.SetActive(false);
            }
            
            Debug.Log($"Guard {name} completed search without finding player");
        }

        public void WakeUp()
        {
            if (currentState == NPCState.Sleeping)
            {
                ChangeState(NPCState.Alert);
                StartCoroutine(WakeUpSequence());
            }
        }

        private IEnumerator WakeUpSequence()
        {
            // Play wake up animation or show dialogue
            Debug.Log($"Guard {name} is waking up!");
            
            yield return new WaitForSeconds(1.5f);
            
            // After waking up, go to alert or patrol state
            if (patrolPoints != null && patrolPoints.Length > 0)
            {
                ChangeState(NPCState.Patrol);
            }
            else
            {
                ChangeState(NPCState.Idle);
            }
        }

        #endregion

        #region Public Methods

        public float GetSuspicionLevel()
        {
            return currentSuspicionLevel;
        }

        public void SetSuspicionLevel(float level)
        {
            currentSuspicionLevel = Mathf.Clamp(level, 0f, maxSuspicionLevel);
            UpdateVisionConeColor();
        }

        public bool IsAlerted()
        {
            return currentSuspicionLevel >= suspicionThreshold;
        }

        public GuardType GetGuardType()
        {
            return guardType;
        }

        #endregion

        #region Gizmos

        protected override void OnDrawGizmos()
        {
            base.OnDrawGizmos();

            // Draw noise detection radius
            if (detectsNoise)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(transform.position, noiseDetectionRadius);
            }

            // Draw alert radius
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, alertOtherGuardsRadius);

            // Draw search radius if searching
            if (isSearching && Application.isPlaying)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(lastKnownPlayerPosition, searchRadius);
                Gizmos.DrawLine(transform.position, currentSearchLocation);
            }
        }

        protected override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();

            // Draw suspicion level
            if (Application.isPlaying)
            {
                #if UNITY_EDITOR
                UnityEditor.Handles.Label(transform.position + Vector3.up * 3f, 
                    $"Suspicion: {currentSuspicionLevel:F1}/{maxSuspicionLevel}");
                #endif
            }
        }

        #endregion
    }
}