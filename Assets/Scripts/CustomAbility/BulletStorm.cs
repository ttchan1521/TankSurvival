﻿using UnityEngine;
using System.Collections;

public class BulletStorm : MonoBehaviour
{

    public ShootObject shootObject;
    public Transform spinObject;

    public float damage = 5;
    public float range = 30;

    public float spinSpeed = 5;

    public int rounds = 5;
    public float shotDelay = .5f;


    void Start()
    {
        StartCoroutine(ShootRoutine());
    }

    void Update()
    {
        spinObject.Rotate(Vector3.up * spinSpeed * Time.deltaTime);
    }

    IEnumerator ShootRoutine()
    {
        AttackStats aStats = new AttackStats();
        aStats.damageMin = damage;
        aStats.damageMax = damage;

        for (int i = 0; i < rounds; i++)
        {
            yield return new WaitForSeconds(shotDelay);

            for (int n = 0; n < spinObject.childCount; n++)
            {
                Transform sp = spinObject.GetChild(n);

                UnitPlayer player = GameControl.GetPlayer();
                AttackInstance attInstance = new AttackInstance(player, aStats);

                ShootObject shootObj = shootObject.GetPoolItem<ShootObject>(sp.position, sp.rotation);
                shootObj.Shoot(player.thisObj.layer, range, sp, attInstance);
            }
        }

        yield return null;

        Destroy(gameObject);
    }

}
