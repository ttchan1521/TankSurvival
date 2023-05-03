using UnityEngine;
using System.Collections;

public class TriggerPlayer_Win : Trigger
{
    public bool triggerSave = false;


    public override void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.GetComponent<UnitPlayer>() != null)
        {
            GameControl.GameOver(true);
            Triggered();
        }
    }


    protected override void OnDrawGizmos()
    {
        Gizmos.color = new Color(0f, 1f, 0.5f, 1f);
        base.OnDrawGizmos();
    }

}
