using UnityEngine;
using UnityEngine.AI;

using System.Collections;
using System.Collections.Generic;


public enum _Behaviour
{
    StandGuard,         //truy đuổi khi trong tầm ngắm
    Aggressive,             //luôn tìm kiếm mục tiêu
    Aggressive_Trigger, //
    Passive,
}

public class UnitAI : Unit
{


    public NavMeshAgent agent;
    public bool destroyUponPlayerContact = false;

    public _Behaviour behaviour = _Behaviour.Aggressive;
    public float aggroRange = 20; //phạm vi truy đuổi
    private bool guardTriggered = false;    //Behaviour.StandGuard (true nếu trong tầm ngắm)

    public bool stopOccasionally = false; //thỉnh thoảng dừng 
    public float stopRate = 0.5f; //tỉ lệ dừng
    public float stopDuration = 1.5f;
    public float stopCooldown = 3;
    private bool stopping = false;
    private float stopCD = 0;

    public bool evadeOccasionally = false; //thỉnh thoảng đi lung tung
    public float evadeRate = 0.5f;
    public float evadeDuration = 1.5f;
    public float evadeCooldown = 3;
    private bool evading = false;
    private float evadeCD = 0;



    public bool shootPeriodically = false; 
    public bool alwaysShootTowardsTarget = false;

    public bool randFirstAttackDelay = true; //random thời gian delay ban đầu
    public float firstAttackDelay = 0;





    public override UnitAI GetUnitAI() { return this; }


    public override void Awake()
    {
        base.Awake();

        thisObj.layer = TDS.GetLayerAIUnit();
        hostileUnit = true;


        if (!randFirstAttackDelay) currentCD = firstAttackDelay;
        else currentCD = Random.Range(0, firstAttackDelay);


        if (enableRangeAttack)
        {
            if (shootPointList.Count == 0) shootPointList.Add(thisT);
            if (shootObject == null)
            {
                enableRangeAttack = false;
            }
        }

        agent = thisObj.GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.stoppingDistance = brakeRange;
            agent.speed = moveSpeed;
        }

