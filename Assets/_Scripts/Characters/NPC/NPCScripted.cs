using UnityEngine;
using System.Collections;
using SHGame.Core;

namespace SHGame.Characters.NPC
{
    /// <summary>
    /// Scripted NPC for specific story sequences (like 后槽 in Level 1)
    /// Executes predefined routines and responds to specific triggers
    /// </summary>
    public class NPCScripted : NPCController
    {
        [Header("Scripted Behavior")]
        public ScriptedSequence[] sequences;
        public bool autoStartSequence = true;
        public int startingSequenceIndex = 0;
        
        [Header("QTE Settings")]
        public bool enableQTE = true;
        public KeyCode qteKey = KeyCode.F;
        public float qteTimeLimit = 3f;
        public string qtePromptText = "制服他！";
        
        [Header("Dialogue")]
        public DialogueLine[] dialogueLines;
        public float dialogueDisplayTime = 3f;
        
        // State
        private int currentSequenceIndex = -1;
        private bool isExecutingSequence = false;
        private Coroutine currentSequenceCoroutine;

        [System.Serializable]
        public class ScriptedSequence
        {
            public string sequenceName;
            public ScriptedAction[] actions;
            public bool loopSequence = false;
            public float delayBeforeStart = 0f;
        }

        [System.Serializable]
        public class ScriptedAction
        {
            public ActionType actionType;
            public Vector3 targetPosition;
            public float duration = 1f;
            public float waitTime = 0f;
            public string dialogueText;
            public string animationName;
            public bool waitForPlayerTrigger = false;
        }

        [System.Serializable]
        public class DialogueLine
        {
            public string speakerName;
            public string text;
            public float displayDuration = 3f;
        }

        public enum ActionType
        {
            MoveTo,
            Wait,
            PlayAnimation,
            ShowDialogue,
            ChangeState,
            TriggerEvent,
            WaitForTrigger
        }

        protected override void Start()
        {
            base.Start();
            
            // Override NPC type to scripted
            npcType = NPCType.Scripted;
            
            // Start the initial sequence if auto-start is enabled
            if (autoStartSequence && sequences != null && sequences.Length > 0)
            {
                StartCoroutine(DelayedSequenceStart());
            }
        }

        private IEnumerator DelayedSequenceStart()
        {
            yield return new WaitForSeconds(0.5f); // Wait for initialization
            StartSequence(startingSequenceIndex);
        }

        #region Sequence Management

        public void StartSequence(int sequenceIndex)
        {
            if (sequences == null || sequenceIndex < 0 || sequenceIndex >= sequences.Length)
            {
                Debug.LogError($"Invalid sequence index: {sequenceIndex}");
                return;
            }

            if (currentSequenceCoroutine != null)
            {
                StopCoroutine(currentSequenceCoroutine);
            }

            currentSequenceIndex = sequenceIndex;
            isExecutingSequence = true;
            
            ScriptedSequence sequence = sequences[sequenceIndex];
            currentSequenceCoroutine = StartCoroutine(ExecuteSequence(sequence));
            
            Debug.Log($"Started sequence: {sequence.sequenceName}");
        }

        public void StopCurrentSequence()
        {
            if (currentSequenceCoroutine != null)
            {
                StopCoroutine(currentSequenceCoroutine);
                currentSequenceCoroutine = null;
            }
            
            isExecutingSequence = false;
            currentSequenceIndex = -1;
        }

        private IEnumerator ExecuteSequence(ScriptedSequence sequence)
        {
            if (sequence.delayBeforeStart > 0)
            {
                yield return new WaitForSeconds(sequence.delayBeforeStart);
            }

            do
            {
                foreach (ScriptedAction action in sequence.actions)
                {
                    yield return ExecuteAction(action);
                }
            } while (sequence.loopSequence && isExecutingSequence);

            isExecutingSequence = false;
        }

