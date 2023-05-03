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
        //nếu k có area, create one
        if (spawnArea == null) spawnArea = gameObject.AddComponent<TDSArea>();
    }

    void Start()
    {

        //nếu tự động spawn
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

        while (true)
        {
            Collectible newObj = SpawnItem(spawnArea.GetRandomPosition());
            if (newObj != null)
            {
                newObj.SetTriggerCallback(ItemDisableCallback);  //set callback function, clear item trong exist list
                existItemList.Add(newObj);
            }


            yield return new WaitForSeconds(spawnCD);
        }
    }

    void Update()
    {
        for (int i = 0; i < spawnItemList.Count; i++)
            spawnItemList[i].currentCD -= Time.deltaTime;
    }



    public Collectible SpawnItem(Vector3 pos)
    {
        if (existItemList.Count >= maxItemCount) return null;

        float chance = spawnChance + (unSpawnCount * failModifier);

        if (Random.value > chance)
        {
            unSpawnCount++;
            return null;
        }

        unSpawnCount = 0;

        List<int> avaiableItemList = new List<int>();

        for (int i = 0; i < spawnItemList.Count; i++)
        {
            if (spawnItemList[i].item == null) continue;
            if (spawnItemList[i].currentCD > 0) continue;
            if (Random.value > spawnItemList[i].chance) continue;

            avaiableItemList.Add(i);
        }


        if (avaiableItemList.Count > 0)
        {

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


        spawnItemList[itemIndex].currentCD = spawnItemList[itemIndex].cooldown;

        return obj;
    }

        return null;
    }


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
