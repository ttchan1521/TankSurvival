using UnityEngine;

using System.Collections;
using System.Collections.Generic;


//theo dõi toàn bộ kẻ thù
public class UnitTracker
{
    private static List<Unit> allUnitList = new List<Unit>();
    public static List<Unit> GetAllUnitList() { return allUnitList; }

    public static int GetUnitCount() { return allUnitList.Count; }

    // public static void ScanForUnit()
    // {
    //     allUnitList = new List<Unit>();
    //     Unit[] list = GameObject.FindObjectsOfType(typeof(UnitAI)) as Unit[];

    //     for (int i = 0; i < list.Length; i++)
    //     {
    //         if (list[i].thisObj.layer == TDS.GetLayerPlayer()) continue;
    //         allUnitList.Add(list[i]);
    //     }
    // }


    public static Unit GetNearestUnit(Vector3 pos)
    {
        int nearestIdx = -1; float nearest = Mathf.Infinity;
        for (int i = 0; i < allUnitList.Count; i++)
        {
            float dist = Vector3.Distance(pos, allUnitList[i].thisT.position);
            if (dist < nearest)
            {
                nearest = dist;
                nearestIdx = i;
            }
        }
        return nearestIdx >= 0 ? allUnitList[nearestIdx] : null;
    }

    public static void AddUnit(Unit unit)
    {
        if (allUnitList.Contains(unit)) return;
        allUnitList.Add(unit);
    }
    
    public static void RemoveUnit(Unit unit)
    {
        allUnitList.Remove(unit);
    }

    public static void Clear() { allUnitList = new List<Unit>(); }
}


public class UnitSpawnerTracker
{
    private static List<UnitSpawner> allSpawnerList = new List<UnitSpawner>();
    public static List<UnitSpawner> GetAllSpawnerList() { return allSpawnerList; }

    public static int GetSpawnerCount() { return allSpawnerList.Count; }

    // public static void ScanForSpawner()
    // {
    //     allSpawnerList = new List<UnitSpawner>();
    //     UnitSpawner[] list = GameObject.FindObjectsOfType(typeof(UnitSpawner)) as UnitSpawner[];

    //     for (int i = 0; i < list.Length; i++) allSpawnerList.Add(list[i]);
    // }

    public static void AddSpawner(UnitSpawner spawner)
    {
        if (allSpawnerList.Contains(spawner)) return;
        allSpawnerList.Add(spawner);
    }

    public static void RemoveSpawner(UnitSpawner spawner)
    {
        allSpawnerList.Remove(spawner);
    }


    public static void Clear() { allSpawnerList = new List<UnitSpawner>(); }
}

