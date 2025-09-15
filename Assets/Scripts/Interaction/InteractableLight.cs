using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Collections;

namespace SHGame.Interaction
{
    /// <summary>
    /// Light source that can be extinguished by the player
    /// Important for creating shadow areas for stealth
    /// </summary>
    public class InteractableLight : InteractableBase
    {
        [Header("Light Settings")]
        public Light2D lightSource;
        public bool canBeExtinguished = true;
        public bool canBeRelit = false;
        public float extinguishTime = 1f;

        [Header("Visual Effects")]
        public ParticleSystem fireEffect;
        public GameObject smokeEffect;
        public bool flickerBeforeExtinguish = true;
        public float flickerDuration = 0.5f;

        // State
        private bool isLit = true;
        private float originalIntensity;
        private Coroutine extinguishCoroutine;

        protected override void Start()
        {
            base.Start();
            
            // Store original light intensity
            if (lightSource != null)
            {
                originalIntensity = lightSource.intensity;
            }
            
            UpdateInteractionPrompt();
        }

        protected override void PerformInteraction()
        {
            if (isLit && canBeExtinguished)
            {
                ExtinguishLight();
            }
            else if (!isLit && canBeRelit)
            {
                RelitLight();
            }
        }

        protected override bool CanInteractInternal()
        {
            return (isLit && canBeExtinguished) || (!isLit && canBeRelit);
        }

        private void ExtinguishLight()
        {
            if (extinguishCoroutine != null)
            {
                StopCoroutine(extinguishCoroutine);
            }
            
            extinguishCoroutine = StartCoroutine(ExtinguishSequence());
        }

        private IEnumerator ExtinguishSequence()
        {
            // Flicker effect before extinguishing
            if (flickerBeforeExtinguish && lightSource != null)
            {
                yield return StartCoroutine(FlickerEffect());
            }

            // Actually extinguish the light
            if (lightSource != null)
            {
                float elapsed = 0f;
                while (elapsed < extinguishTime)
                {
                    elapsed += Time.deltaTime;
                    float progress = elapsed / extinguishTime;
                    lightSource.intensity = Mathf.Lerp(originalIntensity, 0f, progress);
                    yield return null;
                }
                
                lightSource.intensity = 0f;
            }

            // Stop fire effect
            if (fireEffect != null)
            {
                fireEffect.Stop();
            }

            // Show smoke effect
            if (smokeEffect != null)
            {
                smokeEffect.SetActive(true);
                // Auto-hide smoke after a few seconds
                StartCoroutine(HideSmokeAfterDelay(3f));
            }

            isLit = false;
            UpdateInteractionPrompt();

            // Add to information log
            AddInformation("熄灭了灯火");
            ShowSubtitle("灯火熄灭了...");

            Debug.Log($"Light {name} has been extinguished");
        }

        private IEnumerator FlickerEffect()
        {
            if (lightSource == null) yield break;

            float elapsed = 0f;
            while (elapsed < flickerDuration)
            {
                elapsed += Time.deltaTime;
                
                // Random flicker intensity
                float flicker = Random.Range(0.3f, 1f);
                lightSource.intensity = originalIntensity * flicker;
                
                yield return new WaitForSeconds(Random.Range(0.05f, 0.15f));
            }
            
            // Restore original intensity briefly before extinguishing
            lightSource.intensity = originalIntensity;
        }

        private IEnumerator HideSmokeAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (smokeEffect != null)
            {
                smokeEffect.SetActive(false);
            }
        }

        private void RelitLight()
        {
            if (lightSource != null)
            {
                lightSource.intensity = originalIntensity;
            }

            if (fireEffect != null)
            {
                fireEffect.Play();
            }

            if (smokeEffect != null)
            {
                smokeEffect.SetActive(false);
            }

            isLit = true;
            UpdateInteractionPrompt();

            ShowSubtitle("重新点燃了灯火");
            Debug.Log($"Light {name} has been relit");
        }

        private void UpdateInteractionPrompt()
        {
            if (isLit && canBeExtinguished)
            {
                interactionPrompt = "吹灭灯火";
            }
            else if (!isLit && canBeRelit)
            {
                interactionPrompt = "点燃灯火";
            }
            else
            {
                interactionPrompt = "无法操作";
            }
        }

        #region Public Methods

        public bool IsLit()
        {
            return isLit;
        }

        public void ForceExtinguish()
        {
            if (isLit)
            {
                ExtinguishLight();
            }
        }

        public void ForceRelight()
        {
            if (!isLit)
            {
                RelitLight();
            }
        }

        public void SetExtinguishable(bool extinguishable)
        {
            canBeExtinguished = extinguishable;
            UpdateInteractionPrompt();
        }

        public void SetRelightable(bool relightable)
        {
            canBeRelit = relightable;
            UpdateInteractionPrompt();
        }

        #endregion
    }
}