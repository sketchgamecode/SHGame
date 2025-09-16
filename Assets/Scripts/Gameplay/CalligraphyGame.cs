using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using SHGame.Core;

namespace SHGame.Gameplay
{
    /// <summary>
    /// Interactive calligraphy system for writing characters on walls
    /// Used in the finale to write "The killer is Wu Song" on the wall
    /// </summary>
    public class CalligraphyGame : MonoBehaviour
    {
        [Header("UI References")]
        public GameObject calligraphyPanel;
        public RectTransform brushArea;
        public RawImage drawingCanvas;
        public RenderTexture canvasTexture;
        public TextMeshProUGUI instructionText;
        public TextMeshProUGUI progressText;
        public Button clearButton;
        public Button submitButton;
        public Button closeButton;
        
        [Header("Brush Settings")]
        public Material brushMaterial;
        public float brushSize = 10f;
        public Color brushColor = Color.red;
        public Texture2D brushTexture;
        public Texture2D completeTexture; // Texture to show when complete
        
        [Header("Game Settings")]
        public string targetText = "杀人者打虎武松也";
        public float completionThreshold = 0.7f;
        public float matchingThreshold = 0.6f;
        public float minStrokeLength = 20f;
        
        [Header("Character Templates")]
        public List<CalligraphyCharacter> characterTemplates;
        
        [Header("Audio")]
        public AudioClip brushStrokeSound;
        public AudioClip successSound;
        public AudioClip failureSound;
        public AudioClip completionSound;
        
        // State
        private bool isDrawing = false;
        private Vector2 lastDrawPosition;
        private List<Vector2> currentStroke = new List<Vector2>();
        private List<List<Vector2>> strokes = new List<List<Vector2>>();
        private int currentCharacterIndex = 0;
        private bool isGameActive = false;
        private bool characterCompleted = false;
        private Texture2D drawingTexture;
        private int completedCharacters = 0;
        
        // References
        private Camera mainCamera;
        
        [System.Serializable]
        public class CalligraphyCharacter
        {
            public string character;
            public Texture2D templateTexture;
            public float difficultyMultiplier = 1f;
            public string hintText;
            
            [HideInInspector]
            public bool isCompleted = false;
        }
        
        private void Awake()
        {
            // Initialize components
            mainCamera = Camera.main;
            
            // Prepare drawing texture
            if (canvasTexture != null)
            {
                drawingTexture = new Texture2D(canvasTexture.width, canvasTexture.height, TextureFormat.RGBA32, false);
                ClearCanvas();
            }
            
            // Add button listeners
            if (clearButton != null)
                clearButton.onClick.AddListener(ClearCanvas);
                
            if (submitButton != null)
                submitButton.onClick.AddListener(SubmitDrawing);
                
            if (closeButton != null)
                closeButton.onClick.AddListener(CloseCalligraphyGame);
        }
        
        private void Start()
        {
            // Hide calligraphy panel initially
            if (calligraphyPanel != null)
                calligraphyPanel.SetActive(false);
        }
        
        private void Update()
        {
            if (!isGameActive) return;
            
            HandleInput();
        }
        
        private void HandleInput()
        {
            // Convert screen position to canvas local position
            Vector2 mousePosition = Input.mousePosition;
            Vector2 localPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                brushArea, mousePosition, null, out localPos);
                
            // Normalize coordinates (0-1)
            Vector2 normalizedPos = new Vector2(
                (localPos.x + brushArea.rect.width/2) / brushArea.rect.width,
                (localPos.y + brushArea.rect.height/2) / brushArea.rect.height);
                
            // Check if position is within canvas bounds
            bool isWithinBounds = normalizedPos.x >= 0 && normalizedPos.x <= 1 && 
                                  normalizedPos.y >= 0 && normalizedPos.y <= 1;
                                  
