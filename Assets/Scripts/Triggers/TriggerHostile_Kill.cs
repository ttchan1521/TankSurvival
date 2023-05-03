using UnityEngine;
using System.Collections;

public class TriggerHostile_Kill : Trigger
{
    public bool showDestroyEffect = true;

    public int scoreGain = 1;



    public override void OnTriggerEnter(Collider collider)
    {
        UnitAI unit = collider.gameObject.GetComponent<UnitAI>();
        if (unit == null)
        {
            Debug.Log("no unit, return");
            return;
        }

        unit.ClearUnit(showDestroyEffect);
        GameControl.GainScore(scoreGain);

        Triggered();
    }


    protected override void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0.5f, 0.5f, 1f);
        base.OnDrawGizmos();
    }

}