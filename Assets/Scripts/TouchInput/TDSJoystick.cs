﻿using UnityEngine;
using UnityEngine.EventSystems;

using System.Collections;


public class TDSJoystick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{

    public int limit = 50;

    public Camera canvasCamera;
    private Vector3 screenPos;

    private Vector2 value;

    private RectTransform rectT;

    void Awake()
    {
        rectT = gameObject.GetComponent<RectTransform>();
    }


    void Start()
    {
        screenPos = canvasCamera.WorldToScreenPoint(rectT.position);
    }


    public void OnDrag(PointerEventData data)
    {
        Vector2 delta = data.position - new Vector2(screenPos.x, screenPos.y);
        if (delta.magnitude > limit) delta = delta.normalized * limit;

        value = delta;

        rectT.localPosition = new Vector3(delta.x, delta.y, 0);
    }


    public void OnPointerUp(PointerEventData data)
    {
        rectT.localPosition = Vector3.zero;
        value = Vector3.zero;
    }


    public void OnPointerDown(PointerEventData data)
    {

    }


    public Vector2 GetValue()
    {
        return value;
    }
    public float GetMagnitude()
    {
        return value.magnitude;
    }
}
