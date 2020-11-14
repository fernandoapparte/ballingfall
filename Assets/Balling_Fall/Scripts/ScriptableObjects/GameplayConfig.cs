using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameplayConfig", menuName = "Game/Gameplay Config", order = 1)]
public class GameplayConfig : ScriptableObject {
    public float reviveWaitTime = 4f;
    public Vector3 firstHelixPosition = new Vector3(0, -1f, 2.5f);
    public float helixSpace = 5f;
    public float fadingHelixScale = 4f;
    public float fadingHelixTime = 0.5f;
    public int helixPassedCountForBreak = 2;
    public int threeStarPercentTime = 50;
    public int twoStarPercentTime = 30;
    public int oneStarPercentTime = 10;
    public float uIFadingTime = 2f;
    public float ballSplatFadingTime = 2f;
    public Material backgroundMaterial; //This material needs to be assigned to the Sky .Also, this material uses GradientSkyShader with some specific parameters.
                                        //We need additional documentation here!
    public ScriptableObject[] backgroundColors;
    public ScriptableObject[] gameLevels;
    public ScriptableObject[] particles;
  
}
