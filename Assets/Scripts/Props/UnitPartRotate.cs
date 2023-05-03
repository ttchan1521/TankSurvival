//constantly rotate a transform
//used to rotate a parts of a unit


using UnityEngine;
using System.Collections;

//rotate unit
public class UnitPartRotate : MonoBehaviour
{

    private Transform thisT;

    public float rotateRate = 90;

    public Unit unit;

    void Awake()
    {
        thisT = transform;

        unit = TDSUtility.GetUnitOfParentTransform(thisT);
    }


    void Update()
    {
        if (unit != null && unit.IsStunned()) return;
        thisT.Rotate(Vector3.up * Time.deltaTime * rotateRate, Space.World);
    }

}
