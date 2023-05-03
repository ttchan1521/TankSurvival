//the main component that control the general game logic

using UnityEngine;

#if UNITY_5_3_OR_NEWER
using UnityEngine.SceneManagement;
#endif

using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using pvp;
using Users.Entities;
using BestHTTP;

public enum _GameState
{
    Playing,
    Paused,
    GameOver,
}

public class GameControl : MonoBehaviour
{

    private static GameControl instance;
    public static GameControl GetInstance() { return instance; }

    private _GameState gameState = _GameState.Playing;


    public bool enableTimer = false;
    public float timerDuration = 0;
    private bool timesUp = false;
    public bool pvp = false;

    [HideInInspector] public float remainingDuration = 0;

    public static bool EnableTimer() { return instance != null ? instance.enableTimer : false; }
    public static bool TimesUp() { return instance != null ? instance.timesUp : false; }
    public static float GetRemainingDuration() { return instance != null ? instance.remainingDuration : 0; }



    [HideInInspector] public int score = 0;
    public static int GetScore() { return instance.score; }
    public static void GainScore(int value)
    {
        instance.score += (int)Mathf.Round(value * GetPlayer().GetScoreMultiplier());
        if (instance.objective != null) instance.objective.GainScore();
    }

    public static void ColletibleCollected(Collectible item) { if (instance != null && instance.objective != null) instance.objective.ColletibleCollected(item); }


    private UnitPlayer player;
    public static UnitPlayer GetPlayer() { return instance == null ? null : instance.player; }
    public static void SetPlayer(UnitPlayer newPlayer) { if (instance != null) instance.player = newPlayer; }
    public bool enableAbility = false;
    public static bool EnableAbility() { return instance.enableAbility; }

    public bool enableAltFire = false;
    public static bool EnableAltFire() { return instance.enableAltFire; }



    public bool enableAutoReload = true;
    public static bool EnableAutoReload() { return instance.enableAutoReload; }


    [Header("ShootObject hits")]
    public bool friendly = false;
    public bool shootObject = true;
    public bool collectible = false;
    public static bool SOHitFriendly() { return instance.friendly; }
    public static bool SOHitShootObject() { return instance.shootObject; }
    public static bool SOHitCollectible() { return instance.collectible; }


    [Header("Level Objective")]
    public ObjectiveTracker objective;




    public static void UnitSpawnerCleared(UnitSpawner spawner) { if (instance != null) instance._UnitSpawnerCleared(spawner); }
    public void _UnitSpawnerCleared(UnitSpawner spawner)
    {
        if (objective != null) objective.SpawnerCleared(spawner);
    }
  
    public static void UnitDestroyed(Unit unit) { if (instance != null) instance._UnitDestroyed(unit); }
    public void _UnitDestroyed(Unit unit)
    {
        if (objective != null) objective.UnitDestroyed(unit);
    }


    public int playerLife = 0;
    public static int GetPlayerLife() { return instance.playerLife; }
    public static void GainLife() { instance.playerLife += 1; }


    public Transform startPoint;
    private Vector3 respawnPoint;
    public static void SetRespawnPoint(Vector3 pos)
    {
        instance.respawnPoint = pos;
    }


    private bool respawning = false;
    public static void PlayerDestroyed() { instance._PlayerDestroyed(); }
    public void _PlayerDestroyed()
    {
        TDS.PlayerDestroyed();

        if (respawning) return;


        playerLife -= 1;

        if (playerLife <= 0)
        {   
            if (pvp)
            {
                NetworkManager.Instance.Manager.Socket
                    .Emit("playerDestroy", new PlayerDestroy
                    {
                        username = PlayerPrefsManager.Username,
                        roomId = PvP.GetRoom()
                    });
            }
            GameOver(false);
            return;
        }

        respawning = true;


        GameObject obj = (GameObject)Instantiate(player.gameObject, player.thisT.position, player.thisT.rotation);
        player = obj.GetComponent<UnitPlayer>();
        player.hitPoint = player.GetFullHitPoint();
        player.Start();
        player.ClearAllEffect();

        //đợi một thời gian rồi respawn
        obj.SetActive(false);

        StartCoroutine(ActivateRepawnPlayer());
    }
    IEnumerator ActivateRepawnPlayer()
    {
        yield return new WaitForSeconds(1);

        player.thisT.position = respawnPoint;
        player.thisObj.SetActive(true);

        respawning = false;

        TDS.PlayerRespawned();
    }


