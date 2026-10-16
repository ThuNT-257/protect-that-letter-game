using UnityEngine;

namespace ProtectThatLetter.Definitions
{
    public enum ObstacleType
    {
        Normal,
        Swaying,
        Tackle,
        Boss
    }

    public enum ObstacleHitSound
    {
        DefaultHit
    }

    [CreateAssetMenu(fileName = "NewObstacleData", menuName = "Protect That Letter/Obstacle Data")]
    public class ObstacleData : ScriptableObject
    {
        public string obstacleId;
        public string obstacleName;
        public ObstacleType obstacleType = ObstacleType.Normal;
        public Sprite obstacleSprite;
        public PhysicsMaterial2D physicsMaterial;
        public ObstacleHitSound hitSound = ObstacleHitSound.DefaultHit;

        [Header("Movement Settings")]
        public float minFallSpeed = 3f;
        public float maxFallSpeed = 5f;
    }
}
