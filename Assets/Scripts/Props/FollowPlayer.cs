using UnityEngine;
using System.Collections;

//random xung quanh player
public class FollowPlayer : MonoBehaviour
{

    private Transform thisT;

    void Awake()
    {
        thisT = transform;
    }

    void Update()
    {
        UnitPlayer player = GameControl.GetPlayer();
        if (player != null) thisT.position = player.thisT.position;
    }

}
