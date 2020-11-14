using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine;
using BallingFallGame;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {

    public static UIManager Instance { private set; get; }

    [SerializeField] private AppConfig appConfig;
    //Gameplay UI
    [SerializeField] private GameObject gameplayUI;
    [SerializeField] private Text levelTxt;
    [SerializeField] private GameObject pauseBtn;
    [SerializeField] private GameObject unPauseBtn;
    [SerializeField] private GameObject timeBarUI; //20191005: needed to perform animation
    [SerializeField] private Image timeBar;
    [SerializeField] private Color colorNormal, colorAttention, colorDanger;
    [SerializeField] private Text textLevel;
    [SerializeField] private Image progressMask;


    //Revive UI
    [SerializeField] private GameObject reviveUI;
    [SerializeField] private Image reviveCoverImg;

    //GameOver UI
    [SerializeField] private GameObject endGameUI;
    [SerializeField] private GameObject starCoverUI;
    [SerializeField] private GameObject starUI;
    [SerializeField] private GameObject star_1;
    [SerializeField] private GameObject star_2;
    [SerializeField] private GameObject star_3;
    [SerializeField] private GameObject playBtns;
    [SerializeField] private GameObject playBtn;
    [SerializeField] private GameObject nextBtn;
    [SerializeField] private GameObject restartBtn;
    [SerializeField] private GameObject shareBtn;
    [SerializeField] private GameObject soundOnBtn;
    [SerializeField] private GameObject soundOffBtn;
    [SerializeField] private GameObject musicOnBtn;
    [SerializeField] private GameObject musicOffBtn;

    [SerializeField] private Image fadingPanel;

    //References
    [SerializeField] private AnimationClip servicesBtns_Show;
    [SerializeField] private AnimationClip servicesBtns_Hide;
    [SerializeField] private AnimationClip settingBtns_Hide;
    [SerializeField] private AnimationClip settingBtns_Show;
    [SerializeField] private Animator settingAnim;
    [SerializeField] private Animator servicesAnim;
    [SerializeField] private Text gameNameText;



    private float timeCount = 0;
    private const string triggerAnimTextLevel = "ShowInitialLevel";

    private float maxWidthParent = 0;
    private bool distanceCalculate = false;
    private Vector2 posRespawn;
    private float totalDistance = 0;
    Animator barAnimation; //20191005:Introduced to add some attention when the times goes off.


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
        switch (obj)
        {
            case GameState.Prepare:
                break;
            case GameState.Playing:
                if (!GameManager.Instance.IsRevived)
                {
                    //Reset time for player who see video.
                    timeCount = GameManager.Instance.TimeToPassLevel * 10.0f;

                    gameplayUI.SetActive(true);
                    unPauseBtn.SetActive(false);
                    endGameUI.SetActive(false);
                    reviveUI.SetActive(false);
                    StartCoroutine(CountingDownTimeBar());

                }
                setGameNameText();
                break;
            case GameState.Pause:
                break;
            case GameState.Revive:
                StartCoroutine(ShowReviveUI(0.5f));
                break;
            case GameState.PassLevel:
                StartCoroutine(ShowPassLevelUI(2.5f));
                break;
            case GameState.GameOver:
                StartCoroutine(ShowGameOverUI(0.5f));
                break;
            default:
                break;
        }
    }

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


    void Start () {

        if (!GameManager.IsRestart) //This is the first load
        {
            gameplayUI.SetActive(false);
            reviveUI.SetActive(false);
            endGameUI.SetActive(true);

            starCoverUI.SetActive(false);
            starUI.SetActive(false);
            restartBtn.SetActive(false);
            nextBtn.SetActive(false);
            playBtn.SetActive(true);
            shareBtn.SetActive(false);
            timeBar.color = colorNormal;
            setGameNameText();

        }
        else
        {
            textLevel.text = GameManager.CurrentLevel.ToString();
            Animator anim = textLevel.GetComponent<Animator>();

            if (anim != null)
            {
                anim.SetTrigger(triggerAnimTextLevel);
            }

        }
        maxWidthParent = progressMask.transform.parent.GetComponent<RectTransform>().sizeDelta.x;
       // Debug.Log(maxWidthParent);
    }


    /// <summary>
    /// Set the name of the game
    /// </summary>
    private void setGameNameText()
    {
        if (appConfig == null)
        {
            Debug.LogError("Error: AppConfig undefined on ShareManager.cs . Check ShareManager on scene");
        }
        gameNameText.text = appConfig.DisplayName;
    }

    // Update is called once per frame
    void Update () {

        UpdateMusicButtons();
        UpdateMuteButtons();
        UpdateLevelProgress();
    }


    ////////////////////////////Publish functions
    public void PlayButtonSound()
    {
        SoundManager.Instance.PlaySound(SoundManager.Instance.SMC.button);
    }

    public void PauseBtn()
    {
     
        pauseBtn.SetActive(false);
        unPauseBtn.SetActive(true);
        Time.timeScale = 0;
        //GameManager.Instance.PauseGame();
    }
    public void UnPauseBtn()
    {

        pauseBtn.SetActive(true);
        unPauseBtn.SetActive(false);
        Time.timeScale = 1;
        //GameManager.Instance.PlayingGame();
    }
    public void PlayBtn()
    {
        GameManager.Instance.PlayingGame();
        textLevel.text = GameManager.CurrentLevel.ToString();
        Animator anim = textLevel.GetComponent<Animator>();

        if(anim != null)
        {
            anim.SetTrigger(triggerAnimTextLevel);
        }
    }
    public void RestartBtn()
    {
        GameManager.Instance.restartGame();
    }

    public void NextBtn()
    {

        GameManager.Instance.nextGame();

    }



    public void NativeShareBtn()
    {
        ShareManager.Instance.NativeShare();
    }
    public void RateAppBtn()
    {
#if UNITY_IOS
        UnityEngine.iOS.Device.RequestStoreReview();
#elif UNITY_ANDROID
        Application.OpenURL(appConfig.AppUrl);
#endif
    }
    public void SettingBtn()
    {
        servicesAnim.Play(servicesBtns_Hide.name);
        settingAnim.Play(settingBtns_Show.name);
    }
    public void ToggleSound()
    {
        SoundManager.Instance.ToggleMute();
    }

    public void ToggleMusic()
    {
        // SoundManager.Instance.ToggleMusic();
        PlayList.Instance.ToggleMusic();
    }
    public void BackBtn()
    {
        settingAnim.Play(settingBtns_Hide.name);
        servicesAnim.Play(servicesBtns_Show.name);
    }


    /* 20191008: Problem: if we dont have network, how do we suppose to work here?*/

    public void ReviveBtn()
    {
        reviveUI.SetActive(false);
        AdManager.Instance.ShowRewardedVideoAd();
    }

    public void SkipBtn()
    {
        reviveUI.SetActive(false);
        GameManager.Instance.GameOver();
    }

    /////////////////////////////Private functions
    public void UpdateMuteButtons()
    {
        if (SoundManager.Instance.IsMuted())
        {
            soundOnBtn.gameObject.SetActive(false);
            soundOffBtn.gameObject.SetActive(true);
        }
        else
        {
            soundOnBtn.gameObject.SetActive(true);
            soundOffBtn.gameObject.SetActive(false);
        }
    }

    public void UpdateMusicButtons()
    {
        if (PlayList.Instance.IsMusicOff())
        {
            musicOffBtn.gameObject.SetActive(true);
            musicOnBtn.gameObject.SetActive(false);
        }
        else
        {
            musicOffBtn.gameObject.SetActive(false);
            musicOnBtn.gameObject.SetActive(true);
        }
    }

    private void UpdateLevelProgress()
    {
        if(!distanceCalculate)
        {
            GameObject respawn = GameObject.FindWithTag("Respawn");
            if(respawn != null)
            {
                posRespawn = respawn.transform.position;
                totalDistance = Vector2.Distance(PlayerController.Instance.transform.position, posRespawn);

                distanceCalculate = true;

                float currentDistance = Vector2.Distance(PlayerController.Instance.transform.position, posRespawn);
                float width = maxWidthParent - (currentDistance * maxWidthParent / totalDistance);
            }
        }
        else if(distanceCalculate && PlayerController.Instance.PlayerState != PlayerState.PassLevel)
        {
            float currentDistance = Vector2.Distance(PlayerController.Instance.transform.position, posRespawn);
            float width = maxWidthParent - (currentDistance * maxWidthParent / totalDistance); // 400 = max width

            progressMask.rectTransform.sizeDelta = new Vector2(width, progressMask.rectTransform.sizeDelta.y);
        }
        else if(PlayerController.Instance.PlayerState == PlayerState.PassLevel)
        {
            progressMask.rectTransform.sizeDelta = new Vector2(maxWidthParent, progressMask.rectTransform.sizeDelta.y);
        }
    }


    private IEnumerator ShowGameOverUI(float delay)
    {
        yield return new WaitForSeconds(delay);

        endGameUI.SetActive(true);
        shareBtn.SetActive(true);
        playBtns.SetActive(false);
        restartBtn.SetActive(true);
    }
    private IEnumerator ShowPassLevelUI(float delay)
    {
        yield return new WaitForSeconds(delay);

        gameplayUI.SetActive(false);
        endGameUI.SetActive(true);

        starCoverUI.SetActive(true);
        starUI.SetActive(true);
        shareBtn.SetActive(true);
        playBtns.SetActive(true);
        playBtn.SetActive(false);
        nextBtn.SetActive(true);
        restartBtn.SetActive(true);

        star_1.SetActive(false);
        star_2.SetActive(false);
        star_3.SetActive(false);

        float timeUse = GameManager.Instance.TimeToPassLevel - timeCount;
        float percent = (timeUse / GameManager.Instance.TimeToPassLevel) * 100f;

        float delayTime = 0.5f;
        if (percent >= GameManager.Instance.ThreeStarTime) //Show three stars
        {
            yield return new WaitForSeconds(delayTime);
            SoundManager.Instance.PlaySound(SoundManager.Instance.SMC.starCount);
            star_1.SetActive(true);
            yield return new WaitForSeconds(delayTime);
            SoundManager.Instance.PlaySound(SoundManager.Instance.SMC.starCount);
            star_2.SetActive(true);
            yield return new WaitForSeconds(delayTime);
            SoundManager.Instance.PlaySound(SoundManager.Instance.SMC.starCount);
            star_3.SetActive(true);
        }
        else if (percent >= GameManager.Instance.TwoStarTime && percent < GameManager.Instance.ThreeStarTime) //Show two stars
        {
            yield return new WaitForSeconds(delayTime);
            SoundManager.Instance.PlaySound(SoundManager.Instance.SMC.starCount);
            star_1.SetActive(true);
            yield return new WaitForSeconds(delayTime);
            SoundManager.Instance.PlaySound(SoundManager.Instance.SMC.starCount);
            star_2.SetActive(true);
            star_3.SetActive(false);
        }
        else //Show one star 
        {
            yield return new WaitForSeconds(delayTime);
            SoundManager.Instance.PlaySound(SoundManager.Instance.SMC.starCount);
            star_1.SetActive(true);
            star_2.SetActive(false);
            star_3.SetActive(false);
        }
    }

    private IEnumerator ShowReviveUI(float delay)
    {
        yield return new WaitForSeconds(delay);

        reviveUI.SetActive(true);
        StartCoroutine(ReviveCountDown());
    }

    private IEnumerator ReviveCountDown()
    {
        float t = 0;
        while (t < GameManager.Instance.ReviveWaitTime)
        {
            if (!reviveUI.activeInHierarchy)
                yield break;
            t += Time.deltaTime;
            float factor = t / GameManager.Instance.ReviveWaitTime;
            reviveCoverImg.fillAmount = Mathf.Lerp(1, 0, factor);
            yield return null;
        }
        reviveUI.SetActive(false);
        GameManager.Instance.GameOver();
    }
    private IEnumerator CountingDownTimeBar()
    {
        if (barAnimation == null)
        {
            barAnimation = timeBarUI.GetComponent<Animator>();
        }

        //Wait for finished fading
        while (!GameManager.Instance.IsFinishedFading)
        {
            yield return null;
        }

        timeCount = 0;
        while (timeCount < GameManager.Instance.TimeToPassLevel)
        {
            timeCount += Time.deltaTime;
            float factor = timeCount / GameManager.Instance.TimeToPassLevel;
            timeBar.fillAmount = Mathf.Lerp(1, 0, factor);
            //20191005:Change color according fill Amount
            
            if (timeBar.fillAmount>=0.6f)
            {
                timeBar.color = colorNormal;
                barAnimation.SetBool("danger", false);
            } else if (timeBar.fillAmount < 0.6f && timeBar.fillAmount>=0.3f)
            {
                timeBar.color = colorAttention;
                barAnimation.SetBool("danger", false);
            } else
            {
                timeBar.color = colorDanger;
                barAnimation.SetBool("danger", true);
            }
            
            yield return null;
            if (PlayerController.Instance.PlayerState == PlayerState.PassLevel)
                yield break;
            while (PlayerController.Instance.PlayerState != PlayerState.Living)
            {
                yield return null;
            }
        }
		//PlayList.Instance.StopMusic ();
        //Dying when time is up
        PlayerController.Instance.PlayerDies();
        GameManager.Instance.GameOver();
    }




    /// <summary>
    /// Fading the panel out with given fadingTime
    /// </summary>
    /// <param name="fadingTime"></param>
    public void FadeOutPanel(float fadingTime)
    {
        StartCoroutine(FadingOutPanel(fadingTime));
    }
    private IEnumerator FadingOutPanel(float fadingTime)
    {
        fadingPanel.gameObject.SetActive(true);
        float t = 0;
        Color startColor = fadingPanel.color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0);
        while (t < fadingTime)
        {
            t += Time.deltaTime;
            float factor = t / fadingTime;
            fadingPanel.color = Color.Lerp(startColor, endColor, factor);
            yield return null;
        }
        fadingPanel.gameObject.SetActive(false);
    }


    /// <summary>
    /// Show level text with given level number
    /// </summary>
    /// <param name="level"></param>
    public void SetLevelTxt(int level)
    {
        if (GameManager.Instance.testingLevel != 0)
        {
            levelTxt.text = "TEST : " + level.ToString();
            return;
        }
        levelTxt.text = "LEVEL: " + level.ToString();
    }
}
