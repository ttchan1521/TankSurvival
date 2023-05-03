using UnityEngine;
using System.Collections;

public class TriggerPlayer_Teleport : Trigger
{
    public Transform targetTransform;


    public override bool UseAltTriggerEffectObj() { return true; }


    public override void OnTriggerEnter(Collider collider)
    {
        if (targetTransform == null) return;

        UnitPlayer player = collider.gameObject.GetComponent<UnitPlayer>();

        if (player != null)
        {
    
            if (!spawnEffectAtOrigin) effPos = player.transform.position;
            targetEffPos = targetTransform.position;

            player.thisT.position = targetTransform.position;

            Triggered();
        }
    }


    protected override void OnDrawGizmos()
    {
        if (targetTransform != null)
        {
            Gizmos.color = new Color(0.25f, 1f, 0.25f, 1f);
            Gizmos.DrawLine(transform.position, targetTransform.position);
        }

        Gizmos.color = new Color(0f, 1f, 0.5f, 1f);
        base.OnDrawGizmos();
    }

}
