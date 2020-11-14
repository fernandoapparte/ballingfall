using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BallingFallGame;
using UnityEngine.SceneManagement;
using System.IO;

public enum GameState
{
    Prepare,Playing,Pause,Revive,PassLevel,GameOver,
}

public class GameManager : MonoBehaviour
{

    public static GameManager Instance { private set; get; }
    public static event System.Action<GameState> GameStateChanged = delegate { };
    public static int CurrentLevel { private set; get; }
    private const string LevelReached_PPK = "LevelReached";
    public static bool IsRestart { private set; get; }

    //Make GameManager Listen to the PlayerController Events.
    //This require a live instance of PlayerController, otherwise won't work.

    private void OnEnable()
    {
        PlayerController.PlayerStateChanged += PlayerController_PlayerStateChanged;
    }
    private void OnDisable()
    {
        PlayerController.PlayerStateChanged -= PlayerController_PlayerStateChanged;
    }
    private void PlayerController_PlayerStateChanged(PlayerState obj)
    {

        switch (obj)
        {
            case PlayerState.Prepare:
                break;
            case PlayerState.Living:
                break;
            case PlayerState.Pause:
                break;
            case PlayerState.PassLevel:
                PassLevel(); //we pass the level
                break;
            case PlayerState.Die:
                if (IsRevived) //If it is already revived go to game over
                {
                    GameOver();
                    return;
                }
                //Let's check if there is a video ready to be displayed. If it fails, game over
                if (!AdManager.Instance.IsRewardedVideoAdReady())
                {
                    GameOver();
                    return;
                }
                //If it is run the revive process.We are going to try, because if player cancel video, it loose the life.
                TryRevive();
                break;
            default:
                break;
        }
    }

    public void restartGame()
    {
        if (GameState == GameState.PassLevel)
            DecreaseCurrentLevel();
        //Triggers an event on this class, captured by AdmobClass.
        //20191009: currently is configured so when there are 3 restarts , an ad will be displayed.
        //This is exactly how Helix Jump (the original) fires ads.
        PlayList.Instance.InitMusic(0.5f);

        GameState = GameState.Prepare; //Launch Prepare event. WATCH OUT. Prepare can't be launched in the Load Scene. Order in events is relevant!

        LoadScene(SceneManager.GetActiveScene().name, 0.5f);
        ScoreManager.resetScore();
    }

    public void nextGame()
    {
        GameState = GameState.Prepare; //Launch event exactly before starting the game.
        LoadScene(SceneManager.GetActiveScene().name, 0.5f);
    }

    public GameState GameState
    {
        get
        {
            return gameState;
        }
        private set
        {
            if (value != gameState)
            {
                //Debug.Log ("ZZZ: GameState: changing status from " + gameState.ToString() + " to " + value.ToString() );
                gameState = value;
                if (GameState != null)
                {

                    GameStateChanged(gameState);
                }
                else
                {
                    //Debug.Log ("ZZZ: No one is suscribed to GameStateChanged Event!!!");
                }

            }
        }
    }

    [Header("Gameplay Testing")]
    [Header("Put a level number to test that level. Set back to 0 to disable this feature.")]
    //20201010:Changed to public so the UIManager can change according if it is a test or not. See SetLevelTxt method
    [SerializeField] public int testingLevel = 0;

    [Header("Gameplay Config")]
    [SerializeField] private ScriptableObject gamePlayConfig;
    private GameplayConfig GPC;
    
    [Header("Gameplay References")]
    [SerializeField] private Material pillarMaterial;
    [SerializeField] private Material deadPieceMaterial;
    [SerializeField] private Material normalPieceMaterial;
    [SerializeField] private Material brokenPieceMaterial;
    [SerializeField] private Transform rotaterTrans;
    [SerializeField] private GameObject pillar;
    [SerializeField] private GameObject bottomPillar;
    [SerializeField] private GameObject helixPrefab;
    [SerializeField] private GameObject fadingHelixPrefab;
    [SerializeField] private GameObject ballSplatPrefab;
    [SerializeField] private GameObject splatShatterPrefab;

