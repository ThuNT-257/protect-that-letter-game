using UnityEngine;

public class BigTransitionCloud : MonoBehaviour {
    [SerializeField] private float moveSpeed = 3.5f;
    [SerializeField] private float destroyY = -12f;

    private LevelColorData targetPalette;
    private LevelBackgroundManager bgManager;
    private bool isMoving = false;

    public void InitTransition(LevelColorData newPalette, LevelBackgroundManager manager, float speed) {
        targetPalette = newPalette;
        bgManager = manager;
        moveSpeed = speed;
        isMoving = true;
    }

    private void Update() {
        if (!isMoving) return;

        transform.Translate(Vector3.down * moveSpeed * Time.deltaTime);

        if (transform.position.y <= destroyY) {
            if (bgManager != null) {
                bgManager.OnTransitionComplete(targetPalette);
            }
            Destroy(gameObject);
        }
    }
}