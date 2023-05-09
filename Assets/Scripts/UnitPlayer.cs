using UnityEngine;

using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using pvp;

public enum _MovementMode { Rigid, FreeForm }
public enum _TurretAimMode { ScreenSpace, Raycast }

public class UnitPlayer : Unit
{

    public override UnitPlayer GetUnitPlayer() { return this; }

    private Camera cam;
    private Transform camT;

    private Transform turretObjParent;

    [Header("Weapon Info")]
    public Transform weaponMountPoint; //vị trí đặt súng

    public Weapon weapon;
    [HideInInspector] public bool weaponInitiated = false;


    [Header("Aiming")]
    public bool enableTurretRotate = true; //nòng súng có thể xoay
    public _TurretAimMode turretAimMode = _TurretAimMode.ScreenSpace;
    public LayerMask castMask;

    public bool aimAtTravelDirection = true; //xoay theo hướng di chuyển
    public bool enableAutoAim = false;          //auto xoay


    [Header("Movement")]
    public bool faceTravelDirection = true; //xoay xe tăng theo hướng di chuyển
    public bool enabledMovementX = true;
    public bool enabledMovementZ = true;

    public _MovementMode movementMode = _MovementMode.FreeForm;
    public float acceleration = 3; //tốc độ tăng tốc
    public float decceleration = 1; //tốc độ giảm tốc
    public float activeBrakingRate = 1; //tốc độ phanh
    private Vector3 velocity;
    private float momentum;

    public bool useLimit = false; //giới hạn phạm vi di chuyển
    public bool showGizmo = true;
    public float minPosX = -Mathf.Infinity;
    public float minPosZ = -Mathf.Infinity;
    public float maxPosX = Mathf.Infinity;
    public float maxPosZ = Mathf.Infinity;

    private Player data = new Player();


    public override void Awake()
    {
        base.Awake();

        

        isPlayer = true;

        SetDestroyCallback(this.PlayerDestroyCallback);

        if (weaponMountPoint == null) weaponMountPoint = thisT;

        //if(enableTurretRotate && turretObj==null) turretObj=thisT;
        //if(turretObj!=null) turretObjParent=turretObj.parent;

        if (turretObj == null)
        {
            enableTurretRotate = false;
            enableAutoAim = false;
        }
        else turretObjParent = turretObj.parent;


        hitPointBase = hitPointFull;
        energyBase = energyFull;

        InitSpawnImmunity();

        progress = thisObj.GetComponent<PlayerProgression>();
        if (progress != null) progress.SetPlayer(this);

        perk = thisObj.GetComponent<PlayerPerk>();
        if (perk != null) perk.SetPlayer(this);

    }

    public override void Start()
    {
        cam = Camera.main;
        camT = cam.transform;

        if (GameControl.GetInstance() && this != GameControl.GetPlayer())
            thisObj.layer = TDS.GetLayerOtherPlayer();
        else thisObj.layer = TDS.GetLayerPlayer();
        //camPivot=cam.transform.parent;

        // if (enableAbility && GameControl.EnableAbility())
        //     AbilityManager.SetupAbility(abilityIDList, enableAllAbilities);



        if (perk != null)
        {
            Ability ab = AbilityManager.GetAbility();
            ab.SetPlayerPerk(perk);
        }

        if (destroyedEffectObj != null)
        {
            if (!destroyedEffectObj.TryGetComponent<PooledObject>(out var pooledObject))
            {
                pooledObject = destroyedEffectObj.AddComponent<PooledObject>();
            }
        }

        if (GameControl.GetInstance().pvp)
        {
            data.username = PlayerPrefsManager.Username;
            data.roomId = PvP.GetRoom();
        }

        if (GameControl.GetPlayer() == this)
        {
            SetRendererColor(PlayerPrefsManager.mainColor, PlayerPrefsManager.subColor);
            Init(PlayerPrefsManager.weaponSelectID);
        }


    }

