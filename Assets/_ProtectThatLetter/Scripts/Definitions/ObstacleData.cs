using UnityEngine;

namespace ProtectThatLetter.Definitions
{
    /// <summary>
    /// Defines the behavior type of an obstacle.
    /// </summary>
    public enum ObstacleType
    {
        Normal,   // Simple straight fall
        Swaying,  // Zigzag / sine-wave movement
        Tackle,   // Charges toward the player
        Boss      // Special boss behavior
    }

    /// <summary>
    /// Defines which hit sound to play when the obstacle is struck.
    /// </summary>
    public enum ObstacleHitSound
    {
        DefaultHit  // Default impact sound
    }

    /// <summary>
    /// ScriptableObject that stores configuration for a single obstacle type.
    /// Allows designers to create variants in the Inspector without code changes.
    /// </summary>
    [CreateAssetMenu(fileName = "NewObstacleData", menuName = "Protect That Letter/Obstacle Data")]
    public class ObstacleData : ScriptableObject
    {
        #region General Info
        [Header("General Info")]
        public string obstacleId;                               // Unique identifier (for lookups)
        public string obstacleName;                             // Display name
        public ObstacleType obstacleType = ObstacleType.Normal; // Behavior type
        public Sprite obstacleSprite;                           // Visual sprite
        public PhysicsMaterial2D physicsMaterial;               // Bounce/friction settings
        public ObstacleHitSound hitSound = ObstacleHitSound.DefaultHit; // Hit SFX
        #endregion

        #region Movement Settings
        [Header("Movement Settings")]
        public float minFallSpeed = 3f;   // Minimum fall speed
        public float maxFallSpeed = 5f;   // Maximum fall speed
        #endregion

        #region Special Behavior Settings
        [Header("Special Behavior Settings")]
        public float swayAmplitude = 1.5f;  // Horizontal sway amplitude (Swaying)
        public float swayFrequency = 3f;    // Sway oscillation frequency (Swaying)
        public float tackleSpeed = 6f;      // Charge speed (Tackle)
        #endregion

        #region Game Stats
        [Header("GameStats")]
        public int damageValue = 1;         // Damage dealt to player
        #endregion
    }
}