        private IEnumerator ExecuteAction(ScriptedAction action)
        {
            switch (action.actionType)
            {
                case ActionType.MoveTo:
                    yield return MoveToPosition(action.targetPosition, action.duration);
                    break;
                    
                case ActionType.Wait:
                    yield return new WaitForSeconds(action.duration);
                    break;
                    
                case ActionType.PlayAnimation:
                    PlayAnimationAction(action.animationName);
                    if (action.duration > 0)
                        yield return new WaitForSeconds(action.duration);
                    break;
                    
                case ActionType.ShowDialogue:
                    ShowDialogueAction(action.dialogueText);
                    yield return new WaitForSeconds(action.duration > 0 ? action.duration : dialogueDisplayTime);
                    break;
                    
                case ActionType.ChangeState:
                    // Parse state from action text
                    if (System.Enum.TryParse<NPCState>(action.dialogueText, out NPCState newState))
                    {
                        ChangeState(newState);
                    }
                    break;
                    
                case ActionType.WaitForTrigger:
                    yield return new WaitUntil(() => action.waitForPlayerTrigger);
                    break;
            }

            if (action.waitTime > 0)
            {
                yield return new WaitForSeconds(action.waitTime);
            }
        }

        private IEnumerator MoveToPosition(Vector3 targetPosition, float duration)
        {
            Vector3 startPosition = transform.position;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / duration;
                transform.position = Vector3.Lerp(startPosition, targetPosition, progress);
                
                // Face movement direction
                Vector2 direction = (targetPosition - startPosition).normalized;
                if (direction.magnitude > 0.1f)
                {
                    FaceDirection(direction);
                }
                
                yield return null;
            }

            transform.position = targetPosition;
        }

        private void PlayAnimationAction(string animationName)
        {
            if (animator != null && !string.IsNullOrEmpty(animationName))
            {
                int animHash = Animator.StringToHash(animationName);
                animator.Play(animHash);
            }
        }

        private void ShowDialogueAction(string dialogueText)
        {
            if (UIManager.Instance != null && !string.IsNullOrEmpty(dialogueText))
            {
                UIManager.Instance.ShowSubtitle(dialogueText);
            }
        }

        #endregion

        #region Special Trigger Methods (for Level 1 - 后槽 sequence)

        /// <summary>
        /// Triggered when door is pushed repeatedly - makes NPC come investigate angrily
        /// </summary>
        public void TriggerAngryInvestigation(Vector3 doorPosition)
        {
            StopCurrentSequence();
            
            // Show angry dialogue
            ShowDialogueAction("是甚么人在此推门？我来看看！");
            
            // Change state and move to door
            ChangeState(NPCState.Investigate);
            investigationTarget = doorPosition;
            
            // Start angry investigation coroutine
            StartCoroutine(AngryInvestigationSequence(doorPosition));
        }

        private IEnumerator AngryInvestigationSequence(Vector3 doorPosition)
        {
            // Move to door
            yield return MoveToPosition(doorPosition, 2f);
            
            // Show dialogue about coming out
            ShowDialogueAction("谁敢在此作祟！");
            yield return new WaitForSeconds(1f);
            
            // Trigger QTE opportunity
            if (enableQTE)
            {
                TriggerQTESequence();
            }
        }

