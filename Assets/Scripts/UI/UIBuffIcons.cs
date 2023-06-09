﻿using UnityEngine;
using UnityEngine.UI;

using System.Collections;
using System.Collections.Generic;

namespace UI
{
    //panel buff icon
    public class UIBuffIcons : MonoBehaviour
    {

        private int currentID = 0;

        private GameObject thisObj;
        private CanvasGroup canvasGroup;
        private static UIGameOver instance;

        public List<UIObject> itemList = new List<UIObject>(); //list icon

        public void Awake()
        {

            for (int i = 0; i < 1; i++)
            {
                if (i == 0) itemList[0].Init();
                else itemList.Add(UIObject.Clone(itemList[0].rootObj, "icon" + (i + 1)));
                itemList[i].rootObj.SetActive(false);
            }
        }


        void OnEnable()
        {
            TDS.onGainEffectE += NewEffect;
            TDS.onPlayerDestroyedE += PlayerDestroyed;
        }
        void OnDisable()
        {
            TDS.onGainEffectE -= NewEffect;
            TDS.onPlayerDestroyedE -= PlayerDestroyed;
        }


        void PlayerDestroyed() { currentID += 1; }


        void NewEffect(Effect effect)
        {
            if (effect.ID < 0) return;
            int idx = GetUnusedItem();
            StartCoroutine(ShowRoutine(effect, idx));
        }

        IEnumerator ShowRoutine(Effect effect, int idx)
        {
            int ID = currentID;

            itemList[idx].imgIcon.sprite = effect.icon;
            itemList[idx].label.text = effect.duration.ToString("f1") + "s";

            itemList[idx].rootObj.SetActive(true);
            while (effect.duration > 0 && !effect.expired && currentID == ID)
            {
                itemList[idx].label.text = effect.duration.ToString("f1") + "s";
                yield return null;
            }
            itemList[idx].rootObj.SetActive(false);
        }


        private int GetUnusedItem()
        {
            for (int i = 0; i < itemList.Count; i++)
            {
                if (itemList[i].rootObj.activeInHierarchy) continue;
                return i;
            }

            itemList.Add(UIObject.Clone(itemList[0].rootObj, "icon" + (itemList.Count + 1)));
            return itemList.Count - 1;
        }

    }

}