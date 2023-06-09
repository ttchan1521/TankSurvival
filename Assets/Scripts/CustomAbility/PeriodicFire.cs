﻿using UnityEngine;
using System.Collections;

public class PeriodicFire : MonoBehaviour
{

    public ShootObject shootObject;

    public float damage = 5;
    public float range = 30;

    public bool randomCooldown = true;
    public float cooldown = 2f;

    private Transform thisT;

    void Start()
    {
        thisT = transform;
        StartCoroutine(ShootRoutine());
    }


    IEnumerator ShootRoutine()
    {
        AttackStats aStats = new AttackStats();
        aStats.damageMin = damage;
        aStats.damageMax = damage;

        Vector3 offsetPos = new Vector3(0, 0.75f, 0);

        while (true)
        {
            yield return new WaitForSeconds(Random.Range(0.5f * cooldown, 2f * cooldown));

            for (int n = 0; n < 12; n++)
            {
                UnitPlayer player = GameControl.GetPlayer();
                AttackInstance attInstance = new AttackInstance(player, aStats);

                ShootObject shootObj = shootObject.GetPoolItem<ShootObject>(thisT.position + offsetPos, Quaternion.Euler(0, n * 30, 0));
                shootObj.Shoot(player.thisObj.layer, range, thisT, attInstance);
            }
        }
    }
}
