using UnityEngine;
using System.Collections;
using SHGame.Utilities;
using SHGame.Characters.NPC;

namespace SHGame.Core
{
    /// <summary>
    /// Manages level-specific logic, objectives, and progression
    /// Handles level completion conditions and story progression
    /// </summary>
    public class LevelManager : MonoBehaviour
    {
        public static LevelManager Instance { get; private set; }

        [Header("Level Information")]
        public string levelName = "Level 1";
        public string levelDescription = "夜潜张府";
        public int levelIndex = 1;

        [Header("Objectives")]
        public LevelObjective[] objectives;
        public bool requireAllObjectives = true;
        public bool showObjectivesOnStart = true;

        [Header("Completion Settings")]
        public string nextLevelScene;
        public float completionDelay = 2f;
        public bool autoAdvanceToNextLevel = true;

        [Header("Debug")]
        public bool showObjectiveDebug = true;

        // State
        private int completedObjectives = 0;
        private bool levelCompleted = false;
        private bool isInitialized = false;

        [System.Serializable]
        public class LevelObjective
        {
            public string objectiveText;
            public ObjectiveType type;
            public bool isCompleted = false;
            public bool isOptional = false;
            public bool showInUI = true;
            
            // Type-specific parameters
            public string targetTag = "";
            public string requiredInformation = "";
            public int requiredCount = 1;
            public Vector3 targetPosition;
            public float completionRadius = 2f;
        }

        public enum ObjectiveType
        {
            DefeatNPC,          // Kill or subdue specific NPC
            GatherInformation,  // Collect specific intelligence
            ReachLocation,      // Get to a specific position
            InteractWithObject, // Use specific interactable
            AvoidDetection,     // Complete without being seen
            ListenToDialogue,   // Eavesdrop on conversation
            CompleteSequence    // Complete a scripted sequence
        }

        private void Awake()
        {
            // Set up singleton reference
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                // Another LevelManager already exists, destroy this one
                Debug.LogWarning($"Multiple LevelManager instances detected. Destroying duplicate in {gameObject.name}.");
                Destroy(this);
                return;
            }
        }

        private void Start()
        {
            InitializeLevel();
            SubscribeToEvents();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
            
            // Clear instance if this was the active one
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void InitializeLevel()
        {
            if (isInitialized) return;
            
            DebugUtility.Log($"Initializing level: {levelName}");
            
            // Reset objective states
            foreach (var objective in objectives)
            {
                objective.isCompleted = false;
            }
            
            completedObjectives = 0;
            levelCompleted = false;

            // Show level intro
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowSubtitle($"{levelName}: {levelDescription}");
                
                if (showObjectivesOnStart)
                {
                    StartCoroutine(ShowObjectivesWithDelay(2f));
                }
            }

            // Set appropriate mood music
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayMusicForMood(AudioManager.MusicMood.Tension);
            }
            
