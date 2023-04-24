using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DefaultNamespace;
using pvp;
using UI;
using UnityEngine;

public class PvPManager : MonoBehaviour
{
    public static PvPManager instance;
    [SerializeField] private TDSArea[] areas;
    [SerializeField] private GameObject spawnUnitObj;
    [SerializeField] private GameObject spawnCollectibleObj;
    [SerializeField] private GameObject destructible;
    [SerializeField] private UIHUD uihud;
    private UnitList unitList = new UnitList();

    private Dictionary<string, UnitPlayer> otherPlayers = new Dictionary<string, UnitPlayer>();
    private Dictionary<int, UnitAI> enemies = new Dictionary<int, UnitAI>();

    private NetworkManager _networkManager;

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

        _networkManager = NetworkManager.Instance;

        var myPlayer = SpawnPlayer(SpawnAtIndex(PvP.GetLandSpawnPlayer()), Quaternion.identity);
        GameControl.SetPlayer(myPlayer);

        _networkManager.Manager.Socket.On<PlayerInit>("other player play", OnPlayerJoin);
    }

    void Start()
    {
        var player = GameControl.GetPlayer();
        if (PvP.GetLandSpawnPlayer() == 0)
        {
            spawnUnitObj.SetActive(true);
            spawnCollectibleObj.SetActive(true);
            unitList.roomId = PvP.GetRoom();
            StartCoroutine(PostEnemyPosition());
        }
        else
        {
           _networkManager.Manager.Socket.On<UnitInit>("OnSpawnUnit", OnSpawnUnit);
           _networkManager.Manager.Socket.On<UnitList>("OnEnemyMove", OnEnemyMove);
        }
       _networkManager.Manager.Socket
            .Emit("player play", new PlayerInit
            {
                username = PlayerPrefsManager.Username,
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

       _networkManager.Manager.Socket.On<Player>("other player move", OnOtherPlayerMove);
       _networkManager.Manager.Socket.On<PlayerFire>("otherPlayerFire", OnOtherPlayerFire);
       _networkManager.Manager.Socket.On<UnitHealth>("OnUnitHealthChange", OnUnitHealthChange);
       _networkManager.Manager.Socket.On<ClearUnit>("OnUnitClear", OnUnitClear);
       _networkManager.Manager.Socket.On<CollectibleInit>("OnSpawnCollectible", OnSpawnCollectible);
       _networkManager.Manager.Socket.On<AttackPlayer>("OnAttackPlayer", OnAttackPlayer);
       _networkManager.Manager.Socket.On<PlayerDestroy>("OnPlayerDestroy", OnPlayerDestroy);
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
        otherPlayers.Add(playerData.username, _otherPlayer);
        uihud.UpdateOpponentName(playerData.username);
    }

    private void OnOtherPlayerMove(Player playerData)
    {
        Vector3 postion = new Vector3(playerData.position[0], playerData.position[1], playerData.position[2]);
        Quaternion rotation = Quaternion.Euler(playerData.rotation[0], playerData.rotation[1], playerData.rotation[2]);
        otherPlayers[playerData.username].transform.position = postion;
        otherPlayers[playerData.username].transform.rotation = rotation;
        uihud.UpdateSliderHPOpponent(playerData.hp, playerData.hpfull);
        otherPlayers[playerData.username].turretObj.rotation = Quaternion.Euler(playerData.turretRotation[0], playerData.turretRotation[1], playerData.turretRotation[2]);
    }

    private void OnSpawnUnit(UnitInit unitData)
    {
        Vector3 position = new Vector3(unitData.position[0], unitData.position[1], unitData.position[2]);
        Quaternion rotation = Quaternion.Euler(unitData.rotation[0], unitData.rotation[1], unitData.rotation[2]);
        UnitAI unitObj = UnitAI_DB.GetUnitAI(unitData.prefabId).GetPoolItem<UnitAI>(position, rotation);
        unitObj.gameObject.layer = TDS.GetLayerAIUnit();
        unitObj.gameObject.name = unitData.name;

        unitObj.hitPointFull = unitData.hitPointFull;
        unitObj.instanceID = unitData.instanceId;

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
           _networkManager.Manager.Socket
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
        otherPlayers[data.username].turretObj.transform.rotation = MyExtension.ConvertToQuaternion(data.turretRotation);
        otherPlayers[data.username].OnFireWeapon();
    }

    private void OnUnitHealthChange(UnitHealth health)
    {
        if (health.isEnemy)
        {
            enemies[health.instanceID].hitPoint = health.hitPoint;
        }
        else
        {
            Transform obj = destructible.transform.Find(health.name);
            if (obj.gameObject.TryGetComponent<Unit>(out var component))
            {
                component.hitPoint = health.hitPoint;
            }
        }
    }

    private void OnUnitClear(ClearUnit clear)
    {
        if (clear.isEnemy)
        {
            enemies[clear.instanceID].OnPvPClearUnit();
        }
        else
        {
            Transform obj = destructible.transform.Find(clear.name);
            if (obj.gameObject.TryGetComponent<Unit>(out var component))
            {
                component.OnPvPClearUnit();
            }
        }
    }

    private void OnSpawnCollectible(CollectibleInit data)
    {
        var obj = Collectible_DB.GetCollectibleAtIndex(data.collectibleIndex);
        obj.GetPoolItem<Collectible>(MyExtension.ConvertToVector3(data.position), Quaternion.identity);
    }

    private void OnAttackPlayer(AttackPlayer attackData)
    {
        if (attackData.username == PlayerPrefsManager.Username)
            GameControl.GetPlayer().ApplyAttack(attackData.attackInstance);
    }

    private void OnPlayerDestroy(PlayerDestroy data)
    {

        otherPlayers[data.username].OnPvPClearUnit();
        otherPlayers.Remove(data.username);
        if (otherPlayers.Count <= 0)
            GameControl.GameOver(true);
    }

    public string GetIdOtherPlayer(UnitPlayer player)
    {
        return otherPlayers.FirstOrDefault(x => x.Value == player).Key;
    }

    public Unit GetPlayerNearest(Vector3 position)
    {
        Unit u = GameControl.GetPlayer();
        float currentDistance = Vector3.Distance(position, u.transform.position);
        foreach(var kvp in otherPlayers)
        {
            var newDistance = Vector3.Distance(position, kvp.Value.transform.position);
            if (newDistance < currentDistance)
            {
                currentDistance = newDistance;
                u = kvp.Value;
            }
        }
        return u;
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
