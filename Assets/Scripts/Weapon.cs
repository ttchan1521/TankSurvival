using UnityEngine;

using System.Collections;
using System.Collections.Generic;


public class Weapon : MonoBehaviour
{

    public delegate void FireCallback();
    public List<FireCallback> fireCallbackList = new List<FireCallback>();
    public void SetFireCallback(FireCallback callback) { fireCallbackList.Add(callback); }  //not in used atm

    [HideInInspector] public int ID = 0;
    public Sprite icon;
    public string weaponName = "Weapon";
    public string desp = "";

    public GameObject shootObject;
    public List<Transform> shootPointList = new List<Transform>();
    public float shootPointDelay = 0.05f; // delay giữa các điểm bắn

    public float range = 20; //khoảng cách đạn có thể bay
    public float cooldown = 0.15f; //thời gian giữa 2 lần bắn đạn
    [HideInInspector] public float currentCD = 0.25f;


    public int clipSize = 30;
    public int currentClip = 30;

    public int ammoCap = 300; //tổng số viên đạn
    public int ammo = 300; //lượng đạn còn lại

    public float reloadDuration = 2;
    [HideInInspector] public float currentReload = 0;

    public float recoilMagnitude = .2f; //độ chính xác
    [HideInInspector] public float recoil = 1;

    public float recoilCamShake = 0; //độ cam shake

    public int spread = 0; //số viên đạn bắn ra trong một lần
    public float spreadAngle = 15;

    public AttackStats aStats = new AttackStats();
    public AttackStats CloneAttackStats() { return aStats.Clone(); }
    public AttackStats GetRuntimeAttackStats() { return ModifyAttackStatsToPerk(aStats.Clone()); }

    [Header("Audio")]
    public AudioClip shootSFX;
    public AudioClip reloadSFX;



    void Awake()
    {
        for (int i = 0; i < shootPointList.Count; i++)
        {
            if (shootPointList[i] == null)
            {
                shootPointList.RemoveAt(i); i -= 1;
            }
        }

        if (shootPointList.Count == 0) shootPointList.Add(transform);

        if (shootObject != null) InitShootObject();

        aStats.Init();

        Reset();
    }

    public void InitShootObject()
    {
        ShootObject so = shootObject.GetComponent<ShootObject>();

        requireAiming = so.type == _SOType.Homing; //ngắm bắn

    }

    void OnDisable() { reloading = false; }

    public void Reset()
    {
        currentClip = GetClipSize();
        ammo = GetAmmoCap();
        currentCD = 0;
    }

    public void Fire()
    {
        for (int i = 0; i < fireCallbackList.Count; i++) fireCallbackList[i]();

        currentCD = GetCoolDown();
        recoil += GetRecoilMagnitude() * 2;

        AudioManager.PlaySound(shootSFX);

        if (currentClip > 0)
        {
            currentClip -= 1;
            if (currentClip <= 0)
            {

                if (GameControl.EnableAutoReload()) Reload();
            }
        }
    }


    private bool reloading = false;

    public bool Reload()
    {
        if (ammo == 0) return false;                    //out of ammo
        if (reloading) return false;                        //reloading
        if (currentClip == GetClipSize()) return false; //đầy đạn

        StartCoroutine(ReloadRoutine());
        TDS.Reloading();

        AudioManager.PlaySound(reloadSFX);

        return true;
    }
    IEnumerator ReloadRoutine()
    {
        reloading = true;
        currentReload = 0;

        while (currentReload < GetReloadDuration())
        {
            currentReload += Time.deltaTime;
            yield return null;
        }

        currentClip = ammo == -1 ? GetClipSize() : Mathf.Min(GetClipSize(), ammo);
        if (ammo > 0) ammo = Mathf.Max(ammo - GetClipSize(), 0);

        reloading = false;
    }

    public bool Reloading() { return reloading; }
    public bool OnCoolDown() { return currentCD > 0 ? true : false; }
    public bool OutOfAmmo() { return currentClip == 0 ? true : false; }



    public void FullAmmo()
    {
        ammo = GetAmmoCap();
    }

    public int GainAmmo(int value)
    {
        int limit = GetAmmoCap() - ammo;
        ammo += (int)Mathf.Min(value, limit);
        return limit;
    }


    void Update()
    {
        currentCD -= Time.deltaTime;
        recoil = recoil * (1 - Time.deltaTime * 3);

    }



    private bool requireAiming = false;
    public bool RequireAiming() { return requireAiming; }







    private PlayerPerk perk;
    public void SetPlayerPerk(PlayerPerk pPerk) { perk = pPerk; }

    public float GetRange() { return range * (1 + (perk != null ? perk.GetWeaponRangeMul() : 0)); }
    public float GetCoolDown() { return cooldown * (1 + (perk != null ? perk.GetWeaponCDMul() : 0)); }
    public int GetClipSize() { return (int)(clipSize * (1 + (perk != null ? perk.GetWeaponClipSizeMul() : 0))); }
    public int GetAmmoCap() { return (int)(ammoCap * (1 + (perk != null ? perk.GetWeaponAmmoCapMul() : 0))); }
    public float GetReloadDuration() { return reloadDuration * (1 + (perk != null ? perk.GetWeaponReloadDurMul() : 0)); }
    public float GetRecoilMagnitude() { return recoilMagnitude * (1 + (perk != null ? perk.GetWeaponRecoilMagMul() : 0)); }

    public AttackStats ModifyAttackStatsToPerk(AttackStats aStats)
    {   
        if (perk == null) return aStats;

        aStats.damageMin *= (1 + perk.GetWeaponDamageMul());
        aStats.damageMax *= (1 + perk.GetWeaponDamageMul());

        aStats.aoeRadius *= (1 + perk.GetWeaponAOEMul());

        return aStats;
    }

}
