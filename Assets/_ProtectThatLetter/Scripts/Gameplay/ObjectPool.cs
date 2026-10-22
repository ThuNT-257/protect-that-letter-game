using System.Collections.Generic;
using UnityEngine;

namespace ProtectThatLetter.Managers
{
    /// <summary>
    /// Simple prefab-based object pool.
    /// Reuses inactive instances; instantiates new ones only when needed.
    /// </summary>
    public class ObjectPool : MonoBehaviour
    {
        #region Instance
        // Singleton instance with public getter and private setter
        public static ObjectPool Instance { get; private set; }
        #endregion

        #region Private Fields
        // Maps each prefab to its queue of pooled instances
        private readonly Dictionary<GameObject, Queue<GameObject>> poolDictionary
            = new Dictionary<GameObject, Queue<GameObject>>();

        private readonly Dictionary<GameObject, GameObject> instanceToPrefabMap
            = new Dictionary<GameObject, GameObject>();
        #endregion

        #region Unity Lifecycle
        /// <summary>
        /// Ensures singleton integrity
        /// </summary>
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Clears the singleton reference when destroyed
        /// </summary>
        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Retrieves an inactive pooled instance (or instantiates a new one).
        /// Activates it and places it at the given position/rotation.
        /// </summary>
        /// <param name="prefab">The prefab key (must match the original prefab)</param>
        /// <param name="position">World position</param>
        /// <param name="rotation">World rotation</param>
        /// <returns>The activated pooled GameObject, or null if prefab is null</returns>
        public GameObject GetPooledObject(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            if (prefab == null) return null;

            // Ensure a queue exists for this prefab
            if(!poolDictionary.TryGetValue(prefab, out Queue<GameObject> poolQueue))
            {
                poolQueue = new Queue<GameObject>();
                poolDictionary[prefab] = poolQueue;
            }

            GameObject objToUse = null;

            if(poolQueue.Count > 0)
            {
                objToUse = poolQueue.Dequeue();
            }

            if(objToUse == null)
            {
                objToUse = Instantiate(prefab, transform);
                instanceToPrefabMap[objToUse] = prefab;
            }

            objToUse.transform.position = position;
            objToUse.transform.rotation = rotation;
            objToUse.SetActive(true);

            return objToUse;
        }

        public void ReturnToPool(GameObject obj)
        {
            if (obj == null) return;
            obj.SetActive(false);
            if(instanceToPrefabMap.TryGetValue(obj, out GameObject prefab))
            {
                if(poolDictionary.TryGetValue(prefab, out Queue<GameObject> poolQueue))
                {
                    if (!poolQueue.Contains(obj))
                    {
                        poolQueue.Enqueue(obj);
                    }
                }
            }
        }
        #endregion
    }
}