using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DefaultNamespace;
using pvp;
using UnityEngine;

public class PvPManager : MonoBehaviour
{
    public static PvPManager instance;
    [SerializeField] private TDSArea[] areas;
    [SerializeField] private GameObject spawnUnitObj;
    private UnitList unitList = new UnitList();

    private Dictionary<string, UnitPlayer> otherPlayers = new Dictionary<string, UnitPlayer>();
    private Dictionary<int, UnitAI> enemies = new Dictionary<int, UnitAI>();

    private Vector3 SpawnAtIndex(int index)
    {
        if (index >= areas.Length)
            return new Vector3(0, 0, 0);
        return areas[index].GetRandomPosition();
    }
    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        var myPlayer = SpawnPlayer(SpawnAtIndex(PvP.GetLandSpawnPlayer()), Quaternion.identity);
        GameControl.SetPlayer(myPlayer);

        NetworkManager.Instance.Manager.Socket.On<PlayerInit>("other player play", OnPlayerJoin);
    }

    void Start()
    {
        var player = GameControl.GetPlayer();
        if (PvP.GetLandSpawnPlayer() == 0)
        {
            spawnUnitObj.SetActive(true);
            unitList.roomId = PvP.GetRoom();
            StartCoroutine(PostEnemyPosition());
        }
        else
        {
            NetworkManager.Instance.Manager.Socket.On<UnitInit>("OnSpawnUnit", OnSpawnUnit);
            NetworkManager.Instance.Manager.Socket.On<UnitList>("OnEnemyMove", OnEnemyMove);
        }
        NetworkManager.Instance.Manager.Socket
            .Emit("player play", new PlayerInit
            {
                socketId = NetworkManager.Instance.Manager.Socket.Id,
                roomId = PvP.GetRoom(),
                position = new float[]
                {
                    player.transform.position.x,
                    player.transform.position.y,
                    player.transform.position.z
                },
                rotation = new float[]
                {
                    player.transform.rotation.eulerAngles.x,
                    player.transform.rotation.eulerAngles.y,
                    player.transform.rotation.eulerAngles.z
                },
                mainColor = "#" + ColorUtility.ToHtmlStringRGBA(PlayerPrefsManager.mainColor),
                subColor = "#" + ColorUtility.ToHtmlStringRGBA(PlayerPrefsManager.subColor),
                weaponId = PlayerPrefsManager.weaponSelectID
            });

        NetworkManager.Instance.Manager.Socket.On<Player>("other player move", OnOtherPlayerMove);
        NetworkManager.Instance.Manager.Socket.On<PlayerFire>("otherPlayerFire", OnOtherPlayerFire);
    }

    private UnitPlayer SpawnPlayer(Vector3 position, Quaternion rotation)
    {
        var _player = Instantiate(Resources.Load<GameObject>("Player/Player_Arena"), position, Quaternion.identity, null);
        return _player.GetComponent<UnitPlayer>();
    }

    private void OnPlayerJoin(PlayerInit playerData)
    {
        Vector3 postion = new Vector3(playerData.position[0], playerData.position[1], playerData.position[2]);
        Quaternion rotation = Quaternion.Euler(playerData.rotation[0], playerData.rotation[1], playerData.rotation[2]);
        var _otherPlayer = SpawnPlayer(postion, rotation).GetComponent<UnitPlayer>();
        if (ColorUtility.TryParseHtmlString(playerData.mainColor, out Color main) &&
            ColorUtility.TryParseHtmlString(playerData.subColor, out Color sub))
        {
            _otherPlayer.SetRendererColor(main, sub);
        }
        _otherPlayer.Init(playerData.weaponId);
        otherPlayers.Add(playerData.socketId, _otherPlayer);
    }

    private void OnOtherPlayerMove(Player playerData)
    {
        Vector3 postion = new Vector3(playerData.position[0], playerData.position[1], playerData.position[2]);
        Quaternion rotation = Quaternion.Euler(playerData.rotation[0], playerData.rotation[1], playerData.rotation[2]);
        otherPlayers[playerData.socketId].transform.position = postion;
        otherPlayers[playerData.socketId].transform.rotation = rotation;
        otherPlayers[playerData.socketId].turretObj.rotation = Quaternion.Euler(playerData.turretRotation[0], playerData.turretRotation[1], playerData.turretRotation[2]);
    }

    private void OnSpawnUnit(UnitInit unitData)
    {
        Vector3 position = new Vector3(unitData.position[0], unitData.position[1], unitData.position[2]);
        Quaternion rotation = Quaternion.Euler(unitData.rotation[0], unitData.rotation[1], unitData.rotation[2]);
        UnitAI unitObj = UnitAI_DB.GetUnitAI(unitData.prefabId).GetPoolItem<UnitAI>(position, rotation);
        unitObj.gameObject.layer = TDS.GetLayerAIUnit();
        unitObj.gameObject.name = unitData.name;

        unitObj.hitPointFull = unitData.hitPointFull;

        enemies.Add(unitData.instanceId, unitObj);
    }

    IEnumerator PostEnemyPosition()
    {
        while (!GameControl.IsGameOver())
        {
            if (unitList == null || unitList.units.Length <= 0)
                yield return null;
            foreach (var e in unitList.units)
            {
                if (enemies[e.instanceID] == null)
                    continue;
                var trans = enemies[e.instanceID].transform;
                // e.position = MyExtension.ConvertToArrayFromVector3(enemies[e.instanceID].transform.position);
                // e.rotation = MyExtension.ConvertToArrayFromQuaternion(enemies[e.instanceID].transform.rotation);
                e.SetPosition(trans.position.x, trans.position.y, trans.position.z);
                e.SetRotation(trans.rotation.eulerAngles.x, trans.rotation.eulerAngles.y, trans.rotation.eulerAngles.z);
            }
            NetworkManager.Instance.Manager.Socket
                .Emit("enemyMove", unitList);
            yield return null;
        }
    }

    private void OnEnemyMove(UnitList unitList)
    {
        foreach (var e in unitList.units)
        {
            if (enemies[e.instanceID] == null)
                continue;
            var trans = enemies[e.instanceID].transform;
            trans.position = MyExtension.ConvertToVector3(e.position);
            trans.rotation = MyExtension.ConvertToQuaternion(e.rotation);
        }
    }

    public void AddEnemiesData(UnitInit unit, UnitAI aiObject)
    {
        var unitData = new UnitData
        {
            instanceID = unit.instanceId,
            position = new float[] {0, 0, 0},
            rotation = new float[] {0, 0, 0}
        };
        enemies.Add(unit.instanceId, aiObject);
        var list = unitList.units.ToList();
        list.Add(unitData);
        unitList.units = list.ToArray();
    }
    
    private void OnOtherPlayerFire(PlayerFire data)
    {
        otherPlayers[data.socketId].turretObj.transform.rotation = MyExtension.ConvertToQuaternion(data.turretRotation);
        otherPlayers[data.socketId].OnFireWeapon();
    }

    void OnDrawGizmos()
    {
        for (int i = 0; i < areas.Length; i++)
        {
            if (areas[i] == null) continue;
            Gizmos.DrawLine(transform.position, areas[i].GetPos());
            areas[i].gizmoColor = new Color(1, 0, 0.5f, 1);
        }
    }
}
