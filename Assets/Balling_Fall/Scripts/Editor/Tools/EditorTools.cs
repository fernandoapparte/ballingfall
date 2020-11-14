using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class EditorTools : EditorWindow {


    [MenuItem("Tools/Jumy Helix - Reset PlayerPrefs")]
    public static void ResetPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
        Debug.Log("*************** PlayerPrefs Was Deleted ***************");
    }


    [MenuItem("Tools/Jumpy Helix - Capture Screenshot")]
    public static void CaptureScreenshot()
    {
        ScreenCapture.CaptureScreenshot("C:/Users/UserX/Desktop/icon.png");
    }

}
