using UnityEngine;
using UnityEngine.Rendering;

namespace SHGame.Utilities
{
    /// <summary>
    /// Controls rendering order of objects based on their Y position
    /// Used to ensure proper visual layering in 2D side-scrolling games
    /// </summary>
    public class SortingGroupController : MonoBehaviour
    {
        [Header("Sorting Settings")]
        public bool autoUpdateSorting = true;
        public float sortingOrderMultiplier = -10f;  // Negative so lower Y = higher sorting order
        public int baseSortingOrder = 0;
        public bool useYPositionForSorting = true;
        
        [Header("Sorting Group")]
        public SortingGroup sortingGroup;
        public bool createSortingGroupIfMissing = true;
        
        [Header("Children Control")]
        public bool updateChildRenderers = false;
        public int childrenBaseOffset = 0;
        
        [Header("Advanced")]
        public bool useCustomPivot = false;
        public Transform customPivot;
        public float refreshRate = 0.1f;
        
        // Private state
        private float lastUpdateTime;
        private int lastSortingOrder;
        private Vector3 lastPosition;

        private void Awake()
        {
            // Get or create sorting group
            if (sortingGroup == null)
            {
                sortingGroup = GetComponent<SortingGroup>();
                
                if (sortingGroup == null && createSortingGroupIfMissing)
                {
                    sortingGroup = gameObject.AddComponent<SortingGroup>();
                }
            }
            
            // Initialize custom pivot
            if (useCustomPivot && customPivot == null)
            {
                customPivot = transform;
            }
            
            // Initial update
            UpdateSorting();
        }

        private void Update()
        {
            if (!autoUpdateSorting) return;
            
            // Only update at specified refresh rate
            if (Time.time - lastUpdateTime < refreshRate)
            {
                return;
            }
            
            // Only update if position changed significantly
            if (HasPositionChanged())
            {
                UpdateSorting();
            }
            
            lastUpdateTime = Time.time;
        }
        
        private bool HasPositionChanged()
        {
            Vector3 currentPos = useCustomPivot ? customPivot.position : transform.position;
            
            // Only care about the Y position if that's what we're using for sorting
            if (useYPositionForSorting)
            {
                return Mathf.Abs(currentPos.y - lastPosition.y) > 0.01f;
            }
            
            // Otherwise check if position changed significantly
            return Vector3.Distance(currentPos, lastPosition) > 0.01f;
        }
        
        private void UpdateSorting()
        {
            if (sortingGroup == null) return;
            
            // Get position for sorting calculation
            Vector3 position = useCustomPivot ? customPivot.position : transform.position;
            lastPosition = position;
            
            // Calculate sorting order
            int newSortingOrder;
            
            if (useYPositionForSorting)
            {
                // Use Y position for sorting
                newSortingOrder = baseSortingOrder + Mathf.RoundToInt(position.y * sortingOrderMultiplier);
            }
            else
            {
                // Use Z position for sorting (useful for some 2.5D setups)
                newSortingOrder = baseSortingOrder + Mathf.RoundToInt(position.z * sortingOrderMultiplier);
            }
            
            // Only update if changed
            if (newSortingOrder != lastSortingOrder)
            {
                sortingGroup.sortingOrder = newSortingOrder;
                lastSortingOrder = newSortingOrder;
                
                // Update child renderers if needed
                if (updateChildRenderers)
                {
                    UpdateChildRenderers();
                }
            }
        }
        
        private void UpdateChildRenderers()
        {
            // Update all renderers that aren't in sorting groups
            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            
            foreach (Renderer renderer in renderers)
            {
                // Skip if this renderer is part of a sorting group
                if (renderer.gameObject.GetComponentInParent<SortingGroup>() != sortingGroup)
                {
                    continue;
                }
                
                // Update sorting order
                renderer.sortingOrder = lastSortingOrder + childrenBaseOffset;
            }
        }
        
        #region Public Methods
        
        public void ForceUpdateSorting()
        {
            UpdateSorting();
        }
        
        public void SetBaseSortingOrder(int newBase)
        {
            baseSortingOrder = newBase;
            UpdateSorting();
        }
        
        public int GetCurrentSortingOrder()
        {
            return lastSortingOrder;
        }
        
        #endregion
    }
}