            // Handle drawing
            if (Input.GetMouseButtonDown(0) && isWithinBounds)
            {
                StartDrawing(normalizedPos);
            }
            else if (Input.GetMouseButton(0) && isDrawing)
            {
                if (isWithinBounds)
                {
                    ContinueDrawing(normalizedPos);
                }
                else
                {
                    EndDrawing();
                }
            }
            else if (Input.GetMouseButtonUp(0) && isDrawing)
            {
                EndDrawing();
            }
        }
        
        private void StartDrawing(Vector2 position)
        {
            isDrawing = true;
            lastDrawPosition = position;
            
            // Start new stroke
            currentStroke.Clear();
            currentStroke.Add(position);
            
            // Draw first point
            DrawPoint(position);
            
            // Play brush sound
            PlayBrushSound();
        }
        
        private void ContinueDrawing(Vector2 position)
        {
            // Draw line from last position to current
            DrawLine(lastDrawPosition, position);
            
            // Add point to stroke
            currentStroke.Add(position);
            
            // Update last position
            lastDrawPosition = position;
            
            // Play brush sound occasionally
            if (Random.value < 0.1f)
                PlayBrushSound();
        }
        
        private void EndDrawing()
        {
            isDrawing = false;
            
            // Add completed stroke if it's long enough
            if (IsStrokeLongEnough())
            {
                strokes.Add(new List<Vector2>(currentStroke));
            }
            
            // Apply to texture
            ApplyToTexture();
        }
        
        private void DrawPoint(Vector2 position)
        {
            // Convert normalized position to pixel coordinates
            int x = Mathf.RoundToInt(position.x * drawingTexture.width);
            int y = Mathf.RoundToInt(position.y * drawingTexture.height);
            
            // Draw circle
            int radius = Mathf.RoundToInt(brushSize / 2);
            for (int yOffset = -radius; yOffset <= radius; yOffset++)
            {
                for (int xOffset = -radius; xOffset <= radius; xOffset++)
                {
                    if (xOffset * xOffset + yOffset * yOffset <= radius * radius)
                    {
                        int drawX = x + xOffset;
                        int drawY = y + yOffset;
                        
                        if (drawX >= 0 && drawX < drawingTexture.width && 
                            drawY >= 0 && drawY < drawingTexture.height)
                        {
                            drawingTexture.SetPixel(drawX, drawY, brushColor);
                        }
                    }
                }
            }
            
            // Apply changes
            drawingTexture.Apply();
            
            // Update canvas
            Graphics.Blit(drawingTexture, canvasTexture);
        }
        
        private void DrawLine(Vector2 start, Vector2 end)
        {
            // Calculate distance and direction
            float distance = Vector2.Distance(start, end);
            Vector2 direction = (end - start).normalized;
            
            // Draw points along the line
            int steps = Mathf.CeilToInt(distance * 50); // Adjust factor based on desired density
            for (int i = 0; i < steps; i++)
            {
                float t = (float)i / steps;
                Vector2 point = Vector2.Lerp(start, end, t);
                DrawPoint(point);
            }
        }
        
        private void ApplyToTexture()
        {
            // Update canvas texture
            Graphics.Blit(drawingTexture, canvasTexture);
        }
        
        private void ClearCanvas()
        {
            // Clear texture
            Color clearColor = new Color(0, 0, 0, 0);
            for (int y = 0; y < drawingTexture.height; y++)
            {
                for (int x = 0; x < drawingTexture.width; x++)
                {
                    drawingTexture.SetPixel(x, y, clearColor);
                }
            }
            
            drawingTexture.Apply();
            
            // Clear canvas
            Graphics.Blit(drawingTexture, canvasTexture);
            
            // Clear strokes
            strokes.Clear();
            currentStroke.Clear();
        }
        
        private bool IsStrokeLongEnough()
        {
            if (currentStroke.Count < 2) return false;
            
            float strokeLength = 0f;
            for (int i = 1; i < currentStroke.Count; i++)
            {
                strokeLength += Vector2.Distance(
                    currentStroke[i-1] * new Vector2(drawingTexture.width, drawingTexture.height),
                    currentStroke[i] * new Vector2(drawingTexture.width, drawingTexture.height));
            }
            
            return strokeLength >= minStrokeLength;
        }
        