    public void SetRendererColor(Color main, Color sub)
    {
        var renderers = GetComponentsInChildren<Renderer>();

        foreach (var renderer1 in renderers)
        {
            renderer1.material.SetColor($"_Color1", main);
            renderer1.material.SetColor($"_Color2", sub);
        }
    }


    private bool init = false;
    public void Init(int weaponID)
    {
        if (init) return;
        init = true;

        //AbilityManager.SetupAbility(abilityList);

        if (!weaponInitiated)
        {
            weapon = Weapon_DB.GetPrefab(weaponID);
            weaponInitiated = true;


            GameObject obj = MountWeapon((Instantiate(weapon.gameObject)));
            weapon = obj.GetComponent<Weapon>();
            weapon.Reset();

            if (perk != null) weapon.SetPlayerPerk(perk);

            if (weapon == null)
            {
                obj = MountWeapon(null, "defaultObject");
                //weaponList.Add(obj.AddComponent<Weapon>());
                weapon = obj.AddComponent<Weapon>();
                //weaponList[0].aStats.damageMax = 2;
                weapon.aStats.damageMax = 2;
                //weaponList[0].Reset();
                weapon.Reset();

                GameObject soObj = (GameObject)Instantiate(Resources.Load("Prefab_TDSTK/DefaultShootObject", typeof(GameObject)));
                soObj.SetActive(false);

                weapon.shootObject = soObj;
                weapon.InitShootObject();
            }
        }
    }


    private GameObject MountWeapon(GameObject obj, string objName = "")
    {
        if (obj == null) obj = new GameObject();
        obj.transform.parent = weaponMountPoint;
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;
        if (objName != "") obj.name = objName;

        return obj;
    }

    
    public void AimTurretMouse(Vector3 mousePos)
    {
        if (destroyed || IsStunned()) return;

        if (!enableTurretRotate || turretObj == null) return;

        if (enableAutoAim)
        {
            Unit target = UnitTracker.GetNearestUnit(thisT.position);
            Vector3 p1 = turretObj.position; p1.y = 0;
            Vector3 p2 = target.thisT.position; p2.y = 0;

            Quaternion wantedRot = Quaternion.LookRotation(p2 - p1);
            if (!smoothTurretRotation) turretObj.rotation = wantedRot;
            else turretObj.rotation = Quaternion.Slerp(turretObj.rotation, wantedRot, Time.deltaTime * 15);

            return;
        }

        if (turretAimMode == _TurretAimMode.ScreenSpace)
        {
            Vector3 camV = Quaternion.Euler(0, camT.eulerAngles.y, 0) * Vector3.forward;
            Vector3 dir = (mousePos - Camera.main.WorldToScreenPoint(thisT.position)).normalized;
            dir = new Vector3(dir.x, 0, dir.y);

            float angleOffset = camT.eulerAngles.y; //get the camera y-axis angle 
            float sign = dir.x > 0 ? 1 : -1;                    //get the angle direction

            Vector3 dirM = Quaternion.Euler(0, angleOffset, 0) * dir;   //rotate the dir for the camera angle, dir has to be vector3 in order to work

            Quaternion wantedRot = Quaternion.Euler(0, sign * Vector3.Angle(camV, dirM) + angleOffset, 0);
            if (!smoothTurretRotation) turretObj.rotation = wantedRot;
            else turretObj.rotation = Quaternion.Slerp(turretObj.rotation, wantedRot, Time.deltaTime * 15);
        }
        // else if (turretAimMode == _TurretAimMode.Raycast)
        // {
        //     LayerMask mask = 1 << TDS.GetLayerTerrain();
        //     Ray ray = Camera.main.ScreenPointToRay(mousePos);
        //     RaycastHit hit;
        //     if (Physics.Raycast(ray, out hit, Mathf.Infinity, mask))
        //     {
        //         Vector3 point = new Vector3(hit.point.x, thisT.position.y, hit.point.z);

        //         Quaternion wantedRot = Quaternion.LookRotation(point - thisT.position);
        //         if (!smoothTurretRotation) turretObj.rotation = wantedRot;
        //         else turretObj.rotation = Quaternion.Slerp(turretObj.rotation, wantedRot, Time.deltaTime * 15);
        //     }
        // }
    }
    //for DPad (touch input)
    public void AimTurretDPad(Vector2 direction)
    {
        if (destroyed || IsStunned()) return;

        direction = direction.normalized * 100;

        Vector2 screenPos = Camera.main.WorldToScreenPoint(thisT.position);
        float x = screenPos.x + direction.x;
        float y = screenPos.y + direction.y;

        AimTurretMouse(new Vector2(x, y));
    }

