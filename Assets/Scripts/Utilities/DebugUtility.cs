using UnityEngine;

namespace SHGame.Utilities
{
    /// <summary>
    /// Debug utility functions for development and testing
    /// Provides enhanced debug capabilities for the Wu Song game
    /// </summary>
    public static class DebugUtility
    {
        public static bool isDebugMode = true;

        #region Enhanced Debug Logging

        public static void Log(string message, Object context = null)
        {
            if (!isDebugMode) return;
            Debug.Log($"[SHGame] {message}", context);
        }

        public static void LogWarning(string message, Object context = null)
        {
            if (!isDebugMode) return;
            Debug.LogWarning($"[SHGame] {message}", context);
        }

        public static void LogError(string message, Object context = null)
        {
            Debug.LogError($"[SHGame] {message}", context);
        }

        public static void LogPlayer(string message)
        {
            if (!isDebugMode) return;
            Debug.Log($"[Player] {message}");
        }

        public static void LogNPC(string message, string npcName = "")
        {
            if (!isDebugMode) return;
            Debug.Log($"[NPC{(string.IsNullOrEmpty(npcName) ? "" : $":{npcName}")}] {message}");
        }

        public static void LogStealth(string message)
        {
            if (!isDebugMode) return;
            Debug.Log($"[Stealth] {message}");
        }

        public static void LogInteraction(string message)
        {
            if (!isDebugMode) return;
            Debug.Log($"[Interaction] {message}");
        }

        public static void LogAudio(string message)
        {
            if (!isDebugMode) return;
            Debug.Log($"[Audio] {message}");
        }

        #endregion

        #region Visual Debug Helpers

        public static void DrawCircle(Vector3 center, float radius, Color color, float duration = 0f)
        {
            if (!isDebugMode) return;
            
            int segments = 36;
            float angleStep = 360f / segments;
            
            for (int i = 0; i < segments; i++)
            {
                float angle1 = i * angleStep * Mathf.Deg2Rad;
                float angle2 = (i + 1) * angleStep * Mathf.Deg2Rad;
                
                Vector3 point1 = center + new Vector3(Mathf.Cos(angle1), Mathf.Sin(angle1), 0) * radius;
                Vector3 point2 = center + new Vector3(Mathf.Cos(angle2), Mathf.Sin(angle2), 0) * radius;
                
                Debug.DrawLine(point1, point2, color, duration);
            }
        }

        public static void DrawArrow(Vector3 from, Vector3 to, Color color, float duration = 0f)
        {
            if (!isDebugMode) return;
            
            Debug.DrawLine(from, to, color, duration);
            
            Vector3 direction = (to - from).normalized;
            Vector3 right = Vector3.Cross(direction, Vector3.forward).normalized;
            
            float arrowSize = 0.3f;
            Vector3 arrowPoint1 = to - direction * arrowSize + right * arrowSize * 0.5f;
            Vector3 arrowPoint2 = to - direction * arrowSize - right * arrowSize * 0.5f;
            
            Debug.DrawLine(to, arrowPoint1, color, duration);
            Debug.DrawLine(to, arrowPoint2, color, duration);
        }

        public static void DrawFieldOfView(Vector3 position, Vector3 forward, float angle, float range, Color color, float duration = 0f)
        {
            if (!isDebugMode) return;
            
            Vector3 leftBoundary = Quaternion.Euler(0, 0, angle / 2f) * forward;
            Vector3 rightBoundary = Quaternion.Euler(0, 0, -angle / 2f) * forward;
            
            Debug.DrawRay(position, leftBoundary * range, color, duration);
            Debug.DrawRay(position, rightBoundary * range, color, duration);
            
            // Draw arc
            int arcSegments = Mathf.RoundToInt(angle / 5f);
            for (int i = 0; i < arcSegments; i++)
            {
                float currentAngle = -angle / 2f + (angle / arcSegments) * i;
                float nextAngle = -angle / 2f + (angle / arcSegments) * (i + 1);
                
                Vector3 currentPoint = position + Quaternion.Euler(0, 0, currentAngle) * forward * range;
                Vector3 nextPoint = position + Quaternion.Euler(0, 0, nextAngle) * forward * range;
                
                Debug.DrawLine(currentPoint, nextPoint, color, duration);
            }
        }

        #endregion

        #region Performance Monitoring

        private static System.Diagnostics.Stopwatch performanceTimer = new System.Diagnostics.Stopwatch();

        public static void StartPerformanceTimer()
        {
            if (!isDebugMode) return;
            performanceTimer.Restart();
        }

        public static void StopPerformanceTimer(string operationName)
        {
            if (!isDebugMode) return;
            performanceTimer.Stop();
            Debug.Log($"[Performance] {operationName} took {performanceTimer.ElapsedMilliseconds}ms");
        }

        #endregion

