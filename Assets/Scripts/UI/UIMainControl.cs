using UnityEngine;
using UnityEngine.UI;

using System.Collections;
using System.Collections.Generic;

using UnityStandardAssets.ImageEffects;

namespace UI
{

    public class UIMainControl : MonoBehaviour
    {

        private static UIMainControl instance;

        public bool enableMouseNKeyInput = true;

        public bool enableHPOverlay = true;
        public static bool EnableHPOverlay() { return instance.enableHPOverlay; }

        public bool enableTextOverlay = true; //show damage mỗi khi trúng đạn
        public static bool EnableTextOverlay() { return instance.enableTextOverlay; }

        public bool showContinueButtonWhenLost = false;
        public static bool ShowContinueButtonWhenLost() { return instance.showContinueButtonWhenLost; }

        public BlurOptimized uiBlurEffect; //làm mờ

        public bool limitScale = true; //scale ui theo màn hình

        public List<CanvasScaler> scalerList = new List<CanvasScaler>();
        public static float GetScaleFactor()
        {
            if (instance.scalerList.Count == 0) return 1;

            if (instance.scalerList[0].uiScaleMode == CanvasScaler.ScaleMode.ConstantPixelSize)
                return 1f / instance.scalerList[0].scaleFactor;
            if (instance.scalerList[0].uiScaleMode == CanvasScaler.ScaleMode.ScaleWithScreenSize)
                return (float)instance.scalerList[0].referenceResolution.x / (float)Screen.width;

            return 1;
        }



        private float scrollCD = 0; //for switching weapon using mouse scroll


        void Awake()
        {
            instance = this;
        }

        void Start()
        {
            if (limitScale)
            {
                for (int i = 0; i < scalerList.Count; i++)
                {
                    if (Screen.width >= scalerList[i].referenceResolution.x) instance.scalerList[i].uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
                    else instance.scalerList[i].uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                }
            }
        }

        void OnEnable()
        {
            TDS.onEndGame += OnGameOver;
            TDS.onGameMessageE += OnGameMessage;

            TDS.onPlayerRespawnedE += OnPlayerRespawn;
        }
        void OnDisable()
        {
            TDS.onEndGame -= OnGameOver;
            TDS.onGameMessageE -= OnGameMessage;

            TDS.onPlayerRespawnedE -= OnPlayerRespawn;
        }


        void OnPlayerRespawn()
        {
            UILevelPerkMenu.OnPlayerRespawn(GameControl.GetPlayer());
        }


        public static void OnGameMessage(string msg) { instance._OnGameMessage(msg); }
        public void _OnGameMessage(string msg) { UIMessage.Display(msg); }


        //called when the game is over
        public void OnGameOver(bool won)
        {
            StartCoroutine(GameOverDelay(won));
        }
        IEnumerator GameOverDelay(bool won)
        {
            yield return StartCoroutine(WaitForRealSeconds(.1f));
            CameraControl.FadeBlur(uiBlurEffect, 0, 2);
            CameraControl.TurnBlurOn();
            UIGameOver.Show(won);

            TDSTouchInput.Hide();
        }


