using UnityEngine;

using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using pvp;

[System.Serializable]
public class CollectibleSpawnInfo
{
    public Collectible item;
    public float chance = 0.5f;
    public float cooldown = 10;
    [HideInInspector] public float currentCD = 0;
}


public class CollectibleSpawner : MonoBehaviour
{

    public bool spawnUponStart = true;
    private bool spawned = false;

    [HideInInspector] public TDSArea spawnArea;

    public float startDelay = 5;
    public float spawnCD = 10;
    public int maxItemCount = 1;
    private List<Collectible> existItemList = new List<Collectible>();

    public float spawnChance = 0.2f;
    public float failModifier = 0.1f;
    private int unSpawnCount = 0;
    public List<CollectibleSpawnInfo> spawnItemList = new List<CollectibleSpawnInfo>();


    void Awake()
    {
        //if no spawn area has been assigned, create one
        if (spawnArea == null) spawnArea = gameObject.AddComponent<TDSArea>();
    }

    void Start()
    {
        // for (int i = 0; i < spawnItemList.Count; i++)
        // {
        //     if (spawnItemList[i].item == null)
        //     {
        //         spawnItemList.RemoveAt(i);
        //         i--;
        //         continue;
        //     }

        //     Debug.Log(i+"   "+spawnItemList[i].item);
        //     ObjectPoolManager.New(spawnItemList[i].item.gameObject, 1);
        // }

        //if spawnUponStart is enabled, start spawning
        if (spawnUponStart)
            StartSpawn();
    }

    public void StartSpawn()
    {
        if (spawned) return;
        spawned = true;

        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        yield return new WaitForSeconds(startDelay);

        //keep on looping
        while (true)
        {
            Collectible newObj = SpawnItem(spawnArea.GetRandomPosition());
            if (newObj != null)
            {
                newObj.SetTriggerCallback(ItemDisableCallback);  //set callback function for the collectible (which clear the item in existingItemList)
                existItemList.Add(newObj);   //add the new item to existingItemList
            }

            //wait for spawnCD before attempting next spawn
            yield return new WaitForSeconds(spawnCD);
        }
    }

    void Update()
    {
        //iterate the cooldown of each spawn item
        for (int i = 0; i < spawnItemList.Count; i++)
            spawnItemList[i].currentCD -= Time.deltaTime;
    }



    public Collectible SpawnItem(Vector3 pos)
    {
        if (existItemList.Count >= maxItemCount) return null;

        float chance = spawnChance + (unSpawnCount * failModifier);

        //check the chance, if this doesnt pass, dont spawn
        if (Random.value > chance)
        {
            unSpawnCount++;
            return null;
        }

        unSpawnCount = 0;

        //a list of potential item available for spawn
        List<int> avaiableItemList = new List<int>();

        //loop through all item, add the available item to potential list
        for (int i = 0; i < spawnItemList.Count; i++)
        {
            if (spawnItemList[i].item == null) continue;
            if (spawnItemList[i].currentCD > 0) continue;
            if (Random.value > spawnItemList[i].chance) continue;

            avaiableItemList.Add(i);
        }

        //if there's item available to spawn
        if (avaiableItemList.Count > 0)
        {
            //select an random item from the list and spawn it
            int rand = Random.Range(0, avaiableItemList.Count);
            int itemIndex = avaiableItemList[rand];
            //GameObject obj=ObjectPoolManager.Spawn(spawnItemList[ID].item.gameObject, pos, Quaternion.identity);
            Collectible obj = spawnItemList[itemIndex].item.GetPoolItem<Collectible>(pos, Quaternion.identity);
            //GameObject obj=(GameObject)Instantiate(spawnItemList[ID].item.gameObject, pos, Quaternion.identity);

            if (GameControl.GetInstance().pvp)
            {
                NetworkManager.Instance.Manager.Socket
                    .Emit("spawnCollectible", new CollectibleInit
                    {
                        roomId = PvP.GetRoom(),
                        collectibleIndex = Collectible_DB.GetIndexCollectible(spawnItemList[itemIndex].item),
                        position = MyExtension.ConvertToArrayFromVector3(pos)
                    });
        }

        //refresh the item cooldown
        spawnItemList[itemIndex].currentCD = spawnItemList[itemIndex].cooldown;

        return obj;
    }

        return null;
    }


//callback function for when an item is triggered
public void ItemDisableCallback(Collectible obj)
{
    existItemList.Remove(obj);
}


void OnDrawGizmos()
{
    if (gameObject.GetComponent<CollectibleDropManager>()) return;

    Gizmos.DrawIcon(transform.position + new Vector3(0, 0.1f, 0), "SpawnDrop.png", TDS.scaleGizmos);

    if (spawnArea != null)
    {
        Gizmos.DrawLine(transform.position, spawnArea.GetPos());
        spawnArea.gizmoColor = new Color(1, 1f, 0f, 1);
    }
}

}
