using UnityEngine;
using UnityEngine.UI;

using System.Collections;
using System.Collections.Generic;

namespace UI
{

    public class UIAbilityButton : MonoBehaviour
    {

        //private GameObject thisObj;
        //private CanvasGroup canvasGroup;
        //private static UIAbilityButton instance;

        public void Awake()
        {
            //instance=this;
            //~ thisObj=gameObject;
            //~ canvasGroup=thisObj.GetComponent<CanvasGroup>();
            //~ if(canvasGroup==null) canvasGroup=thisObj.AddComponent<CanvasGroup>();

            //~ thisObj.GetComponent<RectTransform>().anchoredPosition=new Vector3(0, 0, 0);
        }

        public RectTransform selectT;
        public UIButton button;


        private bool initiated = false;
        IEnumerator Start()
        {
            yield return null;

            if (!GameControl.EnableAbility() || AbilityManager.GetAbility() == null)
            {
                DisableAllButtons();
                yield break;
            }

            initiated = true;


            button.Init();
            button.labelAlt.enabled = false;
            OnSwitchAbility(AbilityManager.GetAbility());
            selectT.gameObject.SetActive(false);

            // else
            // {
            //     List<Ability> abilityList = AbilityManager.GetAbilityList();

            //     for (int i = 0; i < abilityList.Count; i++)
            //     {
            //         if (i == 0) buttonList[i].Init();
            //         else buttonList.Add(UIButton.Clone(buttonList[0].rootObj, "AbilityButton" + (i + 1)));

            //         buttonList[i].imgIcon.sprite = abilityList[i].icon;
            //         buttonList[i].labelAlt.text = (i + 1).ToString();
            //         buttonList[i].label.text = "";
            //     }

            //     yield return null;  //give it one frame delay for the layoutGroup to rearrange the buttons
            //     selectT.localPosition = buttonList[AbilityManager.GetSelectID()].rectT.localPosition;
            // }
        }

        // void OnNewAbility(Ability ab, int replaceIndex = -1)
        // {
        //     if (replaceIndex >= 0)
        //     {
        //         buttonList[replaceIndex].imgIcon.sprite = ab.icon;
        //     }
        //     else
        //     {
        //         if (!singleButton)
        //         {
        //             int index = buttonList.Count;
        //             buttonList.Add(UIButton.Clone(buttonList[0].rootObj, "AbilityButton" + (index + 1)));

        //             buttonList[index].imgIcon.sprite = ab.icon;
        //             buttonList[index].labelAlt.text = (buttonList.Count).ToString();
        //             buttonList[index].label.text = "";
        //         }
        //     }
        //     OnSwitchAbility(ab);
        // }

        void DisableAllButtons()
        {
            gameObject.SetActive(false);
            foreach (Transform child in transform) child.gameObject.SetActive(false);
            enabled = false;
        }


        // void OnEnable()
        // {
        //     TDS.onSwitchAbilityE += OnSwitchAbility;
        //     TDS.onNewAbilityE += OnNewAbility;
        // }
        // void OnDisable()
        // {
        //     TDS.onSwitchAbilityE -= OnSwitchAbility;
        //     TDS.onNewAbilityE += OnNewAbility;
        // }


        void Update()
        {
            if (!initiated || !GameControl.EnableAbility()) return;

            Ability ability = AbilityManager.GetAbility();

            button.label.text = ability.currentCD <= 0 ? "" : ability.currentCD.ToString("f1") + "s";
            button.button.interactable = ability.IsReady() == "" ? true : false;

            // else
            // {
            //     List<Ability> abilityList = AbilityManager.GetAbilityList();

            //     for (int i = 0; i < buttonList.Count; i++)
            //     {
            //         //Debug.Log(i);
            //         buttonList[i].label.text = abilityList[i].currentCD <= 0 ? "" : abilityList[i].currentCD.ToString("f1") + "s";
            //         buttonList[i].button.interactable = abilityList[i].IsReady() == "" ? true : false;
            //     }
            // }

        }


        void OnSwitchAbility(Ability ability)
        {
                //Debug.Log("switch ability  "+ability+"   "+uiButtonAbility.imgIcon);
                button.imgIcon.sprite = ability.icon;
            
            // else
            // {
            //     if (!initiated || buttonList.Count < AbilityManager.GetSelectID()) return;
            //     selectT.localPosition = buttonList[AbilityManager.GetSelectID()].rectT.localPosition;
            // }
        }

    }

}