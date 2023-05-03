using UnityEngine;
using System.Collections;


[System.Serializable]
public class AttackInstance
{
    public Unit srcUnit;

    public bool isAOE = false;
    public float aoeDistance = 0;   //khoảng cách từ tâm AOE đến mục tiêu
    public AttackStats aStats;

    public AttackInstance()
    {

    }
    
    public AttackInstance(Unit src = null, AttackStats aSt = null)
    {
        srcUnit = src;
        aStats = aSt;
    }

    public AttackInstance Clone()
    {
        AttackInstance aInstance = new AttackInstance();

        aInstance.srcUnit = srcUnit;

        aInstance.isAOE = isAOE;
        aInstance.aoeDistance = aoeDistance;
        aInstance.aStats = aStats.Clone();

        return aInstance;
    }

    public UnitPlayer GetSrcPlayer()
    {
        return srcUnit != null ? srcUnit.GetUnitPlayer() : null;
    }
}


