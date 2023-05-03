using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

using System.Collections;
using System.Collections.Generic;


public class ProgressionStatsDB : MonoBehaviour
{

    public LevelProgressionStats stats;

    private static ProgressionStatsDB instance;

    public static void Init()
    {
        if (instance != null) return;
        instance = LoadDB1();
    }

    public static void CopyStats1(PlayerProgression progress)
    {
        Init();
        progress.stats = instance.stats.Clone();
    }

    public static ProgressionStatsDB LoadDB1()
    {
        GameObject obj = Resources.Load("DB_TDSTK/Obsolete/DB_Progression", typeof(GameObject)) as GameObject;
        return obj.GetComponent<ProgressionStatsDB>();
    }

}

