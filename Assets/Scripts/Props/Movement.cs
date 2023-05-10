using UnityEngine;
using System.Collections;

public class Movement : MonoBehaviour
{

    public enum _MovType
    {
        PingPoing,
        OneShot,
        Loop
    }

    public _MovType type;

    public enum _Axis { x_axis, y_axis, z_axis, }

    public _Axis moveAxis;
    private Vector3 travelV;

    public Space space;

    private Transform thisT;
    public Vector3 startPos;

    private float dir = 1;

    public float limit = 15;
    public float speed = 5;

    public bool randomizeSpeed = false;
    public float speedMin = 3;
    public float speedMax = 6;


    private UnitAI unit;


    void Awake()
    {
        thisT = transform;

        unit = GetComponent<UnitAI>();
    }


    void OnEnable()
    {
        startPos = thisT.position;

        if (randomizeSpeed) speed = Random.Range(speedMin, speedMax);
    }


    void Update()
    {
        if (unit != null && unit.IsStunned()) return;

        if (moveAxis == _Axis.x_axis) travelV = Vector3.right;
        if (moveAxis == _Axis.y_axis) travelV = Vector3.up;
        if (moveAxis == _Axis.z_axis) travelV = Vector3.forward;

        thisT.Translate(dir * travelV * speed * Time.deltaTime, space);

        if (Vector3.Distance(thisT.position, startPos) >= limit)
        {
            if (type == _MovType.PingPoing)
            {
                dir *= -1;
                thisT.Translate(dir * travelV * speed * Time.deltaTime, space);
            }
            else if (type == _MovType.OneShot)
            {
                if (unit != null) unit.ClearUnit();
                else ObjectPoolManager.Unspawn(gameObject); //Destroy(gameObject);
            }
            else if (type == _MovType.Loop)
            {
                thisT.position = startPos;
            }
        }
    }

}