        private void TriggerQTESequence()
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowQTEPrompt(qtePromptText, qteKey, OnQTEComplete);
            }
        }

        private void OnQTEComplete(bool success)
        {
            if (success)
            {
                // QTE success - NPC is defeated
                StartCoroutine(DefeatSequence());
            }
            else
            {
                // QTE failure - struggle sequence
                StartCoroutine(StruggleSequence());
            }
        }

        private IEnumerator DefeatSequence()
        {
            // Play defeat animation
            PlayAnimationAction("NPC_Defeated");
            
            // Show dialogue
            ShowDialogueAction("啊！你...你是谁？");
            yield return new WaitForSeconds(2f);
            
            // Start interrogation dialogue tree
            StartInterrogation();
        }

        private IEnumerator StruggleSequence()
        {
            // Show struggle dialogue
            ShowDialogueAction("你这贼人！看我不打死你！");
            yield return new WaitForSeconds(1f);
            
            // Second QTE chance
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowQTEPrompt("再次制服！", qteKey, (success) =>
                {
                    if (success)
                    {
                        StartCoroutine(DefeatSequence());
                    }
                    else
                    {
                        // Game over - player failed
                        GameManager.Instance?.GameOver();
                    }
                });
            }
        }

        private void StartInterrogation()
        {
            // This would integrate with a dialogue system
            // For now, show key information gathering
            
            ShowDialogueAction("你认得我么？");
            StartCoroutine(ShowInterrogationSequence());
        }

        private IEnumerator ShowInterrogationSequence()
        {
            yield return new WaitForSeconds(2f);
            
            ShowDialogueAction("小人...小人不认得...");
            yield return new WaitForSeconds(2f);
            
            ShowDialogueAction("张都监如今在那里？");
            yield return new WaitForSeconds(2f);
            
            ShowDialogueAction("都...都监在鸳鸯楼上饮酒...");
            yield return new WaitForSeconds(2f);
            
            // Add to information log
            if (UIManager.Instance != null)
            {
                UIManager.Instance.AddToInformationLog("目标在鸳鸯楼上饮酒");
            }
            
            ShowDialogueAction("这话是实么？");
            yield return new WaitForSeconds(2f);
            
            ShowDialogueAction("千真万确！小人不敢撒谎！");
            yield return new WaitForSeconds(2f);
            
            // Final dialogue before execution
            ShowDialogueAction("恁地却饶你不得！");
            yield return new WaitForSeconds(1f);
            
            // Execute NPC
            SetDead();
            
            // Show execution feedback
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayProcessingHit();
            }
            
            // Enable player interaction with clothes/items
            EnablePostExecutionInteractions();
        }

        private void EnablePostExecutionInteractions()
        {
            // Enable nearby interactables (clothes, lamp, etc.)
            Collider2D[] nearbyColliders = Physics2D.OverlapCircleAll(transform.position, 3f);
            
            foreach (Collider2D col in nearbyColliders)
            {
                var interactable = col.GetComponent<Interaction.InteractableBase>();
                if (interactable != null)
                {
                    interactable.SetInteractionEnabled(true);
                }
            }
            
            // Show completion message
            if (UIManager.Instance != null)
            {
                UIManager.Instance.AddToInformationLog("已制服后槽，获得情报和伪装");
            }
        }

        #endregion

        #region Public Interface

        public void ExecuteDialogue(int dialogueIndex)
        {
            if (dialogueLines != null && dialogueIndex >= 0 && dialogueIndex < dialogueLines.Length)
            {
                DialogueLine dialogue = dialogueLines[dialogueIndex];
                ShowDialogueAction($"{dialogue.speakerName}: {dialogue.text}");
            }
        }

        public bool IsExecutingSequence()
        {
            return isExecutingSequence;
        }

        public int GetCurrentSequenceIndex()
        {
            return currentSequenceIndex;
        }

        public void SetSequenceTrigger(int sequenceIndex, bool triggered)
        {
            if (sequences != null && sequenceIndex >= 0 && sequenceIndex < sequences.Length)
            {
                // This could be expanded to handle specific trigger logic
                if (triggered)
                {
                    StartSequence(sequenceIndex);
                }
            }
        }

        #endregion

        #region Debug

        protected override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();
            
            // Draw sequence information
            #if UNITY_EDITOR
            if (sequences != null && currentSequenceIndex >= 0 && currentSequenceIndex < sequences.Length)
            {
                UnityEditor.Handles.Label(transform.position + Vector3.up * 3f, 
                    $"Sequence: {sequences[currentSequenceIndex].sequenceName}\nExecuting: {isExecutingSequence}");
            }
            #endif
        }

        #endregion
    }
}