using UnityEngine;
using UnityEngine.UI;

using System.Collections;
using System.Collections.Generic;

namespace UI
{

    public class UIWeaponAbilityTab : MonoBehaviour
    {

        private GameObject thisObj;
        private CanvasGroup canvasGroup;
        private static UIWeaponAbilityTab instance;
        //public static UIWeaponAbilityTab GetInstance(){ return instance; } 

        public void Awake()
        {
            instance = this;
            thisObj = gameObject;
            canvasGroup = thisObj.GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = thisObj.AddComponent<CanvasGroup>();

            thisObj.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 0, 0);
        }



        public GameObject tabObject;

        public List<UISelectItem> abilityItemList = new List<UISelectItem>();
        public List<UISelectItem> weaponItemList = new List<UISelectItem>();

        [SerializeField] private Image weaponImg;
        [SerializeField] private Image abilityImg;

        private bool inited = false;

        IEnumerator Start()
        {
            yield return null;

            //if (!UIMainControl.EnableItemSelectTab()) thisObj.SetActive(false);
            weaponImg.sprite = Weapon_DB.GetPrefab(PlayerPrefs.GetInt("weapon_selected", 0)).icon;
            abilityImg.sprite = Ability_DB.GetItem(PlayerPrefs.GetInt("ability_selected", 0)).icon;
            tabObject.SetActive(false);
        }

        private void Init()
        {
            List<Ability> abilityList = Ability_DB.Load();
            for (int i = 0; i < abilityList.Count; i++)
            {
                if (i == 0) abilityItemList[i].Init();
                else abilityItemList.Add(UISelectItem.Clone(abilityItemList[0].rootObj, "Item" + (i + 1)));

                abilityItemList[i].imgIcon.sprite = abilityList[i].icon;
                abilityItemList[i].label.text = abilityList[i].name;

                abilityItemList[i].selectHighlight.SetActive(false);
                abilityItemList[i].button.onClick.RemoveAllListeners();
                int index = i;
                abilityItemList[i].button.onClick.AddListener(() => OnSelectAbility(index));
                //abilityItemList[i].buttonObj.SetActive(false);
            }
            if (abilityList.Count == 0) abilityItemList[0].rootObj.SetActive(false);


            //UnitPlayer player = GameControl.GetPlayer();
            List<Weapon> weapons = Weapon_DB.Load();
            for (int i = 0; i < weapons.Count; i++)
            {
                if (i == 0) weaponItemList[i].Init();
                else weaponItemList.Add(UISelectItem.Clone(weaponItemList[0].rootObj, "Item" + (i + 1)));

                weaponItemList[i].imgIcon.sprite = weapons[i].icon;
                weaponItemList[i].label.text = weapons[i].weaponName;
                weaponItemList[i].labelAlt.text = weapons[i].clipSize + "/" + weapons[i].ammo;
                weaponItemList[i].button.onClick.RemoveAllListeners();
                int index = i;
                weaponItemList[i].button.onClick.AddListener(() => OnSelectWeapon(index));

                weaponItemList[i].selectHighlight.SetActive(false);
                //weaponItemList[i].buttonObj.SetActive(false);
            }
            inited = true;
        }

        // void OnEnable()
        // {
        //     TDS.onNewWeaponE += OnNewWeapon;
        //     TDS.onNewAbilityE += OnNewAbility;
        // }
        // void OnDisable()
        // {
        //     TDS.onNewWeaponE -= OnNewWeapon;
        //     TDS.onNewAbilityE -= OnNewAbility;
        // }


        // void OnNewWeapon(Weapon weapon, int replaceIndex = -1)
        // {
        //     if (replaceIndex >= 0)
        //     {
        //         weaponItemList[replaceIndex].imgIcon.sprite = weapon.icon;
        //         weaponItemList[replaceIndex].label.text = weapon.weaponName;
        //     }
        //     else
        //     {
        //         int index = weaponItemList.Count;
        //         weaponItemList.Add(UISelectItem.Clone(weaponItemList[0].rootObj, "Item" + (index)));

