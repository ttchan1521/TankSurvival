using UnityEngine;

using System.Collections;
using System.Collections.Generic;


public class Trigger : MonoBehaviour
{

    public delegate void TriggerCallback(Trigger trigger);
    protected List<TriggerCallback> triggerCallbackList = new List<TriggerCallback>();
    public void SetTriggerCallback(TriggerCallback callback) { triggerCallbackList.Add(callback); }

    public bool destroyedAfterTriggered = true; //destroy trigger sau khi va chạm

    public AudioClip triggeredSFX; //Âm thanh khi va chạm

    [HideInInspector]
    public GameObject triggerEffObj; //spawn effect tại vị trí player
   
    [HideInInspector]
    public bool autoDestroyEffObj = true; // auto destroy effect

    [HideInInspector]
    public float effActiveDur = 2;      //duration effect

    [HideInInspector]
    public bool spawnEffectAtOrigin = false;    //spawn effect tại trigger hay player
    protected Vector3 effPos;

    [HideInInspector]
    public GameObject altTriggerEffObj; //effect ở vị trí mới của player
    // [Tooltip("Check to auto destroy the trigger effect object at new player position")]
    [HideInInspector]
    public bool autoDestroyAltEffObj = true;
    // [Tooltip("The active duration of the effect object at new player position")]
    [HideInInspector]
    public float altEffActiveDur = 2;
    protected Vector3 targetEffPos;

    public virtual bool UseAltTriggerEffectObj() { return false; }


    public virtual void Awake()
    {
        gameObject.layer = TDS.GetLayerTrigger();
    }

    public virtual void OnEnable()
    {
        Renderer rend = gameObject.GetComponent<Renderer>();
        if (rend != null) rend.enabled = false;

        Collider collider = gameObject.GetComponent<Collider>();
        if (collider == null) collider = gameObject.AddComponent<BoxCollider>();
        collider.isTrigger = true;

        //if(triggerEffObj!=null) ObjectPoolManager.New(triggerEffObj, 1);
        if (triggerEffObj)
        {
            if (!triggerEffObj.TryGetComponent<PooledObject>(out var pooledObject))
            {
                pooledObject = triggerEffObj.AddComponent<PooledObject>();
            }
        }
        //if(altTriggerEffObj!=null) ObjectPoolManager.New(altTriggerEffObj, 1);
        if (altTriggerEffObj)
        {
            if (!altTriggerEffObj.TryGetComponent<PooledObject>(out var pooledObject))
            {
                pooledObject = altTriggerEffObj.AddComponent<PooledObject>();
            }
        }

        effPos = transform.position;
    }


    public virtual void OnTriggerEnter(Collider collider)
    {

    }


    protected void Triggered()
    {
        AudioManager.PlaySound(triggeredSFX);

        if (triggerEffObj != null)
        {
            var poolObj = triggerEffObj.GetComponent<ShootObject>().GetPoolItem(effPos, Quaternion.identity);
            if (autoDestroyEffObj)
            {
                poolObj.ReturnToPool(effActiveDur);
            }
        }
        if (UseAltTriggerEffectObj() && altTriggerEffObj == null)
        {
           
            var poolObj = altTriggerEffObj.GetComponent<ShootObject>().GetPoolItem(targetEffPos, Quaternion.identity);
            if (autoDestroyAltEffObj)
            {
                poolObj.ReturnToPool(altEffActiveDur);
            }
        }

        if (destroyedAfterTriggered) Destroy(gameObject);
    }


    protected virtual void OnDrawGizmos()
    {
        Gizmos.DrawIcon(transform.position + new Vector3(0, 0.1f, 0), "Trigger.png", TDS.scaleGizmos);

        Collider collider = gameObject.GetComponent<Collider>();

        Vector3 scale = collider != null ? collider.bounds.size : transform.localScale;
        scale.y = 0;
        Gizmos.DrawWireCube(transform.position, scale);

        //if(collider != null) Gizmos.DrawWireCube(transform.position, collider.bounds.size);
        //else Gizmos.DrawWireCube(transform.position, transform.localScale);
    }

}
