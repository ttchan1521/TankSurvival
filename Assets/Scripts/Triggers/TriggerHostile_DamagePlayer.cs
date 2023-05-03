using UnityEngine;
using System.Collections;


public class TriggerHostile_DamagePlayer : Trigger
{

    public int hitPointLost = 1; //số máu bị trừ

    public int scoreLost = 1; //điểm bị mất

    public bool destroyUnit = true;  //destroy unit khi va chạm


    public override void OnTriggerEnter(Collider collider)
    {
        UnitAI unit = collider.gameObject.GetComponent<UnitAI>();
        if (unit == null)
        {
            return;
        }

        if (destroyUnit) unit.ClearUnit();

        GameControl.GetPlayer().GainHitPoint(-hitPointLost);
        GameControl.GainScore(-scoreLost);

        Triggered();
    }


    protected override void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0.5f, 0.5f, 1f);
        base.OnDrawGizmos();
    }

}
