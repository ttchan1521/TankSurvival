using UnityEngine;
using System.Collections;


public class TriggerPlayer_RespawnPoint : Trigger
{
    public bool triggerSave = false;

    public override void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.GetComponent<UnitPlayer>() != null)
        {
            GameControl.SetRespawnPoint(transform.position);
            Triggered();
        }
    }


    protected override void OnDrawGizmos()
    {
        Gizmos.color = new Color(0f, 1f, 1f, 1f);
        base.OnDrawGizmos();
    }

}
