using UnityEngine;

using System.Collections;
using System.Collections.Generic;


[RequireComponent(typeof(UnitPlayer))]
public class PlayerPerk : MonoBehaviour
{

    public bool enablePerk = true;


    public int perkCurrency = 0;

    public List<int> unavailableIDList = new List<int>();   //ID perk k được sử dụng
    [HideInInspector] public List<int> purchasedIDList = new List<int>();       //ID perk được mua sẵn
    [HideInInspector] public List<Perk> perkList = new List<Perk>();
    public List<Perk> GetPerkList() { return perkList; }
    public int GetPerkListCount() { return perkList.Count; }


    [HideInInspector] public UnitPlayer player;
    public void SetPlayer(UnitPlayer unit)
    {
        player = unit;
        Init();
    }

    public bool init = false;
    public void Init()
    {
        if (init) return;
        init = true;

        //if(!enablePerk) return;

        //loading perks from DB
        List<Perk> dbList = Perk_DB.Load();
        for (int i = 0; i < dbList.Count; i++)
        {
            if (!unavailableIDList.Contains(dbList[i].ID))
            {
                Perk perk = dbList[i].Clone();
                perkList.Add(perk);
            }
        }

        for (int i = 0; i < perkList.Count; i++) perkList[i].SetPlayer(player);

        StartCoroutine(UnlockPurchasedPerk());
    }

    IEnumerator UnlockPurchasedPerk()
    {
        yield return null;
        yield return null;
        for (int i = 0; i < perkList.Count; i++)
        {
            if (purchasedIDList.Contains(perkList[i].ID)) PurchasePerk(perkList[i], false);
        }
    }


    public int GetPerkCurrency() { return perkCurrency; }
    public void SetPerkCurrency(int value)
    {
        perkCurrency = value;
        CurrencyChanged();
    }
    public void SpendCurrency(int value)
    {
        perkCurrency = Mathf.Max(0, perkCurrency - value);
        CurrencyChanged();
    }
    public void GainCurrency(int value)
    {
        perkCurrency += value;
        CurrencyChanged();
    }
    private void CurrencyChanged()
    {
        TDS.OnPerkCurrency(perkCurrency);
        //if (player.SaveUponChange()) Save();
    }







    //public static Perk GetPerk(int perkID){ return instance._GetPerk(perkID); }
    public Perk GetPerkFromIndex(int index) { return perkList[index]; }
    public Perk GetPerk(int perkID)
    {
        for (int i = 0; i < perkList.Count; i++) { if (perkList[i].ID == perkID) return perkList[i]; }
        return null;
    }
    public int GetPerkIndex(int perkID)
    {
        for (int i = 0; i < perkList.Count; i++) { if (perkList[i].ID == perkID) return i; }
        return -1;
    }
    //public static string IsPerkAvailable(int perkID){ return instance._IsPerkAvailable(perkID); }
    public string IsPerkAvailable(int perkID)
    {
        for (int i = 0; i < perkList.Count; i++) { if (perkList[i].ID == perkID) return perkList[i].IsAvailable(); }
        return "PerkID doesnt correspond to any perk in the list   " + perkID;
    }
    //public static bool IsPerkPurchased(int perkID){ return instance._IsPerkPurchased(perkID); }
    public bool IsPerkPurchased(int perkID)
    {
        for (int i = 0; i < perkList.Count; i++) { if (perkList[i].ID == perkID) return perkList[i].purchased > 0; }
        return false;
    }



    //public static string PurchasePerk(int perkID, bool useCurrency=true){ return instance._PurchasePerk(perkID, useCurrency); }
    public string PurchasePerk(int perkID, bool useCurrency = true, bool saving = true)
    {
        for (int i = 0; i < perkList.Count; i++) { if (perkList[i].ID == perkID) return PurchasePerk(perkList[i], useCurrency, saving); }
        return "PerkID doesnt correspond to any perk in the list";
    }
    //public static string PurchasePerk(Perk perk, bool useCurrency=true){ return instance._PurchasePerk(perk, useCurrency); }
    public string PurchasePerk(Perk perk, bool useCurrency = true, bool saving = true)
    {

        string text = perk.Purchase(this, useCurrency);
        if (text != "")
        {
            Debug.Log(text);
            return text;
        }

        for (int i = 0; i < perkList.Count; i++)
        {
            Perk perkTemp = perkList[i];
            if (perkTemp.purchased > 0 || perkTemp.prereq.Count == 0) continue;
            perkTemp.prereq.Remove(perk.ID);
        }

        TDS.PerkPurchased(perk);

        //if (player.SaveUponChange()) Save();

        if (perk.type == _PerkType.ModifyGeneralStats)
        {
            //perkCurrency += perk.perkCurrencyGain;

            hitPointCap += perk.hitPointCap;
            energyCap += perk.energyCap;

            hitPointRegen += perk.hitPointRegen;
            energyRegen += perk.energyRegen;

            player.GainHitPoint(perk.hitPoint);
            player.GainEnergy(perk.energy);

            moveSpeedMul += perk.moveSpeedMul;

            damageMul += perk.dmgMul;

            expGainMul += perk.expGainMul;
            scoreGainMul += perk.scoreGainMul;

        }
        else if (perk.type == _PerkType.ModifyWeapon)
        {

            weapStatG.ModifyWithPerk(perk);

        }
        else if (perk.type == _PerkType.ModifyAbility)
        {

            abilityStatG.ModifyWithPerk(perk);

        }
        else if (perk.type == _PerkType.Custom)
        {
            GameObject obj = (GameObject)Instantiate(perk.customObject);
            obj.name = perk.name + "_CustomObject";
            obj.transform.parent = transform;
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = Quaternion.identity;
        }

        return "";
    }




