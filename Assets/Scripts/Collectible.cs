using UnityEngine;

using System.Collections;
using System.Collections.Generic;


public enum _CollectType { Self, AOEHostile, AllHostile, Ability }

[RequireComponent(typeof(SphereCollider))]
public class Collectible : PooledObject
{

    [HideInInspector] public int ID = -1;
    public Sprite icon;
    public string collectibleName = "Collectible";
    public string desp = "";

    public _CollectType type;

    [Header("Hostile")]
    public float aoeRange = 0;

    public AttackStats aStats;


    [Header("Self")]

    public float hitPoint = 0;
    public float energy = 0;

    public int score = 0;

    public int ammo = 0;        

    public int exp = 0;
    public int perkCurrency = 0;

    public int effectID = -1;
    [HideInInspector] private int effectIdx = -1;



    [Header("Common")]

    public GameObject triggerEffectObj;
    public bool autoDestroyEffectObj = true;
    public float effectObjActiveDuration = 2;

    public AudioClip triggerSFX;


    public bool selfDestruct = false;
    public float selfDestructDuration = 5;

    public bool blinkBeforeDestroy;
    public float blinkDuration;
    public GameObject blinkObj;
    public Transform GetBlinkObjT() { return blinkObj == null ? null : blinkObj.transform; }




    void Awake()
    {
        gameObject.GetComponent<Collider>().isTrigger = true;
        gameObject.layer = TDS.GetLayerCollectible();

        effectIdx = Effect_DB.GetEffectIndex(effectID);

        if (triggerEffectObj != null)
        {
            if (!triggerEffectObj.TryGetComponent<PooledObject>(out var pooledObject))
            {
                pooledObject = triggerEffectObj.AddComponent<PooledObject>();
            }
        }
    }

    void OnEnable()
    {
        if (selfDestruct)
        {
            //Destroy(gameObject, selfDestructDuration);
            //ObjectPoolManager.Unspawn(gameObject, selfDestructDuration);

            ReturnToPool(selfDestructDuration);

            if (blinkBeforeDestroy && blinkObj != null) StartCoroutine(Blink());
        }
    }
    //blink trước khi destroy
    IEnumerator Blink()
    {
        float delay = Mathf.Max(0, selfDestructDuration - blinkDuration);
        yield return new WaitForSeconds(delay);

        while (true)
        {
            blinkObj.SetActive(false);
            yield return new WaitForSeconds(0.15f);
            blinkObj.SetActive(true);
            yield return new WaitForSeconds(0.35f);
        }
    }


    void OnDisable()
    {
        //callback
        for (int i = 0; i < triggerCallbackList.Count; i++)
        {
            if (triggerCallbackList[i] != null) triggerCallbackList[i](this);
        }
    }


    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.layer == TDS.GetLayerOtherPlayer()) ReturnToPool();

        if (col.gameObject.layer != TDS.GetLayerPlayer()) return;

        if (type == _CollectType.Self)
        {
            ApplyEffectSelf(col.gameObject.GetComponent<UnitPlayer>());
        }
        else if (type == _CollectType.AOEHostile)
        {
            ApplyEffectAOE(col);
        }
        else if (type == _CollectType.AllHostile)
        {
            ApplyEffectAll();
        }


        GameControl.ColletibleCollected(this);

        AudioManager.PlaySound(triggerSFX);
        TriggeredEffect(transform.position + new Vector3(0, 0.1f, 0));

        //Destroy(gameObject);
        //ObjectPoolManager.Unspawn(gameObject);
        ReturnToPool();
    }


    void ApplyEffectAll()
    {
        //get all kẻ thù từ UnitTracker
        List<Unit> unitList = UnitTracker.GetAllUnitList();

        for (int i = 0; i < unitList.Count; i++)
        {
            AttackInstance aInstance = new AttackInstance();
            aInstance.aStats = aStats.Clone();
            unitList[i].ApplyAttack(aInstance);
        }
    }


    void ApplyEffectAOE(Collider playerCollider)
    {
        float aoeRadius = aStats.aoeRadius;

        if (aoeRadius > 0)
        {
            Collider[] cols = Physics.OverlapSphere(transform.position, aoeRadius); //get all the collider in range
            for (int i = 0; i < cols.Length; i++)
            {
                if (cols[i] == playerCollider) continue;

                AttackInstance aInstance = new AttackInstance(null, aStats);
                aInstance.isAOE = true;
                aInstance.aoeDistance = Vector3.Distance(transform.position, cols[i].transform.position);

                Unit unitInstance = cols[i].gameObject.GetComponent<Unit>();
                if (unitInstance != null) unitInstance.ApplyAttack(aInstance);
            }
        }

        //lực
        TDSPhysics.ApplyExplosionForce(transform.position, aStats, true);
    }


    void ApplyEffectSelf(UnitPlayer player)
    {

        if (hitPoint > 0)
        {
            float hitPointGained = player.GainHitPoint(hitPoint);

            Vector3 offsetPos = new Vector3(0, Random.value + 0.5f, 0);
            new TextOverlay(transform.position + offsetPos, "+" + hitPointGained.ToString("f0"), new Color(0.3f, 1f, 0.3f, 1));
        }

        if (energy > 0)
        {
            float energyGained = player.GainEnergy(energy);

            Vector3 offsetPos = new Vector3(0, Random.value + 0.5f, 0);
            new TextOverlay(transform.position + offsetPos, "+" + energyGained.ToString("f0"), new Color(.3f, .3f, 1f, 1));
        }


        if (score > 0)
        {
            GameControl.GainScore(score);

            Vector3 offsetPos = new Vector3(0, Random.value + 0.5f, 0);
            new TextOverlay(transform.position + offsetPos, "+" + score.ToString("f0"), new Color(.1f, 1f, 1, 1));
        }

        if (ammo != 0)
        {
            player.GainAmmo(ammo);

            Vector3 offsetPos = new Vector3(0, Random.value + 0.5f, 0);
            new TextOverlay(transform.position + offsetPos, "+ammo");
        }

        if (exp != 0)
        {
            player.GainExp(exp);

            Vector3 offsetPos = new Vector3(0, Random.value + 0.5f, 0);
            new TextOverlay(transform.position + offsetPos, "+exp", new Color(1f, 1f, 1, 1));
        }

        if (perkCurrency != 0)
        {
            player.GainPerkCurrency(perkCurrency);

            Vector3 offsetPos = new Vector3(0, Random.value + 0.5f, 0);
            new TextOverlay(transform.position + offsetPos, "+perk points", new Color(1f, 1f, 1, 1));
        }

        if (effectIdx >= 0) player.ApplyEffect(Effect_DB.CloneItem(effectIdx));

    }


    //effect
    void TriggeredEffect(Vector3 pos)
    {
        if (triggerEffectObj == null) return;

        var triggerObj = triggerEffectObj.GetComponent<PooledObject>().GetPoolItem(pos, Quaternion.identity);
        if (autoDestroyEffectObj)
        {
            triggerObj.ReturnToPool(effectObjActiveDuration);
        }

    }

    public delegate void TriggerCallback(Collectible clt);
    private List<TriggerCallback> triggerCallbackList = new List<TriggerCallback>();
    public void SetTriggerCallback(TriggerCallback callback) { triggerCallbackList.Add(callback); }

}
