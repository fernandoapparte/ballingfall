using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BackgroundConfig", menuName = "Game/Background Color Config", order = 5)]
public class BackgroundConfig : ScriptableObject
{
    public int MinLevel = 1;
    public int MaxLevel = 5;
    public Color TopColor = Color.black; //by default we don't have TopColor. In this particular game camera is pointing slightly down, so we barely see top color
    public float TopExponent = 0.0f; //0=top color is absent. 1=nice blend, Higher values prioritizes top color.
    public Color HorizonColor = new Color32(0x12, 0x65, 0xEE, 0xFF);
    public float SkyIntensity = 1.78f;
    public Color BottomColor = new Color32(0xE5,0x45,0x9E,0xFF);
    public float BottomExponent = 3.56f;
 }

//1265EE00
//From: https://answers.unity.com/questions/1395578/how-can-i-use-hex-color.html
//Color32 greyColour = new Color32(0xBE, 0xBF, 0xBE, 0xFF);
//Default color should be light sky and pink