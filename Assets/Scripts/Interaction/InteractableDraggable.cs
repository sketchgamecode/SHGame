using UnityEngine;
using System.Collections;

namespace SHGame.Interaction
{
    /// <summary>
    /// Draggable object (like bodies, furniture) that player can move
    /// Essential for hiding evidence and clearing paths
    /// </summary>
    public class InteractableDraggable : InteractableBase
    {
        [Header("Dragging Settings")]
        public float dragSpeed = 2f;
        public bool snapToPlayer = false;
        public Vector3 dragOffset = Vector3.zero;
        public float maxDragDistance = 5f;
        
        [Header("Physics")]
        public bool useRigidbody = true;
        public float dragDamping = 5f;
        public bool freezeRotationWhenDragging = true;
        
        [Header("Drop Settings")]
        public LayerMask validDropLayers = -1;
        public bool snapToGround = true;
        public float snapDistance = 1f;

        // State
        private bool isBeingDragged = false;
        private Transform playerTransform;
        private Vector3 originalPosition;
        private Quaternion originalRotation;
        private Vector3 dragStartPosition;
        
        // Components
        private Rigidbody2D rb;
        private Collider2D col;

        protected override void Start()
        {
            base.Start();
            
            rb = GetComponent<Rigidbody2D>();
            col = GetComponent<Collider2D>();
            
            originalPosition = transform.position;
            originalRotation = transform.rotation;
            
            UpdateInteractionPrompt();
        }

        private void Update()
        {
            if (isBeingDragged)
            {
                HandleDragging();
                
                // Check for drop input
                if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(1))
                {
                    StopDragging();
                }
            }
        }

        protected override void PerformInteraction()
        {
            if (!isBeingDragged)
            {
                StartDragging();
            }
            else
            {
                StopDragging();
            }
        }

        protected override bool CanInteractInternal()
        {
            // Can always interact unless it's being dragged by someone else
            return true;
        }

        private void StartDragging()
        {
            if (isBeingDragged) return;

            // Find player
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) return;

            playerTransform = player.transform;
            isBeingDragged = true;
            dragStartPosition = transform.position;

            // Physics setup
            if (rb != null && useRigidbody)
            {
                rb.isKinematic = false;
                if (freezeRotationWhenDragging)
                {
                    rb.freezeRotation = true;
                }
            }

            // Visual feedback
            if (highlightEffect != null)
            {
                highlightEffect.SetActive(true);
            }

            UpdateInteractionPrompt();
            ShowSubtitle("开始拖拽...");
            
