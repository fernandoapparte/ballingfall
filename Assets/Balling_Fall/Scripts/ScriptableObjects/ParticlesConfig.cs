using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Particles", menuName = "Game/Particles", order = 6)]
public class ParticlesConfig : ScriptableObject {

    public int MinLevel = 1;
    public int MaxLevel = 5;
    public  ParticleSystem particleSystem;
    
	}