    private int unitInstanceID = 0;
    public static int GetUnitInstanceID()
    {
        return instance == null ? -1 : instance.unitInstanceID += 1;
    }



    public string mainMenu = "";
    public string nextScene = "";
    public static void LoadMainMenu() { LoadScene(instance.mainMenu); }
    public static void LoadNextScene() { LoadScene(instance.nextScene); }
    public static void RestartScene()
    {
#if UNITY_5_3_OR_NEWER
        LoadScene(SceneManager.GetActiveScene().name);
#else
				LoadScene(Application.loadedLevelName);
#endif
    }
    public static void LoadScene(string sceneName)
    {
        if (sceneName == "") return;
        Time.timeScale = 1;

#if UNITY_5_3_OR_NEWER
        SceneManager.LoadScene(sceneName);
#else
				Application.LoadLevel(sceneName);
#endif
    }


    void Awake()
    {
        instance = this;


        if (pvp)
        {

        }
        else
        {
            player = (UnitPlayer)FindObjectOfType(typeof(UnitPlayer));
        }

        Physics.IgnoreLayerCollision(TDS.GetLayerShootObject(), TDS.GetLayerShootObject(), !shootObject);
        Physics.IgnoreLayerCollision(TDS.GetLayerShootObject(), TDS.GetLayerCollectible(), !collectible);

        Physics.IgnoreLayerCollision(TDS.GetLayerShootObject(), TDS.GetLayerTerrain(), true);
        Physics.IgnoreLayerCollision(TDS.GetLayerShootObject(), TDS.GetLayerTrigger(), true);

        UnitTracker.Clear();
        UnitSpawnerTracker.Clear();

    }
    void OnDestroy()
    {
        UnitTracker.Clear();
        UnitSpawnerTracker.Clear();
    }

    void Start()
    {

        if (enableTimer) StartCoroutine(TimerCountDown());


        if (startPoint != null) SetRespawnPoint(startPoint.position);
        else SetRespawnPoint(player.thisT.position);
    }


    IEnumerator TimerCountDown()
    {
        remainingDuration = timerDuration;

        while (remainingDuration > 0)
        {
            while (!IsGamePlaying()) yield return null; 
            remainingDuration -= Time.deltaTime;
            yield return null;
        }
        timesUp = true;
        objective.CheckObjectiveComplete();
    }


    private bool gameOver = false;
    public static void GameOver(bool won)
    {   
        if (!instance.gameObject.activeInHierarchy) return;
        instance.StartCoroutine(instance._GameOver(won));

        if (string.IsNullOrEmpty(PlayerPrefsManager.UserId))
            return;

        if (instance.pvp && !won) return;

        var scoreInfo = new UpdateScore
        {
            userId = PlayerPrefsManager.UserId,
            op = "incr"
        };
        if (instance.pvp)
        {
            scoreInfo.score = 25;
            scoreInfo.mode = "pvp";
        }
        else
        {
            scoreInfo.score = GetScore();
            scoreInfo.mode = "campaign";
        }

        HTTPRequest request = new HTTPRequest(new System.Uri($"{NetworkManager.UriString}/users"), HTTPMethods.Patch,
            OnRequestFinished);
        request.SetHeader("Content-Type", "application/json; charset=UTF-8");
        request.RawData = System.Text.Encoding.UTF8.GetBytes(JsonUtility.ToJson(scoreInfo));
        request.Send();
    }

    private static void OnRequestFinished(HTTPRequest req, HTTPResponse res)
    {
        
    }
    public IEnumerator _GameOver(bool won)
    {
        if (gameOver) yield break; //stop
        gameOver = true;

        Debug.Log("game over - " + (won ? "win" : "lost"));

        yield return new WaitForSeconds(1.5f);
        gameState = _GameState.GameOver;
        TDS.EndGame(won);
    }


    public static bool IsGamePlaying() { return instance == null ? true : (instance.gameState == _GameState.Playing ? true : false); }
    public static bool IsGamePaused() { return instance == null ? true : (instance.gameState == _GameState.Paused ? true : false); }
    public static bool IsGameOver() { return instance == null ? true : (instance.gameState == _GameState.GameOver ? true : false); }

    public static void PauseGame() { instance.gameState = _GameState.Paused; }
    public static void ResumeGame() { instance.gameState = _GameState.Playing; }

}
