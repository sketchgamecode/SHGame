using UnityEngine;
using SHGame.Core;

namespace SHGame.Characters.Player
{
    /// <summary>
    /// Main player controller handling movement, input, and basic interactions
    /// Wu Song character controller for 2D side-scrolling stealth gameplay
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D), typeof(Animator))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement Settings")]
        public float moveSpeed = 5f;
        public float jumpForce = 10f;
        public float groundCheckDistance = 0.1f;
        public LayerMask groundLayerMask = 1;

        [Header("Input Settings")]
        public KeyCode interactionKey = KeyCode.Space;
        public KeyCode crouchKey = KeyCode.LeftControl;
        public KeyCode informationLogKey = KeyCode.Tab;

        [Header("Animation Settings")]
        public string idleAnimationName = "Hero_Idle";
        public string walkAnimationName = "Hero_Walk";
        public string crouchAnimationName = "Hero_Crouch";
        public string interactAnimationName = "Hero_Interact";

        // Components
        private Rigidbody2D rb;
        private BoxCollider2D col;
        private Animator animator;
        private PlayerStealth stealthSystem;
        private PlayerInteraction interactionSystem;

        // State
        private bool isGrounded;
        private bool isCrouching;
        private bool isMoving;
        private float horizontalInput;
        private bool facingRight = true;

        // Animation hash IDs for performance
        private int idleHash;
        private int walkHash;
        private int crouchHash;
        private int interactHash;

        private void Awake()
        {
            // Get required components
            rb = GetComponent<Rigidbody2D>();
            col = GetComponent<BoxCollider2D>();
            animator = GetComponent<Animator>();
            stealthSystem = GetComponent<PlayerStealth>();
            interactionSystem = GetComponent<PlayerInteraction>();

            // Cache animation hashes
            idleHash = Animator.StringToHash(idleAnimationName);
            walkHash = Animator.StringToHash(walkAnimationName);
            crouchHash = Animator.StringToHash(crouchAnimationName);
            interactHash = Animator.StringToHash(interactAnimationName);
        }

        private void Start()
        {
            // Initialize physics settings
            rb.freezeRotation = true;
            rb.gravityScale = 2f;

            Debug.Log("PlayerController initialized");
        }

        private void Update()
        {
            HandleInput();
            CheckGrounded();
            UpdateAnimations();
        }

        private void FixedUpdate()
        {
            HandleMovement();
        }

        private void HandleInput()
        {
            // Get horizontal input
            horizontalInput = Input.GetAxis("Horizontal");
            isMoving = Mathf.Abs(horizontalInput) > 0.1f;

            // Handle crouching (stealth mode)
            if (Input.GetKeyDown(crouchKey))
            {
                SetCrouching(true);
            }
            else if (Input.GetKeyUp(crouchKey))
            {
                SetCrouching(false);
            }

            // Handle interaction
            if (Input.GetKeyDown(interactionKey))
            {
                PerformInteraction();
            }

            // Handle information log toggle
            if (Input.GetKeyDown(informationLogKey))
            {
                ToggleInformationLog();
            }

            // Handle jump (if needed for certain areas)
            if (Input.GetKeyDown(KeyCode.Space) && isGrounded && !isCrouching)
            {
                Jump();
            }
        }

        private void HandleMovement()
        {
            // Don't move if game is paused or in interaction
            if (GameManager.Instance != null && GameManager.Instance.isPaused)
                return;

            // Calculate move speed (reduced when crouching for stealth)
            float currentMoveSpeed = isCrouching ? moveSpeed * 0.5f : moveSpeed;
            
            // Apply horizontal movement
            Vector2 velocity = rb.linearVelocity;
            velocity.x = horizontalInput * currentMoveSpeed;
            rb.linearVelocity = velocity;

            // Handle sprite flipping
            if (horizontalInput > 0 && !facingRight)
            {
                Flip();
            }
            else if (horizontalInput < 0 && facingRight)
            {
                Flip();
            }

            // Play footstep sounds when moving
            if (isMoving && isGrounded && !isCrouching)
            {
                PlayFootstepSound();
            }
        }

        private void CheckGrounded()
        {
            // Cast a ray downward to check if grounded
            Vector2 rayOrigin = new Vector2(transform.position.x, col.bounds.min.y);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, groundCheckDistance, groundLayerMask);
            
            isGrounded = hit.collider != null;

            // Debug visualization
            Debug.DrawRay(rayOrigin, Vector2.down * groundCheckDistance, isGrounded ? Color.green : Color.red);
        }

        private void Jump()
        {
            if (!isGrounded) return;

            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            
            // Play jump sound effect
            if (AudioManager.Instance != null)
            {
                // AudioManager.Instance.PlayJumpSound(); // Add this to AudioManager if needed
            }
        }

        private void SetCrouching(bool crouch)
        {
            isCrouching = crouch;
            
            // Adjust collider size for crouching
            if (crouch)
            {
                col.size = new Vector2(col.size.x, col.size.y * 0.7f);
                col.offset = new Vector2(col.offset.x, col.offset.y - col.size.y * 0.15f);
            }
            else
            {
                col.size = new Vector2(col.size.x, col.size.y / 0.7f);
                col.offset = new Vector2(col.offset.x, col.offset.y + col.size.y * 0.15f);
            }
        }

        private void PerformInteraction()
        {
            if (interactionSystem != null)
            {
                interactionSystem.PerformInteraction();
            }
        }

        private void ToggleInformationLog()
        {
            if (UIManager.Instance != null)
            {
                // Toggle information panel visibility
                if (UIManager.Instance.informationPanel != null)
                {
                    bool isActive = UIManager.Instance.informationPanel.activeSelf;
                    if (isActive)
                    {
                        UIManager.Instance.HideInformationPanel();
                    }
                    else
                    {
                        UIManager.Instance.ShowInformationPanel();
                    }
                }
            }
        }

        private void Flip()
        {
            facingRight = !facingRight;
            transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
        }

        private void UpdateAnimations()
        {
            if (animator == null) return;

            // Determine which animation to play
            if (isCrouching)
            {
                animator.Play(crouchHash);
            }
            else if (isMoving)
            {
                animator.Play(walkHash);
            }
            else
            {
                animator.Play(idleHash);
            }
        }

        private void PlayFootstepSound()
        {
            // Only play footstep sound occasionally to avoid spam
            if (Time.time % 0.5f < 0.1f && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayFootstep();
            }
        }

        #region Public Methods for External Systems

        public void PlayInteractionAnimation()
        {
            if (animator != null)
            {
                animator.Play(interactHash);
            }
        }

        public void SetMovementEnabled(bool enabled)
        {
            this.enabled = enabled;
        }

        public bool IsPlayerMoving()
        {
            return isMoving;
        }

        public bool IsPlayerCrouching()
        {
            return isCrouching;
        }

        public Vector2 GetPlayerPosition()
        {
            return transform.position;
        }

        public Vector2 GetPlayerVelocity()
        {
            return rb != null ? rb.linearVelocity : Vector2.zero;
        }

        #endregion

        #region Gizmos

        private void OnDrawGizmos()
        {
            // Draw ground check ray
            if (col != null)
            {
                Gizmos.color = isGrounded ? Color.green : Color.red;
                Vector2 rayOrigin = new Vector2(transform.position.x, col.bounds.min.y);
                Gizmos.DrawLine(rayOrigin, rayOrigin + Vector2.down * groundCheckDistance);
            }
        }

        #endregion
    }
}