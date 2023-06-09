﻿using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

using System.Collections;
using System.Collections.Generic;

public class EffectDB : MonoBehaviour
{


    public static EffectDB instance;
    public static void Init()
    {
        if (instance != null) return;
        instance = LoadDB1();
    }

    public static int GetEffectIndex1(int ID)
    {
        Init();
        for (int i = 0; i < instance.effectList.Count; i++) { if (instance.effectList[i].ID == ID) return i; }
        return -1;
    }

    public static Effect CloneItem1(int idx)
    {
        Init();
        if (idx > 0 && idx < instance.effectList.Count) return instance.effectList[idx].Clone();
        return null;
    }



    public List<Effect> effectList = new List<Effect>();

    public static EffectDB LoadDB1()
    {
        GameObject obj = Resources.Load("DB_TDSTK/Obsolete/DB_Effect", typeof(GameObject)) as GameObject;
        return obj.GetComponent<EffectDB>();
    }

    public static List<Effect> Load1()
    {
        return LoadDB1().effectList;
    }

    public static List<Effect> LoadClone1()
    {
        EffectDB instance = LoadDB1();
        List<Effect> newList = new List<Effect>();
        if (instance != null)
        {
            for (int i = 0; i < instance.effectList.Count; i++) newList.Add(instance.effectList[i].Clone());
        }
        return newList;
    }

}

