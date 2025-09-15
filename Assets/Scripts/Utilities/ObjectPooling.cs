using UnityEngine;
using System.Collections.Generic;

namespace SHGame.Utilities
{
    /// <summary>
    /// Simple object pooling system for frequently spawned objects
    /// Useful for particle effects, UI elements, and temporary objects
    /// </summary>
    public class ObjectPool : MonoBehaviour
    {
        [Header("Pool Settings")]
        public GameObject prefab;
        public int initialPoolSize = 10;
        public int maxPoolSize = 50;
        public bool autoExpand = true;

        private Queue<GameObject> availableObjects = new Queue<GameObject>();
        private List<GameObject> allObjects = new List<GameObject>();

        private void Start()
        {
            InitializePool();
        }

        private void InitializePool()
        {
            for (int i = 0; i < initialPoolSize; i++)
            {
                CreateNewObject();
            }
        }

        private GameObject CreateNewObject()
        {
            if (prefab == null) return null;

            GameObject newObj = Instantiate(prefab, transform);
            newObj.SetActive(false);
            
            // Add poolable component if it doesn't exist
            if (newObj.GetComponent<PoolableObject>() == null)
            {
                newObj.AddComponent<PoolableObject>();
            }
            
            newObj.GetComponent<PoolableObject>().SetPool(this);
            
            availableObjects.Enqueue(newObj);
            allObjects.Add(newObj);
            
            return newObj;
        }

        public GameObject GetObject()
        {
            GameObject obj = null;

            if (availableObjects.Count > 0)
            {
                obj = availableObjects.Dequeue();
            }
            else if (autoExpand && allObjects.Count < maxPoolSize)
            {
                obj = CreateNewObject();
                if (obj != null)
                {
                    availableObjects.Dequeue(); // Remove it from available since we're using it
                }
            }

            if (obj != null)
            {
                obj.SetActive(true);
                obj.GetComponent<PoolableObject>()?.OnSpawnFromPool();
            }

            return obj;
        }

        public void ReturnObject(GameObject obj)
        {
            if (obj == null || !allObjects.Contains(obj)) return;

            obj.GetComponent<PoolableObject>()?.OnReturnToPool();
            obj.SetActive(false);
            
            if (!availableObjects.Contains(obj))
            {
                availableObjects.Enqueue(obj);
            }
        }

        public void ReturnAllObjects()
        {
            foreach (GameObject obj in allObjects)
            {
                if (obj != null && obj.activeInHierarchy)
                {
                    ReturnObject(obj);
                }
            }
        }

        public int GetActiveCount()
        {
            int activeCount = 0;
            foreach (GameObject obj in allObjects)
            {
                if (obj != null && obj.activeInHierarchy)
                {
                    activeCount++;
                }
            }
            return activeCount;
        }

        public int GetAvailableCount()
        {
            return availableObjects.Count;
        }
    }

    /// <summary>
    /// Component for objects that can be pooled
    /// Handles initialization and cleanup when spawning/returning
    /// </summary>
    public class PoolableObject : MonoBehaviour
    {
        private ObjectPool parentPool;
        public float autoReturnTime = -1f; // -1 means no auto return
        private float spawnTime;

        public void SetPool(ObjectPool pool)
        {
            parentPool = pool;
        }

        public virtual void OnSpawnFromPool()
        {
            spawnTime = Time.time;
            // Override in derived classes for custom spawn behavior
        }

        public virtual void OnReturnToPool()
        {
            // Override in derived classes for custom return behavior
        }

        private void Update()
        {
            if (autoReturnTime > 0 && Time.time - spawnTime >= autoReturnTime)
            {
                ReturnToPool();
            }
        }

        public void ReturnToPool()
        {
            if (parentPool != null)
            {
                parentPool.ReturnObject(gameObject);
            }
            else
            {
                // Fallback - just deactivate
                gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Static manager for global object pools
    /// </summary>
    public static class PoolManager
    {
        private static Dictionary<string, ObjectPool> pools = new Dictionary<string, ObjectPool>();

        public static void RegisterPool(string poolName, ObjectPool pool)
        {
            if (!pools.ContainsKey(poolName))
            {
                pools[poolName] = pool;
            }
        }

        public static void UnregisterPool(string poolName)
        {
            if (pools.ContainsKey(poolName))
            {
                pools.Remove(poolName);
            }
        }

        public static GameObject GetObject(string poolName)
        {
            if (pools.ContainsKey(poolName))
            {
                return pools[poolName].GetObject();
            }
            return null;
        }

        public static void ReturnObject(string poolName, GameObject obj)
        {
            if (pools.ContainsKey(poolName))
            {
                pools[poolName].ReturnObject(obj);
            }
        }

        public static void ReturnAllObjects(string poolName)
        {
            if (pools.ContainsKey(poolName))
            {
                pools[poolName].ReturnAllObjects();
            }
        }

        public static void ClearAllPools()
        {
            foreach (var pool in pools.Values)
            {
                if (pool != null)
                {
                    pool.ReturnAllObjects();
                }
            }
        }
    }
}