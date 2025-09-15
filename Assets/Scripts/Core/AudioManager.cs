using UnityEngine;
using System.Collections;

namespace SHGame.Core
{
    /// <summary>
    /// Manages all audio in the game including background music, sound effects, and ambient sounds
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Audio Sources")]
        public AudioSource bgmSource;
        public AudioSource sfxSource;
        public AudioSource ambientSource;
        public AudioSource voiceSource;

        [Header("Audio Settings")]
        [Range(0f, 1f)]
        public float masterVolume = 1f;
        [Range(0f, 1f)]
        public float bgmVolume = 0.7f;
        [Range(0f, 1f)]
        public float sfxVolume = 1f;
        [Range(0f, 1f)]
        public float ambientVolume = 0.5f;
        [Range(0f, 1f)]
        public float voiceVolume = 1f;

        [Header("Background Music")]
        public AudioClip mainMenuMusic;
        public AudioClip levelAmbientMusic;
        public AudioClip tensionMusic;
        public AudioClip actionMusic;

        [Header("Sound Effects")]
        public AudioClip[] footstepSounds;
        public AudioClip doorCreakSound;
        public AudioClip lightExtinguishSound;
        public AudioClip bodyDragSound;
        public AudioClip[] processingHitSounds;

        [Header("Ambient Sounds")]
        public AudioClip nightAmbientSound;
        public AudioClip windSound;
        public AudioClip[] firecracklingSounds;