    //fire main weapon
    public void FireWeapon()
    {
        if (destroyed || IsStunned()) return;
        if (!thisObj.activeInHierarchy) return;

        int fireState = CanFire();
        if (fireState == 0)
        {
            OnFireWeapon();

            // if (weapon.useEnergyAsAmmo)
            // {
            //     energy -= weapon.energyCost;
            // }
        }
        else
        {
            string text = "";
            if (fireState == 1) text = "attack disabled";
            if (fireState == 2) text = "weapon's on cooldown";
            if (fireState == 3) text = "weapon's out of ammo";
            if (fireState == 4) text = "weapon's reloading";
            if (fireState == 5) text = "out of energy";

            TDS.FireFail(text);

            if (GetCurrentClip() == 0 && GameControl.EnableAutoReload()) weapon.Reload();
        }
    }

    public void OnFireWeapon()
    {
        if (GameControl.GetInstance().pvp && GameControl.GetPlayer() == this)
        {
            NetworkManager.Instance.Manager.Socket
                .Emit("playerFire", new PlayerFire
                {
                    username = PlayerPrefsManager.Username,
                    roomId = PvP.GetRoom(),
                    turretRotation = MyExtension.ConvertToArrayFromQuaternion(turretObj.transform.rotation)
                });
        }

        // if (weapon.RequireAiming()) //nếu nhắm bắn
        // {
        //     Vector2 cursorPos = Input.mousePosition;

        //     if (weapon.RandCursorForRecoil())
        //     {
        //         float recoil = GameControl.GetPlayer().GetRecoil() * 4;
        //         cursorPos += new Vector2(Random.value - 0.5f, Random.value - 0.5f) * recoil;
        //     }

        //     Ray ray = Camera.main.ScreenPointToRay(cursorPos);
        //     RaycastHit hit;
        //     //LayerMask mask=1<<TDS.GetLayerTerrain();
        //     //Physics.Raycast(ray, out hit, Mathf.Infinity, mask);
        //     Physics.Raycast(ray, out hit, Mathf.Infinity);

        //     ShootObject.AimInfo aimInfo = new ShootObject.AimInfo(hit);

        //     StartCoroutine(ShootRoutine(aimInfo));
        // }
        // else 
        StartCoroutine(ShootRoutine());

        weapon.Fire();
    }
    //alt fire, could fire weapon alt-mode to launch selected ability
    public void FireAbility()
    {
        if (destroyed || IsStunned()) return;
        if (GameControl.EnableAltFire())
        {
            //weapon.FireAlt();
        }
        if (!GameControl.EnableAltFire() && GameControl.EnableAbility())
        {
            AbilityManager.LaunchAbility();
        }
    }
    //launch ability
    public void FireAbilityAlt()
    {
        if (destroyed || IsStunned()) return;
        if (GameControl.EnableAltFire() && GameControl.EnableAbility())
            AbilityManager.LaunchAbility();
    }

    public void Reload()
    {
        if (destroyed || IsStunned()) return;
        weapon.Reload();
    }

    //without this the player can clip into collider when running into a 90 degree corner
    public bool CheckObstacle(Vector3 dir)
    {
        CapsuleCollider collider = thisObj.GetComponent<CapsuleCollider>();

        Vector3 pos = thisT.position + new Vector3(0, 0.25f, 0);
        Vector3 dirSide = thisT.TransformDirection(new Vector3(1f, 0, 0)) * collider.radius * 0.5f;

        bool flag1 = Physics.Raycast(pos + dirSide, dir, collider.radius * 1.1f, castMask);
        bool flag2 = Physics.Raycast(pos - dirSide, dir, collider.radius * 1.1f, castMask);

        return flag1 || flag2;
    }


