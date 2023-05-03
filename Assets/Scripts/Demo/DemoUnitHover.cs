using UnityEngine;
using System.Collections;

public class DemoUnitHover : MonoBehaviour
{

    private Transform thisT;
    private Vector3 defaultPos;

    public float magnitude = 1;
    public float frequency = 6.5f;
    private float offset1;

    void Start()
    {
        thisT = transform;
        defaultPos = thisT.localPosition;
        offset1 = Random.Range(-frequency, frequency);
    }

    void Update()
    {
        float posY = magnitude * Mathf.Sin(frequency * Time.time + offset1);
        thisT.localPosition = defaultPos + new Vector3(0, posY, 0);
    }

}
