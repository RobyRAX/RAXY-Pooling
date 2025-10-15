using System;
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

        public void SetPool(ObjectPoolInstance pool)
        {
            this.pool = pool;
            OriginalName = pool.OriginalObject.name;
        }

        public void Release()
        {
            pool.Release(this);
        }
    }

    [Serializable]
    public class AssetReferencePoolableObject : AssetReferenceT<PoolableObject>
    {
        public AssetReferencePoolableObject(string guid) : base(guid)
        {
        }
    }
}