    /*
    public Vector3 CurbMovementToObjstacle(Vector3 dir){
        CapsuleCollider collider=thisObj.GetComponent<CapsuleCollider>();

        Vector3 pos=thisT.position+new Vector3(0, 0.25f, 0);
        Vector3 dirSide=thisT.TransformDirection(new Vector3(1f, 0, 0))*collider.radius*0.5f;

        float range=dir.magnitude;

        RaycastHit hit;
        LayerMask mask = 1 << 0;
        if(Physics.Raycast(pos+dirSide, dir, out hit, collider.radius+0.1f+range, mask)){
            float limit=(hit.point-thisT.position).magnitude - collider.radius+0.1f;
            dir*=limit/range;
        }

        return dir;
    }
    */

    //move
    public void Move(Vector2 direction)
    {
        if (destroyed || IsStunned()) return;

        direction = direction.normalized;

        Vector3 dirV = Quaternion.Euler(0, camT.eulerAngles.y, 0) * Vector3.forward; //hướng camera
        Vector3 dirH = Quaternion.Euler(0, 90, 0) * dirV;                                          
        dirV = dirV * direction.y * (enabledMovementZ ? 1 : 0);
        dirH = dirH * direction.x * (enabledMovementX ? 1 : 0);
        Vector3 dirHV = (dirH + dirV).normalized;


        if (movementMode == _MovementMode.FreeForm)
        {
            velocity += dirHV * 0.025f * (acceleration - velocity.magnitude);       //remove  '-velocity.magnitude' to retain momentum when changing direction
        } 
        else
        {
            float stopper = !CheckObstacle(dirHV) ? 1 : 0;

            //Vector3 moveMag=CurbMovementToObjstacle(dirHV * moveSpeed * Time.deltaTime * GetTotalSpeedMultiplier() * boost);

            //thisT.Translate(moveMag, Space.World);
            thisT.Translate(dirHV * moveSpeed * Time.deltaTime * GetTotalSpeedMultiplier() * stopper, Space.World);
            velocity = dirHV;
            moved = true;
        }

        //take the angle difference between the travel direction and +z direction to get the rotation
        if (faceTravelDirection && rotateSpeed > 0 && turretObj != thisT)
        {
            //Debug.Log((enableTurretRotate || (!enableTurretRotate && !aimAtTravelDirection))+"   "+turretObjParent);
            if ((enableTurretRotate || (!enableTurretRotate && !aimAtTravelDirection)) && turretObj != null && turretObjParent != null) turretObj.parent = null;

            //~ if(faceTravelDirection && rotateSpeed>0){
            //~ if(turretObj!=null && !aimAtTravelDirection) turretObj.parent=null;

            float sign = dirHV.x > 0 ? 1 : -1;
            Quaternion wantedRot = Quaternion.Euler(0, sign * Vector3.Angle(Vector3.forward, dirHV), 0);
            thisT.rotation = Quaternion.Slerp(thisT.rotation, wantedRot, Time.deltaTime * rotateSpeed);

            if ((enableTurretRotate || (!enableTurretRotate && !aimAtTravelDirection)) && turretObj != null && turretObjParent != null) turretObj.parent = turretObjParent;
        }
    }

    public void Brake()
    {
        if (movementMode != _MovementMode.FreeForm) return;
        velocity = Vector3.Lerp(velocity, Vector3.zero, Time.deltaTime * activeBrakingRate * 5);
    }

    //***********************************************************************


