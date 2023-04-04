using System.Collections;
using System.Collections.Generic;
using ParrelSync;
using UnityEngine;

public class PlayerPrefsManager
{
    private const string PREFS_WEAPON_SELECTED = "weapon_selected";
    private const string PREFS_ABILITY_SELECTED = "ability_selected";
    private const string PREFS_MAIN_COLOR = "main_color_";
    private const string PREFS_SUB_COLOR = "sub_color_";

    public static int weaponSelectID
    {
        get => PlayerPrefs.GetInt(PREFS_WEAPON_SELECTED, 0);
        set => PlayerPrefs.SetInt(PREFS_WEAPON_SELECTED, value);
    }

    public static int abilitySelectID
    {
        get => PlayerPrefs.GetInt(PREFS_ABILITY_SELECTED, 0);
        set => PlayerPrefs.SetInt(PREFS_ABILITY_SELECTED, value);
    }

    public static Color mainColor
    {
        get
        {
            if (ColorUtility.TryParseHtmlString(PlayerPrefs.GetString(GetKeyClone(PREFS_MAIN_COLOR)), out var color))
            {
                return color;
            }

            return new Color(1, 0.5621365f, 0, 1);
        }

        set => PlayerPrefs.SetString(GetKeyClone(PREFS_MAIN_COLOR), '#' + ColorUtility.ToHtmlStringRGBA(value));
    }

    public static Color subColor
    {
        get
        {
            if (ColorUtility.TryParseHtmlString(PlayerPrefs.GetString(GetKeyClone(PREFS_SUB_COLOR)), out var color))
            {
                return color;
            }

            return new Color(0.9254902f, 0.3616368f, 0, 1);
        }

        set => PlayerPrefs.SetString(GetKeyClone(PREFS_SUB_COLOR), '#' + ColorUtility.ToHtmlStringRGBA(value));
    }

    private static string GetKeyClone(string key)
    {
        if (!ClonesManager.IsClone()) return key;
        string customArgument = ClonesManager.GetArgument();
        return $"{customArgument}:{key}";
    }
}