using UnityEngine;

using System.Collections;
using System.Collections.Generic;


public class ObjectiveTracker : MonoBehaviour
{

    public string objectiveName = "Objective";

    public bool waitForTimer = true;

    public bool enableScoring = false;
    public float targetScore = 0;
    private bool scored = false;

    public bool clearAllHostile = true;
    public List<Unit> unitList = new List<Unit>();  //unit exist

    public List<Unit> prefabList = new List<Unit>();        //unit chưa được spawn
    public List<int> prefabCountList = new List<int>();     //số lượng unit cần kill
    [HideInInspector] public List<int> prefabKillCountList = new List<int>(); //số lượng unit đã kill


    public bool clearAllSpawner = true;
    public List<UnitSpawner> spawnerList = new List<UnitSpawner>(); //all unit spawner in scene


    public List<Trigger> triggerList = new List<Trigger>(); //triggeres cần phải activated


    //public bool clearAllCol=false;
    public List<Collectible> collectibleList = new List<Collectible>(); //collectible exist

    public List<Collectible> colPrefabList = new List<Collectible>();   //collectible cần collect
    public List<int> colPrefabCountList = new List<int>();      //số lượng collectible cần collect
    [HideInInspector] public List<int> colPrefabCollectedCountList = new List<int>(); //số lượng collectible đã collect


    private bool isComplete = false;
    public bool IsComplete() { return isComplete; }


    public void CheckObjectiveComplete()
    {
        bool cleared = true;

        if (waitForTimer && !GameControl.TimesUp()) return;

        if (enableScoring && !scored) cleared = false;


        if (colPrefabList.Count > 0)
        {
            for (int i = 0; i < colPrefabList.Count; i++)
            {
                if (colPrefabCountList[i] > colPrefabCollectedCountList[i])
                {
                    cleared = false;
                    break;
                }
            }
        }

        //if(clearAllCol){	
        //	if(GetAllCollectibleCount>0) cleared=false;
        //}
        if (collectibleList.Count > 0) cleared = false;


        if (prefabList.Count > 0)
        {
            for (int i = 0; i < prefabList.Count; i++)
            {
                if (prefabCountList[i] > prefabKillCountList[i])
                {
                    cleared = false;
                    break;
                }
            }
        }

        if (clearAllHostile)
        {   
            if (UnitTracker.GetUnitCount() > 0) cleared = false;
        }
        else
        {   
            if (unitList.Count > 0) cleared = false;
        }

        if (clearAllSpawner)
        {  
            if (UnitSpawnerTracker.GetSpawnerCount() > 0) cleared = false;
        }
        else
        {  
            if (spawnerList.Count > 0) cleared = false;
        }

        if (GameControl.TimesUp())
        {
            isComplete = true;
            GameControl.GameOver(cleared);
        }
        
        else if (!waitForTimer)
        {
            if (cleared)
            {
                isComplete = true;
                GameControl.GameOver(cleared);
            }
        }

    }


    void Start()
    {
        for (int i = 0; i < unitList.Count; i++)
        {
            if (unitList[i] == null)
            {
                unitList.RemoveAt(i); i -= 1;
            }
        }


        for (int i = 0; i < spawnerList.Count; i++)
        {
            if (spawnerList[i] == null)
            {
                spawnerList.RemoveAt(i); i -= 1;
            }
        }

  
        if (clearAllSpawner)
        {
            spawnerList = UnitSpawnerTracker.GetAllSpawnerList();
        }

        for (int i = 0; i < prefabList.Count; i++) prefabKillCountList.Add(0);

        for (int i = 0; i < triggerList.Count; i++) triggerList[i].SetTriggerCallback(this.Triggered);

        //GameControl.SetObjective(this);
    }


    public void GainScore()
    {
        Debug.LogWarning(enableScoring + "   " + scored + "   " + GameControl.GetScore());
        if (enableScoring && !scored)
        {
            if (GameControl.GetScore() >= targetScore)
            {
                scored = true;
                CheckObjectiveComplete();
            }
        }
    }



    public void UnitDestroyed(Unit unit)
    {
        if (unit == null) return;

        unitList.Remove(unit);

        //if the unit's prefab is in prefabList, increase the corresponding kill count
        for (int i = 0; i < prefabList.Count; i++)
        {
            if (unit.prefabID == prefabList[i].prefabID)
            {
                prefabKillCountList[i] += 1;
                break;
            }
        }

        //if unitList is cleared, check if objective is complete
        if (unitList.Count == 0) CheckObjectiveComplete();
    }


    //called from GameControl when a unit spawner has done spawning (or is destroyed)
    public void SpawnerCleared(UnitSpawner spawner)
    {
        if (spawner == null) return;

        //remove the cleared unit spawner from spawnerList
        spawnerList.Remove(spawner);

        //if spawnerList is cleared, check if objective is complete
        if (spawnerList.Count == 0) CheckObjectiveComplete();
    }


    //called from trigger objective
    public void Triggered(Trigger trigger)
    {
        if (trigger == null) return;

        //remove the trigger from triggerList
        if (triggerList.Contains(trigger)) triggerList.Remove(trigger);

        //if trigger list is cleared, check if objective is complete
        if (triggerList.Count == 0) CheckObjectiveComplete();
    }


    //called from collectible OnTriggerEnter
    public void ColletibleCollected(Collectible item)
    {
        //remove the collected item from specific collectible item list
        collectibleList.Remove(item);

        //if the item's prefab is in prefabList, increase the corresponding collected count
        for (int i = 0; i < colPrefabList.Count; i++)
        {
            if (item.ID == colPrefabList[i].ID)
            {
                colPrefabCollectedCountList[i] += 1;
                break;
            }
        }
    }
}
