using UnityEngine;
using System.Collections;

//tạo hiệu ứng nhấp nháy cho súng
public class GunShotMuzzle : MonoBehaviour
{

    private LineRenderer rend;
    public float blinkDuration = 0.02f;

    void Awake()
    {
        rend = gameObject.GetComponent<LineRenderer>();
    }

    void OnEnable()
    {
        if (rend != null) StartCoroutine(DisableRenderer());
    }

    IEnumerator DisableRenderer()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.025f);
            rend.enabled = !rend.enabled;
        }
    }


}
