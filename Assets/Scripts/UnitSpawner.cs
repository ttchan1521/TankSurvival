using UnityEngine;

using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using pvp;

public enum _SpawnMode
{
    WaveBased,
    FreeForm,
}

public enum _OverrideMode { Replace, Addition, Multiply }
public enum _SpawnLimitType { Count, Timed, None }

public class UnitSpawner : MonoBehaviour
{

    public _SpawnMode spawnMode;
    public bool spawnUponStart = true; //auto spawn
    private bool spawnStarted = false;

    public float startDelay = 1;


    public List<TDSArea> spawnAreaList = new List<TDSArea>(); 
    public bool randomRotation = true;



    public List<Unit> spawnUnitList = new List<Unit>();

    public bool overrideHitPoint = false; //chỉnh sửa hp của unit
    public _OverrideMode overrideHPMode = _OverrideMode.Multiply; //type chỉnh sửa hp
    public float startingHitPoint = 25; //giá trị để chỉnh sửa
    public float hitPointIncrement = 5; //hp tăng mỗi lần
    public float hitPointTimeStep = 30; //khoảng cách giữa những lần tăng hp
    private float spawnHP = 0;



    [Header("Wave")]
    private int currentWaveIDX = -1;

    public bool endlessWave = false; //wave vô tận
    public List<Wave> waveList = new List<Wave>();
    public float delayBetweenWave = 3; //delay giữa các wave


    public int maxSubWaveCount = 3; //số lượng subwave max
    public int unitCount = 8; //số lượng unit mỗi wave cơ bản
    public int unitCountInc = 4; //số lượng unit tăng theo wave (wave càng cao càng nhiều unit)
    private Wave waveE = null;  //currentWave (endless)

    public int startingScore = 10; //score cơ bản
    public int scoreIncrement = 10; //score tăng theo wave



    [Header("FreeForm")]
    public float spawnCD = 1.5f; //khoảng cách giữa những lần spawn

    public int activeLimit = 10; //số lượng unit hiện tại tối đa
    public int limitSpawnCount = 20; //số lượng unit spawn tối đa
    public int limitSpawnTime = 20; //thời gian spawnn
    public _SpawnLimitType limitType = _SpawnLimitType.Timed;



    [Header("Stats Tracking")]
    private int activeCount = 0;    //số lượng unit hiện tại
    private int spawnCount = 0; //số lượng unit đã spawn

    private int killCount = 0;      //số lượng unit bị destroyed




    private Transform thisT;
    void Awake()
    {
        thisT = transform;

        //gán id từng wave
        for (int i = 0; i < waveList.Count; i++) waveList[i].waveID = i;

        //remove null element
        for (int i = 0; i < spawnAreaList.Count; i++)
        {
            if (spawnAreaList[i] == null)
            {
                spawnAreaList.RemoveAt(i); i -= 1;
            }
        }
        for (int i = 0; i < spawnUnitList.Count; i++)
        {
            if (spawnUnitList[i] == null)
            {
                spawnUnitList.RemoveAt(i); i -= 1;
            }
        }

        //nếu không có area thì tạo
        if (spawnAreaList.Count == 0)
        {
            spawnAreaList.Add(gameObject.AddComponent<TDSArea>());
        }

        //nếu không có unit
        if (spawnMode == _SpawnMode.FreeForm || (spawnMode == _SpawnMode.WaveBased && endlessWave))
        {
            if (spawnUnitList.Count == 0) Debug.LogWarning("No unit", thisT);
        }
    }


    void Start()
    {
        //InitObjectPool();

        //thêm vào spawn tracker
        UnitSpawnerTracker.AddSpawner(this);

        if (overrideHitPoint) spawnHP = startingHitPoint;

        if (spawnUponStart) StartSpawn();
    }


    void OnDisable()
    {
        //clear in tracker
        Cleared();
    }


    // public void InitObjectPool()
    // {
    //     if (spawnMode == _SpawnMode.FreeForm || (spawnMode == _SpawnMode.WaveBased && endlessWave))
    //     {
    //         for (int i = 0; i < spawnUnitList.Count; i++)
    //         {
    //             ObjectPoolManager.New(spawnUnitList[i].gameObject, 5);
    //         }
    //     }
    //     else
    //     {
    //         for (int i = 0; i < waveList.Count; i++)
    //         {
    //             for (int n = 0; n < waveList[i].subWaveList.Count; n++)
    //             {
    //                 if (waveList[i].subWaveList[n].unitPrefab != null)
    //                     ObjectPoolManager.New(waveList[i].subWaveList[n].unitPrefab.gameObject, 5);
    //                 else
    //                     Debug.LogWarning("unit prefab for wave-" + i + ", subwave-" + n + " is unspecified", this);
    //             }
    //         }
    //     }
    // }


    public void StartSpawn()
    {
        if (!gameObject.activeInHierarchy) return;

        if (spawnStarted) return;
        spawnStarted = true;

        if (spawnMode == _SpawnMode.WaveBased)
        {
            if (!endlessWave) SpawnWaveFromList(startDelay);
            else SpawnGeneratedWave(startDelay);
        }
        else if (spawnMode == _SpawnMode.FreeForm)
        {
            StartCoroutine(SpawnFreeForm());
        }
    }


