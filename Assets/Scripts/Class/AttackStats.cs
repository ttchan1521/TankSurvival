using UnityEngine;
using System.Collections;

[System.Serializable]
public class AttackStats
{
    [Header("Attributes")]
    public float damageMin = 0;
    public float damageMax = 0;

    public float aoeRadius = 0;
    public bool diminishingAOE = true;

    [Header("Physics")]
    public float impactForce = 0;

    public float explosionRadius = 0;
    public float explosionForce = 0;


    [Header("Effects")]
    public int effectID = -1;
    //[HideInInspector] 
    public int effectIdx = -1; 

    public void Init()
    {
        effectIdx = Effect_DB.GetEffectIndex(effectID);
    }

    public AttackStats Clone()
    {
        AttackStats stats = new AttackStats();

        stats.damageMin = damageMin;
        stats.damageMax = damageMax;

        stats.aoeRadius = aoeRadius;
        stats.diminishingAOE = diminishingAOE;

        stats.impactForce = impactForce;
        stats.explosionRadius = explosionRadius;
        stats.explosionForce = explosionForce;

        stats.effectID = effectID;
        stats.effectIdx = effectIdx;


        return stats;
    }
}


