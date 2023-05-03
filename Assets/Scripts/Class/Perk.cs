using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum _PerkType
{
    ModifyGeneralStats,

    AddWeapon,
    ModifyWeapon,

    AddAbility,
    ModifyAbility,

    Custom,
}


[System.Serializable]
public class Perk : Item
{
    public string desp;

    public _PerkType type;
    public bool repeatable = false; //được mua nhiều lần hay không
    public int limit = 0;   //số lượt mua giới hạn nếu repeatable = true

    public int purchased = 0;   //số lượt đã mua
    public bool Purchased() { return purchased > 0; }

    public int cost = 1;
    public int minLevel = 1;
    public List<int> prereq = new List<int>();  //vật phẩm yêu cầu khác

    public UnitPlayer player;
    public void SetPlayer(UnitPlayer unit) { player = unit; }


    public string IsAvailable()
    {
        if (player == null) return "Error: perk has no designated player unit";

        if (purchased > 0 && !repeatable) return "Purchased";
        if (repeatable && limit > 0 && purchased >= limit) return "Limit reached";
        if (player.GetLevel() < minLevel) return "Require level - " + minLevel;

        if (player.GetPerkCurrency() < cost) return "Insufficient currency. Require " + cost;
        
        if (prereq.Count > 0)
        {
            string text = "Require: ";
            bool first = true;
            List<Perk> perkList = player.GetPerkList();
            for (int i = 0; i < prereq.Count; i++)
            {
                for (int n = 0; n < perkList.Count; n++)
                {
                    if (perkList[n].ID == prereq[i])
                    {
                        text += ((!first) ? ", " : "") + perkList[n].name;
                        first = false;
                        break;
                    }
                }
            }
            return text;
        }
        return "";
    }


    public string Purchase(PlayerPerk playerPerk = null, bool usePerkCurrency = true)
    {
        if (purchased > 0 && !repeatable) return "Error trying to re-purchase non-repeatable perk";

        if (repeatable && limit > 0 && purchased >= limit) return "Limit reached";

        if (usePerkCurrency && playerPerk != null)
        {
            if (playerPerk.GetPerkCurrency() < cost) return "Insufficient perk currency"; //không đủ tiền
            playerPerk.SpendCurrency(cost);
        }

        purchased += 1;

        return "";
    }



    //ModifyHitPoint
    public float hitPoint = 0;
    public float hitPointCap = 0;
    public float hitPointRegen = 0;

    //ModifyEnergy
    public float energy = 0;
    public float energyCap = 0;
    public float energyRegen = 0;

    //ModifyMovement
    public float moveSpeedMul = 0;

    //ModifyAttack
    public float dmgMul = 0;

    //
    public float expGainMul = 0;        //experience
    public float scoreGainMul = 0;

    public float weapDmg = 0;

    public float weapAOE = 0;
    public float weapRange = 0;
    public float weapCooldown = 0;
    public float weapClipSize = 0;
    public float weapAmmoCap = 0;
    public float weapReloadDuration = 0;
    public float weapRecoilMagnitude = 0;

    public float abCost;
    public float abCooldown;
    public float abRange = 0;

    public float abDmg = 0;
    public float abAOE = 0;



    //Custom
    public GameObject customObject;



    public Perk Clone()
    {
        Perk perk = new Perk();

        perk.ID = ID;
        perk.icon = icon;
        perk.name = name;
        perk.desp = desp;

        perk.type = type;
        perk.repeatable = repeatable;
        perk.limit = limit;

        perk.purchased = purchased;

        perk.cost = cost;
        perk.minLevel = minLevel;
        perk.prereq = new List<int>(prereq);


        //generic multiplier
        perk.hitPoint = hitPoint;
        perk.hitPointCap = hitPointCap;
        perk.hitPointRegen = hitPointRegen;

        perk.energy = energy;
        perk.energyCap = energyCap;
        perk.energyRegen = energyRegen;

        perk.moveSpeedMul = moveSpeedMul;

        perk.dmgMul = dmgMul;

        perk.expGainMul = expGainMul;

        perk.scoreGainMul = scoreGainMul;

        perk.weapDmg = weapDmg;

        perk.weapAOE = weapAOE;
        perk.weapRange = weapRange;
        perk.weapCooldown = weapCooldown;
        perk.weapClipSize = weapClipSize;
        perk.weapAmmoCap = weapAmmoCap;
        perk.weapReloadDuration = weapReloadDuration;
        perk.weapRecoilMagnitude = weapRecoilMagnitude;

        perk.abCost = abCost;
        perk.abCooldown = abCooldown;
        perk.abRange = abRange;

        perk.abDmg = abDmg;

        perk.abAOE = abAOE;


        //custom
        perk.customObject = customObject;

        return perk;
    }

}