            isInitialized = true;
        }
        
        private IEnumerator ShowObjectivesWithDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            
            // Add all non-hidden objectives to the information log
            if (UIManager.Instance != null)
            {
                foreach (var objective in objectives)
                {
                    if (objective.showInUI)
                    {
                        string optionalText = objective.isOptional ? " (可选)" : "";
                        UIManager.Instance.AddToInformationLog($"目标: {objective.objectiveText}{optionalText}");
                    }
                }
            }
        }

        private void SubscribeToEvents()
        {
            GameEvents.OnNPCDefeated += OnNPCDefeated;
            GameEvents.OnInformationGathered += OnInformationGathered;
            GameEvents.OnObjectInteracted += OnObjectInteracted;
            GameEvents.OnPlayerDetected += OnPlayerDetected;
            GameEvents.OnDialogueStarted += OnDialogueStarted;
        }

        private void UnsubscribeFromEvents()
        {
            GameEvents.OnNPCDefeated -= OnNPCDefeated;
            GameEvents.OnInformationGathered -= OnInformationGathered;
            GameEvents.OnObjectInteracted -= OnObjectInteracted;
            GameEvents.OnPlayerDetected -= OnPlayerDetected;
            GameEvents.OnDialogueStarted -= OnDialogueStarted;
        }

        #region Event Handlers

        private void OnNPCDefeated(NPCController npc)
        {
            CheckObjectiveCompletion(ObjectiveType.DefeatNPC, npc.gameObject.tag);
        }

        private void OnInformationGathered(string information)
        {
            CheckObjectiveCompletion(ObjectiveType.GatherInformation, information);
        }

        private void OnObjectInteracted(Interaction.IInteractable interactable)
        {
            var obj = interactable as MonoBehaviour;
            if (obj != null)
            {
                CheckObjectiveCompletion(ObjectiveType.InteractWithObject, obj.tag);
            }
        }

        private void OnPlayerDetected()
        {
            // Handle detection - might fail stealth objectives
            for (int i = 0; i < objectives.Length; i++)
            {
                if (objectives[i].type == ObjectiveType.AvoidDetection && !objectives[i].isCompleted)
                {
                    DebugUtility.LogError("Stealth objective failed - player detected!");
                    // Could trigger game over or mark objective as failed
                }
            }
        }

        private void OnDialogueStarted(string dialogue)
        {
            CheckObjectiveCompletion(ObjectiveType.ListenToDialogue, dialogue);
        }

        #endregion

        #region Objective Management

        private void CheckObjectiveCompletion(ObjectiveType type, string parameter = "")
        {
            for (int i = 0; i < objectives.Length; i++)
            {
                if (objectives[i].isCompleted || objectives[i].type != type) continue;

                bool objectiveComplete = false;

                switch (type)
                {
                    case ObjectiveType.DefeatNPC:
                        objectiveComplete = string.IsNullOrEmpty(objectives[i].targetTag) || 
                                          objectives[i].targetTag == parameter;
                        break;

                    case ObjectiveType.GatherInformation:
                        objectiveComplete = string.IsNullOrEmpty(objectives[i].requiredInformation) || 
                                          parameter.Contains(objectives[i].requiredInformation);
                        break;

                    case ObjectiveType.InteractWithObject:
                        objectiveComplete = string.IsNullOrEmpty(objectives[i].targetTag) || 
                                          objectives[i].targetTag == parameter;
                        break;

                    case ObjectiveType.ListenToDialogue:
                        objectiveComplete = string.IsNullOrEmpty(objectives[i].requiredInformation) || 
                                          parameter.Contains(objectives[i].requiredInformation);
                        break;

                    case ObjectiveType.ReachLocation:
                        // This needs to be checked in Update
                        break;

                    case ObjectiveType.AvoidDetection:
                        // This is checked when player is detected
                        break;

                    case ObjectiveType.CompleteSequence:
                        // This would be triggered by specific sequence completion
                        break;
                }

                if (objectiveComplete)
                {
                    CompleteObjective(i);
                }
            }
        }

        private void CompleteObjective(int objectiveIndex)
        {
            if (objectiveIndex < 0 || objectiveIndex >= objectives.Length) return;
            if (objectives[objectiveIndex].isCompleted) return;

            objectives[objectiveIndex].isCompleted = true;
            completedObjectives++;

            DebugUtility.Log($"Objective completed: {objectives[objectiveIndex].objectiveText}");

            // Show UI feedback
            if (UIManager.Instance != null && objectives[objectiveIndex].showInUI)
            {
                UIManager.Instance.ShowSubtitle($"目标完成: {objectives[objectiveIndex].objectiveText}");
                UIManager.Instance.AddToInformationLog($"✓ {objectives[objectiveIndex].objectiveText}");
            }

            // Trigger objective completed event
            GameEvents.TriggerObjectiveCompleted(objectives[objectiveIndex].objectiveText);

            // Check if level is complete
            CheckLevelCompletion();
        }

        public void ForceCompleteObjective(int index)
        {
            if (index >= 0 && index < objectives.Length)
            {
                CompleteObjective(index);
            }
        }

        public void ForceCompleteObjective(string objectiveText)
        {
            for (int i = 0; i < objectives.Length; i++)
            {
                if (objectives[i].objectiveText == objectiveText)
                {
                    CompleteObjective(i);
                    break;
                }
            }
        }

        #endregion

        #region Level Completion

        private void CheckLevelCompletion()
        {
            if (levelCompleted) return;

            bool allRequiredComplete = true;
            int requiredObjectives = 0;
            int completedRequired = 0;

            foreach (var objective in objectives)
            {
                if (!objective.isOptional)
                {
                    requiredObjectives++;
                    if (objective.isCompleted)
                    {
                        completedRequired++;
                    }
                    else
                    {
                        allRequiredComplete = false;
                    }
                }
            }

            bool shouldComplete = requireAllObjectives ? allRequiredComplete : (completedRequired > 0);

            if (shouldComplete)
            {
                StartCoroutine(CompleteLevelSequence());
            }
        }

        private IEnumerator CompleteLevelSequence()
        {
            levelCompleted = true;

            DebugUtility.Log($"Level {levelName} completed!");

            // Show completion message
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowSubtitle($"{levelName} 完成！");
            }

            // Play completion music
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayMusicForMood(AudioManager.MusicMood.Calm);
            }

            // Trigger game events
            GameEvents.TriggerLevelCompleted();

            yield return new WaitForSeconds(completionDelay);

            // Advance to next level if specified
            if (autoAdvanceToNextLevel && !string.IsNullOrEmpty(nextLevelScene))
            {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.LoadScene(nextLevelScene);
                }
            }
        }

        public void ForceCompleteLevel()
        {
            if (levelCompleted) return;
            
            StartCoroutine(CompleteLevelSequence());
        }

        #endregion

        #region Update Loop

        private void Update()
        {
            // Check position-based objectives
            CheckPositionObjectives();

            // Debug input
            if (showObjectiveDebug && Input.GetKeyDown(KeyCode.O))
            {
                ShowObjectiveStatus();
            }
        }

        private void CheckPositionObjectives()
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) return;

            for (int i = 0; i < objectives.Length; i++)
            {
                if (objectives[i].isCompleted || objectives[i].type != ObjectiveType.ReachLocation) continue;

                float distance = Vector3.Distance(player.transform.position, objectives[i].targetPosition);
                if (distance <= objectives[i].completionRadius)
                {
                    CompleteObjective(i);
                }
            }
        }

        #endregion

        #region Public Interface

        public bool IsLevelCompleted()
        {
            return levelCompleted;
        }

        public float GetCompletionPercentage()
        {
            if (objectives.Length == 0) return 1f;
            return (float)completedObjectives / objectives.Length;
        }

        public int GetCompletedObjectiveCount()
        {
            return completedObjectives;
        }

        public int GetTotalObjectiveCount()
        {
            return objectives.Length;
        }

        public LevelObjective[] GetObjectives()
        {
            return objectives;
        }

        public void ShowObjectiveStatus()
        {
            DebugUtility.Log($"=== {levelName} Objectives ===");
            for (int i = 0; i < objectives.Length; i++)
            {
                string status = objectives[i].isCompleted ? "✓" : "○";
                string optional = objectives[i].isOptional ? " (Optional)" : "";
                DebugUtility.Log($"{status} {objectives[i].objectiveText}{optional}");
            }
            DebugUtility.Log($"Progress: {completedObjectives}/{objectives.Length}");
        }

        public bool HasObjective(string objectiveText)
        {
            foreach (var objective in objectives)
            {
                if (objective.objectiveText == objectiveText)
                {
                    return true;
                }
            }
            return false;
        }

        public bool IsObjectiveCompleted(string objectiveText)
        {
            foreach (var objective in objectives)
            {
                if (objective.objectiveText == objectiveText)
                {
                    return objective.isCompleted;
                }
            }
            return false;
        }

        public void ResetLevel()
        {
            // Reset objective states
            foreach (var objective in objectives)
            {
                objective.isCompleted = false;
            }
            
            completedObjectives = 0;
            levelCompleted = false;
            isInitialized = false;
            
            // Re-initialize
            InitializeLevel();
        }

        #endregion

        #region Gizmos

        private void OnDrawGizmos()
        {
            if (objectives == null) return;

            // Draw position objectives
            foreach (var objective in objectives)
            {
                if (objective.type == ObjectiveType.ReachLocation)
                {
                    Gizmos.color = objective.isCompleted ? Color.green : Color.yellow;
                    Gizmos.DrawWireSphere(objective.targetPosition, objective.completionRadius);
                    
                    #if UNITY_EDITOR
                    UnityEditor.Handles.Label(objective.targetPosition + Vector3.up, objective.objectiveText);
                    #endif
                }
            }
        }

        #endregion
    }
}