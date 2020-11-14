using BallingFallGame;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SoundManagerConfig", menuName = "Game/Sound Manager Config", order = 3)]
public class SoundManagerConfig : ScriptableObject {
    public Sound button;
    public Sound bounce;
    public Sound breakPieces;
    public Sound[] passedPieces;
    public Sound passLevel;
    public Sound playerExplode;
    public Sound starCount;
}
