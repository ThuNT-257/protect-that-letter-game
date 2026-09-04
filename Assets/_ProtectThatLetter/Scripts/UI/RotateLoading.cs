using UnityEngine;

public class RotateLoading : MonoBehaviour
{
    [SerializeField] private float rotateSpeed = 360f;

    private void Update() {
        transform.Rotate(0f, 0f, -rotateSpeed * Time.deltaTime);
    }
}
