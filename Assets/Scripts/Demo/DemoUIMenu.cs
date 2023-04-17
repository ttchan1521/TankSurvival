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
    [SerializeField] Button pvpButton;
    [SerializeField] Button chatButton;
    [SerializeField] Button leaderboardButton;

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
            username.text = PlayerPrefsManager.Username;
            pvpButton.gameObject.SetActive(true);
            chatButton.gameObject.SetActive(true);
            leaderboardButton.gameObject.SetActive(true);
        }
        else
        {
            pvpButton.gameObject.SetActive(false);
            chatButton.gameObject.SetActive(false);
            leaderboardButton.gameObject.SetActive(false);
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
        else if (butIndex == 4)
        {
            var chat = Resources.Load<GameObject>("Chat");
            Instantiate(chat, GetComponentInChildren<Canvas>().transform);
            return;
        }
        else if (butIndex == 5)
        {
            var leaderboard = Resources.Load<GameObject>("Leaderboard");
            Instantiate(leaderboard, GetComponentInChildren<Canvas>().transform);
            return;
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
}