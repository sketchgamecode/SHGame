namespace SHGame.Interaction
{
    /// <summary>
    /// Interface for all interactable objects in the game
    /// Defines the contract for objects that can be interacted with by the player
    /// </summary>
    public interface IInteractable
    {
        /// <summary>
        /// Called when the player enters the interaction range
        /// </summary>
        void OnPlayerEnterRange();

        /// <summary>
        /// Called when the player exits the interaction range
        /// </summary>
        void OnPlayerExitRange();

        /// <summary>
        /// Performs the interaction logic
        /// </summary>
        void Interact();

        /// <summary>
        /// Checks if the object can currently be interacted with
        /// </summary>
        /// <returns>True if interaction is possible</returns>
        bool CanInteract();

        /// <summary>
        /// Gets the prompt text to display to the player
        /// </summary>
        /// <returns>Interaction prompt text</returns>
        string GetInteractionPrompt();

        /// <summary>
        /// Gets the type of interaction this object provides
        /// </summary>
        /// <returns>Interaction type</returns>
        InteractionType GetInteractionType();
    }

    /// <summary>
    /// Types of interactions available in the game
    /// </summary>
    public enum InteractionType
    {
        Generic,        // General interactions
        Door,           // Door opening/closing
        Light,          // Light manipulation
        Draggable,      // Objects that can be dragged
        Listening,      // Areas for listening to conversations
        NPC,            // NPC interactions
        Item,           // Item pickup/use
        Furniture,      // Furniture interactions
        QTE             // Quick Time Event triggers
    }
}