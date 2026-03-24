using System;
using System.Collections.Generic;
using RAXY.Utility;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Pool;

namespace RAXY.Pooling
{
    public class ObjectPoolService : MonoBehaviour
    {
        private static ObjectPoolService _instance;
        public static ObjectPoolService Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindAnyObjectByType<ObjectPoolService>();

                    if (_instance == null)
                    {
                        var go = new GameObject(nameof(ObjectPoolService));
                        _instance = go.AddComponent<ObjectPoolService>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        [ShowInInspector]
        [HideReferenceObjectPicker]
        public Dictionary<string, ObjectPoolInstance> ObjectPoolDict = new();

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        public ObjectPoolInstance CreatePool(PoolableObject original)
        {
            if (original == null)
            {
                CustomDebug.LogError("[ObjectPool] Cannot create pool: parameter 'original' is null.");
                return null;
            }

            var key = CustomUtility.GetObjectNameWithout_Clone(original.name);
            if (ObjectPoolDict.TryGetValue(key, out var existingPool))
                return existingPool;

            ObjectPoolInstance newPool = new ObjectPoolInstance(original);
            ObjectPoolDict.Add(key, newPool);

            return newPool;
        }

        public PoolableObject GetPoolableObject(string originalName,
                                                Transform parent = null,
                                                bool applyDefaultTransform = false)
        {
            if (ObjectPoolDict == null)
                return null;

            if (originalName == null)
                originalName = "";

            ObjectPoolDict.TryGetValue(CustomUtility.GetObjectNameWithout_Clone(originalName), out var selectedPool);

            if (selectedPool == null)
            {
                CustomDebug.LogWarning("Pool with the key '" + originalName + "' doesnt exist...");
                return null;
            }
            PoolableObject pooledObject = selectedPool.Get();

            if (parent != null)
            {
                pooledObject.transform.SetParent(parent);
                if (applyDefaultTransform)
                {
                    pooledObject.ApplyDefaultTransform();
                }
            }
            else if (applyDefaultTransform)
            {
                pooledObject.ApplyDefaultTransform();
            }

            return pooledObject;
        }

        [Button]
        public PoolableObject GetPoolableObject(PoolableObject original,
                                                Transform parent = null,
                                                bool applyDefaultTransform = false)
        {
            ObjectPoolDict.TryGetValue(CustomUtility.GetObjectNameWithout_Clone(original.name), out var selectedPool);
            if (selectedPool == null)
            {
                selectedPool = CreatePool(original);
            }

            PoolableObject pooledObject = selectedPool.Get();

            if (parent != null)
            {
                pooledObject.transform.SetParent(parent);
                if (applyDefaultTransform)
                {
                    pooledObject.ApplyDefaultTransform();
                }
            }
            else if (applyDefaultTransform)
            {
                pooledObject.ApplyDefaultTransform();
            }

            return pooledObject;
        }

        [Button]
        public void ReleasePoolableObject(PoolableObject pooledObject)
        {
            if (pooledObject == null)
            {
                CustomDebug.LogWarning("[ObjectPool] Release called with null pooledObject");
                return;
            }

            var key = CustomUtility.GetObjectNameWithout_Clone(pooledObject.OriginalName);
            if (!ObjectPoolDict.TryGetValue(key, out var selectedPool))
            {
                CustomDebug.LogWarning($"[ObjectPool] No pool found for key '{key}'. Did you use the same prefab / name?");
                return;
            }

            try
            {
                selectedPool.Release(pooledObject);
            }
            catch (Exception e)
            {
                CustomDebug.LogError($"[ObjectPool] Error releasing object: {e}");
            }
        }
    }

    [HideReferenceObjectPicker]
    public class ObjectPoolInstance
    {
        [ShowInInspector]
        [ReadOnly]
        public PoolableObject OriginalObject { get; set; }

        [HideInInspector]
        public ObjectPool<PoolableObject> pool;
        public Transform poolParent;

        [ShowInInspector]
        public int DefaultCapacity => OriginalObject != null ? OriginalObject.defaultCapacity : 0;

        public ObjectPoolInstance() { }
        public ObjectPoolInstance(PoolableObject original)
        {
            Setup(original);
        }

        public void Setup(PoolableObject original)
        {
            OriginalObject = original;
            Transform managerTransform = ObjectPoolService.Instance.transform;

            string poolParentName = $"{OriginalObject.name} - Pool";
            poolParent = managerTransform.Find(poolParentName);
            if (poolParent == null)
            {
                poolParent = new GameObject(poolParentName).transform;
                poolParent.SetParent(managerTransform);
            }

            pool = new ObjectPool<PoolableObject>(
                        CreateAction,
                        GetAction,
                        ReleaseAction,
                        DestroyAction,
                        true,
                        original.defaultCapacity,
                        original.maxSize);

            if (original.defaultCapacity > 0)
            {
                for (int i = 0; i < original.defaultCapacity; i++)
                {
                    PoolableObject obj = CreateAction();
                    pool.Release(obj);
                }
            }
        }

        PoolableObject CreateAction()
        {
            PoolableObject newPooled = GameObject.Instantiate(OriginalObject, poolParent);
            newPooled.SetPool(this);
            return newPooled;
        }
        void GetAction(PoolableObject pooledObject)
        {
            pooledObject.gameObject.SetActive(true);
        }
        void ReleaseAction(PoolableObject poolableObj)
        {
            poolableObj.transform.SetParent(poolParent);
            poolableObj.gameObject.SetActive(false);
        }
        void DestroyAction(PoolableObject poolableObj)
        {
            if (poolableObj == null)
                return;
            GameObject.Destroy(poolableObj.gameObject);
        }

        public PoolableObject Get()
        {
            return pool.Get();
        }

        public void Release(PoolableObject poolable)
        {
            try
            {
                poolable.transform.SetParent(poolParent);
                poolable.ApplyDefaultTransform();
                pool.Release(poolable);
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
        }
    }
}