    public override void Update()
    {
        if (useLimit)
        {
            float x = Mathf.Clamp(thisT.position.x, minPosX, maxPosX);
            float z = Mathf.Clamp(thisT.position.z, minPosZ, maxPosZ);
            thisT.position = new Vector3(x, thisT.position.y, z);
        }

        if (!GameControl.IsGamePlaying() || this != GameControl.GetPlayer()) return;
        if (destroyed || IsStunned() || IsDashing()) return;

        base.Update();

        if (movementMode == _MovementMode.FreeForm)
        {
            float stopper = !CheckObstacle(velocity) ? 1 : 0;
            thisT.Translate(velocity * moveSpeed * Time.deltaTime * GetTotalSpeedMultiplier() * stopper, Space.World);

            velocity *= (1 - Time.deltaTime * decceleration * velocity.magnitude);
        }

        if (!enableTurretRotate && aimAtTravelDirection && turretObj != null) turretObj.rotation = thisT.rotation;

        if (GameControl.GetInstance().pvp)
        {
            Vector3 vecloc = GetComponent<Rigidbody>().velocity;
            data.SetPosition(thisT.position.x, thisT.position.y, thisT.position.z);
            data.SetRotaion(thisT.rotation.eulerAngles.x, thisT.rotation.eulerAngles.y, thisT.rotation.eulerAngles.z);
            data.SetTurretRotation(turretObj.rotation.eulerAngles.x, turretObj.rotation.eulerAngles.y, turretObj.rotation.eulerAngles.z);
            data.SetVelocity(vecloc.x, vecloc.y, vecloc.z);
            data.hp = hitPoint;
            data.hpfull = hitPointFull;
            NetworkManager.Instance.Manager.Socket
                .Emit("player move", data);
        }
    }


    //for animation
    void LateUpdate()
    {
        if (movementMode == _MovementMode.FreeForm) AnimationMove(velocity);
        else if (movementMode == _MovementMode.Rigid) AnimationMove(moved ? velocity : Vector3.zero);
        moved = false;
    }

    void AnimationMove(Vector3 dir)
    {
        if (uAnimation == null) return;

        //Debug.Log("AnimationMove  "+dir.magnitude+"    "+camT.eulerAngles.y+"    "+dir);

        if (faceTravelDirection)
        {
            if (dir.magnitude > 0.5f) uAnimation.Move(1, 0);
            else uAnimation.Move(0, 0);
            return;
        }

        Vector3 dir1 = Quaternion.Euler(0, -uAnimation.GetAnimatorT().rotation.eulerAngles.y, 0) * dir;
        uAnimation.Move(dir1.z, dir1.x);
    }



    //for camera dynamic zoom
    public float GetVelocity()
    {
        if (movementMode == _MovementMode.FreeForm) return velocity.magnitude * GetTotalSpeedMultiplier();
        return (Input.GetButton("Horizontal") || Input.GetButton("Vertical")) ? moveSpeed * GetTotalSpeedMultiplier() * .15f : 0;
    }



    //modify the attackStats to active effect
    public Effect levelModifier = new Effect();
    protected AttackStats ModifyAttackStatsToLevel(AttackStats aStats)
    {
        if (progress == null) return aStats;

        float dmgMul = GetDamageMultiplier();
        aStats.damageMin *= dmgMul;
        aStats.damageMax *= dmgMul;

        // aStats.critChance *= GetCritChanceMultiplier();
        // aStats.critMultiplier *= GetCritMulMultiplier();

        return aStats;
    }



