using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This is the configurable sequence music.
//The musics is divided in clips and it will be played following the seqPlayList.
[CreateAssetMenu(fileName = "SequencePlayList", menuName = "Game/Sequence Play List", order = 4)]
public class SequencePlayList : ScriptableObject {

    public AudioClip[] clipsPlayList; //list of clips
    public int[] seqPlayList; //sequence order of the clips. First clip is intro and never played again

}
