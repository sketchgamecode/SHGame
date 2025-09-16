using UnityEngine;
using System.Collections;
using SHGame.Core;
using System;

namespace SHGame.Interaction
{
    /// <summary>
    /// Interactable item that can be collected, used or equipped by the player
    /// Used for clothes, keys, and other important game items
    /// </summary>
    public class InteractableItem : InteractableBase
    {
        [Header("Item Settings")]
        public string itemId = "item_01";
        public string itemName = "Item";
        public string itemDescription = "A usable item";
        public ItemType itemType = ItemType.Generic;
        
        [Header("Collection Settings")]
        public bool removeAfterCollection = true;
        public float collectionDelay = 0.5f;
        public GameObject collectionEffect;
        
        [Header("Visual")]
        public Sprite itemIcon;
        public GameObject visualModel;
        public bool showFloatingIcon = true;
        
        [Header("Special Effects")]
        public bool applyEffectOnCollection = false;
        public float effectDuration = 0f;

        // State
        private bool isCollected = false;
        private bool isInInventory = false;

        public enum ItemType
        {
            Generic,
            Clothes,
            Key,
            Weapon,
            Document,
            QuestItem
        }

        protected override void Start()
        {
            base.Start();
            
            // Set default interaction prompt based on item type
            if (string.IsNullOrEmpty(interactionPrompt))
            {
                UpdateInteractionPrompt();
            }
        }

        protected override void PerformInteraction()
        {
            // Don't allow re-collection if already collected
            if (isCollected) return;
            
            StartCoroutine(CollectItemSequence());
        }

        protected override bool CanInteractInternal()
        {
            return !isCollected;
        }

        private IEnumerator CollectItemSequence()
        {
            // Mark as collected
            isCollected = true;
            
            // Play collection effect if available
            if (collectionEffect != null)
            {
                Instantiate(collectionEffect, transform.position, Quaternion.identity);
            }
            
            // Show collection message
            ShowSubtitle($"获得 {itemName}");
            
            // Add to information log
            AddInformation($"获得了 {itemName}：{itemDescription}");
            
            // Register item with inventory system
            AddToInventory();
            
            // Apply any special effects
            if (applyEffectOnCollection)
            {
                ApplyItemEffect();
            }
            
            // Wait before removing if needed
            if (removeAfterCollection)
            {
                yield return new WaitForSeconds(collectionDelay);
                
                // Hide the visual model
                if (visualModel != null)
                {
                    visualModel.SetActive(false);
                }
                else
                {
                    // Hide this object if no separate visual model
                    gameObject.SetActive(false);
                }
            }
        }

        private void AddToInventory()
        {
            // Future: Connect to a proper inventory system
            isInInventory = true;
            
            // For now, notify the game event system
            Utilities.GameEvents.TriggerInformationGathered($"物品: {itemName}");
            
            // Trigger type-specific behavior
            switch (itemType)
            {
                case ItemType.Clothes:
                    HandleClothesCollection();
                    break;
                case ItemType.Key:
                    HandleKeyCollection();
                    break;
                case ItemType.Weapon:
                    HandleWeaponCollection();
                    break;
                case ItemType.Document:
                    HandleDocumentCollection();
                    break;
                case ItemType.QuestItem:
                    HandleQuestItemCollection();
                    break;
            }
        }

        private void ApplyItemEffect()
        {
            // Apply effect based on item type
            if (effectDuration > 0)
            {
                StartCoroutine(ItemEffectSequence());
            }
            else
            {
                // Immediate effect
                ApplyImmediateEffect();
            }
        }

        private IEnumerator ItemEffectSequence()
        {
            // Apply effect start
            OnEffectStart();
            
            // Wait for duration
            yield return new WaitForSeconds(effectDuration);
            
            // Apply effect end
            OnEffectEnd();
        }