            Debug.Log($"Started dragging {name}");
        }

        private void StopDragging()
        {
            if (!isBeingDragged) return;

            isBeingDragged = false;
            playerTransform = null;

            // Try to snap to valid drop location
            if (snapToGround)
            {
                SnapToGround();
            }

            // Physics cleanup
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }

            // Visual feedback
            if (highlightEffect != null)
            {
                highlightEffect.SetActive(false);
            }

            UpdateInteractionPrompt();
            ShowSubtitle("放下物体");
            
            // Check if dropped in a special location
            CheckDropLocation();
            
            Debug.Log($"Stopped dragging {name}");
        }

        private void HandleDragging()
        {
            if (playerTransform == null) return;

            Vector3 targetPosition = playerTransform.position + dragOffset;
            
            // Check max drag distance
            float distanceFromStart = Vector3.Distance(targetPosition, dragStartPosition);
            if (distanceFromStart > maxDragDistance)
            {
                Vector3 direction = (targetPosition - dragStartPosition).normalized;
                targetPosition = dragStartPosition + direction * maxDragDistance;
            }

            if (snapToPlayer)
            {
                // Instant snap to position
                transform.position = targetPosition;
            }
            else if (rb != null && useRigidbody)
            {
                // Physics-based dragging
                Vector2 force = (targetPosition - transform.position) * dragDamping;
                rb.AddForce(force);
                
                // Apply damping to prevent excessive movement
                rb.linearVelocity *= 0.9f;
            }
            else
            {
                // Smooth movement without physics
                transform.position = Vector3.Lerp(transform.position, targetPosition, 
                    dragSpeed * Time.deltaTime);
            }
        }

        private void SnapToGround()
        {
            // Cast ray downward to find ground
            RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, snapDistance, validDropLayers);
            
            if (hit.collider != null)
            {
                // Adjust position to sit on ground
                Vector3 newPosition = transform.position;
                newPosition.y = hit.point.y + (col != null ? col.bounds.extents.y : 0.5f);
                transform.position = newPosition;
            }
        }

        private void CheckDropLocation()
        {
            // Check if dropped in special areas (like hiding spots)
            Collider2D[] overlapping = Physics2D.OverlapBoxAll(
                transform.position, 
                col != null ? col.bounds.size : Vector2.one, 
                0f
            );

            foreach (var overlap in overlapping)
            {
                // Check for hiding spots
                if (overlap.CompareTag("HidingSpot"))
                {
                    OnDroppedInHidingSpot(overlap);
                }
                
                // Check for special drop zones
                var dropZone = overlap.GetComponent<DropZone>();
                if (dropZone != null)
                {
                    dropZone.OnItemDropped(this);
                }
            }
        }

        private void OnDroppedInHidingSpot(Collider2D hidingSpot)
        {
            // Successfully hidden the draggable object
            AddInformation($"将{name}藏在了隐蔽处");
            ShowSubtitle("成功隐藏了痕迹");
            
            // Mark as hidden - could affect game state
            gameObject.tag = "Hidden";
            
            Debug.Log($"{name} was hidden in {hidingSpot.name}");
        }

        private void UpdateInteractionPrompt()
        {
            if (isBeingDragged)
            {
                interactionPrompt = "放下 [右键/空格]";
            }
            else
            {
                interactionPrompt = "拖拽";
            }
        }

        #region Public Methods

        public bool IsBeingDragged()
        {
            return isBeingDragged;
        }

        public void ForceDrop()
        {
            if (isBeingDragged)
            {
                StopDragging();
            }
        }

        public void ResetToOriginalPosition()
        {
            ForceDrop();
            transform.position = originalPosition;
            transform.rotation = originalRotation;
        }

        public Vector3 GetOriginalPosition()
        {
            return originalPosition;
        }

        public float GetDistanceDragged()
        {
            return Vector3.Distance(transform.position, originalPosition);
        }

        #endregion

        #region Gizmos

        protected override void OnDrawGizmos()
        {
            base.OnDrawGizmos();

            // Draw max drag distance
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, maxDragDistance);

            // Draw drag direction if being dragged
            if (isBeingDragged && playerTransform != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(transform.position, playerTransform.position + dragOffset);
            }

            // Draw original position
            Gizmos.color = Color.gray;
            Gizmos.DrawWireCube(originalPosition, Vector3.one * 0.3f);
        }

        protected override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();

            // Draw snap distance
            if (snapToGround)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawRay(transform.position, Vector3.down * snapDistance);
            }
        }

        #endregion
    }

    /// <summary>
    /// Component for marking drop zones where specific items should be placed
    /// </summary>
    public class DropZone : MonoBehaviour
    {
        [Header("Drop Zone Settings")]
        public string zoneName = "Drop Zone";
        public bool acceptAnyItem = true;
        public string[] acceptedTags;

        public virtual void OnItemDropped(InteractableDraggable item)
        {
            if (!acceptAnyItem)
            {
                bool accepted = false;
                foreach (string tag in acceptedTags)
                {
                    if (item.CompareTag(tag))
                    {
                        accepted = true;
                        break;
                    }
                }
                
                if (!accepted) return;
            }

            Debug.Log($"Item {item.name} was dropped in {zoneName}");
            // Override in derived classes for specific behavior
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.magenta;
            var col = GetComponent<Collider2D>();
            if (col != null)
            {
                Gizmos.DrawWireCube(transform.position, col.bounds.size);
            }
        }
    }
}