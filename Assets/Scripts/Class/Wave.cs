using UnityEngine;

using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class SubWave
{
    public int count = 5;
    public Unit unitPrefab;
    public float startDelay = 0;
    public float interval = 1;

    public TDSArea spawnArea;
}


[System.Serializable]
public class Wave
{
    [HideInInspector] public int waveID = -1;
    public List<SubWave> subWaveList = new List<SubWave>();

    public int scoreGain = 0;

    public TDSArea spawnArea;

    [HideInInspector] public int activeUnitCount = 0;   //số lượng unit hiện tại

    [HideInInspector] public int subWaveSpawned = 0; //subWave đã spawn

    [HideInInspector] public bool spawned = false; //tất cả unit đã spawn
    [HideInInspector] public bool cleared = false;  //wave cleared

    public Wave()
    {
        subWaveList.Add(new SubWave());
    }

    public void Completed()
    {
        cleared = true;
  
        GameControl.GainScore(scoreGain);
    }
}

