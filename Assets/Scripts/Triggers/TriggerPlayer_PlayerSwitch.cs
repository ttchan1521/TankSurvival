using UnityEngine;
using System.Collections;

public class TriggerPlayer_PlayerSwitch : Trigger
{

    public UnitPlayer targetPrefab; //prefab replace player unit 

    public bool differentPrefabOnly = false;

    public Transform targetTransform; //vị trí mới


    public override bool UseAltTriggerEffectObj() { return true; }


    public override void OnTriggerEnter(Collider collider)
    {
        UnitPlayer player = collider.gameObject.GetComponent<UnitPlayer>();

        if (player != null)
        {
            if (differentPrefabOnly && targetPrefab.prefabID == player.prefabID) return;

            if (targetTransform == null) targetTransform = transform;

            GameObject newplayerObj = (GameObject)Instantiate(targetPrefab.gameObject, targetTransform.position, targetTransform.rotation);
            GameControl.SetPlayer(newplayerObj.GetComponent<UnitPlayer>());

            //for effect, check parent class
            if (!spawnEffectAtOrigin) effPos = player.transform.position;
            targetEffPos = targetTransform.position;

            Destroy(collider.gameObject);

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
