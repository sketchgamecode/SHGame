namespace SHGame.Utilities
{
    /// <summary>
    /// Game-wide constants and configuration values
    /// Centralized location for tweaking game balance and settings
    /// </summary>
    public static class GameConstants
    {
        #region Tags and Layers

        public const string PLAYER_TAG = "Player";
        public const string NPC_TAG = "NPC";
        public const string INTERACTABLE_TAG = "Interactable";
        public const string HIDDEN_TAG = "Hidden";
        public const string HIDING_SPOT_TAG = "HidingSpot";

        public const int PLAYER_LAYER = 7;
        public const int NPC_LAYER = 8;
        public const int INTERACTABLE_LAYER = 9;
        public const int GROUND_LAYER = 10;

        #endregion

        #region Player Settings

        public const float DEFAULT_MOVE_SPEED = 5f;
        public const float DEFAULT_CROUCH_SPEED_MULTIPLIER = 0.5f;
        public const float DEFAULT_JUMP_FORCE = 10f;
        public const float GROUND_CHECK_DISTANCE = 0.1f;

        #endregion

        #region Stealth Settings

        public const float DEFAULT_HIDE_THRESHOLD = 0.3f;
        public const float STEALTH_CHECK_INTERVAL = 0.1f;
        public const float MOVEMENT_DETECTION_THRESHOLD = 0.1f;
        public const float STEALTH_COLOR_TRANSITION_SPEED = 2f;

        #endregion

        #region NPC Settings

        public const float DEFAULT_NPC_MOVE_SPEED = 2f;
        public const float DEFAULT_INVESTIGATE_SPEED = 3f;
        public const float DEFAULT_CHASE_SPEED = 4f;
        public const float DEFAULT_DETECTION_RANGE = 5f;
        public const float DEFAULT_FIELD_OF_VIEW = 60f;
        public const float DEFAULT_LOSE_PLAYER_TIME = 3f;
        public const float DEFAULT_INVESTIGATION_TIME = 5f;
        public const float DEFAULT_ALERT_COOLDOWN = 10f;

        #endregion

        #region Interaction Settings

        public const float DEFAULT_INTERACTION_RANGE = 2f;
        public const float DEFAULT_DETECTION_ANGLE = 45f;
        public const int DEFAULT_RAYCAST_COUNT = 5;

        #endregion

        #region Audio Settings

        public const float DEFAULT_MASTER_VOLUME = 1f;
        public const float DEFAULT_BGM_VOLUME = 0.7f;
        public const float DEFAULT_SFX_VOLUME = 1f;
        public const float DEFAULT_AMBIENT_VOLUME = 0.5f;
        public const float DEFAULT_VOICE_VOLUME = 1f;
        public const float DEFAULT_AUDIO_FADE_TIME = 1f;

        #endregion

        #region UI Settings

        public const float DEFAULT_SUBTITLE_DISPLAY_TIME = 3f;
        public const float DEFAULT_QTE_TIME_LIMIT = 3f;
        public const float DEFAULT_DIALOGUE_FADE_TIME = 0.5f;

        #endregion

        #region Scene Names

        public const string MAIN_MENU_SCENE = "MainMenu";
        public const string LEVEL_01_HORSE_YARD = "Level01_HorseYard";
        public const string LEVEL_02_KITCHEN = "Level02_Kitchen";
        public const string LEVEL_03_MANDARIN_DUCK_TOWER = "Level03_MandarinDuckTower";
        public const string LEVEL_04_FINAL_SHOWDOWN = "Level04_FinalShowdown";

        #endregion

        #region Animation Names

        // Player Animations
        public const string HERO_IDLE = "Hero_Idle";
        public const string HERO_WALK = "Hero_Walk";
        public const string HERO_CROUCH = "Hero_Crouch";
        public const string HERO_INTERACT = "Hero_Interact";
        public const string HERO_DRAG = "Hero_Drag";
        public const string HERO_KILL_QTE = "Hero_Kill_QTE";

        // NPC Animations
        public const string NPC_IDLE = "NPC_Idle";
        public const string NPC_WALK = "NPC_Walk";
        public const string NPC_ALERT = "NPC_Alert";
        public const string NPC_DEFEATED = "NPC_Defeated";
        public const string NPC_DEATH = "NPC_Death";

        #endregion

        #region Game Balance

        // Stealth Detection
        public const float LIGHT_DETECTION_MULTIPLIER = 1f;
        public const float MOVEMENT_DETECTION_MULTIPLIER = 1.5f;
        public const float DISTANCE_DETECTION_FALLOFF = 0.8f;

        // Door Interaction
        public const int MAX_DOOR_PUSH_ATTEMPTS = 2;
        public const float DOOR_ALERT_RADIUS = 5f;

        // Listening System
        public const float MIN_LISTENING_TIME = 3f;
        public const float MAX_LISTENING_TIME = 8f;
        public const float LISTENING_MOVEMENT_THRESHOLD = 0.05f;

        // QTE System
        public const float QTE_SUCCESS_WINDOW = 0.2f;
        public const float QTE_FAILURE_PENALTY = 1f;

        #endregion

        #region Story Constants

        // Level 1 - Horse Yard (马院)
        public const string STABLE_KEEPER_NAME = "后槽";
        public const string STABLE_KEEPER_DIALOGUE_SLEEPING = "老爷方才睡，你要偷我衣裳也早些哩！";
        public const string STABLE_KEEPER_DIALOGUE_ANGRY = "是甚么人在此推门？";
        public const string STABLE_KEEPER_INFORMATION = "目标在鸳鸯楼上饮酒";

        // Level 2 - Kitchen (厨房)
        public const string KITCHEN_MAID_DIALOGUE = "楼上的人都醉了，正是好时机";
        public const string KITCHEN_INFORMATION = "楼上人已醉";

        // Level 3 - Mandarin Duck Tower (鸳鸯楼)
        public const string TOWER_VILLAIN_DIALOGUE = "杀人者打虎武松也";
        public const int REQUIRED_EVIDENCE_COUNT = 5;

        #endregion

        #region Physics Constants

        public const float GRAVITY_SCALE = 2f;
        public const float DRAG_PHYSICS_DAMPING = 5f;
        public const float SNAP_DISTANCE = 1f;
        public const float MAX_DRAG_DISTANCE = 5f;

        #endregion

        #region Input Keys

        public const KeyCode INTERACTION_KEY = KeyCode.Space;
        public const KeyCode CROUCH_KEY = KeyCode.LeftControl;
        public const KeyCode INFORMATION_LOG_KEY = KeyCode.Tab;
        public const KeyCode PAUSE_KEY = KeyCode.Escape;
        public const KeyCode QTE_KEY = KeyCode.F;

        #endregion

        #region Rendering Constants

        public const int PIXELS_PER_UNIT = 100;
        public const float CAMERA_ORTHOGRAPHIC_SIZE = 5f;
        public const int TARGET_FRAME_RATE = 60;

        #endregion
    }
}