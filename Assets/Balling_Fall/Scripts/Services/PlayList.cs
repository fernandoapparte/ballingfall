using UnityEngine;
using System.Collections;
using System;

public class PlayList : MonoBehaviour
{
    public static PlayList Instance { get; private set; }


    public enum PlayingState
    {
        Playing,
        Paused,
        Stopped
    }
    
    private const string MUTE_PREF_KEY = "MutePreference";
    private const int MUTED = 1;
    private const int UN_MUTED = 0;
    public const string MUSIC_PREF_KEY = "MusicPreference";
    private const int MUSIC_OFF = 0;
    public const int MUSIC_ON = 1;
    public PlayingState musicState;
	public PlayingState lastPlayingState; //used to remember state when showing ad reward
	private float customDelay=0;
    public bool firstTime = true;
    public int currClipPlayList = 0;
    public ScriptableObject audioClipSequence;
    private SequencePlayList SPL { set; get; }

    private AudioSource audioPlayList;
    public AudioSource AudioSource
    {
        get
        {
            if (audioPlayList == null)
            {
                audioPlayList = GetComponent<AudioSource>();
            }

            return audioPlayList;
        }
    }

    //Control the music in some specific cases
    private void OnEnable()
    {
        GameManager.GameStateChanged += GameManager_GameStateChanged;
    }
    
    private void OnDisable()
    {
        GameManager.GameStateChanged -= GameManager_GameStateChanged;
    }

    private void GameManager_GameStateChanged(GameState obj)
    {
        if (obj==GameState.Playing)
        {
            RestartMusic(0.5f);
        }

        if (obj== GameState.PassLevel)
        {
            Debug.Log("PAUSING MUSIC!");
            PauseMusic();
        }

		//Stop music when a video reward is about to start.
		if (obj == GameState.Revive) {
			StopMusic ();
		}

    }


    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        if (!audioClipSequence.GetType().Equals(typeof(SequencePlayList)))
        {
            Debug.LogError("Incorrect Scriptable Object on Audio Clip Sequence!!!");
        }
        SPL = (SequencePlayList)audioClipSequence;

        audioPlayList = GetComponent<AudioSource>();
        audioPlayList.loop = false;
        PlayerPrefs.SetInt(MUSIC_PREF_KEY, MUSIC_ON);
        InitMusic (0.5f);

		Debug.Log ("Start PlayList");
    }

	public void InitMusic (float delay=0)
	{
        if (IsMusicOff()) return;
		customDelay = delay;
		musicState = PlayingState.Stopped;
		audioPlayList.Stop ();
		firstTime = true;
		currClipPlayList = 0;
		audioPlayList.clip = SPL.clipsPlayList[SPL.seqPlayList[0]];
		musicState = PlayingState.Playing;
	}

    // Update is called once per frame
    void Update()
    {
        if (audioPlayList.isPlaying) return;
        if (musicState == PlayingState.Paused) return;
        if (musicState == PlayingState.Stopped) return;
        GetNextClip(); //increase currClipPlay
        audioPlayList.clip = SPL.clipsPlayList[SPL.seqPlayList[currClipPlayList]];
		if (currClipPlayList == 0) {
			audioPlayList.PlayDelayed (customDelay);
		} else {
			audioPlayList.Play();
		}
    }

    private void GetNextClip()
    {
        if (firstTime) {
            firstTime = false;
            currClipPlayList= 0;
        } else 
        {
            currClipPlayList = (currClipPlayList + 1) % SPL.seqPlayList.Length;
            if (currClipPlayList == 0) {
                currClipPlayList= 1;
            } 
        }
     }


	void OnDestroy()
	{
		if (Instance == this)
		{
			Instance = null;
		}
	}

    public void PauseMusic()
    {
			//if (IsMusicOff()) return;
            musicState = PlayingState.Paused;
            Instance.audioPlayList.Pause();
    }

    /// <summary>
    /// Resumes the music.
    /// </summary>
    public void ResumeMusicFromPause()
    {
        if (IsMusicOff()) return;
        if (musicState == PlayingState.Paused)
        {
            musicState = PlayingState.Playing;
            Instance.audioPlayList.UnPause();
 
        }
    }

	public void RestartMusic(float delay=0)
    {
        if (IsMusicOff()) return;
        switch (musicState)
        {
            case PlayingState.Paused:
                ResumeMusicFromPause();
                break;

            case PlayingState.Stopped:
				
                break;

            case PlayingState.Playing:
                break;
        }
    }

	public void StartMusic(float delay=0)
	{
		if (IsMusicOff())return;

		if (musicState == PlayingState.Stopped)
		{
			musicState = PlayingState.Playing;
			Instance.audioPlayList.PlayDelayed (delay);
			return;
		}
	}

    /// <summary>
    /// Stop music.
    /// </summary>
    public void StopMusic()
    {
		if (IsMusicOff())return;

        Instance.audioPlayList.Stop();
        musicState = PlayingState.Stopped;
    }


    public void ToggleMusic()
    {
        if (IsMusicOff())
        {
            // Turn music ON
            PlayerPrefs.SetInt(MUSIC_PREF_KEY, MUSIC_ON);
            if (musicState == PlayingState.Paused)
            {
                ResumeMusicFromPause();
            }

        }
        else
        {
            // Turn music OFF
            PlayerPrefs.SetInt(MUSIC_PREF_KEY, MUSIC_OFF);
            if (musicState == PlayingState.Playing)
            {
                PauseMusic();
            }
        }
    }

    public bool IsMusicOff()
    {
        return (PlayerPrefs.GetInt(MUSIC_PREF_KEY, MUSIC_ON) == MUSIC_OFF);
    }
}
