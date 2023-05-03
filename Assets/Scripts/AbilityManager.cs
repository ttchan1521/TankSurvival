using UnityEngine;

using System.Collections;
using System.Collections.Generic;


public class AbilityManager : MonoBehaviour
{

    private Ability ability;
    public static Ability GetAbility()
    {
        if (instance.ability == null)
            SetupAbility();
        return instance.ability;
    }

    private static AbilityManager instance;
    public static AbilityManager GetInstance() { return instance; }


    void Awake()
    {

        if (instance != null)
        {
            Destroy(this);
            return;
        }

        instance = this;
    }


    public static void SetupAbility()
    {
        if (instance != null) instance._SetupAbility();
    }
    public void _SetupAbility()
    {
        ability = Ability_DB.CloneItem(PlayerPrefsManager.abilitySelectID);

    }


    public static void LaunchAbility() { instance._LaunchAbility(); }
    public void _LaunchAbility()
    {
        if (instance == null) return;

        string status = ability.IsReady();

        if (status != "")
        {
            TDS.AbilityActivationFailFail(status);
            return;
        }

        LaunchAbility(ability);
    }

    public static void LaunchAbility(Ability _ability)
    {
        bool teleport = _ability.type == _AbilityType.Movement & _ability.moveType == _MoveType.Teleport;
        if (_ability.type == _AbilityType.AOE || teleport)
        {
            //vị trí bấm
            Ray ray = CameraControl.GetMainCamera().ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity)) _ability.Activate(hit.point);
            else _ability.Activate(GameControl.GetPlayer().thisT.position); //vị trí không hợp lệ
        }
        else
        {
            //activate on player position
            _ability.Activate(GameControl.GetPlayer().thisT.position);
        }
    }



    void Update()
    {
        if (!GameControl.EnableAbility() || ability == null) return;

        ability.currentCD -= Time.deltaTime;

    }




    public static Ability GetSelectedAbility()
    {

        return instance.ability;
    }


}
