using UnityEngine;
using UnityEngine.UI;
using DefaultNamespace;
using BestHTTP;
using System;

#if UNITY_5_3_OR_NEWER
using UnityEngine.SceneManagement;
#endif

public class DemoUIMenu : MonoBehaviour
{
    public bool loadMobileScene = false;
    [SerializeField] Text username;

    private Camera _cameraMain;
    private bool activeColorInput = true;

    private void Awake()
    {
        _cameraMain = Camera.main;
    }

    void Start()
    {
        var unit = FindObjectOfType<UnitPlayer>();
        // Vector3 screenPos = Camera.main.WorldToScreenPoint(unit.transform.position);
        // colorBtn.localPosition = screenPos;
        var _renderers = unit.GetComponentsInChildren<Renderer>();
        foreach (var renderer1 in _renderers)
        {
            renderer1.material.SetColor($"_Color1", PlayerPrefsManager.mainColor);
            renderer1.material.SetColor($"_Color2", PlayerPrefsManager.subColor);
        }

        TDS.onSignupE += OnSignUp;

        TDS.OnSignUp(!string.IsNullOrEmpty(PlayerPrefsManager.UserId));
    }

    void OnDestroy()
    {
        TDS.onSignupE -= OnSignUp;
    }

    private void OnSignUp(bool signup)
    {
        if (signup)
        {
            var panel = Resources.Load<GameObject>("NetworkUI");
            Instantiate(panel, GetComponentInChildren<Canvas>().transform);
            GetUser();
        }
        else
        {
            var panel = Resources.Load<GameObject>("SignUp");
            Instantiate(panel, GetComponentInChildren<Canvas>().transform);
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = _cameraMain.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hitInfo))
            {
                ShowColorPicker(hitInfo);
            }
        }

        foreach (Touch touch in Input.touches)
        {
            if (touch.phase == TouchPhase.Began)
            {
                Ray ray = _cameraMain.ScreenPointToRay(touch.position);
                if (Physics.Raycast(ray, out var hitInfo))
                {
                    ShowColorPicker(hitInfo);
                }
            }
        }
    }

    void ShowColorPicker(RaycastHit hitInfo)
    {
        if (activeColorInput && hitInfo.transform.TryGetComponent<UnitPlayer>(out var unit))
        {
            var picker = Resources.Load<GameObject>("Picker 2.0 Variant");
            Instantiate(picker, GetComponentInChildren<Canvas>().transform);
        }
    }

    public void ActiveColorPickerInput(bool active)
    {
        activeColorInput = active;
    }

    public void OnButton(int butIndex)
    {
        string prefix = loadMobileScene ? "Mobile" : "Demo";
        string sceneName = "";

        if (butIndex == 0)
        {
            if (string.IsNullOrEmpty(PlayerPrefsManager.UserId)) return;
            RequestJoinRoom();
            return;
        }
        else if (butIndex == 1)
        {
            sceneName = prefix + "SideScroller";
        }
        else if (butIndex == 2)
        {
            sceneName = prefix + "Arena";
        }
        else if (butIndex == 3)
        {
            sceneName = prefix + "Gauntlet";
        }

#if UNITY_5_3_OR_NEWER
        SceneManager.LoadScene(sceneName);
#else
			Application.LoadLevel(sceneName);
#endif
    }

    private void RequestJoinRoom()
    {
        NetworkManager.Instance.Manager.Socket
            .Emit("createPvp");
        NetworkManager.Instance.Manager.Socket.On<string, int>("joined", JoinRoom);
    }

    private void JoinRoom(string roomId, int land)
    {
        PvP.SetRoom(roomId);
        PvP.SetLand(land);
        SceneManager.LoadScene("MobilePvP");
    }

    private void GetUser()
    {
        HTTPRequest request = new HTTPRequest(new Uri($"{NetworkManager.UriString}/users/user?userId=" + PlayerPrefsManager.UserId),
            OnRequestFinished);
        request.Send();
    }

    private void OnRequestFinished(HTTPRequest req, HTTPResponse res)
    {
        switch (req.State)
            {
                // The request finished without any problem.
                case HTTPRequestStates.Finished:
                    if (res.IsSuccess)
                    {
                        var user = JsonUtility.FromJson<Users.Entities.User>(res.DataAsText);
                        if (user != null)
                            username.text = user.username;
                    }
                    else
                    {
                        Debug.LogWarning(string.Format(
                            "Request finished Successfully, but the server sent an error. Status Code: {0}-{1} Message: {2}",
                            res.StatusCode,
                            res.Message,
                            res.DataAsText));
                       
                    }

                    break;

                // The request finished with an unexpected error. The request's Exception property may contain more info about the error.
                case HTTPRequestStates.Error:
                    Debug.LogError("Request Finished with Error! " + (req.Exception != null
                        ? (req.Exception.Message + "\n" + req.Exception.StackTrace)
                        : "No Exception"));
                    break;

                // The request aborted, initiated by the user.
                case HTTPRequestStates.Aborted:
                    Debug.LogWarning("Request Aborted!");
                    break;

                // Connecting to the server is timed out.
                case HTTPRequestStates.ConnectionTimedOut:
                    Debug.LogError("Connection Timed Out!");
                    break;

                // The request didn't finished in the given time.
                case HTTPRequestStates.TimedOut:
                    Debug.LogError("Processing the request Timed Out!");
                    break;
            }
    }
}