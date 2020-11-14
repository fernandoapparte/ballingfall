/**READ DOCUMENTATION AT THE END!*/
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Advertisements;
//For your knowledge, OG_ADMOB is defined on Player Settings-->Other Settings-->Scripting Define Symbols.
using GoogleMobileAds.Api;

enum AdType {    UnityAd,    Admob,}

[System.Serializable]
class ShowAdConfig
{
    public GameState GamestateForShowAd = GameState.GameOver;
    public AdType AdType = AdType.Admob;
    public int GameStateCountForShowAd = 2;
    public float ShowAdDelay = 1;
}

//Todo:
//2-Where should Interstitial Video(I'll need to create in the Console first, this i new
//3- If the ads are not enabled, I would reconfigure the game to be VERY difficult
//4- Should exist a replacement when video reward have an error.

namespace BallingFallGame
{
    public class AdManager : MonoBehaviour
    {
      
        public static AdManager Instance { get; set; }

        [SerializeField]
        private AppConfig appConfig;
        [Header("Unity Ads Config")]
        [SerializeField]
        private string unityAdsId = "1611450";
        [SerializeField]
        private string videoAdPlacementID = "video";
        [SerializeField]
        private string rewardedVideoAdPlacementID = "rewardedVideo";

        [Header("Test Mode: if enabled, ALL IDs are replaced for testeable Ads.")] 
        //See https://developers.google.com/admob/android/test-ads
        [SerializeField]
        private Boolean testMode = false;

        private String testBanner= "ca-app-pub-3940256099942544/6300978111";
        private String testInterstitial ="ca-app-pub-3940256099942544/1033173712";
        private String testInterstitialVideo = "ca-app-pub-3940256099942544/8691691433";
        private String testVideoReward = "ca-app-pub-3940256099942544/5224354917";
        [SerializeField]
        private AdPosition bannerPosition = AdPosition.Bottom;

        //IMPORTANTE: La compilación final debe hacerse desde la línea de comandos para tener en cuenta estas variables.
        private string AdmobAppId      = "undefined";
        private string BannerId = "undefined";
        private string InterstitialId = "undefined";
        private string RewardedVideoId = "undefined";

        [SerializeField]
        private AdType rewardedVideoType = AdType.Admob;
        [SerializeField]
        private float rewardedVideoDelay = 1f;

        [Header("Interstitial Ad Configuration")]
        [SerializeField]
        private List<ShowAdConfig> listShowInterstitialAdConfig = new List<ShowAdConfig>();

        private List<int> listShowAdCount = new List<int>();

        private BannerView bannerView;
        private InterstitialAd interstitial;
        private RewardBasedVideoAd rewardBasedVideo;
        private bool isAdmobRewardedVideoFinished = false;

        //Register this class to GameManager events
        private void OnEnable()  { GameManager.GameStateChanged += GameManager_GameStateChanged; }
        private void OnDisable() { GameManager.GameStateChanged -= GameManager_GameStateChanged; }

        private void Awake()
        {
            if (Instance)
            {
                DestroyImmediate(gameObject);
            }
            else
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
        }

        /// <summary>
        /// Checks for network connectivity . Used te enable or not some UI if there is no network.
        /// 
        /// </summary>
        /// <returns></returns>
        public bool checkInternetConnection()
        {

#if UNITY_EDITOR
            /*
            if (Network.player.ipAddress.ToString() != "127.0.0.1") { return  true;  }
            */
            return false;
#endif

            Boolean checkInternet = (Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork);
            Debug.Log("checkInternet result:" + checkInternet);
            return checkInternet;
        }

        // Use this for initialization
        void Start()
        {
            if (appConfig == null)
            {
                Debug.LogError("Error: AppConfig undefined on ShareManager.cs . Check ShareManager on scene");
            }
            string AdmobAppId = appConfig.AdmobAppId;
            string BannerId = appConfig.BannerId;
            string InterstitialId = appConfig.InterstitialId;
            string RewardedVideoId = appConfig.RewardedVideoId;

            //
            foreach (ShowAdConfig o in listShowInterstitialAdConfig)
            {
                listShowAdCount.Add(o.GameStateCountForShowAd);
            }
            Advertisement.Initialize(unityAdsId);
            MobileAds.Initialize(AdmobAppId);

#region rewardInitialization
            // Get singleton reward based video ad reference.
            rewardBasedVideo = RewardBasedVideoAd.Instance;

            // RewardBasedVideoAd is a singleton, so handlers should only be registered once.
            rewardBasedVideo.OnAdClosed += HandleRewardBasedVideoClosed;
            rewardBasedVideo.OnAdRewarded += HandleRewardBasedVideoRewarded;
            //Added to additional inspection
            rewardBasedVideo.OnAdCompleted += RewardBasedVideo_OnAdCompleted;
            rewardBasedVideo.OnAdFailedToLoad += RewardBasedVideo_OnAdFailedToLoad;
            rewardBasedVideo.OnAdLeavingApplication += RewardBasedVideo_OnAdLeavingApplication;
            rewardBasedVideo.OnAdLoaded += RewardBasedVideo_OnAdLoaded;
            rewardBasedVideo.OnAdOpening += RewardBasedVideo_OnAdOpening;
            rewardBasedVideo.OnAdStarted += RewardBasedVideo_OnAdStarted;
#endregion

            //Request banner, interstitial and rewarded base video ads
            RequestAdmobBanner();
            RequestAdmobInterstitial();
            RequestAdmobRewardBasedVideo();
        }

#region rewardedVideoEvents

