using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace SHGame.Utilities
{
    /// <summary>
    /// Adds flickering effect to 2D lights to simulate candles, lanterns or torches
    /// Can be customized for different light types and atmospheric effects
    /// </summary>
    public class LightFlicker : MonoBehaviour
    {
        [Header("Light References")]
        public Light2D targetLight;
        
        [Header("Intensity Settings")]
        public float minIntensity = 0.8f;
        public float maxIntensity = 1.2f;
        public float flickerSpeed = 1.0f;
        public bool usePerlinNoise = true;
        public bool useSineWave = false;
        
        [Header("Color Settings")]
        public bool flickerColor = false;
        public Color baseColor = Color.yellow;
        public Color flickerColorTint = new Color(1.0f, 0.6f, 0.4f);
        public float colorFlickerSpeed = 0.5f;
        
        [Header("Range Settings")]
        public bool flickerRange = false;
        public float minRange = 0.9f;
        public float maxRange = 1.1f;
        public float rangeFlickerSpeed = 0.7f;
        
        [Header("Advanced Settings")]
        public bool useRandomSeed = true;
        public int seed = 0;
        public AnimationCurve intensityCurve = AnimationCurve.Linear(0, 0, 1, 1);
        
        // Private state
        private float initialIntensity;
        private float initialOuterRadius;
        private float baseIntensity;
        private float timeOffset;
        private Color initialColor;

        private void Awake()
        {
            // Auto-find light if not assigned
            if (targetLight == null)
            {
                targetLight = GetComponent<Light2D>();
            }
            
            // Fall back to looking in children
            if (targetLight == null)
            {
                targetLight = GetComponentInChildren<Light2D>();
            }
            
            if (targetLight == null)
            {
                Debug.LogWarning("LightFlicker script on " + gameObject.name + " has no Light2D assigned and none could be found!");
                enabled = false;
                return;
            }
            
            // Initialize state
            initialIntensity = targetLight.intensity;
            baseIntensity = initialIntensity;
            initialOuterRadius = targetLight.pointLightOuterRadius;
            initialColor = targetLight.color;
            
            // Generate random offset for each light to avoid synchronized flickering
            if (useRandomSeed)
            {
                timeOffset = Random.Range(0f, 1000f);
            }
            else
            {
                // Use provided seed for deterministic flickering
                Random.InitState(seed);
                timeOffset = Random.Range(0f, 1000f);
            }
        }

        private void Update()
        {
            if (targetLight == null) return;
            
            // Update flicker
            UpdateIntensity();
            
            if (flickerColor)
            {
                UpdateColor();
            }
            
            if (flickerRange)
            {
                UpdateRange();
            }
        }
        
        private void UpdateIntensity()
        {
            float flickerValue;
            
            if (usePerlinNoise)
            {
                // Perlin noise creates a more natural, random flickering
                flickerValue = Mathf.PerlinNoise(Time.time * flickerSpeed + timeOffset, 0);
            }
            else if (useSineWave)
            {
                // Sine wave creates a rhythmic pulsing
                flickerValue = Mathf.Sin(Time.time * flickerSpeed + timeOffset) * 0.5f + 0.5f;
            }
            else
            {
                // Simple random flickering
                flickerValue = Random.Range(0f, 1f);
            }
            
            // Apply animation curve for more control
            flickerValue = intensityCurve.Evaluate(flickerValue);
            
            // Calculate new intensity
            float newIntensity = Mathf.Lerp(minIntensity, maxIntensity, flickerValue) * baseIntensity;
            
            // Apply to light
            targetLight.intensity = newIntensity;
        }
        
        private void UpdateColor()
        {
            float t;
            
            if (usePerlinNoise)
            {
                t = Mathf.PerlinNoise(Time.time * colorFlickerSpeed + timeOffset + 100, 0);
            }
            else
            {
                t = Mathf.Sin(Time.time * colorFlickerSpeed + timeOffset) * 0.5f + 0.5f;
            }
            
            // Calculate color
            Color newColor = Color.Lerp(baseColor, flickerColorTint, t);
            
            // Apply to light
            targetLight.color = newColor;
        }
        
        private void UpdateRange()
        {
            float t;
            
            if (usePerlinNoise)
            {
                t = Mathf.PerlinNoise(Time.time * rangeFlickerSpeed + timeOffset + 200, 0);
            }
            else
            {
                t = Mathf.Sin(Time.time * rangeFlickerSpeed + timeOffset) * 0.5f + 0.5f;
            }
            
            // Calculate range
            float newRange = Mathf.Lerp(minRange, maxRange, t) * initialOuterRadius;
            
            // Apply to light
            targetLight.pointLightOuterRadius = newRange;
        }
        
        public void SetIntensityMultiplier(float multiplier)
        {
            baseIntensity = initialIntensity * multiplier;
        }
        
        public void SetFlickerSpeed(float speed)
        {
            flickerSpeed = speed;
        }
        
        public void ResetLight()
        {
            if (targetLight == null) return;
            
            targetLight.intensity = initialIntensity;
            targetLight.pointLightOuterRadius = initialOuterRadius;
            targetLight.color = initialColor;
        }
        
        #region Preset Methods
        
        public void SetCandlePreset()
        {
            minIntensity = 0.8f;
            maxIntensity = 1.2f;
            flickerSpeed = 2.0f;
            usePerlinNoise = true;
            useSineWave = false;
            
            flickerColor = true;
            baseColor = new Color(1f, 0.9f, 0.7f);
            flickerColorTint = new Color(1f, 0.6f, 0.4f);
            colorFlickerSpeed = 1.0f;
            
            flickerRange = true;
            minRange = 0.9f;
            maxRange = 1.1f;
            rangeFlickerSpeed = 1.5f;
        }
        
        public void SetTorchPreset()
        {
            minIntensity = 0.7f;
            maxIntensity = 1.3f;
            flickerSpeed = 3.0f;
            usePerlinNoise = true;
            useSineWave = false;
            
            flickerColor = true;
            baseColor = new Color(1f, 0.8f, 0.6f);
            flickerColorTint = new Color(1f, 0.5f, 0.2f);
            colorFlickerSpeed = 2.0f;
            
            flickerRange = true;
            minRange = 0.85f;
            maxRange = 1.15f;
            rangeFlickerSpeed = 2.0f;
        }
        
        public void SetLanternPreset()
        {
            minIntensity = 0.9f;
            maxIntensity = 1.1f;
            flickerSpeed = 1.0f;
            usePerlinNoise = true;
            useSineWave = false;
            
            flickerColor = true;
            baseColor = new Color(1f, 0.95f, 0.8f);
            flickerColorTint = new Color(1f, 0.9f, 0.7f);
            colorFlickerSpeed = 0.7f;
            
            flickerRange = true;
            minRange = 0.95f;
            maxRange = 1.05f;
            rangeFlickerSpeed = 0.8f;
        }
        
        public void SetFireplacePreset()
        {
            minIntensity = 0.6f;
            maxIntensity = 1.4f;
            flickerSpeed = 3.5f;
            usePerlinNoise = true;
            useSineWave = false;
            
            flickerColor = true;
            baseColor = new Color(1f, 0.7f, 0.3f);
            flickerColorTint = new Color(1f, 0.4f, 0.1f);
            colorFlickerSpeed = 2.5f;
            
            flickerRange = true;
            minRange = 0.8f;
            maxRange = 1.2f;
            rangeFlickerSpeed = 2.5f;
        }
        
        #endregion
    }
}