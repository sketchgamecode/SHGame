using UnityEngine;
using System;

namespace SHGame.Utilities
{
    /// <summary>
    /// Central event system for game-wide communication
    /// Allows decoupled communication between systems
    /// </summary>
    public static class GameEvents
    {
        // Player Events
        public static event Action<bool> OnPlayerStealthChanged;
        public static event Action<Vector3> OnPlayerPositionChanged;
        public static event Action OnPlayerDetected;
        public static event Action OnPlayerDied;

        // NPC Events
        public static event Action<Characters.NPC.NPCController> OnNPCStateChanged;
        public static event Action<Characters.NPC.NPCController> OnNPCDefeated;
        public static event Action<Characters.NPC.NPCController, Vector3> OnNPCAlerted;

        // Interaction Events
        public static event Action<Interaction.IInteractable> OnObjectInteracted;
        public static event Action<string> OnInformationGathered;
        public static event Action<string> OnDialogueStarted;
        public static event Action OnDialogueEnded;

        // Game State Events
        public static event Action OnLevelCompleted;
        public static event Action OnGameOver;
        public static event Action<string> OnSceneTransition;
        public static event Action<string> OnObjectiveCompleted;
        public static event Action<Core.LevelManager.LevelObjective> OnNewObjectiveAdded;

        // QTE Events
        public static event Action<bool> OnQTECompleted;
        public static event Action<KeyCode, float> OnQTEStarted;

        // Audio Events
        public static event Action<AudioClip> OnPlaySFX;
        public static event Action<AudioClip> OnPlayBGM;
        public static event Action<Core.AudioManager.MusicMood> OnMoodChanged;

        // Rage System Events
        public static event Action<float> OnRageIncreased;
        public static event Action OnRevengeReady;
        public static event Action OnRevengeExecuted;

        // Calligraphy System Events
        public static event Action<int> OnCharacterCompleted;
        public static event Action OnCalligraphyCompleted;

        // Item/Inventory Events
        public static event Action<string, string> OnItemCollected;
        public static event Action<string> OnItemUsed;

        #region Player Event Triggers

        public static void TriggerPlayerStealthChanged(bool isHidden)
        {
            OnPlayerStealthChanged?.Invoke(isHidden);
        }

        public static void TriggerPlayerPositionChanged(Vector3 position)
        {
            OnPlayerPositionChanged?.Invoke(position);
        }

        public static void TriggerPlayerDetected()
        {
            OnPlayerDetected?.Invoke();
        }

        public static void TriggerPlayerDied()
        {
            OnPlayerDied?.Invoke();
        }

        #endregion

        #region NPC Event Triggers

        public static void TriggerNPCStateChanged(Characters.NPC.NPCController npc)
        {
            OnNPCStateChanged?.Invoke(npc);
        }

        public static void TriggerNPCDefeated(Characters.NPC.NPCController npc)
        {
            OnNPCDefeated?.Invoke(npc);
        }

        public static void TriggerNPCAlerted(Characters.NPC.NPCController npc, Vector3 alertPosition)
        {
            OnNPCAlerted?.Invoke(npc, alertPosition);
        }

        #endregion

        #region Interaction Event Triggers

        public static void TriggerObjectInteracted(Interaction.IInteractable interactable)
        {
            OnObjectInteracted?.Invoke(interactable);
        }

        public static void TriggerInformationGathered(string information)
        {
            OnInformationGathered?.Invoke(information);
        }

        public static void TriggerDialogueStarted(string dialogue)
        {
            OnDialogueStarted?.Invoke(dialogue);
        }

        public static void TriggerDialogueEnded()
        {
            OnDialogueEnded?.Invoke();
        }

        #endregion

        #region Game State Event Triggers

        public static void TriggerLevelCompleted()
        {
            OnLevelCompleted?.Invoke();
        }

        public static void TriggerGameOver()
        {
            OnGameOver?.Invoke();
        }