        private void SubmitDrawing()
        {
            if (!isGameActive || characterTemplates.Count == 0) return;
            
            // Check if current character is complete
            CalligraphyCharacter currentChar = characterTemplates[currentCharacterIndex];
            float matchScore = CalculateMatchScore(currentChar.templateTexture);
            
            bool success = matchScore >= matchingThreshold;
            
            if (success)
            {
                // Mark as completed
                currentChar.isCompleted = true;
                completedCharacters++;
                
                // Show success feedback
                ShowFeedback(true, matchScore);
                
                // Check if we should advance to next character
                if (currentCharacterIndex < characterTemplates.Count - 1)
                {
                    StartCoroutine(AdvanceToNextCharacter());
                }
                else
                {
                    // All characters completed
                    StartCoroutine(CompleteCalligraphyGame());
                }
            }
            else
            {
                // Show failure feedback
                ShowFeedback(false, matchScore);
            }
        }
        
        private float CalculateMatchScore(Texture2D template)
        {
            // This is a simplified matching algorithm
            // A more sophisticated version would use image processing techniques
            
            // Copy the canvas texture to a readable texture
            RenderTexture tempRT = RenderTexture.GetTemporary(
                canvasTexture.width, canvasTexture.height, 0, RenderTextureFormat.ARGB32);
            Graphics.Blit(canvasTexture, tempRT);
            
            // Read the pixels
            RenderTexture.active = tempRT;
            Texture2D tempTex = new Texture2D(tempRT.width, tempRT.height);
            tempTex.ReadPixels(new Rect(0, 0, tempRT.width, tempRT.height), 0, 0);
            tempTex.Apply();
            RenderTexture.active = null;
            RenderTexture.ReleaseTemporary(tempRT);
            
            // Compare with template
            int totalPixels = 0;
            int matchedPixels = 0;
            int drawnPixels = 0;
            
            for (int y = 0; y < tempTex.height; y++)
            {
                for (int x = 0; x < tempTex.width; x++)
                {
                    Color drawnColor = tempTex.GetPixel(x, y);
                    Color templateColor = template.GetPixel(
                        Mathf.Clamp(x, 0, template.width-1), 
                        Mathf.Clamp(y, 0, template.height-1));
                        
                    bool isDrawnPixel = drawnColor.a > 0.5f;
                    bool isTemplatePixel = templateColor.a > 0.5f;
                    
                    if (isTemplatePixel)
                    {
                        totalPixels++;
                        if (isDrawnPixel) matchedPixels++;
                    }
                    
                    if (isDrawnPixel) drawnPixels++;
                }
            }
            
            // Calculate coverage and accuracy
            float coverage = (float)matchedPixels / Mathf.Max(1, totalPixels);
            float accuracy = (float)matchedPixels / Mathf.Max(1, drawnPixels);
            
            // Combine into a single score
            float score = (coverage * 0.7f) + (accuracy * 0.3f);
            
            // Clean up
            Destroy(tempTex);
            
            return score;
        }
        
        private void ShowFeedback(bool success, float score)
        {
            // Update instruction text with feedback
            if (instructionText != null)
            {
                if (success)
                {
                    instructionText.text = $"完成！匹配度：{score:P0}";
                    instructionText.color = Color.green;
                    
                    // Play success sound
                    if (AudioManager.Instance != null && successSound != null)
                    {
                        AudioManager.Instance.PlaySFX(successSound);
                    }
                }
                else
                {
                    instructionText.text = $"不够像，请重试。匹配度：{score:P0}";
                    instructionText.color = Color.red;
                    
                    // Play failure sound
                    if (AudioManager.Instance != null && failureSound != null)
                    {
                        AudioManager.Instance.PlaySFX(failureSound);
                    }
                }
            }
            
            // Update progress text
            UpdateProgressText();
        }
        
