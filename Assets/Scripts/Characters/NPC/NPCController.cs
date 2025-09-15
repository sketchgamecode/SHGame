using UnityEngine;
using System.Collections;
using SHGame.Core;

namespace SHGame.Characters.NPC
{
    /// <summary>
    /// Base NPC controller with finite state machine
    /// Handles basic AI behaviors for guards and other NPCs
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class NPCController : MonoBehaviour
    {
        [Header("NPC Settings")]
        public string npcName = "NPC";
        public NPCType npcType = NPCType.Guard;
        public NPCState currentState = NPCState.Idle;
        
        [Header("Movement")]
        public float moveSpeed = 2f;
        public float investigateSpeed = 3f;
        public float chaseSpeed = 4f;
        public Transform[] patrolPoints;
        public float waitAtPatrolPoint = 2f;
        
        [Header("Detection")]
        public float detectionRange = 5f;
        public float fieldOfViewAngle = 60f;
        public LayerMask playerLayerMask = 1 << 7; // Assuming player is on layer 7
        public float losePlayerTime = 3f;
        
        [Header("Investigation")]
        public float investigationRadius = 2f;
        public float investigationTime = 5f;
        public float alertCooldownTime = 10f;

        // State machine
        protected NPCState previousState;
        protected float stateTimer;
        protected int currentPatrolIndex;
        protected Vector3 investigationTarget;
        protected bool playerDetected;
        protected float lastPlayerSeenTime;
        
        // Components
        protected Rigidbody2D rb;
        protected Collider2D col;
        protected Animator animator;
        
        // Player reference
        protected Transform playerTransform;

        // Animation hashes
        protected int idleHash;
        protected int walkHash;
        protected int alertHash;

        public enum NPCType
        {
            Guard,
            Civilian,
            Scripted
        }

        public enum NPCState
        {
            Idle,
            Patrol,
            Investigate,
            Chase,
            Alert,
            Dead,
            Sleeping
        }

        protected virtual void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            col = GetComponent<Collider2D>();
            animator = GetComponent<Animator>();
            
            // Find player
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }

