using UnityEngine;

using System.Collections;
using System.Collections.Generic;


public class AudioManager : MonoBehaviour
{

    private static AudioManager instance;

    private List<AudioSource> avaiableSourceList = new List<AudioSource>();

    public float volume = .3f;

    void Awake()
    {
        instance = this;


        CreateMoveIdleAudioSource();
    }


    public static void SetVolume(float val) { instance._SetVolume(val); }
    public void _SetVolume(float val)
    {
        volume = val;
        foreach (var item in avaiableSourceList)
            item.volume = volume;
    }

    public static void PlaySound(AudioClip clip, bool loop = false)
    {
        if (instance == null) return;
        instance._PlaySound(clip, loop);
    }
    public void _PlaySound(AudioClip clip, bool loop = false)
    {
        if (clip == null) return;
        AudioSource audio = GetAvaiableAudioSourceIndex();

        audio.clip = clip;
        audio.Play();
    }

    private AudioSource IntantiateSource()
    {
        GameObject obj = new GameObject();
        obj.name = "AudioSource" + (avaiableSourceList.Count);

        AudioSource src = obj.AddComponent<AudioSource>();
        src.playOnAwake = false;
        src.loop = false;
        src.spatialBlend = 0;
        src.volume = volume;

        obj.transform.parent = transform;
        obj.transform.localPosition = Vector3.zero;

        avaiableSourceList.Add(src);

        return src;
    }


    private AudioSource GetAvaiableAudioSourceIndex()
    {
        for (int i = 0; i < avaiableSourceList.Count; i++)
        {
            if (!avaiableSourceList[i].isPlaying) return avaiableSourceList[i];
        }
        return IntantiateSource(); 
    }


    void OnEnable()
    {
        TDS.onEndGame += OnGameOver;

        TDS.onPlayerDamagedE += OnPlayerDamaged;
        TDS.onPlayerDestroyedE += OnPlayerDestroyed;
        TDS.onPlayerRespawnedE += OnPlayerRespawned;

        TDS.onFireFailE += OnFireFail;
        TDS.onFireAltFailE += OnFireAltFail;
        TDS.onAbilityActivationFailE += OnAbilityActivationFail;

        TDS.onPlayerLevelUpE += OnPlayerLevelUp;
        TDS.onPerkPurchasedE += OnPerkPurchasedUp;
    }
    void OnDisable()
    {
        TDS.onEndGame -= OnGameOver;

        TDS.onPlayerDamagedE -= OnPlayerDamaged;
        TDS.onPlayerDestroyedE -= OnPlayerDestroyed;
        TDS.onPlayerRespawnedE -= OnPlayerRespawned;

        TDS.onFireFailE -= OnFireFail;
        TDS.onFireAltFailE -= OnFireAltFail;
        TDS.onAbilityActivationFailE -= OnAbilityActivationFail;

        TDS.onPlayerLevelUpE -= OnPlayerLevelUp;
        TDS.onPerkPurchasedE -= OnPerkPurchasedUp;
    }

    public AudioClip gameWinAudio;
    public AudioClip gameLostAudio;
    void OnGameOver(bool win)
    {
        PlaySound(win ? gameWinAudio : gameLostAudio);
    }

    public AudioClip objectiveCompletedClip;
    void OnObjectiveCompleted() { PlaySound(objectiveCompletedClip); }

    public AudioClip timeWarningClip;
    void OnTimeWarning() { PlaySound(timeWarningClip); }

    public AudioClip timesUpClip;
    void OnTimeUp() { PlaySound(timesUpClip); }

    public AudioClip playerDamagedClip;
    void OnPlayerDamaged(float dmg) { PlaySound(playerDamagedClip); }

    public AudioClip playerDestroyedClip;
    void OnPlayerDestroyed() { PlaySound(playerDestroyedClip); }

    public AudioClip playerRespawnedClip;
    void OnPlayerRespawned() { PlaySound(playerRespawnedClip); }


    public AudioClip fireFailedClip;
    void OnFireFail(string msg) { PlaySound(fireFailedClip); }

    public AudioClip fireAltFailedClip;
    void OnFireAltFail(string msg) { PlaySound(fireAltFailedClip); }

    public AudioClip fireAbilityFailedClip;
    void OnAbilityActivationFail(string msg) { PlaySound(fireAbilityFailedClip); }


    public AudioClip playerLevelUpClip;
    void OnPlayerLevelUp(UnitPlayer player) { PlaySound(playerLevelUpClip); }

    public AudioClip perkPurchasedClip;
    void OnPerkPurchasedUp(Perk perk) { PlaySound(perkPurchasedClip); }





    public bool playMoveIdleSound = false;
    public float moveIdleVolume = 0.5f;

    public AudioClip playerMoveClip;

    public AudioClip playerIdleClip;

    private UnitPlayer player;
    private AudioSource moveIdleAudioSource;

    void Update()
    {
        player = GameControl.GetPlayer();

        if (player != null && playMoveIdleSound)
        {
            if (player.GetVelocity() > 0.15f)
            {
                if (playerMoveClip != null)
                {
                    if (moveIdleAudioSource.clip != playerMoveClip)
                    {
                        moveIdleAudioSource.clip = playerMoveClip;
                        moveIdleAudioSource.Play();
                    }
                    else
                    {
                        if (!moveIdleAudioSource.isPlaying) moveIdleAudioSource.Play();
                    }
                }
                else if (moveIdleAudioSource.isPlaying) moveIdleAudioSource.Stop();
            }
            else
            {
                if (playerIdleClip != null)
                {
                    if (moveIdleAudioSource.clip != playerIdleClip)
                    {
                        moveIdleAudioSource.clip = playerIdleClip;
                        moveIdleAudioSource.Play();
                    }
                    else
                    {
                        if (!moveIdleAudioSource.isPlaying) moveIdleAudioSource.Play();
                    }
                }
                else
                {
                    if (moveIdleAudioSource.isPlaying) moveIdleAudioSource.Stop();
                }
            }
        }
    }

    private void CreateMoveIdleAudioSource()
    {
        if (moveIdleAudioSource != null) return;

        GameObject mObj = new GameObject();
        mObj.name = "moveIdleAudioSource";
        mObj.transform.parent = transform;
        mObj.transform.localPosition = Vector3.zero;

        moveIdleAudioSource = mObj.AddComponent<AudioSource>();
        moveIdleAudioSource.playOnAwake = false;
        moveIdleAudioSource.loop = true;
        moveIdleAudioSource.spatialBlend = 0;
        moveIdleAudioSource.volume = volume * moveIdleVolume;
    }

}
