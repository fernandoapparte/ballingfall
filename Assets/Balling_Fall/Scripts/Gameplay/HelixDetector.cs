using BallingFallGame;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelixDetector : MonoBehaviour {

    public int PassedCount { private set; get; }
    private GameObject currentHelix = null;

    public delegate void DetectorEvents(int passedCount);
    public static event DetectorEvents PastPieces;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            GameObject parent = other.transform.parent.gameObject;
            int currentP = PassedCount;

            if (PassedCount > SoundManager.Instance.SMC.passedPieces.Length - 1)
            {
                currentP = SoundManager.Instance.SMC.passedPieces.Length - 1;
            }

            if (parent != currentHelix)
            {
                //SoundManager.Instance.PlaySound(SoundManager.Instance.passedPieces);
                if (SoundManager.Instance.SMC.passedPieces.Length!=0)
                {
                    SoundManager.Instance.PlaySound(SoundManager.Instance.SMC.passedPieces[currentP]);
                } else
                {
                    Debug.LogError("There are no sound for pieces in Passed Pieces");
                }
                
                currentHelix = parent;
                PassedCount++;
            }
        }
    }

    public void ResetPassedCount()
    {
        if (PastPieces != null && PassedCount > 0)
        {
            PastPieces.Invoke(PassedCount);
        }
        PassedCount = 0;
    }
}
