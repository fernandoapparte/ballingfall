using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "Game/Level Data", order = 2)]
public class GameLevelData : ScriptableObject {
    public int MinLevel =1;
    public int MaxLevel =5;
    public int MinHelixNumber =10;
    public int MaxHelixNumber =20;
    public int MinDisablePiecesNumber=4;
    public int MaxDisablePiecesNumber=6;
    public int MinDeadPiecesNumber=1;
    public int MaxDeadPiecesNumber=2;
    public bool MovPieces;
    public bool RandomValues;
    public bool OneDirection;
    public bool LerpMov;
    [Range(1, 100)] public int percentMov ;
    [Range(1, 360)] public int angleMov;
    [Range(0, 300)] public float velMov;
    [Range(0, 2)] public float velLerp;
    [Range(1, 10)] public int dstMeta;
    public int MinTimeToPassLevel;
    public int MaxTimeToPassLevel;
    public Color DeadPieceColor;
    public Color NormalPieceColor;
    public Color BrokenPieceColor;
    public Color BallColor;
    public Color PillarColor;

}
