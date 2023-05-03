using UnityEngine;

using System.Collections;
using System.Collections.Generic;

public enum _AbilityType
{
    AOE,
    AOESelf,
    //Ray,
    All,
    Self,
    Shoot,
    Movement,
    Custom,
}

public enum _MoveType { Dash, Teleport }

[System.Serializable]
public class Ability : Item
{
    public string desp;

    public _AbilityType type;

    //sử dụng cho movement
    public _MoveType moveType;
    public float duration = 0.2f;
    //end sử dụng cho movement

    public float cost;

    public float cooldown = 0.15f;
    [HideInInspector] public float currentCD = 0.0f;

    public AttackStats aStats = new AttackStats();

    public float range = 20; //khoảng cách tối đa có thể di chuyển
    public GameObject shootObject;
    public Vector3 shootPosOffset = new Vector3(0, 1, 0);

    public GameObject launchObj;
    public bool autoDestroyLaunchObj = true;
    public float launchObjActiveDuration = 2;

    public AudioClip launchSFX;

    //call when an ability added to player
    public void Init()
    {
        aStats.Init();

        currentCD = 0;
    }


    //check if the ability is ready
    public string IsReady()
    {
        if (GameControl.GetPlayer().energy < GetCost()) return "Insufficient Energy";
        if (currentCD > 0) return "Ability on Cooldown";
        return "";
    }


    //active ability
    public void Activate(Vector3 pos = default(Vector3), bool useCostNCD = true)
    {
        if (useCostNCD)
        {
            currentCD = GetCooldown();                          //set cooldown
            GameControl.GetPlayer().energy -= GetCost();    //giảm energy player theo cost
        }

        AudioManager.PlaySound(launchSFX);

        //custom type
        if (launchObj != null)
        {
            GameObject obj = (GameObject)MonoBehaviour.Instantiate(launchObj, pos, Quaternion.identity);
            if (autoDestroyLaunchObj) MonoBehaviour.Destroy(obj, launchObjActiveDuration);
        }

        if (type == _AbilityType.AOE || type == _AbilityType.AOESelf)
        {
            //get all collider in range
            Collider[] cols = Physics.OverlapSphere(pos, GetAOERadius());
            for (int i = 0; i < cols.Length; i++)
            {
                Unit unitInstance = cols[i].gameObject.GetComponent<Unit>();

                //apply attack lên unit không phải player
                if (unitInstance != null && unitInstance != GameControl.GetPlayer())
                {
                    AttackInstance aInstance = new AttackInstance(GameControl.GetPlayer(), GetRuntimeAttackStats());
                    aInstance.isAOE = true;
                    aInstance.aoeDistance = Vector3.Distance(pos, cols[i].transform.position);
                    //apply attack
                    unitInstance.ApplyAttack(aInstance);
                }
            }

            //apply lực đẩy
            TDSPhysics.ApplyExplosionForce(pos, aStats);
        }

        //apply effect lên toàn bộ kẻ thù
        else if (type == _AbilityType.All)
        {
            //lấy tất cả kẻ thù
            List<Unit> unitList = new List<Unit>(UnitTracker.GetAllUnitList());
            for (int i = 0; i < unitList.Count; i++)
            {
                AttackInstance aInstance = new AttackInstance(GameControl.GetPlayer(), GetRuntimeAttackStats());
                unitList[i].ApplyAttack(aInstance);
            }
        }

        //apply lên player
        else if (type == _AbilityType.Self)
        {
            AttackInstance aInstance = new AttackInstance(GameControl.GetPlayer(), GetRuntimeAttackStats());
            GameControl.GetPlayer().ApplyAttack(aInstance);
        }

        //shoot type
        else if (type == _AbilityType.Shoot)
        {
            //get the position của nòng súng player
            Transform srcT = GetShootObjectSrcTransform();
            Vector3 shootPos = srcT.TransformPoint(shootPosOffset); //chuyển local to world space
            pos.y = shootPos.y;
            Quaternion shootRot = GameControl.GetPlayer().turretObj.rotation;

            //create the AttackInstance với nguồn gây ra là player
            AttackInstance aInstance = new AttackInstance(GameControl.GetPlayer(), GetRuntimeAttackStats());

            //Instantiate the shoot-object
            ShootObject soInstance = shootObject.GetComponent<ShootObject>().GetPoolItem<ShootObject>(shootPos, shootRot);
            soInstance.Shoot(GameControl.GetPlayer().thisObj.layer, GetRange(), srcT, aInstance);
        }

        else if (type == _AbilityType.Movement)
        {
            if (moveType == _MoveType.Dash) //tốc biến
            {
                GameControl.GetPlayer().Dash(range, duration);
            }
            else if (moveType == _MoveType.Teleport) //di chuyển sang vị trí khác
            {
                Transform playerT = GameControl.GetPlayer().thisT;
                Vector3 tgtPos = new Vector3(pos.x, playerT.position.y, pos.z);

                if (Vector3.Distance(playerT.position, tgtPos) > range)
                {
                    tgtPos = playerT.position + (tgtPos - playerT.position).normalized * range;
                }

                playerT.position = tgtPos;
            }
        }
    }


    //get nòng súng player
    public Transform GetShootObjectSrcTransform()
    {
        return GameControl.GetPlayer().turretObj != null ? GameControl.GetPlayer().turretObj : GameControl.GetPlayer().thisT;
    }


    public Ability Clone()
    {
        Ability ab = new Ability();

        ab.ID = ID;
        ab.name = name;
        ab.icon = icon;
        ab.desp = desp;

        ab.type = type;

        ab.cost = cost;
        ab.cooldown = cooldown;
        ab.currentCD = currentCD;

        ab.aStats = aStats.Clone();

        ab.range = range;
        ab.shootObject = shootObject;
        ab.shootPosOffset = shootPosOffset;

        ab.moveType = moveType;
        ab.duration = duration;

        ab.launchObj = launchObj;
        ab.autoDestroyLaunchObj = autoDestroyLaunchObj;
        ab.launchObjActiveDuration = launchObjActiveDuration;

        return ab;
    }




    private PlayerPerk perk;
    public void SetPlayerPerk(PlayerPerk pPerk) { perk = pPerk; }


    public float GetCost() { return cost * (1 + (perk != null ? perk.GetAbilityCostMul() : 0)); }
    public float GetCooldown() { return cooldown * (1 + (perk != null ? perk.GetAbilityCooldownMul() : 0)); }
    public float GetRange() { return range * (1 + (perk != null ? perk.GetAbilityRangeMul() : 0)); }
    public float GetAOERadius() { return aStats.aoeRadius * (1 + (perk != null ? perk.GetAbilityAOEMul() : 0)); }

    public AttackStats GetRuntimeAttackStats()
    {
        if (perk == null) return aStats.Clone();

        aStats.damageMin *= (1 + perk.GetAbilityDamageMul());
        aStats.damageMax *= (1 + perk.GetAbilityDamageMul());

        aStats.aoeRadius *= (1 + perk.GetAbilityAOEMul());

        return aStats;
    }

}