        public static void TriggerSceneTransition(string sceneName)
        {
            OnSceneTransition?.Invoke(sceneName);
        }
        
        public static void TriggerObjectiveCompleted(string objectiveText)
        {
            OnObjectiveCompleted?.Invoke(objectiveText);
        }
        
        public static void TriggerNewObjectiveAdded(Core.LevelManager.LevelObjective objective)
        {
            OnNewObjectiveAdded?.Invoke(objective);
        }

        #endregion

        #region QTE Event Triggers

        public static void TriggerQTECompleted(bool success)
        {
            OnQTECompleted?.Invoke(success);
        }

        public static void TriggerQTEStarted(KeyCode key, float timeLimit)
        {
            OnQTEStarted?.Invoke(key, timeLimit);
        }

        #endregion

        #region Audio Event Triggers

        public static void TriggerPlaySFX(AudioClip clip)
        {
            OnPlaySFX?.Invoke(clip);
        }

        public static void TriggerPlayBGM(AudioClip clip)
        {
            OnPlayBGM?.Invoke(clip);
        }

        public static void TriggerMoodChanged(Core.AudioManager.MusicMood mood)
        {
            OnMoodChanged?.Invoke(mood);
        }

        #endregion
        
        #region Rage System Event Triggers
        
        public static void TriggerRageIncreased(float amount)
        {
            OnRageIncreased?.Invoke(amount);
        }
        
        public static void TriggerRevengeReady()
        {
            OnRevengeReady?.Invoke();
        }
        
        public static void TriggerRevengeExecuted()
        {
            OnRevengeExecuted?.Invoke();
        }
        
        #endregion
        
        #region Calligraphy System Event Triggers
        
        public static void TriggerCharacterCompleted(int characterIndex)
        {
            OnCharacterCompleted?.Invoke(characterIndex);
        }
        
        public static void TriggerCalligraphyCompleted()
        {
            OnCalligraphyCompleted?.Invoke();
        }
        
        #endregion
        
        #region Item/Inventory Event Triggers
        
        public static void TriggerItemCollected(string itemId, string itemName)
        {
            OnItemCollected?.Invoke(itemId, itemName);
        }
        
        public static void TriggerItemUsed(string itemId)
        {
            OnItemUsed?.Invoke(itemId);
        }
        
        #endregion

        #region Utility Methods

        /// <summary>
        /// Clear all event subscriptions (useful for scene transitions)
        /// </summary>
        public static void ClearAllEvents()
        {
            OnPlayerStealthChanged = null;
            OnPlayerPositionChanged = null;
            OnPlayerDetected = null;
            OnPlayerDied = null;

            OnNPCStateChanged = null;
            OnNPCDefeated = null;
            OnNPCAlerted = null;

            OnObjectInteracted = null;
            OnInformationGathered = null;
            OnDialogueStarted = null;
            OnDialogueEnded = null;

            OnLevelCompleted = null;
            OnGameOver = null;
            OnSceneTransition = null;
            OnObjectiveCompleted = null;
            OnNewObjectiveAdded = null;

            OnQTECompleted = null;
            OnQTEStarted = null;

            OnPlaySFX = null;
            OnPlayBGM = null;
            OnMoodChanged = null;
            
            OnRageIncreased = null;
            OnRevengeReady = null;
            OnRevengeExecuted = null;
            
            OnCharacterCompleted = null;
            OnCalligraphyCompleted = null;
            
            OnItemCollected = null;
            OnItemUsed = null;
        }
        
        /// <summary>
        /// Log an event for debugging purposes
        /// </summary>
        public static void LogEvent(string eventName, object data = null)
        {
            if (Core.GameManager.Instance != null && Core.GameManager.Instance.IsDebugModeEnabled())
            {
                if (data != null)
                {
                    Debug.Log($"[Event] {eventName}: {data}");
                }
                else
                {
                    Debug.Log($"[Event] {eventName}");
                }
            }
        }

        #endregion
    }
}