        private void RewardBasedVideo_OnAdStarted(object sender, EventArgs e)
        {
            Debug.Log("Ad Video Reward Started");
        }

        private void RewardBasedVideo_OnAdOpening(object sender, EventArgs e)
        {
            Debug.Log("Ad Video Reward OnAdOpening");
        }

        private void RewardBasedVideo_OnAdLoaded(object sender, EventArgs e)
        {
            Debug.Log("Ad Video Reward OnAdLoaded");
        }

        private void RewardBasedVideo_OnAdLeavingApplication(object sender, EventArgs e)
        {
            Debug.Log("Ad Video Reward LeavingApp");
        }

        private void RewardBasedVideo_OnAdFailedToLoad(object sender, AdFailedToLoadEventArgs e)
        {
            Debug.Log("Ad Video Reward Failed to Load because " + e.Message);
        }

        private void RewardBasedVideo_OnAdCompleted(object sender, EventArgs e)
        {
            Debug.Log("Ad Video Reward Ad Completed");
        }
#endregion

        /// <summary>
        /// Show the rewarded video ad with delay time
        /// </summary>
        /// <param name="delay"></param>
        public void ShowRewardedVideoAd()
        {
            if (rewardedVideoType == AdType.UnityAd)
            {
                ShowUnityRewardedVideo(rewardedVideoDelay);
            }
            else
            {
                ShowAdmobRewardBasedVideoAd(rewardedVideoDelay);
            }
        }

        private void GameManager_GameStateChanged(GameState obj)
        {
            Debug.Log("Fired:" + obj.ToString());
            for (int i = 0; i < listShowAdCount.Count; i++)
            {
                if (listShowInterstitialAdConfig[i].GamestateForShowAd == obj)
                {
                    listShowAdCount[i]--;
                    if (listShowAdCount[i] <= 0)
                    {
                        //Reset gameCount 
                        listShowAdCount[i] = listShowInterstitialAdConfig[i].GameStateCountForShowAd;

                        if (listShowInterstitialAdConfig[i].AdType == AdType.UnityAd)
                            ShowUnityVideoAd(listShowInterstitialAdConfig[i].ShowAdDelay);
                        else
                            Debug.Log("Fired: ShowAdmobInterstitialAd" + obj.ToString());
                        ShowAdmobInterstitialAd(listShowInterstitialAdConfig[i].ShowAdDelay);
                    }
                }
            }
        }


        /// <summary>
        /// Determines whether rewarded video ad is ready.
        /// 20191008:Added check internet connection. This needs a lot of testing....!
        /// 
        /// </summary>
        /// <returns></returns>
        public bool IsRewardedVideoAdReady()
        {
            if (!checkInternetConnection()) return false;

            if (rewardedVideoType == AdType.UnityAd)
            {
                return Advertisement.IsReady(rewardedVideoAdPlacementID);
            }
            else
            {
                return rewardBasedVideo.IsLoaded();
            }
        }

        /// <summary>
        /// Show the video ad with delay time
        /// </summary>
        /// <param name="delay"></param>
        public void ShowUnityVideoAd(float delay)
        {
            if (Advertisement.IsReady(videoAdPlacementID))
                StartCoroutine(UnityVideoAd(delay));
        }

        IEnumerator UnityVideoAd(float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
            ShowOptions option = new ShowOptions();
            Advertisement.Show(videoAdPlacementID, option);
        }

        /// <summary>
        /// Show unity rewarded video with delay time
        /// </summary>
        /// <param name="delay"></param>
        public void ShowUnityRewardedVideo(float delay)
        {
            if (Advertisement.IsReady(rewardedVideoAdPlacementID))
                StartCoroutine(UnityRewardedVideoAd(delay));
        }

        IEnumerator UnityRewardedVideoAd(float delay)
        {
            yield return new WaitForSeconds(delay);
            ShowOptions option = new ShowOptions();
            option.resultCallback = OnUnityRewardedAdShowResult;
            Advertisement.Show(rewardedVideoAdPlacementID, option);
        }

        private void OnUnityRewardedAdShowResult(ShowResult result)
        {
            switch (result)
            {
                case ShowResult.Finished:
                    {
                        HandleOnRewardedVideoClosed();
                        break;
                    }
            }
        }