        private void UpdateInteractionPrompt()
        {
            switch (itemType)
            {
                case ItemType.Clothes:
                    interactionPrompt = "换上衣服";
                    break;
                case ItemType.Key:
                    interactionPrompt = "拾取钥匙";
                    break;
                case ItemType.Weapon:
                    interactionPrompt = "拿起武器";
                    break;
                case ItemType.Document:
                    interactionPrompt = "阅读文书";
                    break;
                case ItemType.QuestItem:
                    interactionPrompt = "拿取物品";
                    break;
                default:
                    interactionPrompt = "拾取物品";
                    break;
            }
        }

        #region Type-Specific Handlers

        private void HandleClothesCollection()
        {
            // Change player appearance
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                // In a more complex system, this would change the player's sprite/model
                // For now, just log it
                Debug.Log($"Player changed clothes to: {itemName}");
                
                // Optional: Change player sprite if available
                var spriteRenderer = player.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null && itemIcon != null)
                {
                    spriteRenderer.sprite = itemIcon;
                }
            }
        }

        private void HandleKeyCollection()
        {
            // Unlock doors with matching ID
            var doors = FindObjectsOfType<InteractableDoor>();
            foreach (var door in doors)
            {
                // Check if the door requires a key and is locked
                if (door != null && door.isLocked && door.requiresKey && door.requiredKeyName == itemId)
                {
                    door.SetLocked(false);
                    AddInformation($"钥匙可以打开特定的门");
                }
            }
        }

        private void HandleWeaponCollection()
        {
            // Enable attack capabilities
            // This would connect to a combat system if implemented
            Debug.Log($"Player equipped weapon: {itemName}");
        }

        private void HandleDocumentCollection()
        {
            // Show document content in UI
            if (UIManager.Instance != null)
            {
                // Show document content
                UIManager.Instance.ShowSubtitle($"文书内容: {itemDescription}");
                
                // Maybe open a document UI panel in a more complex implementation
            }
        }

        private void HandleQuestItemCollection()
        {
            // Update quest progress
            Debug.Log($"Quest item collected: {itemName}");
            
            // Future: Connect to quest system
            Utilities.GameEvents.TriggerInformationGathered($"任务物品: {itemName}");
        }

        #endregion

        #region Effect Methods

        protected virtual void OnEffectStart()
        {
            // Override in derived classes for specific effects
            Debug.Log($"Item effect started: {itemName}");
        }

        protected virtual void OnEffectEnd()
        {
            // Override in derived classes for specific effects
            Debug.Log($"Item effect ended: {itemName}");
        }

        protected virtual void ApplyImmediateEffect()
        {
            // Override in derived classes for specific immediate effects
            Debug.Log($"Item immediate effect applied: {itemName}");
        }

        #endregion

        #region Public Methods

        public bool IsCollected()
        {
            return isCollected;
        }

        public bool IsInInventory()
        {
            return isInInventory;
        }

        public string GetItemId()
        {
            return itemId;
        }

        public string GetItemName()
        {
            return itemName;
        }

        public ItemType GetItemType()
        {
            return itemType;
        }

        public void SetCollected(bool collected)
        {
            isCollected = collected;
            
            if (collected && visualModel != null && removeAfterCollection)
            {
                visualModel.SetActive(false);
            }
        }

        public override string GetInteractionPrompt()
        {
            return interactionPrompt;
        }

        #endregion

        #region Gizmos

        protected override void OnDrawGizmos()
        {
            base.OnDrawGizmos();

            // Draw item type icon
            Gizmos.color = GetGizmoColorForItemType();
            Gizmos.DrawWireSphere(transform.position, 0.7f);
        }

        private Color GetGizmoColorForItemType()
        {
            switch (itemType)
            {
                case ItemType.Clothes:
                    return Color.cyan;
                case ItemType.Key:
                    return Color.yellow;
                case ItemType.Weapon:
                    return Color.red;
                case ItemType.Document:
                    return Color.white;
                case ItemType.QuestItem:
                    return Color.magenta;
                default:
                    return Color.gray;
            }
        }

        #endregion
    }
}