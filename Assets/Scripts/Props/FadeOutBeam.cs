using UnityEngine;
using System.Collections;

//đạn laser
public class FadeOutBeam : MonoBehaviour
{

    public float fadeDuration = .5f; //thời gian tồn tại
    public float startingWidth = .5f;

    void OnEnable()
    {
        LineRenderer line = gameObject.GetComponent<LineRenderer>();
        if (line != null) StartCoroutine(Fade(line));
    }

    IEnumerator Fade(LineRenderer line)
    {
        float durationModifier = 1f / fadeDuration;

        float duration = 0;
        while (duration < 1)
        {

            float width = Mathf.Lerp(startingWidth, 0, duration);
            //line.SetWidth(width, width);
            line.startWidth = width; line.endWidth = width;

            duration += Time.deltaTime * durationModifier;
            yield return null;
        }

        //line.SetWidth(0, 0);
        line.startWidth = 0; line.endWidth = 0;
    }

}