    IEnumerator ShootRoutine(ShootObject.AimInfo aimInfo = null)
    {
        if (uAnimation != null) uAnimation.AttackMelee();

        AttackStats aStats = ModifyAttackStatsToLevel(weapon.GetRuntimeAttackStats());
        aStats = ModifyAttackStatsToExistingEffect(aStats);
        //aStats=ModifyAttackStatsToExistingEffect(weaponList[weaponID].GetRuntimeAttackStats());
        AttackInstance aInstance = new AttackInstance(this, aStats);

        //int weapID = weaponID;  //to prevent weapon switch and state change while delay and firing multiple so

        int spread = weapon.spread;
        if (spread > 1)
        {
            aInstance.aStats.damageMin /= spread;
            aInstance.aStats.damageMax /= spread;
        }

        float startAngle = spread > 1 ? -weapon.spreadAngle / 2f : 0;
        float angleDelta = spread > 1 ? weapon.spreadAngle / (spread - 1) : 0;

        List<Collider> soColliderList = new List<Collider>();   //colliders of all the so fired, used to tell each so to ignore each other

        for (int i = 0; i < weapon.shootPointList.Count; i++)
        {
            Transform shootPoint = weapon.shootPointList[i];

            float recoilSign = (Random.value < recoilSignTH ? -1 : 1);
            recoilSignTH = Mathf.Clamp(recoilSignTH + (recoilSign > 0 ? 0.25f : -0.25f), 0, 1);
            float recoilValue = recoilSign * Random.Range(0.1f, 1f) * GetRecoil();
            Quaternion baseShootRot = shootPoint.rotation * Quaternion.Euler(0, recoilValue, 0);

            for (int m = 0; m < Mathf.Max(1, spread); m++)
            {
                Vector3 shootPos = shootPoint.position;
                if (spread > 1) shootPos = shootPoint.TransformPoint(new Vector3(0, 0, Random.Range(-1.5f, 1.5f)));
                Quaternion shootRot = baseShootRot * Quaternion.Euler(0, startAngle + (m * angleDelta), 0);

                //GameObject soObj=(GameObject)Instantiate(weaponList[weapID].shootObject, shootPos, shootRot);
                //GameObject soObj=ObjectPoolManager.Spawn(weaponList[weapID].shootObject, shootPos, shootRot);
                ShootObject soInstance = weapon.shootObject.GetComponent<ShootObject>().GetPoolItem<ShootObject>(shootPos, shootRot);

                soInstance.IgnoreCollider(GetCollider());
                for (int n = 0; n < soColliderList.Count; n++) soInstance.IgnoreCollider(soColliderList[n]);
                if (soInstance.GetCollider() != null) soColliderList.Add(soInstance.GetCollider());

                soInstance.Shoot(thisObj.layer, GetRange(), shootPoint, aInstance.Clone(), aimInfo);
                //soInstance.Shoot(thisObj.layer, GetRange(), shootPoint, aInstance.Clone(), hit);
            }

            TDS.CameraShake(weapon.recoilCamShake);

            if (weapon.shootPointDelay > 0) yield return new WaitForSeconds(weapon.shootPointDelay);

            //if (weapID >= weaponList.Count) break;
        }

    }





    private bool disableFire = false;   //for whatever reason some external component need to stop player from firing
    public void DisableFire() { disableFire = true; }
    public void EnableFire() { disableFire = false; }

    public int CanFire()
    {
        if (disableFire) return 1;
        if (weapon.OnCoolDown()) return 2;
        if (weapon.OutOfAmmo()) return 3;
        if (weapon.Reloading()) return 4;
        //if (weapon.useEnergyAsAmmo && energy < weapon.energyCost) return 5;
        return 0;
    }

    //public bool ContinousFire() { return GameControl.EnableContinousFire() & weapon.continousFire; }

    public override float GetRange() { return weapon.GetRange(); }

    private float recoilSignTH = 0.5f;
    public float GetRecoil() { return weapon.GetRecoilMagnitude(); }

    //public bool UseEnergyAsAmmo() { return weapon.useEnergyAsAmmo; }

    public bool Reloading() { return weapon.Reloading(); }
    public float GetReloadDuration() { return weapon.GetReloadDuration(); }
    public float GetCurrentReload() { return weapon.currentReload; }
    public int GetCurrentClip() { return weapon.currentClip; }
    public int GetAmmo() { return weapon.ammo; }




    public void GainAmmo(int value)
    {
        if (value == -1)
        {
            weapon.FullAmmo();
        }
        else
        {
            weapon.GainAmmo(value);
        }

        return;

    }

    public override bool ApplyEffect(Effect effect)
    {
        if (!base.ApplyEffect(effect)) return false;
        TDS.GainEffect(effect); //for UIBuffIcons
        return true;
    }


    public void PlayerDestroyCallback()
    {
        //if (weapon.temporary) RemoveWeapon();
        if (GameControl.GetPlayer() == this)
            GameControl.PlayerDestroyed();
    }





