using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyExtension
{
    public static Vector3 ConvertToVector3(float[] coordinate)
    {
        return new Vector3(coordinate[0], coordinate[1], coordinate[2]);
    }

    public static float[] ConvertToArrayFromVector3(Vector3 vector3)
    {
        return new float[] {vector3.x, vector3.y, vector3.z};
    }

    public static Quaternion ConvertToQuaternion(float[] coordinate)
    {
        return Quaternion.Euler(coordinate[0], coordinate[1], coordinate[2]);
    }

    public static float[] ConvertToArrayFromQuaternion(Quaternion quaternion)
    {
        return new float[] 
        {
            quaternion.eulerAngles.x,
            quaternion.eulerAngles.y,
            quaternion.eulerAngles.z
        };
    }
}
