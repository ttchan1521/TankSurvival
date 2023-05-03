using UnityEngine;
using UnityEngine.UI;

using System.Collections;

namespace UI
{

    public class UIHUD : MonoBehaviour
    {
        public Text lbScore;
        public Text lbTimer;

        public Text lbRespawnCount;

        public Text lbHP;
        public Text lbEnergy;

        public Slider sliderHPBar;
        public Slider sliderEnergyBar;

        public Slider sliderExpBar;
        public Slider opponentHPBar;
        public Text lbOpponentHP;
        public Text opponentUsername;
        private PlayerProgression playerProgress;

        private float hitPointFull;

        public UIButton uiButtonWeapon;
        public UIButton uiButtonAltFire;
        public GameObject abilityButtonObj;


        private int score = 0;
        private float minUpdateSpeed = 0.05f;
        private float updateSpeed = 0f;


        void Awake()
        {
            uiButtonWeapon.Init();
            uiButtonAltFire.Init();

            uiButtonWeapon.labelAlt2.text = "";

        }

        void Start()
        {
            if (!GameControl.EnableAltFire()) uiButtonAltFire.rootObj.SetActive(false);
            if (!GameControl.EnableAbility()) abilityButtonObj.SetActive(false);

            //credit = GameControl.GetCredits();
            score = GameControl.GetScore();

            UnitPlayer player = GameControl.GetPlayer();
            if (player != null) playerProgress = player.GetPlayerProgression();
        }

        void OnEnable()
        {
            TDS.onSwitchWeaponE += OnSwitchWeapon;
            TDS.onReloadingE += OnReloading;

            TDS.onPlayerRespawnedE += OnPlayerRespawn;
        }
        void OnDisable()
        {
            TDS.onSwitchWeaponE -= OnSwitchWeapon;
            TDS.onReloadingE -= OnReloading;

            TDS.onPlayerRespawnedE -= OnPlayerRespawn;
        }

        void OnPlayerRespawn()
        {
            UnitPlayer player = GameControl.GetPlayer();
            if (player != null) playerProgress = player.GetPlayerProgression();
        }

        // Update is called once per frame
        void Update()
        {
            //nếu tính thời gian
            if (GameControl.EnableTimer())
            {
                float remainingDuration = GameControl.GetRemainingDuration();
                if (remainingDuration > 0)
                {

                    int minR = (int)Mathf.Floor(remainingDuration / 60);
                    int secR = (int)Mathf.Floor(remainingDuration % 60);

                    lbTimer.text = "Time Left: " + minR + ":" + (secR > 9 ? secR.ToString() : "0" + secR);
                }
                else
                {
                    lbTimer.text = "Time Left: 0:00";
                }
            }
            else lbTimer.text = "";



            lbRespawnCount.text = GameControl.GetPlayerLife().ToString();

            UnitPlayer player = GameControl.GetPlayer();
            if (player == null)
            {
                lbHP.text = "0/" + player.GetFullHitPoint();
                sliderHPBar.value = 0;
                return;
            }



            sliderHPBar.value = player.hitPoint / player.GetFullHitPoint();
            lbHP.text = Mathf.Round(player.hitPoint) + "/" + Mathf.Round(player.GetFullHitPoint());

            sliderEnergyBar.value = player.energy / player.GetFullEnergy();
            lbEnergy.text = Mathf.Round(player.energy) + "/" + Mathf.Round(player.GetFullEnergy());


            if (playerProgress != null) sliderExpBar.value = Mathf.Max(0.01f, playerProgress.GetCurrentLevelProgress());


            if (!reloading)
            {

                string clip = player.GetCurrentClip() < 0 ? "∞" : player.GetCurrentClip().ToString();
                string ammo = player.GetAmmo() < 0 ? "∞" : player.GetAmmo().ToString();
                uiButtonWeapon.labelAlt.text = clip + "/" + ammo;

            }


            if (GameControl.EnableAltFire())
            {
                Ability ability = player.GetWeaponAbility();
                if (ability != null)
                {
                    uiButtonAltFire.label.text = ability.currentCD <= 0 ? "" : ability.currentCD.ToString("f1") + "s";
                    uiButtonAltFire.button.interactable = ability.IsReady() == "" ? true : false;
                }
            }


            int scoreTgt = GameControl.GetScore();
            if (score != scoreTgt)
            {
                updateSpeed = Mathf.Max(minUpdateSpeed, Mathf.Abs(1f / (float)(scoreTgt - score)));
                score = (int)Mathf.Round(Mathf.Lerp(score, scoreTgt, updateSpeed));
            }

            lbScore.text = "Score: " + score;
        }


        private bool reloading = false;
        void OnReloading()
        {
            if (!reloading) StartCoroutine(ReloadRoutine());
        }
        IEnumerator ReloadRoutine()
        {
            yield return null; 

            reloading = true;
            uiButtonWeapon.button.interactable = false;
            uiButtonWeapon.labelAlt.alignment = TextAnchor.MiddleLeft;

            UnitPlayer player = GameControl.GetPlayer();

            while (player != null && player.Reloading())
            {
                string dot = "";
                int count = (int)Mathf.Floor((Time.time * 3) % 4);
                for (int i = 0; i < count; i++) dot += ".";
                for (int i = count; i < 3; i++) dot += " ";
                uiButtonWeapon.labelAlt.text = "Reloading" + dot;

                float durationRemain = player.GetReloadDuration() - player.GetCurrentReload();
                uiButtonWeapon.labelAlt2.text = durationRemain <= 0 ? "" : durationRemain.ToString("f1") + "s";

                yield return null;
            }

            uiButtonWeapon.labelAlt.text = "";
            uiButtonWeapon.labelAlt.alignment = TextAnchor.MiddleRight;

            uiButtonWeapon.labelAlt2.text = "";
            uiButtonWeapon.button.interactable = true;

            reloading = false;
        }


        void OnSwitchWeapon(Weapon weapon)
        {
            uiButtonWeapon.imgIcon.sprite = weapon.icon;
            uiButtonWeapon.label.text = weapon.weaponName;

            if (GameControl.EnableAltFire())
            {
                if (weapon.ability != null)
                {
                    uiButtonAltFire.imgIcon.sprite = weapon.ability.icon;
                }
                else
                {
                    uiButtonAltFire.imgIcon.sprite = null;
                    uiButtonAltFire.label.text = "";
                }
            }

            if (reloading) reloading = false;
        }

        public void UpdateSliderHPOpponent(float hp, float hpfull)
        {
            if (opponentHPBar != null)
            {
                opponentHPBar.maxValue = hpfull;
                opponentHPBar.value = hp;
                lbOpponentHP.text = Mathf.Round(hp) + "/" + hpfull;
            }
        }

        public void UpdateOpponentName(string name)
        {
            opponentUsername.text = name;
        }

    }

}