    [HideInInspector] public int playerID = 0;  //for saving
    public bool loadProgress = false;
    public bool saveProgress = false;

    public bool saveUponChange = false;
    public bool SaveUponChange() { return saveProgress & saveUponChange; }



    [HideInInspector] public PlayerProgression progress;
    public PlayerProgression GetPlayerProgression() { return progress; }
    [HideInInspector] public PlayerPerk perk;
    public PlayerPerk GetPlayerPerk() { return perk; }

    public int GetLevel() { return progress != null ? progress.GetLevel() : level; }
    public int GetPerkCurrency() { return perk != null ? perk.GetPerkCurrency() : 0; }
    //public int GetPerkPoint() { return perk != null ? perk.GetPerkPoint() : 0; }
    public List<Perk> GetPerkList() { return perk != null ? perk.GetPerkList() : new List<Perk>(); }

    private float hitPointBase = 0;
    private float energyBase = 0;
    public float GetBaseHitPoint() { return hitPointBase; }
    public float GetBaseEnergy() { return energyBase; }

    //void OnGUI(){
    //	GUI.Label(new Rect(50, 300, 200, 30), ""+GetFullHitPoint()+"    "+GetPerkHitPoint());
    //	GUI.Label(new Rect(50, 320, 200, 30), ""+(1+GetLevelDamageMul()+GetPerkDamageMul()));
    //}

    public override float GetFullHitPoint() { return hitPointBase + GetLevelHitPoint() + GetPerkHitPoint(); }
    public override float GetFullEnergy() { return energyBase + GetLevelEnergy() + GetPerkEnergy(); }
    public override float GetHitPointRegen() { return hpRegenRate + GetLevelHitPointRegen() + GetPerkHitPointRegen(); }
    public override float GetEnergyRegen() { return energyRate + GetLevelEnergyRegen() + GetPerkEnergyRegen(); }

    public float GetDamageMultiplier() { return 1 + GetLevelDamageMul() + GetPerkDamageMul(); }
    // public float GetCritChanceMultiplier() { return 1 + GetLevelCritMul() + GetPerkCritMul(); }
    // public float GetCritMulMultiplier() { return 1 + GetLevelCritMulMul() + GetPerkCritMulMul(); }
    public override float GetSpeedMultiplier() { return 1 + GetLevelSpeedMul() + GetPerkSpeedMul(); }


    public float GetEffSpeedMultiplier() { return activeEffect.speedMul; }
    public float GetTotalSpeedMultiplier() { return GetEffSpeedMultiplier() * GetSpeedMultiplier(); }



    public float GetLevelHitPoint() { return progress != null ? progress.GetHitPointGain() : 0; }
    public float GetLevelEnergy() { return progress != null ? progress.GetEnergyGain() : 0; }
    public float GetLevelHitPointRegen() { return progress != null ? progress.GetHitPointRegenGain() : 0; }
    public float GetLevelEnergyRegen() { return progress != null ? progress.GetEnergyRegenGain() : 0; }

    public float GetLevelDamageMul() { return progress != null ? progress.GetDamageMulGain() : 0; }
    // public float GetLevelCritMul() { return progress != null ? progress.GetCritChanceMulGain() : 0; }
    // public float GetLevelCritMulMul() { return progress != null ? progress.GetCritMultiplierMulGain() : 0; }
    public float GetLevelSpeedMul() { return progress != null ? progress.GetSpeedMulGain() : 0; }



    public float GetPerkHitPoint() { return perk != null ? perk.GetBonusHitPoint() : 0; }
    public float GetPerkEnergy() { return perk != null ? perk.GetBonusEnergy() : 0; }
    public float GetPerkHitPointRegen() { return perk != null ? perk.GetBonusHitPointRegen() : 0; }
    public float GetPerkEnergyRegen() { return perk != null ? perk.GetBonusEnergyRegen() : 0; }

    public float GetPerkSpeedMul() { return perk != null ? perk.GetMoveSpeedMul() : 0; }