            // Cache animation hashes
            if (animator != null)
            {
                idleHash = Animator.StringToHash("NPC_Idle");
                walkHash = Animator.StringToHash("NPC_Walk");
                alertHash = Animator.StringToHash("NPC_Alert");
            }
        }

        protected virtual void Start()
        {
            // Initialize physics
            rb.freezeRotation = true;
            rb.gravityScale = 2f;
            
            // Start in patrol state if we have patrol points
            if (patrolPoints != null && patrolPoints.Length > 0)
            {
                ChangeState(NPCState.Patrol);
            }
            else
            {
                ChangeState(NPCState.Idle);
            }

            Debug.Log($"NPC {npcName} initialized in state: {currentState}");
        }

        protected virtual void Update()
        {
            stateTimer += Time.deltaTime;
            
            // Update current state
            UpdateCurrentState();
            
            // Check for player detection (unless dead or sleeping)
            if (currentState != NPCState.Dead && currentState != NPCState.Sleeping)
            {
                CheckPlayerDetection();
            }
            
            // Update animations
            UpdateAnimations();
        }

        #region State Machine

        protected virtual void UpdateCurrentState()
        {
            switch (currentState)
            {
                case NPCState.Idle:
                    UpdateIdleState();
                    break;
                case NPCState.Patrol:
                    UpdatePatrolState();
                    break;
                case NPCState.Investigate:
                    UpdateInvestigateState();
                    break;
                case NPCState.Chase:
                    UpdateChaseState();
                    break;
                case NPCState.Alert:
                    UpdateAlertState();
                    break;
                case NPCState.Dead:
                    UpdateDeadState();
                    break;
                case NPCState.Sleeping:
                    UpdateSleepingState();
                    break;
            }
        }

        public virtual void ChangeState(NPCState newState)
        {
            if (currentState == newState) return;

            // Exit current state
            OnExitState(currentState);
            
            previousState = currentState;
            currentState = newState;
            stateTimer = 0f;
            
            // Enter new state
            OnEnterState(newState);
            
            Debug.Log($"NPC {npcName} changed state from {previousState} to {currentState}");
        }

        protected virtual void OnEnterState(NPCState state)
        {
            switch (state)
            {
                case NPCState.Idle:
                    rb.velocity = Vector2.zero;
                    break;
                case NPCState.Patrol:
                    if (patrolPoints == null || patrolPoints.Length == 0)
                    {
                        ChangeState(NPCState.Idle);
                    }
                    break;
                case NPCState.Investigate:
                    rb.velocity = Vector2.zero;
                    break;
                case NPCState.Chase:
                    break;
                case NPCState.Alert:
                    rb.velocity = Vector2.zero;
                    break;
                case NPCState.Dead:
                    rb.velocity = Vector2.zero;
                    col.enabled = false;
                    break;
                case NPCState.Sleeping:
                    rb.velocity = Vector2.zero;
                    break;
            }
        }

        protected virtual void OnExitState(NPCState state)
        {
            // Override in derived classes for specific exit behavior
        }

        #endregion

        #region State Updates

        protected virtual void UpdateIdleState()
        {
            // Just wait - can be overridden for specific idle behaviors
        }

        protected virtual void UpdatePatrolState()
        {
            if (patrolPoints == null || patrolPoints.Length == 0)
            {
                ChangeState(NPCState.Idle);
                return;
            }

            Transform targetPoint = patrolPoints[currentPatrolIndex];
            float distanceToTarget = Vector2.Distance(transform.position, targetPoint.position);

            if (distanceToTarget < 0.5f)
            {
                // Reached patrol point
                if (stateTimer >= waitAtPatrolPoint)
                {
                    // Move to next patrol point
                    currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
                    stateTimer = 0f;
                }
                rb.velocity = Vector2.zero;
            }
            else
            {
                // Move towards patrol point
                Vector2 direction = (targetPoint.position - transform.position).normalized;
                rb.velocity = direction * moveSpeed;
                
                // Face movement direction
                FaceDirection(direction);
            }
        }

        protected virtual void UpdateInvestigateState()
        {
            float distanceToTarget = Vector2.Distance(transform.position, investigationTarget);

            if (distanceToTarget > investigationRadius)
            {
                // Move towards investigation target
                Vector2 direction = (investigationTarget - transform.position).normalized;
                rb.velocity = direction * investigateSpeed;
                FaceDirection(direction);
            }
            else
            {
                // At investigation point - look around
                rb.velocity = Vector2.zero;
                
                if (stateTimer >= investigationTime)
                {
                    // Investigation complete - return to previous behavior
                    if (patrolPoints != null && patrolPoints.Length > 0)
                    {
                        ChangeState(NPCState.Patrol);
                    }
                    else
                    {
                        ChangeState(NPCState.Idle);
                    }
                }
            }
        }

        protected virtual void UpdateChaseState()
        {
            if (playerTransform == null)
            {
                ChangeState(NPCState.Alert);
                return;
            }

            // Check if we lost the player
            if (!playerDetected && Time.time - lastPlayerSeenTime > losePlayerTime)
            {
                // Lost player - investigate last known position
                investigationTarget = playerTransform.position;
                ChangeState(NPCState.Investigate);
                return;
            }

            // Chase player
            Vector2 direction = (playerTransform.position - transform.position).normalized;
            rb.velocity = direction * chaseSpeed;
            FaceDirection(direction);
        }

        protected virtual void UpdateAlertState()
        {
            // Stay alert for a while, then return to normal behavior
            if (stateTimer >= alertCooldownTime)
            {
                if (patrolPoints != null && patrolPoints.Length > 0)
                {
                    ChangeState(NPCState.Patrol);
                }
                else
                {
                    ChangeState(NPCState.Idle);
                }
            }
        }

        protected virtual void UpdateDeadState()
        {
            // Dead NPCs don't update
        }

        protected virtual void UpdateSleepingState()
        {
            // Sleeping NPCs don't move
        }

        #endregion

        #region Player Detection

        protected virtual void CheckPlayerDetection()
        {
            if (playerTransform == null) return;

            float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
            bool wasDetected = playerDetected;
            playerDetected = false;

            // Check if player is in range
            if (distanceToPlayer <= detectionRange)
            {
                // Check if player is in field of view
                Vector2 directionToPlayer = (playerTransform.position - transform.position).normalized;
                Vector2 forward = GetForwardDirection();
                
                float angle = Vector2.Angle(forward, directionToPlayer);
                
                if (angle <= fieldOfViewAngle / 2f)
                {
                    // Check line of sight
                    if (HasLineOfSightToPlayer())
                    {
                        // Check if player is hidden
                        var playerStealth = playerTransform.GetComponent<Characters.Player.PlayerStealth>();
                        if (playerStealth == null || !playerStealth.IsPlayerHidden())
                        {
                            playerDetected = true;
                            lastPlayerSeenTime = Time.time;
                            
                            // React to detection
                            if (!wasDetected)
                            {
                                OnPlayerDetected();
                            }
                        }
                    }
                }
            }
        }

        protected virtual bool HasLineOfSightToPlayer()
        {
            if (playerTransform == null) return false;

            Vector2 rayStart = new Vector2(transform.position.x, transform.position.y + 1f); // Eye level
            Vector2 direction = (playerTransform.position - transform.position).normalized;
            float distance = Vector2.Distance(transform.position, playerTransform.position);
            
            RaycastHit2D hit = Physics2D.Raycast(rayStart, direction, distance, ~playerLayerMask);
            
            // Debug ray
            Debug.DrawRay(rayStart, direction * distance, playerDetected ? Color.red : Color.green);
            
            return hit.collider == null; // No obstacles in the way
        }

        protected virtual void OnPlayerDetected()
        {
            Debug.Log($"NPC {npcName} detected the player!");
            
            // Change to appropriate state based on NPC type
            switch (npcType)
            {
                case NPCType.Guard:
                    ChangeState(NPCState.Chase);
                    break;
                case NPCType.Civilian:
                    ChangeState(NPCState.Alert);
                    break;
            }
            
            // Trigger game over if this is a detection that should end the game
            // This can be customized per NPC or level
            if (ShouldTriggerGameOver())
            {
                GameManager.Instance?.GameOver();
            }
        }

        protected virtual bool ShouldTriggerGameOver()
        {
            // Override in derived classes for specific game over conditions
            return npcType == NPCType.Guard && currentState == NPCState.Chase;
        }

        #endregion

        #region Public Methods

        public virtual void InvestigatePosition(Vector3 position)
        {
            investigationTarget = position;
            ChangeState(NPCState.Investigate);
        }

        public virtual void SetDead()
        {
            ChangeState(NPCState.Dead);
        }

        public virtual void SetSleeping(bool sleeping)
        {
            if (sleeping)
            {
                ChangeState(NPCState.Sleeping);
            }
            else if (currentState == NPCState.Sleeping)
            {
                ChangeState(NPCState.Idle);
            }
        }

        public NPCState GetCurrentState()
        {
            return currentState;
        }

        public bool IsPlayerDetected()
        {
            return playerDetected;
        }

        #endregion

        #region Utility Methods

        protected virtual void FaceDirection(Vector2 direction)
        {
            if (direction.x > 0)
            {
                transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
            else if (direction.x < 0)
            {
                transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
        }

        protected virtual Vector2 GetForwardDirection()
        {
            return transform.localScale.x > 0 ? Vector2.right : Vector2.left;
        }

        protected virtual void UpdateAnimations()
        {
            if (animator == null) return;

            switch (currentState)
            {
                case NPCState.Idle:
                case NPCState.Sleeping:
                    animator.Play(idleHash);
                    break;
                case NPCState.Patrol:
                case NPCState.Investigate:
                case NPCState.Chase:
                    animator.Play(walkHash);
                    break;
                case NPCState.Alert:
                    animator.Play(alertHash);
                    break;
                case NPCState.Dead:
                    // Play death animation if available
                    break;
            }
        }

        #endregion

        #region Gizmos

        protected virtual void OnDrawGizmos()
        {
            // Draw detection range
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRange);
            
            // Draw field of view
            if (fieldOfViewAngle > 0)
            {
                Vector2 forward = GetForwardDirection();
                Vector2 leftBoundary = Quaternion.Euler(0, 0, fieldOfViewAngle / 2f) * forward;
                Vector2 rightBoundary = Quaternion.Euler(0, 0, -fieldOfViewAngle / 2f) * forward;
                
                Gizmos.color = playerDetected ? Color.red : Color.green;
                Gizmos.DrawRay(transform.position, leftBoundary * detectionRange);
                Gizmos.DrawRay(transform.position, rightBoundary * detectionRange);
            }
            
            // Draw patrol points
            if (patrolPoints != null)
            {
                Gizmos.color = Color.blue;
                for (int i = 0; i < patrolPoints.Length; i++)
                {
                    if (patrolPoints[i] != null)
                    {
                        Gizmos.DrawWireSphere(patrolPoints[i].position, 0.5f);
                        
                        // Draw path to next point
                        int nextIndex = (i + 1) % patrolPoints.Length;
                        if (patrolPoints[nextIndex] != null)
                        {
                            Gizmos.DrawLine(patrolPoints[i].position, patrolPoints[nextIndex].position);
                        }
                    }
                }
            }
            
            // Draw investigation target
            if (currentState == NPCState.Investigate)
            {
                Gizmos.color = Color.orange;
                Gizmos.DrawWireSphere(investigationTarget, investigationRadius);
                Gizmos.DrawLine(transform.position, investigationTarget);
            }
        }

        protected virtual void OnDrawGizmosSelected()
        {
            // Draw state information
            #if UNITY_EDITOR
            UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, 
                $"{npcName}\nState: {currentState}\nDetected: {playerDetected}");
            #endif
        }

        #endregion
    }
}