    public Material DeadPieceMaterial { private set; get; }
    public Material NormalPieceMaterial { private set; get; }
    public Material BrokenPieceMaterial { private set; get; }
    public float ReviveWaitTime { private set; get; }
    public int PassedCountForBreakHelix { private set; get; }
    public int TimeToPassLevel { private set; get; }
    public int ThreeStarTime { private set; get; }
    public int TwoStarTime { private set; get; }
    public int OneStarTime { private set; get; }
    public bool IsFinishedFading { private set; get; }
    public bool IsRevived { private set; get; }

    private GameState gameState = GameState.GameOver;
    private List<BallSplatController> listBallSplatControl = new List<BallSplatController>();
    private List<FadingHelixController> listFadingHelixControl = new List<FadingHelixController>();
    private List<ParticleSystem> listSplatShatter = new List<ParticleSystem>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            DestroyImmediate(Instance.gameObject);
            Instance = this;
        }
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    // Use this for initialization
    void Start()
    {
       // PlayerPrefs.SetInt(LevelReached_PPK, 1);
        GPC = (GameplayConfig)gamePlayConfig;
        if (GPC == null)
        {
            Debug.LogError("Please check GamePlayConfig object in Manager because is null!!!!!!");
        }
        //Debug.Log ("GameManager Start");
        Application.targetFrameRate = 60;

        //Add another actions here
        DeadPieceMaterial = deadPieceMaterial;
        NormalPieceMaterial = normalPieceMaterial;
        BrokenPieceMaterial = brokenPieceMaterial;

        PassedCountForBreakHelix = GPC.helixPassedCountForBreak;
        ReviveWaitTime = GPC.reviveWaitTime;
        ThreeStarTime = GPC.threeStarPercentTime;
        TwoStarTime = GPC.twoStarPercentTime;
        OneStarTime = GPC.oneStarPercentTime;
        IsRevived = false;
        Time.timeScale = 1f; //Reset time if there was some error when making slow motion...

        //Set current level
        if (!PlayerPrefs.HasKey(LevelReached_PPK))
        {
            PlayerPrefs.SetInt(LevelReached_PPK, 1);
            CurrentLevel = 1;
        }

        if (!IsRestart)
            CurrentLevel = PlayerPrefs.GetInt(LevelReached_PPK);

        if (testingLevel != 0)
            CurrentLevel = testingLevel;

        //Show level on UI
        UIManager.Instance.SetLevelTxt(CurrentLevel);
        //Create background

        foreach (ParticlesConfig ps in GPC.particles)
        {
            if (CurrentLevel >= ps.MinLevel && CurrentLevel <= ps.MaxLevel)
            {
                CreateParticles(ps);
            }
        }
        
        foreach (BackgroundConfig b in GPC.backgroundColors)
        {
            if(CurrentLevel>=b.MinLevel && CurrentLevel <= b.MaxLevel)
            {
                CreateBackground(GPC.backgroundMaterial, b);
                break;
            }
        }
        
        //Create level
        foreach (GameLevelData o in GPC.gameLevels)
        {
            if (CurrentLevel >= o.MinLevel && CurrentLevel <= o.MaxLevel)
            {
                CreateLevel(o);
                break;
            }
        }

        StartCoroutine(ResetIsFinishedFadingValue());
        UIManager.Instance.FadeOutPanel(GPC.uIFadingTime);
        if (IsRestart)
            PlayingGame();
    }

    /// <summary>
    /// Actual start the game
    /// </summary>
    public void PlayingGame()
    {
        GameState = GameState.Playing;         //Fire event
    }


    /// <summary>
    /// Pause the game
    /// </summary>
    public void PauseGame()
    {
        GameState = GameState.Pause; //Fire event
    }

    /// <summary>
    /// Call Revive event
    /// </summary>
    public void TryRevive()
    {
        GameState = GameState.Revive; //Fire event
        Time.timeScale = 0.3f;
        StartCoroutine(slowMotion(1f, 7f)); //go to slow motion for 2 seconds
    }

    public void coroutineSlowMotion()
    {
        StartCoroutine(slowMotion(1f, 7f));
    }

    /*
*https://www.reddit.com/r/Unity3D/comments/4k1g3j/gradually_go_from_slow_motion_to_normal_speed/
Trying going from slow motion to normal speed to give user a chance of recovering after seeing video reward
*/
    IEnumerator slowMotion(float _lerpTimeTo, float _timeToTake)
    {
        float endTime = Time.time + _timeToTake;
        float startTimeScale = Time.timeScale;
        float i = 0f;
        while (Time.time < endTime)
        {
            i += (1 / _timeToTake) * Time.deltaTime;
            Time.timeScale = Mathf.Lerp(startTimeScale, _lerpTimeTo, i);
            yield return null;
        }
        Time.timeScale = _lerpTimeTo;
    }


    /// <summary>
    /// Call PassLevel event
    /// </summary>
    public void PassLevel()
    {
        GameState = GameState.PassLevel; //also fire event
        SoundManager.Instance.PlaySound(SoundManager.Instance.SMC.passLevel);
        ShareManager.Instance.CreateScreenshot();
        IsRestart = true;
        CurrentLevel++;
        PlayerPrefs.SetInt(LevelReached_PPK, CurrentLevel);
    }

    /// <summary>
    /// Call GameOver event
    /// </summary>
    public void GameOver()
    {
        GameState = GameState.GameOver; //Fire event.It will afect AdManager & UIManager

        //Add another actions here
        ShareManager.Instance.CreateScreenshot();
        IsRestart = true;
        CurrentLevel = PlayerPrefs.GetInt(LevelReached_PPK);
        PlayList.Instance.StopMusic();
        Debug.Log("Loding level: " + CurrentLevel);
    }


    public void LoadScene(string sceneName, float delay)
    {
        StartCoroutine(LoadingScene(sceneName, delay));
    }

    private IEnumerator LoadingScene(string sceneName, float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(sceneName);
    }

    private IEnumerator ResetIsFinishedFadingValue()
    {
        IsFinishedFading = false;
        yield return new WaitForSeconds(GPC.uIFadingTime);
        IsFinishedFading = true;
    }

    private IEnumerator PlayParticle(ParticleSystem par)
    {
        par.Play();
        yield return new WaitForSeconds(par.main.startLifetimeMultiplier);
        par.gameObject.SetActive(false);
    }

    //Get an inactive fading helix
    private FadingHelixController GetFadingHelixControl()
    {
        //Find on the list
        foreach (FadingHelixController o in listFadingHelixControl)
        {
            if (!o.gameObject.activeInHierarchy)
                return o;
        }

        //Didn't find one -> create new one
        FadingHelixController fadingHelixControl = Instantiate(fadingHelixPrefab, Vector3.zero, Quaternion.identity).GetComponent<FadingHelixController>();
        listFadingHelixControl.Add(fadingHelixControl);
        fadingHelixControl.gameObject.SetActive(false);
        return fadingHelixControl;
    }


    //Get an inactive ballSplatControl
    private BallSplatController GetBallSplatControl()
    {
        //Find on the list
        foreach (BallSplatController o in listBallSplatControl)
        {
            if (!o.gameObject.activeInHierarchy)
                return o;
        }

        //Didn't find one -> create new one
        BallSplatController ballSplatControl = Instantiate(ballSplatPrefab, Vector3.zero, Quaternion.identity).GetComponent<BallSplatController>();
        listBallSplatControl.Add(ballSplatControl);
        ballSplatControl.gameObject.SetActive(false);
        return ballSplatControl;
    }

    //Get an inactive splatShatter
    private ParticleSystem GetSplatShatter()
    {
        //Find on the list
        foreach (ParticleSystem o in listSplatShatter)
        {
            if (!o.gameObject.activeInHierarchy)
                return o;
        }

        //Didn't find one -> create new one
        ParticleSystem splatShatter = Instantiate(splatShatterPrefab, Vector3.zero, Quaternion.identity).GetComponent<ParticleSystem>();
        listSplatShatter.Add(splatShatter);
        splatShatter.gameObject.SetActive(false);
        return splatShatter;
    }

    //Instantiate a Prefab which it has a ParticleConfig and activates it.
    private void CreateParticles (ParticlesConfig pc)
    {
        ParticleSystem go = Instantiate<ParticleSystem>(pc.particleSystem, Camera.main.transform);
        go.gameObject.SetActive(true);
    }

    /*Assign colors to background. The material has to be assigned to the background, otherwise it won't work*/
    private void CreateBackground(Material Mat , BackgroundConfig b)
    {
        Mat.SetColor("_SkyColor1", b.TopColor);
        Mat.SetFloat("_SkyExponent1", b.TopExponent);
        Mat.SetColor("_SkyColor2", b.HorizonColor);
        Mat.SetColor("_SkyColor3", b.BottomColor);
        Mat.SetFloat("_SkyExponent2", b.BottomExponent);
        Mat.SetFloat("_SkyIntensity", b.SkyIntensity);
    }

  

    /// <summary>
    /// Creates the level using the data from levelData
    /// </summary>
    /// <param name="levelData"></param>
    private void CreateLevel(GameLevelData levelData)
    {
        #region Documentation
        //Calculate dificulty % , with 0 as minimun and 1 maximum
        //For example if level is between 6 and 10, difficutly would be 0,0.25,0.50,0.5 and 1 respectively, making the difficulty increasing for every level.
        //This value will be used for giving a integer value between 2 vars affecting difficulty.
        //In short , this function calculates an increasing difficulty for a level.
        #endregion

        float difficulty = (1.0f / (levelData.MaxLevel - levelData.MinLevel)) * (CurrentLevel - levelData.MinLevel);

        //Define how many helixes would this level have, according the difficult settings.
        //More advanced levels will have more helixes.
        //Example MinHelixNumber=25 and MaxHelixNumber=35 -> the result would be something like 25,25,30,33 and 35

        int helixNumber = Mathf.RoundToInt(Mathf.Lerp(levelData.MinHelixNumber, levelData.MaxHelixNumber, difficulty));  //Random.Range(levelData.MinHelixNumber, levelData.MaxHelixNumber);

        //Assign colors to different pieces
        deadPieceMaterial.color = levelData.DeadPieceColor;
        normalPieceMaterial.color = levelData.NormalPieceColor;
        brokenPieceMaterial.color = levelData.BrokenPieceColor;
        PlayerController.Instance.SetBallColor(levelData.BallColor);
        pillarMaterial.color = levelData.PillarColor;

        //Calculate time needed for this leve, increasing according the level.
        TimeToPassLevel = Mathf.RoundToInt(Mathf.Lerp(levelData.MinTimeToPassLevel, levelData.MaxTimeToPassLevel, difficulty)); //.Range(levelData.MinTimeToPassLevel, levelData.MaxTimeToPassLevel);

        //Create the first helix . Every disc is a 12-pieces helix
        HelixController firstHelixControl = Instantiate(helixPrefab, GPC.firstHelixPosition, Quaternion.identity).GetComponent<HelixController>();
        
        //Pass how many pieces will be disabled , how many would be a kill piece and the colors for each one
        firstHelixControl.HandleHelix(Random.Range(levelData.MinDisablePiecesNumber, levelData.MaxDeadPiecesNumber), 0, levelData.NormalPieceColor, levelData.DeadPieceColor);
        firstHelixControl.transform.SetParent(rotaterTrans);

        //Calculate the height of all helixs, space and distance between the pillar and the first helix
        float oneHelixHeight = helixPrefab.transform.GetChild(0).GetComponent<Renderer>().bounds.size.y;
        float totalHelixHeight = oneHelixHeight * helixNumber - (helixNumber - 1) * oneHelixHeight + GPC.helixSpace;
        float totalSpace = GPC.helixSpace * (helixNumber - 1);
        float distance = Vector3.Distance(GPC.firstHelixPosition + Vector3.up * oneHelixHeight, pillar.transform.position);

        //Calculate and set the pillar's height
        float pillarHeight = totalSpace + totalHelixHeight + Mathf.Round(distance);
        pillar.transform.localScale = new Vector3(1, pillarHeight, 1);

        //Create helixes
        Vector3 nextHelixPos = GPC.firstHelixPosition + Vector3.down * GPC.helixSpace;
        for (int i = 0; i < helixNumber - 1; i++)
        {
            HelixController helixControl = Instantiate(helixPrefab, nextHelixPos, Quaternion.identity).GetComponent<HelixController>();

            if (levelData.MovPieces) helixControl.SetMovPiece(levelData.RandomValues, levelData.OneDirection, levelData.LerpMov, levelData.angleMov, levelData.velMov, levelData.velLerp, levelData.dstMeta, levelData.percentMov);

            helixControl.HandleHelix(Random.Range(levelData.MinDisablePiecesNumber, levelData.MaxDisablePiecesNumber),
                                     Mathf.RoundToInt(Mathf.Lerp(levelData.MinDeadPiecesNumber, levelData.MaxDeadPiecesNumber, difficulty)),
                                     levelData.NormalPieceColor, levelData.DeadPieceColor);
            helixControl.transform.SetParent(rotaterTrans);
            nextHelixPos = helixControl.transform.position + Vector3.down * GPC.helixSpace;
        }

        //Move bottomHelix object to the bottom
        bottomPillar.transform.position = nextHelixPos + Vector3.up * oneHelixHeight; ;
    }


    //////////////////////////////////////Publish functions

    /// <summary>
    /// Continue the game
    /// </summary>
    public void SetContinueGame()
    {
        IsRevived = true;
        //GameManager.Instance.TimeToPassLevel
        PlayingGame();
    }

    /// <summary>
    /// Create a fading helix object at given position
    /// </summary>
    /// <param name="pos"></param>
    public void CreateFadingHelix(Vector3 pos)
    {
        FadingHelixController fadingHelixControl = GetFadingHelixControl();
        fadingHelixControl.transform.position = pos;
        fadingHelixControl.gameObject.SetActive(true);
        fadingHelixControl.FadingHelix(brokenPieceMaterial.color, GPC.fadingHelixScale, GPC.fadingHelixTime);
    }

    /// <summary>
    /// Create a ballSplat object at given position
    /// </summary>
    /// <param name="pos"></param>
    public void CreateBallSplat(Vector3 pos, Color playerColor, Transform parent)
    {
        BallSplatController ballSplatControl = GetBallSplatControl();
        ballSplatControl.transform.position = pos;
        ballSplatControl.transform.eulerAngles = new Vector3(90, Random.Range(0f, 360f), 0);
        ballSplatControl.gameObject.SetActive(true);
        ballSplatControl.FadeOut(playerColor, GPC.ballSplatFadingTime);
        ballSplatControl.transform.SetParent(parent);
    }


    /// <summary>
    /// Play splatShatter at given position
    /// </summary>
    /// <param name="pos"></param>
    public void PlaySplatShatter(Vector3 pos)
    {
        ParticleSystem splatShatter = GetSplatShatter();
        splatShatter.transform.position = pos;
        splatShatter.gameObject.SetActive(true);
        StartCoroutine(PlayParticle(splatShatter));
    }


    /// <summary>
    /// Decrease CurrentLevel by 1
    /// </summary>
    public void DecreaseCurrentLevel()
    {
        if (testingLevel == 0) //Isn't on testing level
            CurrentLevel--;
    }
}
