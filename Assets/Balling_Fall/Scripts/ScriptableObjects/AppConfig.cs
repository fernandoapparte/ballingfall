using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AppConfig", menuName = "Game/App Configuration", order = 9)]
public class AppConfig : ScriptableObject {

    [Header("Basic app information needed for compilation")]
    public string CompanyName = "";
    public string ProductName = "";
    public string DisplayName = ""; //Name that would be displayed on the game
    public string ApplicationIdentifier = "";
    public string BundleVersion = "";
    public int AndroidBundleVersionCode;
    public string IconPath;
	public string IconSplashPath;

    [Header("Keystore information important for the compiling process and signing app")]
    public string KeyAliasPass;
    public string KeystorePass;
    public string AndroidKeyAliasName;
    public string AndroidKeyAliasPass;
    public string AndroidKeystoreName;
    public string AndroidKeystorePass;
    public string DeployPath;

    [Header("Facebook information. Leave empty if you won't use it")]
    public string AppUrl;
    public string FbAppId;
    public string FbSharedCaption;
    public string FbPictureUrl;
    public string FbDescription;

    public string ScreenShotName = "screenshot.png";
    public string ShareText = "Can yo beat my score?";
    public string ShareSubject = "Share Via";

    [Header("Twitter sharing config")]
    public string TwitterAddress = "http://twitter.com/intent/tweet";
    public string TwitterTextToDisplay = "Hey Guys! Check out my score:";
    public string TwitterLanguage = "en";
    
    [Header("Admob Id for monetization")] 
    public string AdmobAppId;
    public string BannerId;
    public string InterstitialId;
    public string RewardedVideoId;
    
}
