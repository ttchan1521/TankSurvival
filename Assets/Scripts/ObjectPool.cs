using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class ObjectPool : MonoBehaviour
{
    IObjectPool<PooledObject> _pool;
    PooledObject prefab;

    public static ObjectPool CreatePool(PooledObject pooledObject)
    {
        GameObject obj = new GameObject(pooledObject + " pool");
        ObjectPool objectPool = obj.AddComponent<ObjectPool>();
        objectPool.prefab = pooledObject;
        return objectPool;
    }

    public IObjectPool<PooledObject> pool
    {
        get
        {
            if (_pool == null)
            {
                _pool = new ObjectPool<PooledObject>(CreatedPoolItem, OnTakeFromPool, OnReturnToPool, OnDestroyPoolObject, false, 10, 50);
            }
            return _pool;
        }
    }

    PooledObject CreatedPoolItem()
    {
        if (prefab)
        {
            PooledObject pooledObject = Instantiate(prefab, transform, false);
            pooledObject.pool = this;
            return pooledObject;
        }
        return null;
    }

    void OnTakeFromPool(PooledObject pooledObject)
    {
        pooledObject.gameObject.SetActive(true);
    }

    void OnReturnToPool(PooledObject pooledObject)
    {
        pooledObject.gameObject.SetActive(false);
        pooledObject.transform.SetParent(transform, false);
    }

    void OnDestroyPoolObject(PooledObject pooledObject)
    {
        Destroy(pooledObject.gameObject);
    }
}
