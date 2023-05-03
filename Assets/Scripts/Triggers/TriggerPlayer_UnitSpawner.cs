using UnityEngine;

using System.Collections;
using System.Collections.Generic;


public class TriggerPlayer_UnitSpawner : Trigger
{

    public List<UnitSpawner> spawnerList = new List<UnitSpawner>();

    public override void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.GetComponent<UnitPlayer>() != null)
        {
            for (int i = 0; i < spawnerList.Count; i++)
            {
                spawnerList[i].StartSpawn();
            }

            Triggered();
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 1f, 1f);
        for (int i = 0; i < spawnerList.Count; i++)
        {
            if (spawnerList[i] == null) continue;
            Gizmos.DrawLine(spawnerList[i].transform.position, transform.position);
        }
    }

}
