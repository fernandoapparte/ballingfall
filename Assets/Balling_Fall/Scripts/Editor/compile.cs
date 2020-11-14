using UnityEngine;
using UnityEditor;
using System.Diagnostics;

/*
 * 20201028: new compilation version , simpler than previous one, using ScriptableObjects.
 * */

public class compile
{
    private static bool compileToBundle = true; //Si se compila a bundle la extension es .aab , sino es .apk

    private static string getExtension()
    {
        if (compile.compileToBundle)
        {
            EditorUserBuildSettings.buildAppBundle = true;
            return "aab";
        }
        EditorUserBuildSettings.buildAppBundle = false;
        return "apk"; 
    }

	public static void ballingfall() {
		UnityEngine.Debug.Log("Iniciando el proceso...WAIT!");
		AppConfig app = (AppConfig)AssetDatabase.LoadAssetAtPath("Assets/Balling_Fall/Scripts/ScriptableObjects/Game1/AppConfig.asset", typeof(AppConfig));
		processData(app);
	}
	
	public static void processData(AppConfig app)
    {
        if (app==null)
        {
            UnityEngine.Debug.LogError("AppConfig is null! Is it currently the right PATH?????");
        }
        Stopwatch sw = Stopwatch.StartNew();

        PlayerSettings.companyName = app.CompanyName;
        PlayerSettings.productName = app.ProductName;
        PlayerSettings.applicationIdentifier = app.ApplicationIdentifier;
        PlayerSettings.bundleVersion = app.BundleVersion;
        PlayerSettings.Android.bundleVersionCode = app.AndroidBundleVersionCode;

        //TODO: setear el icono del juego.        //Cambiar el icono
        FileUtil.ReplaceFile(app.IconPath,"Assets/Balling_Fall/Sprites/icon-app.png");
        //Cambiar la pantalla de splash
        FileUtil.ReplaceFile(app.IconSplashPath, "Assets/Balling_Fall/Sprites/logo-splash.png");
        AssetDatabase.Refresh();

        //Indicar la escena base
        string[] levels = {  "Assets/Balling_Fall/Scenes/Gameplay.unity" };
        string deployPath = app.DeployPath + "." + getExtension();
        //string deployPath = "../other/spinner-fall-release." + getExtension();
        PlayerSettings.keyaliasPass = app.KeyAliasPass;
        PlayerSettings.keystorePass = app.KeystorePass;
        PlayerSettings.Android.keyaliasName = app.AndroidKeyAliasName;
        PlayerSettings.Android.keyaliasPass = app.AndroidKeyAliasPass;
        PlayerSettings.Android.keystoreName = app.AndroidKeystoreName;
        PlayerSettings.Android.keystorePass = app.KeystorePass;

        string[] parameters = System.Environment.GetCommandLineArgs();
        foreach (string p in parameters)
        {
            if (p.Equals("-compile"))
            {
                UnityEngine.Debug.Log("ENVIANDO A COMPILAR!!! YA!");
                BuildPipeline.BuildPlayer(levels, deployPath, BuildTarget.Android, BuildOptions.None);
            }
        }
        sw.Stop();
        UnityEngine.Debug.Log("Tiempo total: " + sw.ElapsedMilliseconds / 1000 + "  segundos");

    }
}