        void Update()
        {
            if (!enableMouseNKeyInput) return;

            if (Input.GetButtonDown("Cancel") || Input.GetKeyDown(KeyCode.Escape) && !GameControl.IsGameOver())
            {
                _TogglePause();
            }


            if (GameControl.IsGamePlaying())
            {
                UnitPlayer player = GameControl.GetPlayer();
                if (player != null && !player.IsDestroyed() && Input.touchCount == 0)
                {

                    //movement
                    bool boost = Input.GetKey(KeyCode.LeftShift);
                    if (Input.GetButton("Horizontal") || Input.GetButton("Vertical"))
                        player.Move(new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")), boost);

                    //phanh
                    if (Input.GetKey(KeyCode.Space)) player.Brake();

               
                    scrollCD -= Time.deltaTime;


                    bool hasJoystick = false;

                    //turret facing
                    if (hasJoystick)
                    {
                        if (Input.GetButton("RightThumbStick_X") || Input.GetButton("RightThumbStick_Y"))
                            player.AimTurretDPad(new Vector2(Input.GetAxisRaw("RightThumbStick_X"), Input.GetAxisRaw("RightThumbStick_Y")));
                        else player.AimTurretMouse(Input.mousePosition);
                    }
                    else player.AimTurretMouse(Input.mousePosition);

                    //fire
                    bool continousFire = Input.GetMouseButton(0) || Input.GetButton("Fire1");
                    if (Input.GetMouseButtonDown(0) || Input.GetButtonDown("Fire1") || continousFire) player.FireWeapon();

                    if (Input.GetMouseButtonDown(1) || Input.GetButtonDown("Fire2")) player.FireAbility();

                    if (Input.GetMouseButtonDown(2) || Input.GetButtonDown("Fire3")) player.FireAbilityAlt();

                    //reload
                    if (Input.GetKeyDown(KeyCode.R) || Input.GetButtonDown("Jump")) player.Reload();

                    //hiển thị level perk
                    if (UILevelPerkMenu.Enabled() && Input.GetKeyDown(KeyCode.C)) ToggleLevelPerkMenu();
                }
            }

        }


        public static void ToggleLevelPerkMenu() { instance._ToggleLevelPerkMenu(); }
        public void _ToggleLevelPerkMenu()
        {
            if (GameControl.IsGamePlaying()) ShowLevelPerkMenu();
            else if (GameControl.IsGamePaused()) CloseLevelPerkMenu();
        }

        public static void ShowLevelPerkMenu() { instance._ShowLevelPerkMenu(); }
        public void _ShowLevelPerkMenu()
        {
            CameraControl.FadeBlur(uiBlurEffect, 0, 2);
            CameraControl.TurnBlurOn();
            GameControl.PauseGame();
            UILevelPerkMenu.Show();

            TDSTouchInput.Hide();

            Time.timeScale = 0;
        }
        public static void CloseLevelPerkMenu() { instance.StartCoroutine(instance._CloseLevelPerkMenu()); }
        public IEnumerator _CloseLevelPerkMenu()
        {
            CameraControl.FadeBlur(uiBlurEffect, 2, 0);
            CameraControl.TurnBlurOff();
            UILevelPerkMenu.Hide();

            TDSTouchInput.Show();

            yield return StartCoroutine(WaitForRealSeconds(0.25f));

            Time.timeScale = 1;
            GameControl.ResumeGame();
        }



        public static void TogglePause() { instance._TogglePause(); }
        public void _TogglePause()
        {
            if (GameControl.IsGamePlaying()) PauseGame();
            else if (GameControl.IsGamePaused()) ResumeGame();
        }


        public static void PauseGame() { instance._PauseGame(); }
        public void _PauseGame()
        {
            CameraControl.FadeBlur(uiBlurEffect, 0, 2);
            CameraControl.TurnBlurOn();
            GameControl.PauseGame(); 
            UIPauseMenu.Show();

            TDSTouchInput.Hide();

            Time.timeScale = 0;
        }
        public static void ResumeGame() { instance.StartCoroutine(instance._ResumeGame()); }
        IEnumerator _ResumeGame()
        {
            CameraControl.FadeBlur(uiBlurEffect, 2, 0);
            CameraControl.TurnBlurOff();
            GameControl.ResumeGame();
            UIPauseMenu.Hide();

            TDSTouchInput.Show();

            yield return StartCoroutine(WaitForRealSeconds(0.25f));
            Time.timeScale = 1;
        }



        public static IEnumerator WaitForRealSeconds(float time)
        {
            float start = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup < start + time) yield return null;
        }



        public static void FadeOut(CanvasGroup canvasGroup, float duration = 0.25f, GameObject obj = null)
        {
            instance.StartCoroutine(instance._FadeOut(canvasGroup, 1f / duration, obj));
        }
        IEnumerator _FadeOut(CanvasGroup canvasGroup, float timeMul, GameObject obj)
        {
            float duration = 0;
            while (duration < 1)
            {
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, duration);
                duration += Time.unscaledDeltaTime * timeMul;
                yield return null;
            }
            canvasGroup.alpha = 0f;

            if (obj != null) obj.SetActive(false);
        }
        public static void FadeIn(CanvasGroup canvasGroup, float duration = 0.25f, GameObject obj = null)
        {
            instance.StartCoroutine(instance._FadeIn(canvasGroup, 1f / duration, obj));
        }
        IEnumerator _FadeIn(CanvasGroup canvasGroup, float timeMul, GameObject obj)
        {
            if (obj != null) obj.SetActive(true);

            float duration = 0;
            while (duration < 1)
            {
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, duration);
                duration += Time.unscaledDeltaTime * timeMul;
                yield return null;
            }
            canvasGroup.alpha = 1f;
        }


    }

}