        evadeCD = evadeCooldown;
        stopCD = stopCooldown;
    }

    public override void Start()
    {
        base.Start();
    }


    public override void Update()
    {
        if (GameControl.GetInstance() && GameControl.GetInstance().pvp && PvP.GetLandSpawnPlayer() > 0) return;
        if (GameControl.IsGameOver() || destroyed || IsStunned()) return;

        target = GameControl.GetPlayer();

        if (GameControl.GetInstance().pvp)
        {
            target = PvPManager.instance.GetPlayerNearest(thisT.position);
        }
        if (target != null && !target.thisObj.activeInHierarchy) target = null;

        currentCD -= Time.deltaTime;
        contactCurrentCD -= Time.deltaTime;

        if (shootPeriodically) ShootTarget();

        if (target == null)
        {
            ResetTurret();
            return;
        }

        float targetDist = Vector3.Distance(thisT.position, target.thisT.position);


        if (behaviour == _Behaviour.Aggressive)
        {
            EngageHostile(targetDist);
        }
        else if (behaviour == _Behaviour.Aggressive_Trigger)
        {

            if (Vector3.Distance(thisT.position, target.thisT.position) <= aggroRange) behaviour = _Behaviour.Aggressive;
        }
        else if (behaviour == _Behaviour.StandGuard)
        {
            if (guardTriggered)
            {  
                EngageHostile(targetDist); 
                if (targetDist > aggroRange * 2)
                {
                    guardTriggered = false;

                    StopNavMeshAgent();
                }
            }
            else
            {
                
                if (targetDist <= aggroRange) guardTriggered = true;
            }
        }
    }


    void LateUpdate()
    {
        if (uAnimation != null)
        {
            if (agent == null) uAnimation.Move(moved ? 1 : 0, 0);
            else uAnimation.Move(agent.destination != thisT.position ? 1 : 0, 0);
        }

        moved = false;
    }


    void EngageHostile(float targetDist)
    {
        AimAtTarget(targetDist);
        ShootTarget(targetDist);

        if (agent != null)
        {
            ChaseTarget();
            return;
        }


        stopCD -= Time.deltaTime;
        evadeCD -= Time.deltaTime;

        if (stopOccasionally && !stopping && stopCD < 0) StartCoroutine(StopRoutine());
        if (evadeOccasionally && !evading && evadeCD < 0) StartCoroutine(EvadeRoutine());

        if (stopping || evading) return;

        ChaseTarget();
    }


    IEnumerator StopRoutine()
    {
        float duration = Random.Range(stopDuration * 0.8f, stopDuration * 1.2f);
        stopCD = stopCooldown + duration;

        if (Random.value > stopRate) yield break;

        stopping = true;
        yield return new WaitForSeconds(duration);
        stopping = false;
    }
    
    IEnumerator EvadeRoutine()
    {
    
        float duration = Random.Range(evadeDuration * 0.8f, evadeDuration * 1.2f);
        evadeCD = evadeCooldown + duration;

        if (Random.value > evadeRate) yield break;

        //random hướng
        float x = Random.Range(0.5f, 1f) * (Random.value < 0.5f ? -1 : 1);
        float z = Random.Range(.5f, .1f);
        Vector3 dummyPos = thisT.TransformPoint(new Vector3(x, 0, z).normalized * moveSpeed * duration * 2);

        evading = true;
        while (duration > 0)
        {
            MoveToPoint(dummyPos, 2);
            duration -= Time.deltaTime;
            yield return null;
        }
        evading = false;
    }

    void MoveToPoint(Vector3 targetPoint, float brakeTH = 0, float speedMultiplier = 1)
    {
        if (moveSpeed <= 0) return;

        Quaternion wantedRot = Quaternion.LookRotation(targetPoint - thisT.position);

        if (Vector3.Distance(thisT.position, targetPoint) > brakeTH)
        {

            thisT.rotation = Quaternion.Lerp(thisT.rotation, wantedRot, GetRotateSpeed(thisT.rotation, wantedRot, rotateSpeed * speedMultiplier));

            thisT.Translate(Vector3.forward * Time.deltaTime * moveSpeed * GetSpeedMultiplier() * speedMultiplier);

            moved = true;
        }

        else
        {
            
            if (turretObj == null || Quaternion.Angle(wantedRot, thisT.rotation) > 90)
            {
                thisT.rotation = Quaternion.Lerp(thisT.rotation, wantedRot, GetRotateSpeed(thisT.rotation, wantedRot, rotateSpeed));
            }
        }
    }

    void AimAtTarget(float targetDist)
    {
        if (turretObj == null) return;

        if (target == null || targetDist > GetRange() * 1.25f)
        {
            ResetTurret();
            return;
        }

        Vector3 tgtPos = target.thisT.position;
        Vector3 turretPos = new Vector3(turretObj.position.x, tgtPos.y, turretObj.position.z);
        Quaternion wantedRot = Quaternion.LookRotation(tgtPos - turretPos);

        if (!smoothTurretRotation) turretObj.rotation = wantedRot;
        else turretObj.rotation = Quaternion.Lerp(turretObj.rotation, wantedRot, GetRotateSpeed(turretObj.rotation, wantedRot, turretTrackingSpeed));
    }

    void ResetTurret()
    {
        if (turretObj == null) return;
        if (!smoothTurretRotation) turretObj.rotation = Quaternion.identity;
        else turretObj.localRotation = Quaternion.Lerp(turretObj.localRotation, Quaternion.identity, Time.deltaTime * turretTrackingSpeed);
    }


    void ChaseTarget()
    {
        if (agent != null)
        {
            agent.speed = moveSpeed * GetSpeedMultiplier();
            agent.SetDestination(target.thisT.position);
        }
        else
        {
            MoveToPoint(target.thisT.position, brakeRange);
        }
    }


    private Vector3 targetLastPos;

    void ShootTarget(float targetDist = 0)
    {
        if (enableRangeAttack && currentCD <= 0) StartCoroutine(_ShootTarget(targetDist));
    }
    IEnumerator _ShootTarget(float targetDist)
    {
        if (targetDist > GetRange()) yield break;

        currentCD = cooldown;

        if (uAnimation != null) uAnimation.AttackRange(); //play anim

        List<Collider> soColliderList = new List<Collider>();


        for (int i = 0; i < shootPointList.Count; i++)
        {

            AttackInstance attInstance = new AttackInstance(this, ModifyAttackStatsToExistingEffect(attackStats.Clone()));

            //lưu lại position của target, phòng trường hợp target == null
            if (target != null) targetLastPos = target.thisT.position;

            ShootObject.AimInfo aimInfo = target != null ? new ShootObject.AimInfo(target) : new ShootObject.AimInfo(targetLastPos);


            Quaternion shootRot = shootPointList[i].rotation;
            if (alwaysShootTowardsTarget && target != null) shootRot = Quaternion.LookRotation(target.thisT.position - thisT.position);


            ShootObject soInstance = shootObject.GetComponent<ShootObject>().GetPoolItem<ShootObject>(shootPointList[i].position, shootRot);

            soInstance.IgnoreCollider(GetCollider());
            for (int n = 0; n < soColliderList.Count; n++) soInstance.IgnoreCollider(soColliderList[n]);
            if (soInstance.GetCollider() != null) soColliderList.Add(soInstance.GetCollider());

            soInstance.Shoot(thisObj.layer, GetRange(), shootPointList[i], attInstance, aimInfo);

            if (shootPointDelay > 0) yield return new WaitForSeconds(shootPointDelay);
        }

        yield return null;
    }




    private float GetRotateSpeed(Quaternion srcRot, Quaternion wantedRot, float rotSpd)
    {
        float angle = Quaternion.Angle(srcRot, wantedRot);
        return angle == 0 ? 0 : Time.deltaTime * rotSpd * GetSpeedMultiplier() / (Quaternion.Angle(srcRot, wantedRot));
    }


    void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.layer != TDS.GetLayerPlayer()) return;

        OnPlayerContact();
        if (destroyUponPlayerContact) ClearUnit();
    }

    void OnCollisionStay(Collider col)
    {
        if (col.gameObject.layer != TDS.GetLayerPlayer()) return;

        OnPlayerContact();
        if (destroyUponPlayerContact) ClearUnit();
    }


    void OnPlayerContact()
    {
        if (!enableContactAttack) return;

        if (contactCurrentCD > 0) return;

        if (uAnimation != null) uAnimation.AttackMelee();

        contactCurrentCD = contactCooldown;

        AttackInstance attInstance = new AttackInstance(this, ModifyAttackStatsToExistingEffect(contactAttackStats.Clone()));
        GameControl.GetPlayer().ApplyAttack(attInstance);
    }



    public void _Destroyed()
    {
        StopNavMeshAgent();
    }

    private void StopNavMeshAgent()
    {
        if (agent == null) return;
        agent.speed = 0;
        agent.velocity = Vector3.zero;
        agent.SetDestination(thisT.position);
    }

}
