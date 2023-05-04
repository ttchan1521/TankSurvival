using UnityEngine;

using System.Collections;
using System.Collections.Generic;


[RequireComponent(typeof(UnitPlayer))]
public class PlayerProgression : MonoBehaviour
{

    public bool enableLeveling = true;

    public bool loadProgress = false;
    public bool saveProgress = false;
    public bool resetOnStart = true;

    public int level = 0;
    public int GetLevel() { return level; }
    public int GetLevelCap() { return stats.levelCap; }

    public int exp = 0;
    public int GetCurrentExp() { return exp; }


    public bool loadStatsFromDB = true;
    public LevelProgressionStats stats;



    public int testVariable = 0;



    [HideInInspector] public UnitPlayer player;
    public void SetPlayer(UnitPlayer unit)
    {
        player = unit;
        Init();
    }

    [HideInInspector] public bool init = false;
    public void Init()
    {
        if (init) return;
        init = false;

        if (!enableLeveling) return;

        if (loadStatsFromDB) ProgressionStats_DB.CopyStats(this);
        else stats.VerifyExpList();
    }

    IEnumerator Start()
    {
        //if(player==null) yield break;
        yield return null;

        if (resetOnStart) _Reset();
    }


    //public static void Reset(){ instance._Reset(); }
    public void _Reset()
    {
        level = 0;
        exp = 0;

        GainExp();
    }



    //public static void GainExp(int gain=0){ if(instance!=null) instance._GainExp(gain); }
    public void GainExp(int gain = 0)
    {
        if (!enableLeveling) return;

        if (stats.expThresholdList.Count <= level) return;

        exp += gain;
        if (exp >= stats.expThresholdList[level] && level < stats.levelCap) _LevelUp(); //expThresholdList điểm exp cộng dồn

        if (level == stats.levelCap) exp = stats.expThresholdList[stats.expThresholdList.Count - 1];

    }


    //public static void LevelUp(){ instance._LevelUp(); }
    public void _LevelUp()
    {

        level += 1;

        if (level > 1)
        {
            player.GainHitPoint(stats.hitPointGain);
            player.GainEnergy(stats.energyGain);

            player.GainPerkCurrency(stats.perkCurrencyGain);

            TDS.PlayerLevelUp(player);
            TDS.OnGameMessage("level Up!");
        }

        GainExp();  //check lại level
    }

    public int GetCurrentLvllUpExp()
    {
        return stats.expThresholdList[level];
    }
    public int GetPrevLvllUpExp()
    {
        return stats.expThresholdList[Mathf.Max(0, level - 1)];
    }
    //public static float GetCurrentLevelProgress(){ return instance._GetCurrentLevelProgress(); }
    public float GetCurrentLevelProgress()
    {
        float denominator = (float)(GetCurrentLvllUpExp() - GetPrevLvllUpExp());
        return denominator == 0 ? 0 : (float)(exp - GetPrevLvllUpExp()) / denominator;
    }



    public float GetHitPointGain() { return (level - 1) * stats.hitPointGain; }
    public float GetHitPointRegenGain() { return (level - 1) * stats.hitPointRegenGain; }
    public float GetEnergyGain() { return (level - 1) * stats.energyGain; }
    public float GetEnergyRegenGain() { return (level - 1) * stats.energyRegenGain; }
    public float GetSpeedMulGain() { return (level - 1) * stats.speedMulGain; }
    public float GetDamageMulGain() { return (level - 1) * stats.dmgMulGain; }


    public static List<int> GenerateExpTH(bool sumR = true, float m = 1.5f, float c = 10f, int lvlCap = 50)
    {
        List<int> thList = new List<int> { 0 };

        int sum = 0;
        for (int i = 0; i < lvlCap - 1; i++)
        {
            if (sumR) sum += (int)Mathf.Round(m * (float)i + c);
            else sum = (int)Mathf.Round(m * (float)i + c);

            thList.Add(sum);
        }

        return thList;
    }


}
