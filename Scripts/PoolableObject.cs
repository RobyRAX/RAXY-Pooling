using System;
using RAXY.Utility;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Pool;

namespace RAXY.Pooling
{
    public class PoolableObject : MonoBehaviour
    {
        public string OriginalName { get; private set; }
        public int defaultCapacity = 5;
        public int maxSize = 5;

        [TitleGroup("Debug")]
        [SerializeField]
        [ReadOnly]
        Vector3 defaultRot;

        [TitleGroup("Debug")]
        [SerializeField]
        [ReadOnly]
        Vector3 defaultScale = new Vector3(1, 1, 1);

        protected ObjectPoolInstance pool;

        void Awake()
        {
            StoreTransformSetting();
            OriginalName = CustomUtility.GetObjectNameWithout_Clone(gameObject.name);
        }

        [TitleGroup("Debug")]
        [Button]
        void StoreTransformSetting()
        {
            defaultRot = transform.localEulerAngles;
            defaultScale = transform.localScale;
        }

        public void ApplyDefaultTransform()
        {
            transform.localEulerAngles = defaultRot;
            transform.localScale = defaultScale;
        }

        public void SetPool(ObjectPoolInstance poolInstance)
        {
            pool = poolInstance;
        }

        public bool Release()
        {
            if (pool == null)
                return false;

            pool.Release(this);
            return true;
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            StoreTransformSetting();
        }
#endif
    }

    [Serializable]
    public class AssetReferencePoolableObject : AssetReferenceT<PoolableObject>
    {
        public AssetReferencePoolableObject(string guid) : base(guid)
        {
        }
    }
}