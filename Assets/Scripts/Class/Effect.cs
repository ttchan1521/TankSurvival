using UnityEngine;
using System.Collections;


[System.Serializable]
public class Effect : Item
{
    public string desp = "";
    public bool showOnUI = true;  

    public float duration = 2.5f;

    [Header("Direct effect on target")]
    public float restoreHitPoint = 0;   //per second
    public float restoreEnergy = 0; //per second

    public float speedMul = 1;
    public bool stun = false;
    public bool invincible = false;

    [Header("Multipliers on target's stats")]
    public float damageMul = 1;

    // public float critChanceMul = 1;
    // public float critMultiplierMul = 1;

    [HideInInspector] public bool expired = false;



    public bool Applicable()
    {
        if (ID < 0) return false;
        if (duration <= 0) return false;
        return true;
    }

    public Effect Clone()
    {
        Effect eff = new Effect();

        eff.ID = ID;
        eff.name = name;
        eff.icon = icon;

        eff.showOnUI = showOnUI;

        eff.restoreHitPoint = restoreHitPoint;
        eff.restoreEnergy = restoreEnergy;

        eff.invincible = invincible;
        eff.stun = stun;
        eff.speedMul = speedMul;

        eff.damageMul = damageMul;

        // eff.critChanceMul = critChanceMul;
        // eff.critMultiplierMul = critMultiplierMul;

        eff.duration = duration;

        return eff;
    }
}


