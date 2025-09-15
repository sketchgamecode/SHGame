using UnityEngine;
using UnityEngine.Rendering.Universal;
using System;
using SHGame.Core;

namespace SHGame.Characters.Player
{
    /// <summary>
    /// Handles player stealth mechanics using URP 2D lighting system
    /// Determines if Wu Song is hidden in shadows or exposed to light
    /// </summary>
    public class PlayerStealth : MonoBehaviour
    {
        [Header("Stealth Settings")]
        [Range(0f, 1f)]
        public float hideThreshold = 0.3f;
        [Range(0f, 5f)]
        public float detectionRadius = 1f;
        public LayerMask lightLayerMask = -1;

        [Header("Visual Feedback")]
        public bool enableVisualFeedback = true;
        public Color hiddenColor = new Color(0.5f, 0.5f, 0.5f, 1f);
        public Color exposedColor = Color.white;
        public float colorTransitionSpeed = 2f;

        [Header("Detection Settings")]
        public float stealthCheckInterval = 0.1f;
        public bool requireMovementForDetection = true;
        public float movementThreshold = 0.1f;

        // State
        private bool isHidden = false;
        private bool wasHidden = false;
        private float currentLightIntensity = 0f;
        private float stealthCheckTimer = 0f;

        // Components
        private SpriteRenderer spriteRenderer;
        private PlayerController playerController;
        private Light2D[] nearbyLights;

        // Events
        public static event Action<bool> OnStealthStatusChanged;
        public static event Action OnPlayerDetected;
        public static event Action OnPlayerHidden;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            playerController = GetComponent<PlayerController>();
            
            // Find all lights in the scene initially
            RefreshNearbyLights();
        }

        private void Start()
        {
            // Subscribe to player controller events if needed
            if (UIManager.Instance != null)
            {
                OnStealthStatusChanged += UIManager.Instance.UpdateStealthStatus;
            }

            Debug.Log("PlayerStealth system initialized");
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (UIManager.Instance != null)
            {
                OnStealthStatusChanged -= UIManager.Instance.UpdateStealthStatus;
            }
        }

        private void Update()
        {
            // Perform stealth check at intervals for performance
            stealthCheckTimer += Time.deltaTime;
            if (stealthCheckTimer >= stealthCheckInterval)
            {
                stealthCheckTimer = 0f;
                CheckStealthStatus();
            }

            // Update visual feedback
            if (enableVisualFeedback && spriteRenderer != null)
            {
                UpdateVisualFeedback();
            }
        }

        private void CheckStealthStatus()
        {
            // Calculate total light intensity at player position
            currentLightIntensity = CalculateLightIntensityAtPosition(transform.position);
            
            // Determine if player is hidden
            bool newHiddenState = currentLightIntensity < hideThreshold;
            
            // Apply movement-based detection rules
            if (requireMovementForDetection && playerController != null)
            {
                bool isPlayerMoving = playerController.GetPlayerVelocity().magnitude > movementThreshold;
                
                // If player is moving, they're more likely to be detected
                if (isPlayerMoving)
                {
                    newHiddenState = newHiddenState && currentLightIntensity < (hideThreshold * 0.7f);
                }
            }

            // Update stealth state if changed
            if (newHiddenState != isHidden)
            {
                SetStealthStatus(newHiddenState);
            }
        }

        private float CalculateLightIntensityAtPosition(Vector3 position)
        {
            float totalIntensity = 0f;
            
            // Refresh nearby lights periodically
            if (Time.time % 2f < 0.1f) // Every 2 seconds
            {
                RefreshNearbyLights();
            }

            // Calculate intensity from all nearby lights
            if (nearbyLights != null)
            {
                foreach (Light2D light in nearbyLights)
                {
                    if (light == null || !light.enabled || !light.gameObject.activeInHierarchy)
                        continue;

                    float intensity = CalculateIntensityFromLight(light, position);
                    totalIntensity += intensity;
                }
            }

            // Add global/ambient light contribution
            totalIntensity += GetGlobalLightContribution();

            return Mathf.Clamp01(totalIntensity);
        }

        private float CalculateIntensityFromLight(Light2D light, Vector3 position)
        {
            if (light == null) return 0f;

            Vector3 lightPosition = light.transform.position;
            float distance = Vector2.Distance(lightPosition, position);

            // Check if position is within light range
            float lightRange = light.pointLightOuterRadius;
            if (distance > lightRange) return 0f;

            float intensity = light.intensity;
            
            // Calculate distance falloff
            float falloff = 1f - (distance / lightRange);
            falloff = Mathf.Clamp01(falloff);

            // Handle different light types
            switch (light.lightType)
            {
                case Light2D.LightType.Point:
                    return intensity * falloff;
                    
                case Light2D.LightType.Freeform: // Spot 替换为 Freeform
                    // 可以自定义 Freeform 的角度检测逻辑（如有需要）
                    return intensity * falloff;
                    
                case Light2D.LightType.Global:
                    return intensity;
                    
                default:
                    return intensity * falloff;
            }
        }

