using UnityEngine;
using System.Collections;

public class DemoUIInstruction : MonoBehaviour
{

    private bool show = false;

    public GameObject instructionObjShow;
    public GameObject instructionObjHide;

    void Start()
    {
        UpdateShow();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            show = !show;
            UpdateShow();
        }
    }


    void UpdateShow()
    {
        instructionObjShow.SetActive(show);
        instructionObjHide.SetActive(!show);
    }

}