        //         weaponItemList[index].imgIcon.sprite = weapon.icon;
        //         weaponItemList[index].label.text = weapon.weaponName;

        //         weaponItemList[index].rootObj.SetActive(true);
        //         weaponItemList[index].selectHighlight.SetActive(false);
        //     }
        // }
        // void OnNewAbility(Ability ability, int replaceIndex = -1)
        // {
        //     if (replaceIndex >= 0)
        //     {
        //         abilityItemList[replaceIndex].imgIcon.sprite = ability.icon;
        //         abilityItemList[replaceIndex].label.text = ability.name;
        //     }
        //     else
        //     {
        //         int index = abilityItemList.Count;
        //         abilityItemList.Add(UISelectItem.Clone(abilityItemList[0].rootObj, "Item" + (index)));

        //         abilityItemList[index].imgIcon.sprite = ability.icon;
        //         abilityItemList[index].label.text = ability.name;

        //         abilityItemList[index].rootObj.SetActive(true);
        //         abilityItemList[index].selectHighlight.SetActive(false);
        //     }
        // }



        void UpdateTab()
        {
            int indexSelectedAbility = Ability_DB.GetIndexAbility(PlayerPrefs.GetInt("ability_selected", 0));
            for (int i = 0; i < abilityItemList.Count; i++)
            {
                abilityItemList[i].selectHighlight.SetActive(i == indexSelectedAbility);
                abilityItemList[i].button.interactable = i != indexSelectedAbility;
            }

            int indexSelectedWeapon = Weapon_DB.GetIndexWeapon(PlayerPrefs.GetInt("weapon_selected", 0));
            for (int i = 0; i < weaponItemList.Count; i++)
            {
                weaponItemList[i].selectHighlight.SetActive(i == indexSelectedWeapon);
                weaponItemList[i].button.interactable = i != indexSelectedWeapon;
            }
        }

        public void OnSelectWeapon(int index)
        {
            int oldIndex = Weapon_DB.GetIndexWeapon(PlayerPrefs.GetInt("weapon_selected", 0));
            if (index == oldIndex) return;

            weaponItemList[index].selectHighlight.SetActive(true);
            weaponItemList[index].button.interactable = false;

            weaponItemList[oldIndex].selectHighlight.SetActive(false);
            weaponItemList[oldIndex].button.interactable = true;

            var weapon = Weapon_DB.Load()[index];

            weaponImg.sprite = weapon.icon;
            PlayerPrefs.SetInt("weapon_selected", weapon.ID);
        }

        public void OnSelectAbility(int index)
        {
            int oldIndex = Ability_DB.GetIndexAbility(PlayerPrefs.GetInt("ability_selected", 0));
            if (index == oldIndex) return;

            abilityItemList[index].selectHighlight.SetActive(true);
            abilityItemList[index].button.interactable = false;


            abilityItemList[oldIndex].selectHighlight.SetActive(false);
            abilityItemList[oldIndex].button.interactable = true;

            var ability = Ability_DB.Load()[index];

            abilityImg.sprite = ability.icon;
            PlayerPrefs.SetInt("ability_selected", Ability_DB.Load()[index].ID);
        }


        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                _TurnTabOn();
            }
            else if (Input.GetKeyUp(KeyCode.Tab))
            {
                _TurnTabOff();
            }
        }


        public static void TurnTabOn() { instance._TurnTabOn(); }
        public void _TurnTabOn()
        {
            if (!inited) Init();
            Time.timeScale = 0;
            Cursor.visible = true;

            isOn = true;

            TDSTouchInput.Hide();

            //GameControl.GetPlayer().DisableFire();
            UpdateTab();
            tabObject.SetActive(true);
        }
        public static void TurnTabOff() { instance._TurnTabOff(); }
        public void _TurnTabOff()
        {
            Time.timeScale = 1;
            Cursor.visible = false;

            isOn = false;

            TDSTouchInput.Show();

            //GameControl.GetPlayer().EnableFire();
            tabObject.SetActive(false);
        }


        public void OnCloseButton()
        {
            _TurnTabOff();
        }


        private bool isOn = false;
        public static bool IsOn() { return instance.isOn; }

    }

}