        /// <summary>
        /// Request admob banner ad
        /// </summary>
        public void RequestAdmobBanner()
        {
            // Clean up banner ad before creating a new one.
            if (bannerView != null)
            {
                bannerView.Destroy();
            }

            // Create a 320x50 banner at the top of the screen.
            if (testMode) {
                bannerView = new BannerView(testBanner, AdSize.SmartBanner, bannerPosition);
            } else  {
                bannerView = new BannerView(BannerId, AdSize.SmartBanner, bannerPosition);    
            }
            // Load a banner ad.
			bannerView.LoadAd(new AdRequest.Builder().Build());
        }

        //Request admob interstitial ad
        private void RequestAdmobInterstitial()
        {
            // Clean up interstitial ad before creating a new one.
            if (interstitial != null)
            {
                interstitial.Destroy();
            }

            // Create an interstitial.

            if (testMode){
                interstitial = new InterstitialAd(testInterstitial);
            } else
            {
                interstitial = new InterstitialAd(InterstitialId);    
            }
            // Register for ad events.
            interstitial.OnAdClosed += HandleInterstitialClosed;

            // Load an interstitial ad.
			interstitial.LoadAd(new AdRequest.Builder().Build());          
        }

        public void RequestAdmobRewardBasedVideo()
        {
            if (testMode) {
                rewardBasedVideo.LoadAd(new AdRequest.Builder().Build(), testVideoReward);
                return;
            }
            rewardBasedVideo.LoadAd(new AdRequest.Builder().Build(), RewardedVideoId);
        }

        /// <summary>
        /// Show admob interstitial ad with delay time 
        /// </summary>
        /// <param name="delay"></param>
        public void ShowAdmobInterstitialAd(float delay)
        {
            StartCoroutine(ShowAdmobInterstitial(delay));
        }

        IEnumerator ShowAdmobInterstitial(float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
            if (interstitial.IsLoaded())
            {
                interstitial.Show();
            }
            else
            {
                RequestAdmobInterstitial();
            }
        }

        /// <summary>
        /// Show admob rewarded base video ad with delay time 
        /// </summary>
        /// <param name="delay"></param>
        public void ShowAdmobRewardBasedVideoAd(float delay)
        {
            StartCoroutine(ShowAdmobRewardBasedVideo(delay));
        }
        IEnumerator ShowAdmobRewardBasedVideo(float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
            if (rewardBasedVideo.IsLoaded())
            {
                rewardBasedVideo.Show();
            }
            else
            {
                RequestAdmobRewardBasedVideo();
            }
        }


        private void HandleOnRewardedVideoClosed()
        {
            if (rewardedVideoType == AdType.Admob) //Admob ads
            {
                if (isAdmobRewardedVideoFinished) //User complet the video
                {
                    Debug.Log("User end seeing video successfully");
                    //Architectural glitch: AdManager shouldn't handle the responsibility of making theses changes.
                    GameManager.Instance.SetContinueGame();
                    Time.timeScale = 0.3f;
                    GameManager.Instance.coroutineSlowMotion(); //go to slow motion for 7 seconds
                }
                else //User skip the video
                {
					Debug.Log("User skipped video");
                    if (GameManager.Instance.GameState == GameState.Revive)
                        GameManager.Instance.GameOver();
                }
				//Restore music
				PlayList.Instance.StartMusic (); //Restart music or none depending on music status.
                isAdmobRewardedVideoFinished = false;
            }
            else //Unity ads
            {
                GameManager.Instance.SetContinueGame();
            }         
        }

        //Events callback
        public void HandleInterstitialClosed(object sender, EventArgs args)
        {
            RequestAdmobInterstitial();
        }

        private void HandleRewardBasedVideoRewarded(object sender, Reward args)
        {
            isAdmobRewardedVideoFinished = true;
        }
        private void HandleRewardBasedVideoClosed(object sender, EventArgs args)
        {
            RequestAdmobRewardBasedVideo();
            HandleOnRewardedVideoClosed();
        }
    }
}

/*
- Added test devices for Admob (for reward video, only Android)
- Improved: switch to test with one checkbox
- 20181021: Feature removed. The some (unknown) reason, using Device ID always return 'Ad failed to load because no fill'
- We are using admob test and it works on EVERY device, so we discarded this feature.

 20191009:
 After some inspection, I found there is too much complexity just for extending a life when the player loses, returning an excesive amount of errors
 because of this. I decided to simplify, supressing the chance of extending the player if he/she looses. 
 That will bring less income, but a LOT less bugs or game problems. 
 
 -Reorganized code in banner, interstitial and video reward (last one will be off by the moment).

  I'm refactoring the code to make it a little more simple. It had too many and conditions to be realistic when it is working.
  The less , the better.

-Suppressed the IOS #if..endif. I'm not using at this moment and brings unnecesary complexity.
-Currently the class will support Unity Ads & Admob. So I also will take out some intrinsecal useless complexity

 */