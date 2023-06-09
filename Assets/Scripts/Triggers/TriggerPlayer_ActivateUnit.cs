﻿using UnityEngine;

using System.Collections;
using System.Collections.Generic;


public class TriggerPlayer_ActivateUnit : Trigger
{

    public List<UnitAI> unitList = new List<UnitAI>();

    public override void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.GetComponent<UnitPlayer>() != null)
        {
            for (int i = 0; i < unitList.Count; i++)
            {
                if (unitList[i] == null) continue;
                unitList[i].enabled = true;
            }

            Triggered();
        }
    }


    protected override void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0.5f, 0.0f, 1f);
        base.OnDrawGizmos();
    }

}
