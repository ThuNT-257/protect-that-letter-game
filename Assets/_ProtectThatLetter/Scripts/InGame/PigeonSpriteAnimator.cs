using UnityEngine;

namespace ProtectThatLetter.Controllers {
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(PolygonCollider2D))]
    public class PigeonSpriteAnimator : MonoBehaviour {
        [Header("Animation Settings")]
        [SerializeField] private Sprite[] animationFrames;
        [SerializeField] private float frameRate = 0.1f;

        private SpriteRenderer spriteRenderer;
        private PolygonCollider2D polygonCollider;
        private int currentFrame;
        private float timer;

        private void Awake() {
            spriteRenderer = GetComponent<SpriteRenderer>();
            polygonCollider = GetComponent<PolygonCollider2D>();
        }

        private void Update() {
            if (animationFrames == null || animationFrames.Length == 0) return;

            timer += Time.deltaTime;
            if (timer >= frameRate) {
                timer -= frameRate;
                currentFrame = (currentFrame + 1) % animationFrames.Length;

                Sprite newSprite = animationFrames[currentFrame];
                spriteRenderer.sprite = newSprite;

                UpdatePolygonCollider(newSprite);
            }
        }

        private void UpdatePolygonCollider(Sprite sprite) {
            if (sprite == null || polygonCollider == null) return;

            polygonCollider.pathCount = 0;

            for (int i = 0; i < sprite.GetPhysicsShapeCount(); i++) {
                var path = new System.Collections.Generic.List<Vector2>();
                sprite.GetPhysicsShape(i, path);
                polygonCollider.pathCount++;
                polygonCollider.SetPath(polygonCollider.pathCount - 1, path);
            }
        }
    }
}