        private Coroutine bgmFadeCoroutine;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeAudio();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            // Subscribe to game events
            GameManager.OnGameStateChanged += OnGameStateChanged;
        }

        private void OnDestroy()
        {
            GameManager.OnGameStateChanged -= OnGameStateChanged;
        }

        private void InitializeAudio()
        {
            // Create audio sources if they don't exist
            if (bgmSource == null)
            {
                GameObject bgmObject = new GameObject("BGM AudioSource");
                bgmObject.transform.SetParent(transform);
                bgmSource = bgmObject.AddComponent<AudioSource>();
                bgmSource.loop = true;
                bgmSource.playOnAwake = false;
            }

            if (sfxSource == null)
            {
                GameObject sfxObject = new GameObject("SFX AudioSource");
                sfxObject.transform.SetParent(transform);
                sfxSource = sfxObject.AddComponent<AudioSource>();
                sfxSource.playOnAwake = false;
            }

            if (ambientSource == null)
            {
                GameObject ambientObject = new GameObject("Ambient AudioSource");
                ambientObject.transform.SetParent(transform);
                ambientSource = ambientObject.AddComponent<AudioSource>();
                ambientSource.loop = true;
                ambientSource.playOnAwake = false;
            }

            if (voiceSource == null)
            {
                GameObject voiceObject = new GameObject("Voice AudioSource");
                voiceObject.transform.SetParent(transform);
                voiceSource = voiceObject.AddComponent<AudioSource>();
                voiceSource.playOnAwake = false;
            }

            UpdateAllVolumes();
            Debug.Log("AudioManager initialized successfully");
        }

        #region Volume Control

        public void SetMasterVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);
            UpdateAllVolumes();
        }

        public void SetBGMVolume(float volume)
        {
            bgmVolume = Mathf.Clamp01(volume);
            UpdateBGMVolume();
        }

        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
            UpdateSFXVolume();
        }

        public void SetAmbientVolume(float volume)
        {
            ambientVolume = Mathf.Clamp01(volume);
            UpdateAmbientVolume();
        }

        public void SetVoiceVolume(float volume)
        {
            voiceVolume = Mathf.Clamp01(volume);
            UpdateVoiceVolume();
        }

        private void UpdateAllVolumes()
        {
            UpdateBGMVolume();
            UpdateSFXVolume();
            UpdateAmbientVolume();
            UpdateVoiceVolume();
        }

        private void UpdateBGMVolume()
        {
            if (bgmSource != null)
                bgmSource.volume = masterVolume * bgmVolume;
        }

        private void UpdateSFXVolume()
        {
            if (sfxSource != null)
                sfxSource.volume = masterVolume * sfxVolume;
        }

        private void UpdateAmbientVolume()
        {
            if (ambientSource != null)
                ambientSource.volume = masterVolume * ambientVolume;
        }

        private void UpdateVoiceVolume()
        {
            if (voiceSource != null)
                voiceSource.volume = masterVolume * voiceVolume;
        }

        #endregion

        #region Background Music

        public void PlayBGM(AudioClip clip, bool fadeIn = false, float fadeTime = 1f)
        {
            if (clip == null || bgmSource == null) return;

            if (bgmFadeCoroutine != null)
            {
                StopCoroutine(bgmFadeCoroutine);
            }

            if (fadeIn)
            {
                bgmFadeCoroutine = StartCoroutine(FadeInBGM(clip, fadeTime));
            }
            else
            {
                bgmSource.clip = clip;
                bgmSource.Play();
            }
        }

        public void StopBGM(bool fadeOut = false, float fadeTime = 1f)
        {
            if (bgmSource == null) return;

            if (bgmFadeCoroutine != null)
            {
                StopCoroutine(bgmFadeCoroutine);
            }

            if (fadeOut)
            {
                bgmFadeCoroutine = StartCoroutine(FadeOutBGM(fadeTime));
            }
            else
            {
                bgmSource.Stop();
            }
        }

        private IEnumerator FadeInBGM(AudioClip clip, float fadeTime)
        {
            bgmSource.clip = clip;
            bgmSource.volume = 0f;
            bgmSource.Play();

            float targetVolume = masterVolume * bgmVolume;
            float currentTime = 0f;

            while (currentTime < fadeTime)
            {
                currentTime += Time.deltaTime;
                bgmSource.volume = Mathf.Lerp(0f, targetVolume, currentTime / fadeTime);
                yield return null;
            }

            bgmSource.volume = targetVolume;
        }

        private IEnumerator FadeOutBGM(float fadeTime)
        {
            float startVolume = bgmSource.volume;
            float currentTime = 0f;

            while (currentTime < fadeTime)
            {
                currentTime += Time.deltaTime;
                bgmSource.volume = Mathf.Lerp(startVolume, 0f, currentTime / fadeTime);
                yield return null;
            }

            bgmSource.Stop();
            bgmSource.volume = startVolume;
        }

        #endregion

        #region Sound Effects

        public void PlaySFX(AudioClip clip, float volumeScale = 1f)
        {
            if (clip == null || sfxSource == null) return;
            
            sfxSource.PlayOneShot(clip, volumeScale);
        }

        public void PlayRandomSFX(AudioClip[] clips, float volumeScale = 1f)
        {
            if (clips == null || clips.Length == 0) return;
            
            AudioClip randomClip = clips[Random.Range(0, clips.Length)];
            PlaySFX(randomClip, volumeScale);
        }

        public void PlayFootstep()
        {
            PlayRandomSFX(footstepSounds, 0.8f);
        }

        public void PlayDoorCreak()
        {
            PlaySFX(doorCreakSound);
        }

        public void PlayLightExtinguish()
        {
            PlaySFX(lightExtinguishSound);
        }

        public void PlayBodyDrag()
        {
            PlaySFX(bodyDragSound);
        }

        public void PlayProcessingHit()
        {
            PlayRandomSFX(processingHitSounds);
        }

        #endregion

        #region Ambient Sounds

        public void PlayAmbientSound(AudioClip clip, bool loop = true)
        {
            if (clip == null || ambientSource == null) return;

            ambientSource.clip = clip;
            ambientSource.loop = loop;
            ambientSource.Play();
        }

        public void StopAmbientSound()
        {
            if (ambientSource != null)
            {
                ambientSource.Stop();
            }
        }

        public void PlayNightAmbient()
        {
            PlayAmbientSound(nightAmbientSound);
        }

        public void PlayFireCrackling()
        {
            PlayRandomSFX(firecracklingSounds, 0.6f);
        }

        #endregion

        #region Voice and Dialogue

        public void PlayVoiceClip(AudioClip clip)
        {
            if (clip == null || voiceSource == null) return;
            
            voiceSource.clip = clip;
            voiceSource.Play();
        }

        public void StopVoice()
        {
            if (voiceSource != null)
            {
                voiceSource.Stop();
            }
        }

        #endregion

        #region Event Handlers

        private void OnGameStateChanged(GameManager.GameState newState)
        {
            switch (newState)
            {
                case GameManager.GameState.MainMenu:
                    PlayBGM(mainMenuMusic, true);
                    StopAmbientSound();
                    break;
                    
                case GameManager.GameState.Playing:
                    PlayBGM(levelAmbientMusic, true);
                    PlayNightAmbient();
                    break;
                    
                case GameManager.GameState.Paused:
                    // Lower all volumes when paused
                    if (bgmSource != null) bgmSource.volume *= 0.5f;
                    if (ambientSource != null) ambientSource.volume *= 0.5f;
                    break;
                    
                case GameManager.GameState.GameOver:
                    StopBGM(true);
                    break;
                    
                case GameManager.GameState.Victory:
                    StopBGM(true);
                    break;
            }
        }

        #endregion

        #region Utility Methods

        public void PlayMusicForMood(MusicMood mood)
        {
            AudioClip clipToPlay = null;
            
            switch (mood)
            {
                case MusicMood.Calm:
                    clipToPlay = levelAmbientMusic;
                    break;
                case MusicMood.Tension:
                    clipToPlay = tensionMusic;
                    break;
                case MusicMood.Action:
                    clipToPlay = actionMusic;
                    break;
            }
            
            if (clipToPlay != null)
            {
                PlayBGM(clipToPlay, true, 2f);
            }
        }

        public enum MusicMood
        {
            Calm,
            Tension,
            Action
        }

        #endregion
    }
}