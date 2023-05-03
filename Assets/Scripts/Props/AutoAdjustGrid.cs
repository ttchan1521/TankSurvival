using UnityEngine;
using System.Collections;

//Điều chỉnh tỉ lệ texture
public class AutoAdjustGrid : MonoBehaviour
{

    public float gridSize = 0.5f;

    void Start()
    {
        Renderer rend = transform.GetComponent<Renderer>();
        if (rend == null) return;

        Material mat = rend.material;

        mat.mainTextureScale = new Vector2(transform.localScale.x * gridSize, transform.localScale.y * gridSize);
    }

}