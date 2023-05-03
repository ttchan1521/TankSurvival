using UnityEngine;
using System.Collections;

//spawn collectible when a unit is killed
[RequireComponent(typeof(CollectibleSpawner))]
public class CollectibleDropManager : MonoBehaviour
{

    public static CollectibleDropManager instance;

    public CollectibleSpawner spawner;

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        if (spawner == null)
            spawner = gameObject.GetComponent<CollectibleSpawner>();

        spawner.spawnUponStart = false; 
    }


    public void UnitDestroyed(Vector3 pos)
    {
        if (instance == null) return;
        if (instance.spawner == null) return;

        spawner.SpawnItem(pos);
    }

}