    public float GetPerkDamageMul() { return perk != null ? perk.GetDamageMul() : 0; }
    // public float GetPerkCritMul() { return perk != null ? perk.GetCritMul() : 0; }
    // public float GetPerkCritMulMul() { return perk != null ? perk.GetCirtMulMul() : 0; }


    //for perk that modify the weapon attack effect
    // public void ChangeAllWeaponEffect(int effectID)
    // {
    //     int effectIndex = Effect_DB.GetEffectIndex(effectID);
    //     weapon.ChangeEffect(effectID, effectIndex);
    // }

    // //for perk that modify the weapon ability
    // public void ChangeAllWeaponAbility(int abilityID)
    // {
    //     for (int i = 0; i < weaponList.Count; i++) weaponList[i].ChangeAbility(abilityID);
    // }
    // public void ChangeWeaponAbility(int weaponID, int abilityID)
    // {
    //     for (int i = 0; i < weaponList.Count; i++)
    //     {
    //         if (weaponList[i].ID == weaponID)
    //         {
    //             weaponList[i].ChangeAbility(abilityID);
    //             break;
    //         }
    //     }
    // }

    //for perk that modify the ability attack effect
    // public void ChangeAllAbilityEffect(int effectID)
    // {
    //     int effectIndex = Effect_DB.GetEffectIndex(effectID);
    //     Ability ab = AbilityManager.GetAbility();
    //     ab.ChangeEffect(effectID, effectIndex);
    // }
    // public void ChangeAbilityEffect(int abilityID, int effectID)
    // {
    //     int effectIndex = Effect_DB.GetEffectIndex(effectID);
    //     Ability ab = AbilityManager.GetAbility();
    //     if (ab.ID == abilityID) ab.ChangeEffect(effectID, effectIndex);

    // }



    private bool dashing = false;
    public bool IsDashing() { return dashing; }
    public void Dash(float range, float dur) { StartCoroutine(_Dash(range, dur)); }
    public IEnumerator _Dash(float range, float dur)
    {
        dashing = true;
        Vector3 startPos = thisT.position;
        Vector3 tgtPos = thisT.TransformPoint(Vector3.forward * range);
        float step = 1f / dur;
        float duration = 0;
        while (duration < 1)
        {
            thisT.position = Vector3.Lerp(startPos, tgtPos, duration);
            duration += Time.deltaTime * step;
            yield return null;
        }
        thisT.position = tgtPos;
        dashing = false;
    }



    public float GetScoreMultiplier()
    {
        return perk != null ? 1 + perk.GetScoreGainMul() : 1;
    }

    public void GainPerkCurrency(int value)
    {
        if (perk != null && value > 0) perk.GainCurrency(value);
    }

    public void GainExp(int val)
    {
        if (progress == null) return;
        float multiplier = 1 + (perk != null ? perk.GetExpGainMul() : 0);
        progress.GainExp((int)(val * multiplier));
    }

    public override float GainHitPoint(float value)
    {
        // float multiplier = 1 + (perk != null ? perk.GetHitPointGainMul() : 0);
        return base.GainHitPoint(value);
    }
    public override float GainEnergy(float value)
    {
        //float multiplier = 1 + (perk != null ? perk.GetEnergyGainMul() : 0);
        return base.GainEnergy(value);
    }




    void OnDrawGizmos()
    {
        if (useLimit && showGizmo)
        {
            Vector3 p1 = new Vector3(minPosX, transform.position.y, maxPosZ);
            Vector3 p2 = new Vector3(maxPosX, transform.position.y, maxPosZ);
            Vector3 p3 = new Vector3(maxPosX, transform.position.y, minPosZ);
            Vector3 p4 = new Vector3(minPosX, transform.position.y, minPosZ);

            Gizmos.color = Color.blue;
            Gizmos.DrawLine(p1, p2);
            Gizmos.DrawLine(p2, p3);
            Gizmos.DrawLine(p3, p4);
            Gizmos.DrawLine(p4, p1);
        }
    }

}

