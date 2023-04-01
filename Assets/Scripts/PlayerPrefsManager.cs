using System.Collections;
using System.Collections.Generic;
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
            Color _color = new Color();
            _color.a = PlayerPrefs.GetFloat(PREFS_MAIN_COLOR + "a", 1);
            _color.r = PlayerPrefs.GetFloat(PREFS_MAIN_COLOR + "r", 1);
            _color.g = PlayerPrefs.GetFloat(PREFS_MAIN_COLOR + "g", 0.5621365f);
            _color.b = PlayerPrefs.GetFloat(PREFS_MAIN_COLOR + "b", 0);
            return _color;
        }

        set
        {
            PlayerPrefs.SetFloat(PREFS_MAIN_COLOR + "a", value.a);
            PlayerPrefs.SetFloat(PREFS_MAIN_COLOR + "r", value.r);
            PlayerPrefs.SetFloat(PREFS_MAIN_COLOR + "g", value.g);
            PlayerPrefs.SetFloat(PREFS_MAIN_COLOR + "b", value.b);
        }
    }

    public static Color subColor
    {
        get
        {
            Color _color = new Color();
            _color.a = PlayerPrefs.GetFloat(PREFS_SUB_COLOR + "a", 1);
            _color.r = PlayerPrefs.GetFloat(PREFS_SUB_COLOR + "r", 0.9254902f);
            _color.g = PlayerPrefs.GetFloat(PREFS_SUB_COLOR + "g", 0.3616368f);
            _color.b = PlayerPrefs.GetFloat(PREFS_SUB_COLOR + "b", 0);
            return _color;
        }

        set
        {
            PlayerPrefs.SetFloat(PREFS_SUB_COLOR + "a", value.a);
            PlayerPrefs.SetFloat(PREFS_SUB_COLOR + "r", value.r);
            PlayerPrefs.SetFloat(PREFS_SUB_COLOR + "g", value.g);
            PlayerPrefs.SetFloat(PREFS_SUB_COLOR + "b", value.b);
        }
    }
}