    [Header("Player General Stats")]
    public float hitPointCap = 0;
    public float hitPointRegen = 0;
    public float energyCap = 0;
    public float energyRegen = 0;

    public float GetBonusHitPoint() { return hitPointCap; }
    public float GetBonusEnergy() { return energyCap; }
    public float GetBonusHitPointRegen() { return hitPointRegen; }
    public float GetBonusEnergyRegen() { return energyRegen; }


    public float moveSpeedMul = 0;
    public float GetMoveSpeedMul() { return moveSpeedMul; }


    public float damageMul = 0;
    public float GetDamageMul() { return damageMul; }


    public float expGainMul = 0;        //experience
    public float scoreGainMul = 0;

    public float GetExpGainMul() { return expGainMul; }

    public float GetScoreGainMul() { return scoreGainMul; }




    [Header("Weapons Multiplier")]
    public WeaponStatMultiplier weapStatG = new WeaponStatMultiplier();
    //public List<WeaponStatMultiplier> weapStatList = new List<WeaponStatMultiplier>();

    public float GetWeaponDamageMul() { return weapStatG.dmg; }
    // public float GetWeaponCritMul(int ID) { return weapStatG.crit; }
    // public float GetWeaponCritMulMul(int ID) { return weapStatG.critMul; }
    public float GetWeaponAOEMul() { return weapStatG.aoe; }
    public float GetWeaponRangeMul() { return weapStatG.range; }
    public float GetWeaponCDMul() { return weapStatG.cooldown; }
    public float GetWeaponClipSizeMul() { return weapStatG.clipSize; }
    public float GetWeaponAmmoCapMul() { return weapStatG.ammoCap; }
    public float GetWeaponReloadDurMul() { return weapStatG.reloadDuration; }
    public float GetWeaponRecoilMagMul() { return weapStatG.recoilMagnitude; }




    [Header("Abilities Multiplier")]
    public AbilityStatMultiplier abilityStatG = new AbilityStatMultiplier();
    // public List<AbilityStatMultiplier> abilityStatList = new List<AbilityStatMultiplier>();

    public float GetAbilityCostMul() { return abilityStatG.abCost; }
    public float GetAbilityCooldownMul() { return abilityStatG.abCooldown; }
    public float GetAbilityRangeMul() { return abilityStatG.abRange; }
    public float GetAbilityDamageMul() { return abilityStatG.abDmg; }
    public float GetAbilityAOEMul() { return abilityStatG.abAOE; }



    [System.Serializable]
    public class WeaponStatMultiplier
    {
        public int prefabID = 0;
        public float dmg = 0;
        public float aoe = 0;
        public float range = 0;
        public float cooldown = 0;
        public float clipSize = 0;
        public float ammoCap = 0;
        public float reloadDuration = 0;
        public float recoilMagnitude = 0;

        public void ModifyWithPerk(Perk perk)
        {
            dmg += perk.weapDmg;
            aoe += perk.weapAOE;
            range += perk.weapRange;
            cooldown += perk.weapCooldown;
            clipSize += perk.weapClipSize;
            ammoCap += perk.weapAmmoCap;
            reloadDuration += perk.weapReloadDuration;
            recoilMagnitude += perk.weapRecoilMagnitude;
        }
    }

    [System.Serializable]
    public class AbilityStatMultiplier
    {
        public int prefabID = 0;
        public float abCost;
        public float abCooldown;
        public float abRange = 0;
        public float abDmg = 0;
        public float abAOE = 0;


        public void ModifyWithPerk(Perk perk)
        {
            abCost += perk.abCost;
            abCooldown += perk.abCooldown;
            abRange += perk.abRange;
            abDmg += perk.abDmg;
            abAOE += perk.abAOE;
        }
    }

}