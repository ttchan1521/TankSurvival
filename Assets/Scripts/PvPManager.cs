using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using pvp;
using UnityEngine;

public class PvPManager : MonoBehaviour
{
    public static PvPManager instance;
    [SerializeField] private TDSArea[] areas;

    private Dictionary<string, UnitPlayer> otherPlayers = new Dictionary<string, UnitPlayer>();

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
                subColor = "#" + ColorUtility.ToHtmlStringRGBA(PlayerPrefsManager.subColor)
            });
        
        NetworkManager.Instance.Manager.Socket.On<Player>("other player move", OnOtherPlayerMove);
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
