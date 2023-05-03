using UnityEngine;
using System.Collections;

public class TriggerPlayer_Objective : Trigger
{


    public override void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.GetComponent<UnitPlayer>() != null)
        {
            for (int i = 0; i < triggerCallbackList.Count; i++)
            {
                if (triggerCallbackList[i] != null) triggerCallbackList[i](this);
            }

            Triggered();
        }
    }


    protected override void OnDrawGizmos()
    {
        Gizmos.color = new Color(0f, 1f, 0.5f, 1f);
        base.OnDrawGizmos();
    }

}