        #region Game State Debug

        public static void LogGameState()
        {
            if (!isDebugMode) return;
            
            var gameManager = Core.GameManager.Instance;
            if (gameManager != null)
            {
                Debug.Log($"[GameState] Current State: {gameManager.currentGameState}, Paused: {gameManager.isPaused}");
            }
        }

        public static void LogPlayerState()
        {
            if (!isDebugMode) return;
            
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                var controller = player.GetComponent<Characters.Player.PlayerController>();
                var stealth = player.GetComponent<Characters.Player.PlayerStealth>();
                
                if (controller != null && stealth != null)
                {
                    Debug.Log($"[PlayerState] Position: {player.transform.position}, " +
                             $"Moving: {controller.IsPlayerMoving()}, " +
                             $"Crouching: {controller.IsPlayerCrouching()}, " +
                             $"Hidden: {stealth.IsPlayerHidden()}");
                }
            }
        }

        public static void LogNPCStates()
        {
            if (!isDebugMode) return;
            
            var npcs = GameObject.FindObjectsOfType<Characters.NPC.NPCController>();
            foreach (var npc in npcs)
            {
                Debug.Log($"[NPCState] {npc.name}: State={npc.GetCurrentState()}, " +
                         $"PlayerDetected={npc.IsPlayerDetected()}");
            }
        }

        #endregion

        #region Cheat Functions (Development Only)

        public static void TeleportPlayerTo(Vector3 position)
        {
            if (!isDebugMode) return;
            
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                player.transform.position = position;
                Debug.Log($"[Cheat] Teleported player to {position}");
            }
        }

        public static void SetPlayerInvincible(bool invincible)
        {
            if (!isDebugMode) return;
            
            // This would be implemented when health system is added
            Debug.Log($"[Cheat] Player invincibility: {invincible}");
        }

        public static void KillAllNPCs()
        {
            if (!isDebugMode) return;
            
            var npcs = GameObject.FindObjectsOfType<Characters.NPC.NPCController>();
            foreach (var npc in npcs)
            {
                npc.SetDead();
            }
            Debug.Log($"[Cheat] Killed {npcs.Length} NPCs");
        }

        public static void CompleteCurrentLevel()
        {
            if (!isDebugMode) return;
            
            GameEvents.TriggerLevelCompleted();
            Debug.Log("[Cheat] Level completed");
        }

        #endregion

        #region Input Debug

        public static void ShowInputDebugInfo()
        {
            if (!isDebugMode) return;
            
            if (Input.anyKey)
            {
                string pressedKeys = "";
                for (KeyCode key = KeyCode.A; key <= KeyCode.Z; key++)
                {
                    if (Input.GetKeyDown(key))
                    {
                        pressedKeys += key.ToString() + " ";
                    }
                }
                
                if (!string.IsNullOrEmpty(pressedKeys))
                {
                    Debug.Log($"[Input] Keys pressed: {pressedKeys}");
                }
            }
        }

        #endregion
    }

    /// <summary>
    /// Debug UI component for in-game debug information
    /// Attach to a GameObject to display debug info on screen
    /// </summary>
    public class DebugUI : MonoBehaviour
    {
        [Header("Debug UI Settings")]
        public bool showFPS = true;
        public bool showPlayerInfo = true;
        public bool showGameState = true;
        public bool showMemoryUsage = false;
        
        private float fps;
        private float updateTimer;
        
        private void Update()
        {
            if (!DebugUtility.isDebugMode) return;
            
            // Update FPS counter
            updateTimer += Time.deltaTime;
            if (updateTimer >= 0.2f)
            {
                fps = 1f / Time.deltaTime;
                updateTimer = 0f;
            }
        }

        private void OnGUI()
        {
            if (!DebugUtility.isDebugMode) return;
            
            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.BeginVertical("box");
            
            GUILayout.Label("Wu Song Game Debug", GUI.skin.label);
            
            if (showFPS)
            {
                GUILayout.Label($"FPS: {fps:F1}");
            }
            
            if (showGameState && Core.GameManager.Instance != null)
            {
                GUILayout.Label($"Game State: {Core.GameManager.Instance.currentGameState}");
                GUILayout.Label($"Paused: {Core.GameManager.Instance.isPaused}");
            }
            
            if (showPlayerInfo)
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    var stealth = player.GetComponent<Characters.Player.PlayerStealth>();
                    if (stealth != null)
                    {
                        GUILayout.Label($"Player Hidden: {stealth.IsPlayerHidden()}");
                        GUILayout.Label($"Light Intensity: {stealth.GetCurrentLightIntensity():F2}");
                    }
                }
            }
            
            if (showMemoryUsage)
            {
                GUILayout.Label($"Memory: {System.GC.GetTotalMemory(false) / 1024 / 1024}MB");
            }
            
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
    }
}