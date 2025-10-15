using System;
using System.Collections.Generic;
using RAXY.Utility;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Pool;

namespace RAXY.Pooling
{
    public class ObjectPoolManager : Singleton<ObjectPoolManager>
    {
        [ShowInInspector]
        [HideReferenceObjectPicker]
        public Dictionary<string, ObjectPoolInstance> ObjectPoolDict = new();

        public ObjectPoolInstance CreatePool(PoolableObject original)
        {
            if (original == null)
            {
                CustomDebug.LogError("[ObjectPool] Cannot create pool: parameter 'original' is null.");
                return null;
            }

            if (ObjectPoolDict.TryGetValue(original.name, out var existingPool))
            {
                return existingPool; // already exists, return it
            }

            ObjectPoolInstance newPool = new ObjectPoolInstance(original);
            ObjectPoolDict.Add(original.name, newPool);

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

            ObjectPoolDict.TryGetValue(originalName, out var selectedPool);
            if (selectedPool == null)
            {
                CustomDebug.LogWarning("Pool with the key '" + originalName + "' doesnt exist...");
                return null;
            }
            PoolableObject pooledObject = selectedPool.Get();

            if (parent != null)
            {
                if (applyDefaultTransform)
                {
                    pooledObject.ApplyDefaultTransform();
                }

                pooledObject.transform.SetParent(parent);
            }

            return pooledObject;
        }

        [Button]
        public PoolableObject GetPoolableObject(PoolableObject original,
                                                Transform parent = null,
                                                bool applyDefaultTransform = false)
        {
            ObjectPoolDict.TryGetValue(original.name, out var selectedPool);
            if (selectedPool == null)
            {
                selectedPool = CreatePool(original);
            }

            PoolableObject pooledObject = selectedPool.Get();

            if (applyDefaultTransform)
            {
                pooledObject.ApplyDefaultTransform();
            }

            if (parent != null)
            {
                pooledObject.transform.SetParent(parent);
            }

            return pooledObject;
        }

        [Button]
        public void ReleasePoolableObject(PoolableObject pooledObject)
        {
            ObjectPoolInstance selectedPool = ObjectPoolDict[pooledObject.OriginalName];
            selectedPool.Release(pooledObject);
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
            Transform managerTransform = ObjectPoolManager.Instance.transform;

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
                        original.defaultCapacity);

            if (original.defaultCapacity > 0)
            {
                var temp = new List<PoolableObject>(original.defaultCapacity);
                for (int i = 0; i < original.defaultCapacity; i++)
                {
                    var obj = pool.Get();
                    temp.Add(obj);
                }
                // Push them back into pool
                foreach (var obj in temp)
                    pool.Release(obj);
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
            poolableObj.gameObject.SetActive(false);
        }
        void DestroyAction(PoolableObject poolableObj)
        {
            GameObject.Destroy(poolableObj);
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
                pool.Release(poolable);
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
        }
    }
}