    //spawn next wave in wavelist
    void SpawnWaveFromList(float delay = 0)
    {
        //final wave
        if (currentWaveIDX + 1 >= waveList.Count)
        {
            Cleared();
            return;
        }
        StartCoroutine(SpawnWave(waveList[currentWaveIDX += 1], delay));
    }
    //tạo wave for endless mode
    void SpawnGeneratedWave(float delay = 0)
    {
        waveE = GenerateWave(currentWaveIDX += 1);
        StartCoroutine(SpawnWave(waveE, delay));
    }
    IEnumerator SpawnWave(Wave wave, float delay)
    {
        yield return new WaitForSeconds(delay);

        Debug.Log(gameObject.name + " - start spawning wave " + currentWaveIDX);

        if (currentWaveIDX > 0) spawnHP += hitPointIncrement;

        wave.subWaveSpawned = 0;

        for (int i = 0; i < wave.subWaveList.Count; i++) StartCoroutine(SpawnSubWave(wave, i));

        //wait until subwave finish spawn
        while (wave.subWaveSpawned < wave.subWaveList.Count) yield return null;

        wave.spawned = true;
        Debug.Log(gameObject.name + " - wave " + currentWaveIDX + " spawned complete");
    }

    IEnumerator SpawnSubWave(Wave wave, int subWaveIdx)
    {
        SubWave subWave = wave.subWaveList[subWaveIdx];
        TDSArea sArea = subWave.spawnArea != null ? subWave.spawnArea : spawnAreaList[0]; //nếu subwave không có area thì lấy mặc định ở vị trí mặc định

        yield return new WaitForSeconds(subWave.startDelay);

        if (subWave.unitPrefab != null)
        {
            for (int i = 0; i < subWave.count; i++)
            {
                //delay giữa các lần spawn unit
                if (i > 0) yield return new WaitForSeconds(subWave.interval);

                Quaternion rot = !randomRotation ? sArea.GetRotation() : Quaternion.Euler(0, Random.Range(0, 360), 0);

                UnitAI unitInstance = SpawnUnit(subWave.unitPrefab, sArea.GetRandomPosition(), rot, subWave.unitPrefab.gameObject.name + "_" + spawnCount);
                unitInstance.SetWaveID(this, wave.waveID);  //để biết wave của unit khi unit destroy

                wave.activeUnitCount += 1;
            }
        }

        wave.subWaveSpawned += 1;
        yield return null;
    }

    //add unit to parent wave (unit destroy sinh ra unit khác)
    public void AddUnitToWave(Unit unitInstance)
    {
        waveList[unitInstance.waveID].activeUnitCount += 1;
        AddUnit(unitInstance);
    }

    //callback when unit destroyed, check wave is cleared
    public void UnitCleared(int waveID)
    {
        bool waveCleared = false;

        if (!endlessWave)
        {
            waveList[waveID].activeUnitCount -= 1;
            if (waveList[waveID].spawned && waveList[waveID].activeUnitCount == 0)
            {
                waveCleared = true;
                waveList[waveID].Completed();
                SpawnWaveFromList(delayBetweenWave); //next wave
            }
        }
        else
        {
            waveE.activeUnitCount -= 1;
            if (waveE.spawned && waveE.activeUnitCount == 0)
            {
                waveCleared = true;
                waveE.Completed();
                SpawnGeneratedWave(delayBetweenWave);   //next wave
            }
        }
    }





    //free form spawn mode
    IEnumerator SpawnFreeForm()
    {
        yield return new WaitForSeconds(startDelay);

        if (limitType == _SpawnLimitType.Timed) StartCoroutine(SpawnLimitTimerRoutine());

        if (overrideHitPoint) StartCoroutine(OverridingHitPointRoutine()); //tăng hp sau một khoảng thời gian

        
        while (true)
        {
            while (spawnUnitList.Count == 0) yield return null;

            while (activeCount == activeLimit) yield return null;

            //hết thời gian spawn
            if (limitType == _SpawnLimitType.Timed && freeformTimeOut) break;

            int rand = Random.Range(0, spawnAreaList.Count);
            Vector3 pos = spawnAreaList[rand].GetRandomPosition();
            Quaternion rot = !randomRotation ? spawnAreaList[rand].GetRotation() : Quaternion.Euler(0, Random.Range(0, 360), 0);

            int randU = Random.Range(0, spawnUnitList.Count);
            SpawnUnit(spawnUnitList[randU], pos, rot, spawnUnitList[randU].gameObject.name + "_" + spawnCount);

            if (limitType == _SpawnLimitType.Count && spawnCount == limitSpawnCount) break;

            yield return new WaitForSeconds(spawnCD);
        }

        while (activeCount > 0) yield return null;

        Cleared();
    }


    private bool freeformTimeOut = false;
    IEnumerator SpawnLimitTimerRoutine()
    {
        yield return new WaitForSeconds(limitSpawnTime);
        freeformTimeOut = true;
    }

