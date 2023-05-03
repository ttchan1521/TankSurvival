using UnityEngine;

using System.Collections;
using System.Collections.Generic;

using UnityStandardAssets.ImageEffects;


public class CameraControl : MonoBehaviour
{
    private static CameraControl instance;

    [SerializeField] private Camera cam;
    public static Camera GetMainCamera() { return instance.cam; }

    [HideInInspector] public BlurOptimized blurEffect;

    public float trackSpeed;

    public Vector3 posOffset;

    public bool enableLimit = false;
    public float minPosX = -30;
    public float minPosZ = -30;
    public float maxPosX = 30;
    public float maxPosZ = 30;

    public bool enableDynamicZoom = true;
    public float zoomNormalizeFactor = 3;
    public float zoomSpeed = 2;
    public float defaultZoom;
    public float defaultZoomOrtho;

     //camera shake
    public float shakeMultiplier = 0.5f;
    private float camShakeMagnitude = 0;

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        if (cam == null)
            cam = GetComponentInChildren<Camera>();

        defaultZoom = cam.transform.localPosition.z;
        defaultZoomOrtho = cam.orthographicSize;

        blurEffect = gameObject.GetComponentInChildren<BlurOptimized>();
        if (blurEffect != null) blurEffect.enabled = false;
    }

    void Start()
    {
        if (trackSpeed > 0 && GameControl.GetPlayer() != null)
            transform.position = GameControl.GetPlayer().thisT.position + posOffset;
    }

    void OnEnable()
    {
        TDS.onCameraShakeE += CameraShake;
    }
    void OnDisable()
    {
        TDS.onCameraShakeE -= CameraShake;
    }

    void Update()
    {
        Shake();

        float wantedZoom = defaultZoom;
        float wantedZoomOrtho = cam.orthographicSize;

        UnitPlayer player = GameControl.GetPlayer();
        if (player != null)
        {
            //Quaternion wantedRot=Quaternion.Euler(thisT.rotation.eulerAngles.x, player.thisT.eulerAngles.y, 0);
            //thisT.rotation=Quaternion.Lerp(thisT.rotation, wantedRot, Time.deltaTime);

            Vector3 targetPos = player.thisT.position + posOffset;

            //nếu player gần biên, không di chuyển camera
            if (enableLimit)
            {
                targetPos.x = Mathf.Clamp(targetPos.x, minPosX, maxPosX);
                targetPos.z = Mathf.Clamp(targetPos.z, minPosZ, maxPosZ);
            }

            //di chuyển theo player
            transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * trackSpeed);

            //điều chỉnh zoom
            wantedZoom = defaultZoom * (1 + (player.GetVelocity() / zoomNormalizeFactor));
            wantedZoomOrtho = defaultZoomOrtho * (1 + (player.GetVelocity() / zoomNormalizeFactor));
        }

        //điều chỉnh zoom camera theo tham số trên
        if (enableDynamicZoom)
        {
            cam.transform.localPosition = new Vector3(cam.transform.localPosition.x, cam.transform.localPosition.y, Mathf.Lerp(cam.transform.localPosition.z, wantedZoom, Time.deltaTime * zoomSpeed));
            cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, wantedZoomOrtho, Time.deltaTime * zoomSpeed);
        }
    }

    //thay đổi shakeMagnitude
    public void CameraShake(float magnitude = 1)
    {
        if (magnitude == 0) return;
        instance.camShakeMagnitude = magnitude * 0.5f;
    }

    //call in update, giảm dần magnitude
    public void Shake()
    {
        if (Time.timeScale == 0 || camShakeMagnitude <= 0) return;    //paused game
        
        float x = 2 * (Random.value - 0.5f) * camShakeMagnitude * shakeMultiplier;
        float y = 2 * (Random.value - 0.5f) * camShakeMagnitude * shakeMultiplier;
        cam.transform.localPosition = new Vector3(x, y, cam.transform.localPosition.z);

        camShakeMagnitude *= (1 - Time.deltaTime * 5);
    }


    //paused game or game over
    public static void TurnBlurOn()
    {
        if (instance == null || instance.blurEffect == null) return;
        instance.StartCoroutine(instance.FadeBlurRoutine(instance.blurEffect, 0, 2));
    }
    public static void TurnBlurOff()
    {
        if (instance == null || instance.blurEffect == null) return;
        instance.StartCoroutine(instance.FadeBlurRoutine(instance.blurEffect, 2, 0));
    }

    public static void FadeBlur(BlurOptimized blurEff, float startValue = 0, float targetValue = 0)
    {
        if (blurEff == null && instance == null) return;
        instance.StartCoroutine(instance.FadeBlurRoutine(blurEff, startValue, targetValue));
    }
    //làm mở từ từ
    IEnumerator FadeBlurRoutine(BlurOptimized blurEff, float startValue = 0, float targetValue = 0)
    {
        blurEff.enabled = true;

        float duration = 0;
        while (duration < 1)
        {
            float value = Mathf.Lerp(startValue, targetValue, duration);
            blurEff.blurSize = value;
            duration += Time.unscaledDeltaTime * 4f;
            yield return null;
        }
        blurEff.blurSize = targetValue;

        if (targetValue == 0) blurEff.enabled = false;
        if (targetValue == 1) blurEff.enabled = true;
    }



   

    





    public bool showGizmo = true;
    void OnDrawGizmos()
    {
        if (enableLimit && showGizmo)
        {
            Vector3 p1 = new Vector3(minPosX, transform.position.y, maxPosZ);
            Vector3 p2 = new Vector3(maxPosX, transform.position.y, maxPosZ);
            Vector3 p3 = new Vector3(maxPosX, transform.position.y, minPosZ);
            Vector3 p4 = new Vector3(minPosX, transform.position.y, minPosZ);

            Gizmos.color = new Color(0f, 1f, 1f, 1f);
            Gizmos.DrawLine(p1, p2);
            Gizmos.DrawLine(p2, p3);
            Gizmos.DrawLine(p3, p4);
            Gizmos.DrawLine(p4, p1);
        }
    }

}
