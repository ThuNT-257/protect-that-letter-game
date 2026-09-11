using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Cloud : MonoBehaviour {
    private float speed;
    private float screenBottomY;
    private SpriteRenderer spriteRenderer;

    private void Awake() {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void SetupCloud(Sprite[] cloudSprites, float minScale, float maxScale, float minSpeed, float maxSpeed, float screenBottom) {
        if (cloudSprites != null && cloudSprites.Length > 0) {
            spriteRenderer.sprite = cloudSprites[Random.Range(0, cloudSprites.Length)];
        }

        float randomScale = Random.Range(minScale, maxScale);
        transform.localScale = new Vector3(randomScale, randomScale, 1f);

        speed = Random.Range(minSpeed, maxSpeed);

        float cloudHalfHeight = spriteRenderer.bounds.extents.y;
        screenBottomY = screenBottom - cloudHalfHeight;

        gameObject.SetActive(true);
    }

    private void Update() {
        transform.Translate(Vector3.down * (speed * Time.deltaTime));

        if (transform.position.y <= screenBottomY) {
            gameObject.SetActive(false);
        }
    }
}