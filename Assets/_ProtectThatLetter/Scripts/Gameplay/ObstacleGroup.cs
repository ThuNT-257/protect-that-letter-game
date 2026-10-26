using ProtectThatLetter.Managers;
using UnityEngine;

namespace ProtectThatLetter.Gameplay
{
    [DisallowMultipleComponent]
    public class ObstacleGroup : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 2f;
        [SerializeField] private float destroyYThreshold = -10f;

        private bool isMoving = false;

        private void Update()
        {
            if (!isMoving) return;

            transform.Translate(Vector3.down * moveSpeed * Time.deltaTime);

            if (transform.position.y < destroyYThreshold)
            {
                Destroy(gameObject);
            }
        }

        public void InitGroup(float speed)
        {
            moveSpeed = speed;
            isMoving = true;
        }

        private void OnDisable()
        {
            isMoving = false;
        }

        private void RecycleToPool()
        {
            isMoving = false;
            ObjectPool.Instance.ReturnToPool(gameObject);
        }
    }
}