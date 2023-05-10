using UnityEngine;

using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using pvp;

[SelectionBase]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class Unit : PooledObject
{

    public delegate void DestroyCallback();
    private List<DestroyCallback> destroyCallbackList = new List<DestroyCallback>();
    public void SetDestroyCallback(DestroyCallback callback) { destroyCallbackList.Add(callback); }


    [HideInInspector] public Transform thisT;
    [HideInInspector] public GameObject thisObj;

    [HideInInspector] public int prefabID = 0;
    [HideInInspector] public int instanceID = -1;
    public Sprite icon;
    public string unitName = "Unit";
    public string desp = "";

    protected bool hostileUnit = false;
    protected bool isPlayer = false;



    [Header("Basic Stats")]
    public int level = 1;

    public float hitPointFull = 10;
    public float hitPoint = 10;
    public bool startHitPointAtFull = true;

    public float hpRegenRate = 0;
    public float hpRegenStagger = 5;
    private float hpRegenStaggerCounter = 0;


    public float energyFull = 10;
    public float energy = 10;
    public float energyRate = 0.5f;
    public bool startEnergyAtFull = true;


    public bool anchorDown = false; //đứng im

    public float moveSpeed = 5;
    public float brakeRange = 2; //ngưỡng khoảng cách so với target, k di chuyển
    public float rotateSpeed = 90;


    [HideInInspector] public Unit target;


    [Header("Range Attack Setting")]
    public bool enableRangeAttack = false; //enable shoot target

    public GameObject shootObject;
    public List<Transform> shootPointList = new List<Transform>();
    public float shootPointDelay = 0; //delay giữa các shootPoint

    public Transform turretObj;
    public bool smoothTurretRotation = true; //quay mượt
    public float turretTrackingSpeed = 90;      //tốc độ quay nếu mượt

    public float range = 30; // khoảng cách đạn bay
    public float cooldown = 5f; //khoảng cách giữa các lần bắn đạn
    protected float currentCD = 0.25f;
    public AttackStats attackStats;


    [Header("Contact Attack Stats")]
    public bool enableContactAttack = false; //enable va chạm với player
    public float contactCooldown = 1f; //delay giữa những lần contact
    [HideInInspector] public float contactCurrentCD = .0f;
    public AttackStats contactAttackStats;



    [Header("Destroyed Setting ")] //Phần thưởng khi destroy
    public int valueScore = 0;
    public int valueHitPoint = 0;
    public int valueEnergy = 0;
    public int valueExp = 0;
    public int valuePerkC = 0;


    public float destroyCamShake = 0;

    public GameObject destroyedEffectObj;
    public bool autoDestroyDObj = true; //destroy effect
    public float dObjActiveDuration = 2; //duration effect

    public bool useDropManager = true; //spawn collectible when destroyed
    public GameObject dropObject; //collectible
    public float dropChance = 0.5f; //tỉ lệ rơi vật phẩm

    public Unit spawnUponDestroy; //spawn unit when destroyed
    public int spawnUponDestroyCount = 2;


    protected bool destroyed = false;
    public bool IsDestroyed() { return destroyed; }


    protected Collider thisCollider;
    public Collider GetCollider() { return thisCollider; }

    [Space(10)] public float spawnImmunity = 3; //thời gian bất tử ban đầu
    private float immunityCounter = -1;
    public void InitSpawnImmunity() { immunityCounter = spawnImmunity; }

    [Space(10)] public UnitAnimation uAnimation;
    public void SetUnitAnimation(UnitAnimation uAnim) { uAnimation = uAnim; }
    protected bool moved = false;   //để gọi anim


    public virtual float GetRange() { return range; }


    public virtual void Awake()
    {
        thisT = transform;
        thisObj = gameObject;

        if (startHitPointAtFull) hitPoint = hitPointFull;

        if (startEnergyAtFull) energy = energyFull;
        else energy = Mathf.Clamp(energy, 0, energyFull);

        currentCD = 0;

        thisCollider = thisObj.GetComponent<Collider>();
        if (anchorDown)
        {
            thisObj.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
            thisObj.GetComponent<Rigidbody>().isKinematic = true;
            moveSpeed = 0;
            rotateSpeed = 0;
        }
        else
        {
            Rigidbody rBody = thisObj.GetComponent<Rigidbody>();
            if (rBody != null) rBody.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
        }


        attackStats.Init();
        contactAttackStats.Init();


    }

    public virtual void Start()
    {

        if (instanceID <= 0)
            instanceID = GameControl.GetUnitInstanceID();

        
        if (hostileUnit) UnitTracker.AddUnit(this);

        TDS.NewUnit(this);

        if (dropObject != null)
        {
            if (!dropObject.TryGetComponent<PooledObject>(out var pooledObject))
            {
                pooledObject = dropObject.AddComponent<PooledObject>();
            }
        }

        if (destroyedEffectObj != null)
        {
            if (!destroyedEffectObj.TryGetComponent<PooledObject>(out var pooledObject))
            {
                pooledObject = destroyedEffectObj.AddComponent<PooledObject>();
            }
        }
    }


    public void OverrideHitPoint(float value, _OverrideMode mode)
    {
        if (mode == _OverrideMode.Replace) hitPointFull = value;
        else if (mode == _OverrideMode.Addition) hitPointFull += value;
        else if (mode == _OverrideMode.Multiply) hitPointFull *= value;

        hitPoint = hitPointFull;
    }


    public virtual void Update()
    {
        //regenerate energy
        if (energy < GetFullEnergy() && GetEnergyRegen() > 0)
        {
            energy = Mathf.Clamp(energy + GetEnergyRegen() * Time.deltaTime, 0, GetFullEnergy());
        }

        //regenerate hit point
        hpRegenStaggerCounter -= Time.deltaTime;
        if (hpRegenStaggerCounter <= 0 && GetHitPointRegen() > 0)
        {
            hitPoint = Mathf.Min(GetFullHitPoint(), hitPoint + GetHitPointRegen() * Time.deltaTime);
        }

        if (immunityCounter > 0) immunityCounter -= Time.deltaTime;
    }


    public virtual float GainHitPoint(float value)
    {
        float limit = GetFullHitPoint() - hitPoint;
        hitPoint += Mathf.Min(value, limit);

        if (value < 0)
        {
            hpRegenStaggerCounter = hpRegenStagger;


            if (thisObj.layer == TDS.GetLayerPlayer()) TDS.PlayerDamaged(value);
        }


        if (hitPoint <= 0) Destroyed();

        return limit;
    }

    public virtual float GainEnergy(float value)
    {
        float limit = GetFullEnergy() - energy;
        energy += Mathf.Min(value, limit);
        return limit;
    }


    public virtual float GetSpeedMultiplier() { return activeEffect.speedMul; }
    public bool IsStunned() { return activeEffect.stun; }
    public bool IsInvincible() { return activeEffect.invincible; }



    public void ApplyAttack(AttackInstance attInstance)
    {
        if (thisObj.layer == TDS.GetLayerOtherPlayer())
        {
            attInstance.srcUnit = null;
            NetworkManager.Instance.Manager.Socket
                .Emit("attackPlayer", new AttackPlayer
                {
                    username = PvPManager.instance.GetIdOtherPlayer((UnitPlayer)this),
                    roomId = PvP.GetRoom(),
                    attackInstance = attInstance
                });
            return;
        }
        if (immunityCounter > 0) return;


        if (IsInvincible())
        {
            Debug.Log("Immuned");
            Vector3 osPos = new Vector3(0, Random.value + 0.5f, 0);
            new TextOverlay(thisT.position + osPos, "Immuned");
            return;
        }

        AttackStats aStats = attInstance.aStats;

        
        float damage = Random.Range(aStats.damageMin, aStats.damageMax);
        if (attInstance.isAOE && aStats.diminishingAOE)
        {
            damage *= Mathf.Clamp(1 - attInstance.aoeDistance / aStats.aoeRadius, 0, 1);
        }

        if (damage > 0)
        {

            if (GameControl.GetInstance().pvp && thisObj.layer != TDS.GetLayerPlayer())
            {
                NetworkManager.Instance.Manager.Socket
                    .Emit("unitHealthChange", new UnitHealth 
                    {
                        roomId = PvP.GetRoom(),
                        isEnemy = thisObj.layer == TDS.GetLayerAIUnit(),
                        instanceID = instanceID,
                        name = gameObject.name,
                        hitPoint = hitPoint - damage
                    });
            }
            Vector3 offsetPos = new Vector3(0, Random.value + 0.5f, 0);
           
            new TextOverlay(thisT.position + offsetPos, damage.ToString("f0"), new Color(1f, 0.9f, 0.9f, 1f), 1.5f);


            if (thisObj.layer == TDS.GetLayerPlayer()) TDS.PlayerDamaged(damage);


            hpRegenStaggerCounter = hpRegenStagger;

            hitPoint -= damage;
            if (hitPoint <= 0) Destroyed(attInstance.GetSrcPlayer());

            if (hitPoint > 0 && uAnimation != null) uAnimation.Hit();
        }


        //if(aStats.effect!=null) ApplyEffect(aStats.effect);
        if (hitPoint > 0 && aStats.effectIdx >= 0) ApplyEffect(Effect_DB.CloneItem(aStats.effectIdx));
    }



    public void Destroyed(UnitPlayer player = null)
    {
        if (destroyed) return;


        destroyed = true;

        if (spawnUponDestroy != null)
        {

            for (int i = 0; i < spawnUponDestroyCount; i++)
            {
            
                Unit unitObj = spawnUponDestroy.GetPoolItem<Unit>(thisT.position, thisT.rotation);
                unitObj.gameObject.layer = thisObj.layer;


                if (waveID >= 0)
                {
                    unitObj.SetWaveID(spawner, waveID);
                    spawner.AddUnitToWave(unitObj);
                }
                else if (spawner != null) spawner.AddUnit(unitObj);
            }
        }


        if (useDropManager)
        {  
        
            CollectibleDropManager.instance.UnitDestroyed(thisT.position);
        }
        else
        {

            if (dropObject != null && Random.value < dropChance)
                //ObjectPoolManager.Spawn(dropObject.gameObject, thisT.position, Quaternion.identity);
                dropObject.GetComponent<PooledObject>().GetPoolItem(thisT.position, Quaternion.identity);
        }


        if (player != null)
        {
            
            if (valueScore > 0) GameControl.GainScore(valueScore);

            if (valuePerkC > 0) player.GainPerkCurrency(valuePerkC);
            if (valueExp > 0) player.GainExp(valueExp);
            if (valueHitPoint > 0) player.GainHitPoint(valueHitPoint);
            if (valueEnergy > 0) player.GainEnergy(valueEnergy);
        }

        if (GetUnitAI()) GetUnitAI()._Destroyed();

        


        TDS.CameraShake(destroyCamShake);

        float delay = uAnimation != null ? uAnimation.Destroyed() : 0;

        ClearUnit(true, delay);
    }


    public void ClearUnit(bool showDestroyEffect = true, float delay = 0) 
    { 
        if (GameControl.GetInstance().pvp && thisObj.layer != TDS.GetLayerPlayer())
            {
                NetworkManager.Instance.Manager.Socket
                    .Emit("unitClear", new ClearUnit
                    {
                        roomId = PvP.GetRoom(),
                        isEnemy = thisObj.layer == TDS.GetLayerAIUnit(),
                        instanceID = instanceID,
                        name = gameObject.name
                    });
            }
        StartCoroutine(_ClearUnit(showDestroyEffect, delay)); 
    }

    public void OnPvPClearUnit()
    {
        StartCoroutine(_ClearUnit());
    }
    public IEnumerator _ClearUnit(bool showDestroyEffect = true, float delay = 0)
    {
        destroyed = true;

        if (spawner != null && waveID >= 0)
        {
            spawner.UnitCleared(waveID);
        }

        if (hostileUnit)
        {
            UnitTracker.RemoveUnit(this);
            GameControl.UnitDestroyed(this);
        }


        //remove effect
        for (int i = 0; i < effectList.Count; i++) effectList[i].expired = true;


        for (int i = 0; i < destroyCallbackList.Count; i++) destroyCallbackList[i]();

        if (showDestroyEffect) DestroyedEffect(thisT.position + new Vector3(0, 0.1f, 0));

        if (delay > 0) yield return new WaitForSeconds(delay);


        Destroy(thisObj);
        //ReturnToPool();

        yield return null;
    }


    void DestroyedEffect(Vector3 pos)
    {
        if (destroyedEffectObj == null) return;

        var effectObj = destroyedEffectObj.GetComponent<PooledObject>().GetPoolItem(pos, thisT.rotation);
        if (autoDestroyDObj)
        {
            effectObj.ReturnToPool(dObjActiveDuration);
        }
    }



    [HideInInspector] public int waveID = -1;
    [HideInInspector] public UnitSpawner spawner;
    public void SetWaveID(UnitSpawner sp, int id)
    {
        spawner = sp;
        waveID = id;
    }




    private List<Effect> effectList = new List<Effect>();

    public void ClearAllEffect()
    {
        effectList = new List<Effect>();
        UpdateActiveEffect();
    }


    public virtual bool ApplyEffect(Effect effect)
    {
        if (effect == null || !effect.Applicable()) return false;


        effectList.Add(effect);
        UpdateActiveEffect();

        if (!effCoroutine) StartCoroutine(EffectRoutine());

        return true;
    }

    private bool effCoroutine = false;   //ngăn chạy coroutine 2 lần
                                        
    IEnumerator EffectRoutine()
    {
        effCoroutine = true;

        while (true)
        {

            if (activeEffect.restoreHitPoint != 0) GainHitPoint(activeEffect.restoreHitPoint * Time.deltaTime);
            if (activeEffect.restoreEnergy != 0) GainEnergy(activeEffect.restoreEnergy * Time.deltaTime);

            bool updateEffect = false;
            for (int i = 0; i < effectList.Count; i++)
            {
                effectList[i].duration -= Time.deltaTime;
                if (effectList[i].duration <= 0)
                {
                    effectList.RemoveAt(i);
                    i -= 1;
                    updateEffect = true;    //remove khỏi effect tổng
                }
            }
            if (updateEffect) UpdateActiveEffect();

            yield return null;
        }
    }


    public Effect activeEffect = new Effect();

    public void UpdateActiveEffect()
    {
        activeEffect = new Effect();
        for (int i = 0; i < effectList.Count; i++)
        {
            activeEffect.restoreHitPoint += effectList[i].restoreHitPoint;
            activeEffect.restoreEnergy += effectList[i].restoreEnergy;

            activeEffect.invincible |= effectList[i].invincible;
            activeEffect.stun |= effectList[i].stun;
            activeEffect.speedMul *= effectList[i].speedMul;

            activeEffect.damageMul *= effectList[i].damageMul;

        }
    }

    protected AttackStats ModifyAttackStatsToExistingEffect(AttackStats aStats)
    {
        aStats.damageMin *= activeEffect.damageMul;
        aStats.damageMax *= activeEffect.damageMul;


        return aStats;
    }



    public virtual float GetFullHitPoint() { return hitPointFull; }
    public virtual float GetFullEnergy() { return energyFull; }
    public virtual float GetHitPointRegen() { return hpRegenRate; }
    public virtual float GetEnergyRegen() { return energyRate; }

    public virtual UnitPlayer GetUnitPlayer() { return null; }
    public virtual UnitAI GetUnitAI() { return null; }

}
