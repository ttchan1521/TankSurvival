﻿using UnityEngine;
using System.Collections;

//xoay collectible hướng về camera
public class CollectibleProps : MonoBehaviour
{

    public bool faceCamera = true;
    public bool oscillate = true;

    private Transform thisT;
    private Vector3 defaultScale;
    void Awake()
    {
        thisT = transform;

        defaultScale = thisT.localScale;
    }

    void Update()
    {
        if (faceCamera) thisT.rotation = Camera.main.transform.rotation;

        if (oscillate)
        {
            float scale = 1 + 0.1f * Mathf.Abs(Mathf.Sin(Time.time * 3));
            thisT.localScale = defaultScale * scale;
        }
    }

}