        private float GetGlobalLightContribution()
        {
            // This represents ambient/global lighting (like moonlight)
            // Can be adjusted based on time of day or scene requirements
            return 0.1f; // Very low ambient light for night scenes
        }

        private void RefreshNearbyLights()
        {
            // Find all Light2D components in the scene
            nearbyLights = FindObjectsOfType<Light2D>();
            
            // Filter to only nearby lights for performance
            var nearbyLightsList = new System.Collections.Generic.List<Light2D>();
            
            foreach (Light2D light in nearbyLights)
            {
                if (light == null) continue;
                
                float distance = Vector2.Distance(light.transform.position, transform.position);
                float maxCheckDistance = light.pointLightOuterRadius + detectionRadius;
                
                if (distance <= maxCheckDistance)
                {
                    nearbyLightsList.Add(light);
                }
            }
            
            nearbyLights = nearbyLightsList.ToArray();
        }

        private void SetStealthStatus(bool hidden)
        {
            wasHidden = isHidden;
            isHidden = hidden;

            // Trigger events
            OnStealthStatusChanged?.Invoke(isHidden);
            
            if (isHidden && !wasHidden)
            {
                OnPlayerHidden?.Invoke();
                Debug.Log("Player is now hidden in shadows");
            }
            else if (!isHidden && wasHidden)
            {
                OnPlayerDetected?.Invoke();
                Debug.Log("Player is now exposed to light!");
            }
        }

        private void UpdateVisualFeedback()
        {
            Color targetColor = isHidden ? hiddenColor : exposedColor;
            spriteRenderer.color = Color.Lerp(spriteRenderer.color, targetColor, 
                colorTransitionSpeed * Time.deltaTime);
        }

        #region Public Methods

        public bool IsPlayerHidden()
        {
            return isHidden;
        }

        public float GetCurrentLightIntensity()
        {
            return currentLightIntensity;
        }

        public void SetHideThreshold(float threshold)
        {
            hideThreshold = Mathf.Clamp01(threshold);
        }

        public void ForceStealthCheck()
        {
            CheckStealthStatus();
        }

        /// <summary>
        /// Temporarily disable stealth detection (for cutscenes, etc.)
        /// </summary>
        public void SetStealthEnabled(bool enabled)
        {
            this.enabled = enabled;
        }

        #endregion

        #region Debug and Gizmos

        private void OnDrawGizmos()
        {
            // Draw detection radius
            Gizmos.color = isHidden ? Color.green : Color.red;
            Gizmos.DrawWireSphere(transform.position, detectionRadius);
            
            // Draw light intensity indicator
            Gizmos.color = Color.yellow;
            Vector3 intensityIndicator = transform.position + Vector3.up * (currentLightIntensity * 2f);
            Gizmos.DrawLine(transform.position, intensityIndicator);
            
            // Draw threshold line
            Gizmos.color = Color.blue;
            Vector3 thresholdLine = transform.position + Vector3.up * (hideThreshold * 2f);
            Gizmos.DrawWireSphere(thresholdLine, 0.1f);
        }

        private void OnDrawGizmosSelected()
        {
            // Draw light sources in range
            if (nearbyLights != null)
            {
                foreach (Light2D light in nearbyLights)
                {
                    if (light == null) continue;
                    
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawLine(transform.position, light.transform.position);
                    
                    Gizmos.color = Color.white;
                    Gizmos.DrawWireSphere(light.transform.position, light.pointLightOuterRadius);
                }
            }
        }

        #endregion

        #region Debug Information

        [System.Serializable]
        public class StealthDebugInfo
        {
            public bool isHidden;
            public float currentLightIntensity;
            public float hideThreshold;
            public int nearbyLightCount;
            public bool isPlayerMoving;
        }

        public StealthDebugInfo GetDebugInfo()
        {
            return new StealthDebugInfo
            {
                isHidden = this.isHidden,
                currentLightIntensity = this.currentLightIntensity,
                hideThreshold = this.hideThreshold,
                nearbyLightCount = nearbyLights?.Length ?? 0,
                isPlayerMoving = playerController?.IsPlayerMoving() ?? false
            };
        }

        #endregion
    }
}