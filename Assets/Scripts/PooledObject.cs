using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PooledObject : MonoBehaviour
{
    [HideInInspector]
    public ObjectPool pool;

    public void CreatePool()
    {
        if (!pool)
        {
            pool = ObjectPool.CreatePool(this);
        }
    }

    public PooledObject GetPoolItem(Vector3 pos, Quaternion rot, Transform parent = default)
    {
        return GetPoolItem<PooledObject>(pos, rot);
    }

    public T GetPoolItem<T>(Vector3 pos, Quaternion rot, Transform parent = default) where T : PooledObject
    {
        if (!pool)
        {
            pool = ObjectPool.CreatePool(this);
        }

        var obj = pool.pool.Get();
        if (parent)
        {
            obj.transform.parent = parent;
        }

        obj.transform.SetPositionAndRotation(pos, rot);
        return (T)obj;
    }

    public void ReturnToPool(float seconds = default)
    {
        StartCoroutine(ReturnToPoolYield(seconds));
    }

    private IEnumerator ReturnToPoolYield(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        if (pool)
        {
            pool.pool.Release(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
