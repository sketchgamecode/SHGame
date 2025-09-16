using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;

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
        public AudioSource uiSource;

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
        [Range(0f, 1f)]
        public float uiVolume = 0.8f;

        [Header("Audio Mixer")]
        public AudioMixer audioMixer;
        public string masterVolumeParam = "MasterVolume";
        public string musicVolumeParam = "MusicVolume";
        public string sfxVolumeParam = "SFXVolume";
        public string ambientVolumeParam = "AmbientVolume";
        public string voiceVolumeParam = "VoiceVolume";
        public string uiVolumeParam = "UIVolume";

        [Header("Background Music")]
        public AudioClip mainMenuMusic;
        public AudioClip levelAmbientMusic;
        public AudioClip tensionMusic;
        public AudioClip actionMusic;
        public AudioClip victoryMusic;
        public AudioClip defeatMusic;

        [Header("Sound Effects")]
        public AudioClip[] footstepSounds;
        public AudioClip doorCreakSound;
        public AudioClip lightExtinguishSound;
        public AudioClip bodyDragSound;
        public AudioClip[] processingHitSounds;
        public AudioClip buttonClickSound;
        public AudioClip alertSound;

        [Header("Ambient Sounds")]
        public AudioClip nightAmbientSound;
        public AudioClip windSound;
        public AudioClip[] firecracklingSounds;
        public AudioClip rainSound;
        public AudioClip crowdSound;

        [Header("Scene-Specific Settings")]
        public SceneMusicSettings[] sceneMusicSettings;

        private Coroutine bgmFadeCoroutine;
        private float lastBgmVolume;
        private string currentSceneName = "";
        private bool isFading = false;
        private MusicMood currentMusicMood = MusicMood.Calm;

        [System.Serializable]
        public class SceneMusicSettings
        {
            public string sceneName;
            public AudioClip bgmClip;
            public AudioClip ambientClip;
            public float bgmVolume = 1f;
            public float ambientVolume = 1f;
            public bool playBgmOnLoad = true;
            public bool playAmbientOnLoad = true;
            public bool fadeInMusic = true;
            public float fadeInTime = 2f;
        }

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
            SceneManager.sceneLoaded += OnSceneLoaded;
            
            // Save initial BGM volume
            lastBgmVolume = bgmVolume;
            
            // Get current scene name
            currentSceneName = SceneManager.GetActiveScene().name;
        }

        private void OnDestroy()
        {
            GameManager.OnGameStateChanged -= OnGameStateChanged;
            SceneManager.sceneLoaded -= OnSceneLoaded;
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
                bgmSource.priority = 0; // Highest priority
            }

            if (sfxSource == null)
            {
                GameObject sfxObject = new GameObject("SFX AudioSource");
                sfxObject.transform.SetParent(transform);
                sfxSource = sfxObject.AddComponent<AudioSource>();
                sfxSource.playOnAwake = false;
                sfxSource.priority = 128;
            }

            if (ambientSource == null)
            {
                GameObject ambientObject = new GameObject("Ambient AudioSource");
                ambientObject.transform.SetParent(transform);
                ambientSource = ambientObject.AddComponent<AudioSource>();
                ambientSource.loop = true;
                ambientSource.playOnAwake = false;
                ambientSource.priority = 64;
            }

            if (voiceSource == null)
            {
                GameObject voiceObject = new GameObject("Voice AudioSource");
                voiceObject.transform.SetParent(transform);
                voiceSource = voiceObject.AddComponent<AudioSource>();
                voiceSource.playOnAwake = false;
                voiceSource.priority = 32;
            }
            
            if (uiSource == null)
            {
                GameObject uiObject = new GameObject("UI AudioSource");
                uiObject.transform.SetParent(transform);
                uiSource = uiObject.AddComponent<AudioSource>();
                uiSource.playOnAwake = false;
                uiSource.priority = 96;
            }

            // Configure audio mixer if available
            if (audioMixer != null)
            {
                // Assign mixer groups to audio sources
                if (bgmSource != null && audioMixer.FindMatchingGroups("Music").Length > 0)
                {
                    bgmSource.outputAudioMixerGroup = audioMixer.FindMatchingGroups("Music")[0];
                }
                
                if (sfxSource != null && audioMixer.FindMatchingGroups("SFX").Length > 0)
                {
                    sfxSource.outputAudioMixerGroup = audioMixer.FindMatchingGroups("SFX")[0];
                }
                
                if (ambientSource != null && audioMixer.FindMatchingGroups("Ambient").Length > 0)
                {
                    ambientSource.outputAudioMixerGroup = audioMixer.FindMatchingGroups("Ambient")[0];
                }
                
                if (voiceSource != null && audioMixer.FindMatchingGroups("Voice").Length > 0)
                {
                    voiceSource.outputAudioMixerGroup = audioMixer.FindMatchingGroups("Voice")[0];
                }
                
                if (uiSource != null && audioMixer.FindMatchingGroups("UI").Length > 0)
                {
                    uiSource.outputAudioMixerGroup = audioMixer.FindMatchingGroups("UI")[0];
                }
                
                // Set initial mixer values
                UpdateMixerVolumes();
            }
            else
            {
                // No mixer, use direct volume control
                UpdateAllVolumes();
            }

            Debug.Log("AudioManager initialized successfully");
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            currentSceneName = scene.name;
            
            // Apply scene-specific music settings
            ApplySceneSpecificSettings(scene.name);
        }

        private void ApplySceneSpecificSettings(string sceneName)
        {
            if (sceneMusicSettings == null) return;
            
            foreach (var settings in sceneMusicSettings)
            {
                if (settings.sceneName == sceneName)
                {
                    Debug.Log($"Applying audio settings for scene: {sceneName}");
                    
                    // Play BGM if specified
                    if (settings.playBgmOnLoad && settings.bgmClip != null)
                    {
                        PlayBGM(settings.bgmClip, settings.fadeInMusic, settings.fadeInTime);
                        
                        // Set BGM volume if different
                        if (settings.bgmVolume != bgmVolume)
                        {
                            float originalVolume = bgmVolume;
                            bgmVolume = settings.bgmVolume;
                            UpdateBGMVolume();
                            bgmVolume = originalVolume; // Restore original volume setting
                        }
                    }
                    
                    // Play ambient if specified
                    if (settings.playAmbientOnLoad && settings.ambientClip != null)
                    {
                        PlayAmbientSound(settings.ambientClip);
                        
                        // Set ambient volume if different
                        if (settings.ambientVolume != ambientVolume)
                        {
                            float originalVolume = ambientVolume;
                            ambientVolume = settings.ambientVolume;
                            UpdateAmbientVolume();
                            ambientVolume = originalVolume; // Restore original volume setting
                        }
                    }
                    
                    break;
                }
            }
        }

        #region Volume Control

        public void SetMasterVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);
            
            if (audioMixer != null)
            {
                // Convert to decibels
                float decibelValue = ConvertToDecibels(masterVolume);
                audioMixer.SetFloat(masterVolumeParam, decibelValue);
            }
            else
            {
                UpdateAllVolumes();
            }
            
            // Save volume setting to PlayerPrefs
            PlayerPrefs.SetFloat("MasterVolume", masterVolume);
            PlayerPrefs.Save();
        }

        public void SetBGMVolume(float volume)
        {
            bgmVolume = Mathf.Clamp01(volume);
            
            if (audioMixer != null)
            {
                float decibelValue = ConvertToDecibels(bgmVolume);
                audioMixer.SetFloat(musicVolumeParam, decibelValue);
            }
            else
            {
                UpdateBGMVolume();
            }
            
            // Save volume setting to PlayerPrefs
            PlayerPrefs.SetFloat("BGMVolume", bgmVolume);
            PlayerPrefs.Save();
        }

        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
            
            if (audioMixer != null)
            {
                float decibelValue = ConvertToDecibels(sfxVolume);
                audioMixer.SetFloat(sfxVolumeParam, decibelValue);
            }
            else
            {
                UpdateSFXVolume();
            }
            
            // Save volume setting to PlayerPrefs
            PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
            PlayerPrefs.Save();
        }

        public void SetAmbientVolume(float volume)
        {
            ambientVolume = Mathf.Clamp01(volume);
            
            if (audioMixer != null)
            {
                float decibelValue = ConvertToDecibels(ambientVolume);
                audioMixer.SetFloat(ambientVolumeParam, decibelValue);
            }
            else
            {
                UpdateAmbientVolume();
            }
            
            // Save volume setting to PlayerPrefs
            PlayerPrefs.SetFloat("AmbientVolume", ambientVolume);
            PlayerPrefs.Save();
        }

        public void SetVoiceVolume(float volume)
        {
            voiceVolume = Mathf.Clamp01(volume);
            
            if (audioMixer != null)
            {
                float decibelValue = ConvertToDecibels(voiceVolume);
                audioMixer.SetFloat(voiceVolumeParam, decibelValue);
            }
            else
            {
                UpdateVoiceVolume();
            }
            
            // Save volume setting to PlayerPrefs
            PlayerPrefs.SetFloat("VoiceVolume", voiceVolume);
            PlayerPrefs.Save();
        }
        
        public void SetUIVolume(float volume)
        {
            uiVolume = Mathf.Clamp01(volume);
            
            if (audioMixer != null)
            {
                float decibelValue = ConvertToDecibels(uiVolume);
                audioMixer.SetFloat(uiVolumeParam, decibelValue);
            }
            else if (uiSource != null)
            {
                uiSource.volume = masterVolume * uiVolume;
            }
            
            // Save volume setting to PlayerPrefs
            PlayerPrefs.SetFloat("UIVolume", uiVolume);
            PlayerPrefs.Save();
        }

        private void UpdateAllVolumes()
        {
            UpdateBGMVolume();
            UpdateSFXVolume();
            UpdateAmbientVolume();
            UpdateVoiceVolume();
            if (uiSource != null) uiSource.volume = masterVolume * uiVolume;
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
        
        private void UpdateMixerVolumes()
        {
            if (audioMixer == null) return;
            
            // Update all mixer volumes based on current settings
            audioMixer.SetFloat(masterVolumeParam, ConvertToDecibels(masterVolume));
            audioMixer.SetFloat(musicVolumeParam, ConvertToDecibels(bgmVolume));
            audioMixer.SetFloat(sfxVolumeParam, ConvertToDecibels(sfxVolume));
            audioMixer.SetFloat(ambientVolumeParam, ConvertToDecibels(ambientVolume));
            audioMixer.SetFloat(voiceVolumeParam, ConvertToDecibels(voiceVolume));
            audioMixer.SetFloat(uiVolumeParam, ConvertToDecibels(uiVolume));
        }
        
        private float ConvertToDecibels(float normalizedVolume)
        {
            // Convert normalized volume (0-1) to decibels (-80 to 0)
            if (normalizedVolume <= 0.001f)
                return -80f; // Silent
                
            return Mathf.Log10(normalizedVolume) * 20f;
        }
        
        public void LoadSavedVolumeSettings()
        {
            // Load volume settings from PlayerPrefs
            if (PlayerPrefs.HasKey("MasterVolume"))
                SetMasterVolume(PlayerPrefs.GetFloat("MasterVolume"));
                
            if (PlayerPrefs.HasKey("BGMVolume"))
                SetBGMVolume(PlayerPrefs.GetFloat("BGMVolume"));
                
            if (PlayerPrefs.HasKey("SFXVolume"))
                SetSFXVolume(PlayerPrefs.GetFloat("SFXVolume"));
                
            if (PlayerPrefs.HasKey("AmbientVolume"))
                SetAmbientVolume(PlayerPrefs.GetFloat("AmbientVolume"));
                
            if (PlayerPrefs.HasKey("VoiceVolume"))
                SetVoiceVolume(PlayerPrefs.GetFloat("VoiceVolume"));
                
            if (PlayerPrefs.HasKey("UIVolume"))
                SetUIVolume(PlayerPrefs.GetFloat("UIVolume"));
        }

        #endregion

        #region Background Music

        public void PlayBGM(AudioClip clip, bool fadeIn = false, float fadeTime = 1f)
        {
            if (clip == null || bgmSource == null) return;
            
            // Don't restart if it's already playing the same clip
            if (bgmSource.clip == clip && bgmSource.isPlaying)
                return;

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
                bgmSource.volume = masterVolume * bgmVolume;
                bgmSource.Play();
            }
            
            Debug.Log($"Playing BGM: {clip.name}");
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
            isFading = true;
            
            // If a different clip is playing, fade it out first
            if (bgmSource.isPlaying && bgmSource.clip != clip)
            {
                yield return StartCoroutine(FadeOutBGM(fadeTime * 0.5f));
            }
            
            bgmSource.clip = clip;
            
            if (audioMixer != null)
            {
                // Start at -80dB (silent)
                audioMixer.SetFloat(musicVolumeParam, -80f);
                bgmSource.Play();
                
                float startVolume = -80f;
                float endVolume = ConvertToDecibels(bgmVolume);
                float currentTime = 0f;
                
                while (currentTime < fadeTime)
                {
                    currentTime += Time.deltaTime;
                    float vol = Mathf.Lerp(startVolume, endVolume, currentTime / fadeTime);
                    audioMixer.SetFloat(musicVolumeParam, vol);
                    yield return null;
                }
                
                // Make sure we end at the target volume
                audioMixer.SetFloat(musicVolumeParam, endVolume);
            }
            else
            {
                // Direct volume control
                bgmSource.volume = 0f;
                bgmSource.Play();
                
                float targetVolume = masterVolume * bgmVolume;
                float currentTime = 0f;
                
                while (currentTime < fadeTime)
                {
                    currentTime += Time.deltaTime;
                    float vol = Mathf.Lerp(0f, targetVolume, currentTime / fadeTime);
                    bgmSource.volume = vol;
                    yield return null;
                }
                
                // Make sure we end at the target volume
                bgmSource.volume = targetVolume;
            }
            
            isFading = false;
        }

        private IEnumerator FadeOutBGM(float fadeTime)
        {
            if (!bgmSource.isPlaying)
            {
                yield break;
            }
            
            isFading = true;
            
            if (audioMixer != null)
            {
                // Fade out in mixer
                float startVolume = ConvertToDecibels(bgmVolume);
                float currentTime = 0f;
                
                while (currentTime < fadeTime)
                {
                    currentTime += Time.deltaTime;
                    float vol = Mathf.Lerp(startVolume, -80f, currentTime / fadeTime);
                    audioMixer.SetFloat(musicVolumeParam, vol);
                    yield return null;
                }
                
                // Stop playing and reset volume
                bgmSource.Stop();
                audioMixer.SetFloat(musicVolumeParam, startVolume);
            }
            else
            {
                // Direct volume fade
                float startVolume = bgmSource.volume;
                float currentTime = 0f;
                
                while (currentTime < fadeTime)
                {
                    currentTime += Time.deltaTime;
                    float vol = Mathf.Lerp(startVolume, 0f, currentTime / fadeTime);
                    bgmSource.volume = vol;
                    yield return null;
                }
                
                // Stop and reset
                bgmSource.Stop();
                bgmSource.volume = masterVolume * bgmVolume;
            }
            
            isFading = false;
        }

        public void PauseBGM()
        {
            if (bgmSource != null && bgmSource.isPlaying)
            {
                bgmSource.Pause();
            }
        }

        public void ResumeBGM()
        {
            if (bgmSource != null && !bgmSource.isPlaying)
            {
                bgmSource.UnPause();
            }
        }

        public void DuckBGM(float duckLevel = 0.3f, float fadeTime = 0.5f)
        {
            if (bgmSource == null || isFading) return;
            
            lastBgmVolume = bgmVolume;
            StartCoroutine(DuckBGMCoroutine(duckLevel, fadeTime));
        }

        public void UnduckBGM(float fadeTime = 0.5f)
        {
            if (bgmSource == null || isFading) return;
            
            StartCoroutine(UnduckBGMCoroutine(fadeTime));
        }

        private IEnumerator DuckBGMCoroutine(float duckLevel, float fadeTime)
        {
            isFading = true;
            
            float startVolume = bgmVolume;
            float targetVolume = bgmVolume * duckLevel;
            float currentTime = 0f;
            
            while (currentTime < fadeTime)
            {
                currentTime += Time.deltaTime;
                bgmVolume = Mathf.Lerp(startVolume, targetVolume, currentTime / fadeTime);
                
                if (audioMixer != null)
                {
                    audioMixer.SetFloat(musicVolumeParam, ConvertToDecibels(bgmVolume));
                }
                else
                {
                    UpdateBGMVolume();
                }
                
                yield return null;
            }
            
            bgmVolume = targetVolume;
            if (audioMixer != null)
            {
                audioMixer.SetFloat(musicVolumeParam, ConvertToDecibels(bgmVolume));
            }
            else
            {
                UpdateBGMVolume();
            }
            
            isFading = false;
        }

        private IEnumerator UnduckBGMCoroutine(float fadeTime)
        {
            isFading = true;
            
            float startVolume = bgmVolume;
            float targetVolume = lastBgmVolume;
            float currentTime = 0f;
            
            while (currentTime < fadeTime)
            {
                currentTime += Time.deltaTime;
                bgmVolume = Mathf.Lerp(startVolume, targetVolume, currentTime / fadeTime);
                
                if (audioMixer != null)
                {
                    audioMixer.SetFloat(musicVolumeParam, ConvertToDecibels(bgmVolume));
                }
                else
                {
                    UpdateBGMVolume();
                }
                
                yield return null;
            }
            
            bgmVolume = targetVolume;
            if (audioMixer != null)
            {
                audioMixer.SetFloat(musicVolumeParam, ConvertToDecibels(bgmVolume));
            }
            else
            {
                UpdateBGMVolume();
            }
            
            isFading = false;
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
        
        public void PlayButtonClick()
        {
            if (buttonClickSound != null && uiSource != null)
            {
                uiSource.PlayOneShot(buttonClickSound);
            }
        }
        
        public void PlayAlertSound()
        {
            PlaySFX(alertSound, 1.5f);
        }

        #endregion

        #region Ambient Sounds

        public void PlayAmbientSound(AudioClip clip, bool loop = true, float fadeIn = 0f)
        {
            if (clip == null || ambientSource == null) return;
            
            // Don't restart if already playing
            if (ambientSource.clip == clip && ambientSource.isPlaying)
                return;
                
            ambientSource.clip = clip;
            ambientSource.loop = loop;
            
            if (fadeIn > 0f)
            {
                StartCoroutine(FadeInAmbient(fadeIn));
            }
            else
            {
                ambientSource.volume = masterVolume * ambientVolume;
                ambientSource.Play();
            }
        }

        public void StopAmbientSound(float fadeOut = 0f)
        {
            if (ambientSource == null) return;
            
            if (fadeOut > 0f)
            {
                StartCoroutine(FadeOutAmbient(fadeOut));
            }
            else
            {
                ambientSource.Stop();
            }
        }

        private IEnumerator FadeInAmbient(float fadeTime)
        {
            ambientSource.volume = 0f;
            ambientSource.Play();
            
            float targetVolume = masterVolume * ambientVolume;
            float currentTime = 0f;
            
            while (currentTime < fadeTime)
            {
                currentTime += Time.deltaTime;
                ambientSource.volume = Mathf.Lerp(0f, targetVolume, currentTime / fadeTime);
                yield return null;
            }
            
            ambientSource.volume = targetVolume;
        }

        private IEnumerator FadeOutAmbient(float fadeTime)
        {
            float startVolume = ambientSource.volume;
            float currentTime = 0f;
            
            while (currentTime < fadeTime)
            {
                currentTime += Time.deltaTime;
                ambientSource.volume = Mathf.Lerp(startVolume, 0f, currentTime / fadeTime);
                yield return null;
            }
            
            ambientSource.Stop();
            ambientSource.volume = masterVolume * ambientVolume;
        }

        public void PlayNightAmbient(float fadeIn = 0f)
        {
            PlayAmbientSound(nightAmbientSound, true, fadeIn);
        }

        public void PlayFireCrackling()
        {
            PlayRandomSFX(firecracklingSounds, 0.6f);
        }
        
        public void PlayRainAmbient(float fadeIn = 1f)
        {
            if (rainSound != null)
            {
                PlayAmbientSound(rainSound, true, fadeIn);
            }
        }
        
        public void PlayCrowdAmbient(float fadeIn = 1f)
        {
            if (crowdSound != null)
            {
                PlayAmbientSound(crowdSound, true, fadeIn);
            }
        }

        #endregion

        #region Voice and Dialogue

        public void PlayVoiceClip(AudioClip clip, float volumeScale = 1f)
        {
            if (clip == null || voiceSource == null) return;
            
            // Stop any currently playing voice clip
            voiceSource.Stop();
            
            // Play new clip
            voiceSource.clip = clip;
            voiceSource.volume = masterVolume * voiceVolume * volumeScale;
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
                    StopAmbientSound(1f);
                    break;
                    
                case GameManager.GameState.Playing:
                    // Scene-specific music is handled by scene loaded event
                    break;
                    
                case GameManager.GameState.Paused:
                    // Lower music volume when paused
                    DuckBGM(0.5f, 0.2f);
                    break;
                    
                case GameManager.GameState.GameOver:
                    PlayBGM(defeatMusic, true, 2f);
                    break;
                    
                case GameManager.GameState.Victory:
                    PlayBGM(victoryMusic, true, 2f);
                    break;
            }
        }

        #endregion

        #region Utility Methods

        public void PlayMusicForMood(MusicMood mood)
        {
            if (currentMusicMood == mood) return;
            
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
                currentMusicMood = mood;
            }
        }

        public bool IsMusicPlaying()
        {
            return bgmSource != null && bgmSource.isPlaying;
        }
        
        public AudioClip GetCurrentMusic()
        {
            return bgmSource != null ? bgmSource.clip : null;
        }
        
        public MusicMood GetCurrentMusicMood()
        {
            return currentMusicMood;
        }
        
        public bool IsSoundEffectPlaying()
        {
            return sfxSource != null && sfxSource.isPlaying;
        }

        public void MuteAll(bool mute)
        {
            if (audioMixer != null)
            {
                audioMixer.SetFloat(masterVolumeParam, mute ? -80f : ConvertToDecibels(masterVolume));
            }
            else
            {
                if (bgmSource != null) bgmSource.mute = mute;
                if (sfxSource != null) sfxSource.mute = mute;
                if (ambientSource != null) ambientSource.mute = mute;
                if (voiceSource != null) voiceSource.mute = mute;
                if (uiSource != null) uiSource.mute = mute;
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