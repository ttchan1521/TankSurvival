using UnityEngine;
using UnityEngine.UI;

using System.Collections;
using System.Collections.Generic;

namespace UI
{

    public class UIAbilityButton : MonoBehaviour
    {

        public void Awake()
        {

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

        }


        void DisableAllButtons()
        {
            gameObject.SetActive(false);
            foreach (Transform child in transform) child.gameObject.SetActive(false);
            enabled = false;
        }


        void Update()
        {
            if (!initiated || !GameControl.EnableAbility()) return;

            Ability ability = AbilityManager.GetAbility();

            button.label.text = ability.currentCD <= 0 ? "" : ability.currentCD.ToString("f1") + "s";
            button.button.interactable = ability.IsReady() == "" ? true : false;

        }


        void OnSwitchAbility(Ability ability)
        {

            button.imgIcon.sprite = ability.icon;


        }

    }

}