    IEnumerator OverridingHitPointRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(hitPointTimeStep);
            spawnHP += hitPointIncrement;
        }
    }





    public bool IsSpawnCompleted()
    {
        if (spawnMode == _SpawnMode.WaveBased)
        {
            if (endlessWave) return false;

            bool allSpawned = true;
            for (int i = 0; i < waveList.Count; i++)
            {
                if (!waveList[i].spawned)
                {
                    allSpawned = false;
                    break;
                }
            }
            return allSpawned;
        }
        else if (spawnMode == _SpawnMode.FreeForm)
        {

            if (limitType == _SpawnLimitType.Count && spawnCount >= limitSpawnCount) return true;
            else if (limitType == _SpawnLimitType.Timed && freeformTimeOut) return true;
        }


        return false;
    }




    private UnitAI SpawnUnit(PooledObject prefab, Vector3 spawnPos, Quaternion rot, string name = "")
    {
        //GameObject unitObj=(GameObject)Instantiate(prefab, spawnPos, rot);
        UnitAI unitObj = prefab.GetPoolItem<UnitAI>(spawnPos, rot);
        unitObj.gameObject.layer = TDS.GetLayerAIUnit();
        unitObj.gameObject.name = name;

        unitObj.target = GameControl.GetPlayer();

        if (overrideHitPoint) unitObj.OverrideHitPoint(spawnHP, overrideHPMode);

        if (GameControl.GetInstance().pvp && PvP.GetLandSpawnPlayer() == 0)
        {
            if (unitObj.instanceID <= 0)
                unitObj.instanceID = GameControl.GetUnitInstanceID();
            UnitInit dataInit = new UnitInit {
                    roomId = PvP.GetRoom(),
                    instanceId = unitObj.instanceID,
                    prefabId = UnitAI_DB.GetIndexUnitAI((UnitAI)prefab),
                    name = name,
                    hitPointFull = unitObj.hitPointFull,
                    position = MyExtension.ConvertToArrayFromVector3(unitObj.transform.position),
                    rotation = MyExtension.ConvertToArrayFromQuaternion(unitObj.transform.rotation)
                };
            NetworkManager.Instance.Manager.Socket
                .Emit("spawnUnit", dataInit);
            PvPManager.instance.AddEnemiesData(dataInit, unitObj);
        }

        AddUnit(unitObj);   //track unit

        return unitObj;
    }


    //track unit
    public void AddUnit(Unit unitInstance)
    {
        unitInstance.SetDestroyCallback(this.UnitDestroy);
        spawnCount += 1;
        activeCount += 1;
    }
    //untrack unit
    public void UnitDestroy()
    {
        activeCount -= 1;
        killCount += 1;
    }


    public void Cleared()
    {
        UnitSpawnerTracker.RemoveSpawner(this);
        GameControl.UnitSpawnerCleared(this);
    }



    //tạo wave trong chế độ endless
    Wave GenerateWave(int waveIDX)
    {
        Wave wave = new Wave();

        wave.waveID = waveIDX;
        wave.spawnArea = spawnAreaList[Random.Range(0, spawnAreaList.Count)];

        //wave.creditGain = startingCredit + creditIncrement * waveIDX;
        wave.scoreGain = startingScore + scoreIncrement * waveIDX;

        int subWaveCount = Random.Range(1, waveIDX);
        subWaveCount = Mathf.Clamp(subWaveCount, 1, maxSubWaveCount);
        subWaveCount = Mathf.Clamp(subWaveCount, 1, spawnUnitList.Count);

        List<int> countList = new List<int>();
        for (int i = 0; i < subWaveCount; i++) countList.Add(1);

        int totalUnitCount = unitCount + unitCountInc * waveIDX;
        if (subWaveCount <= 0) totalUnitCount = 0;

        int count = 0;
        while (totalUnitCount > 0)
        {
            int rand = Random.Range(1, totalUnitCount);
            countList[count] += rand;
            totalUnitCount -= rand;
            if ((count += 1) >= subWaveCount) count = 0;
        }

        List<Unit> unitList = new List<Unit>(spawnUnitList);

        wave.subWaveList = new List<SubWave>();
        for (int i = 0; i < subWaveCount; i++)
        {
            SubWave subWave = new SubWave();
            subWave.count = countList[i];

            int rand = Random.Range(0, unitList.Count);
            subWave.unitPrefab = unitList[rand];
            unitList.RemoveAt(rand);

            subWave.startDelay = 0.5f;
            subWave.interval = Random.Range(1f, 2f);
            subWave.spawnArea = spawnAreaList[Random.Range(0, spawnAreaList.Count)];
            wave.subWaveList.Add(subWave);
        }

        return wave;
    }




    void OnDrawGizmos()
    {
        Gizmos.DrawIcon(transform.position + new Vector3(0, 0.1f, 0), "SpawnUnit.png", TDS.scaleGizmos);

        for (int i = 0; i < spawnAreaList.Count; i++)
        {
            if (spawnAreaList[i] == null) continue;
            Gizmos.DrawLine(transform.position, spawnAreaList[i].GetPos());
            spawnAreaList[i].gizmoColor = new Color(1, 0, 0.5f, 1);
        }
    }

}