        private IEnumerator AdvanceToNextCharacter()
        {
            // Short delay before advancing
            yield return new WaitForSeconds(1.0f);
            
            // Clear canvas
            ClearCanvas();
            
            // Advance to next character
            currentCharacterIndex++;
            
            // Update UI
            UpdateCharacterUI();
        }
        
        private IEnumerator CompleteCalligraphyGame()
        {
            // Play completion sound
            if (AudioManager.Instance != null && completionSound != null)
            {
                AudioManager.Instance.PlaySFX(completionSound);
            }
            
            // Show completion message
            if (instructionText != null)
            {
                instructionText.text = "题字完成！";
                instructionText.color = Color.green;
            }
            
            // Show complete texture if available
            if (completeTexture != null)
            {
                // Copy complete texture to canvas
                Graphics.Blit(completeTexture, canvasTexture);
            }
            
            // Short delay
            yield return new WaitForSeconds(2.0f);
            
            // Close calligraphy game
            CloseCalligraphyGame();
            
            // Trigger completion event
            TriggerCompletion();
        }
        
        private void UpdateCharacterUI()
        {
            if (currentCharacterIndex < 0 || currentCharacterIndex >= characterTemplates.Count)
                return;
                
            CalligraphyCharacter currentChar = characterTemplates[currentCharacterIndex];
            
            // Update instruction text
            if (instructionText != null)
            {
                string baseText = $"请书写：{currentChar.character}";
                if (!string.IsNullOrEmpty(currentChar.hintText))
                {
                    baseText += $"\n{currentChar.hintText}";
                }
                
                instructionText.text = baseText;
                instructionText.color = Color.white;
            }
            
            // Update progress text
            UpdateProgressText();
        }
        
        private void UpdateProgressText()
        {
            if (progressText != null)
            {
                progressText.text = $"进度: {completedCharacters}/{characterTemplates.Count}";
            }
        }
        
        private void PlayBrushSound()
        {
            if (AudioManager.Instance != null && brushStrokeSound != null)
            {
                AudioManager.Instance.PlaySFX(brushStrokeSound, Random.Range(0.8f, 1.2f));
            }
        }
        
        private void TriggerCompletion()
        {
            // Notify level manager or game manager
            if (LevelManager.Instance != null)
            {
                // Assuming there's an objective for calligraphy
                LevelManager.Instance.ForceCompleteObjective("完成墙上题字");
            }
            
            // Show subtitle
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowSubtitle("武松题下 [杀人者打虎武松也]，飘然离去。");
            }
        }
        
        #region Public Interface
        
        public void StartCalligraphyGame()
        {
            if (calligraphyPanel == null) return;
            
            // Reset state
            isGameActive = true;
            currentCharacterIndex = 0;
            completedCharacters = 0;
            
            // Reset character completion states
            foreach (var character in characterTemplates)
            {
                character.isCompleted = false;
            }
            
            // Clear canvas
            ClearCanvas();
            
            // Show panel
            calligraphyPanel.SetActive(true);
            
            // Update UI
            UpdateCharacterUI();
            
            // Duck background music
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.DuckBGM(0.3f, 1.0f);
            }
        }
        
        public void CloseCalligraphyGame()
        {
            if (calligraphyPanel == null) return;
            
            // Hide panel
            calligraphyPanel.SetActive(false);
            
            // Reset state
            isGameActive = false;
            
            // Restore background music
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.UnduckBGM(1.0f);
            }
        }
        
        public bool IsActive()
        {
            return isGameActive;
        }
        
        public float GetCompletionPercentage()
        {
            if (characterTemplates.Count == 0) return 0f;
            return (float)completedCharacters / characterTemplates.Count;
        }
        
        public bool IsCompleted()
        {
            return completedCharacters >= characterTemplates.Count;
        }
        
        #endregion
    }
}