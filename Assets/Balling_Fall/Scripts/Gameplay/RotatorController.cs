using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotatorController : MonoBehaviour {

    [Header("Rotator Config")]
    [SerializeField] private float rotatingSpeed = 50f;


    private void Update()
    {
        if (GameManager.Instance.GameState == GameState.Playing)
        {
            //20191004: Notice that this translate CORRECTLY to Android and touch events WITHOUT any 
            //additional code.
            if (Input.GetMouseButton(0) && GameManager.Instance.IsFinishedFading)
            {
                float x = Camera.main.ScreenToViewportPoint(Input.mousePosition).x;
                Rotate(x);
            }
            //20191004: Handling alternative input for testing
            //https://www.youtube.com/watch?v=chMxcadsT4U
            //Check if we are using a keyboard to test the game directly
            //This can be 'rewired' inside Unity for testings
            float keyPressed =Input.GetAxis("Horizontal");
            if (keyPressed!=0 && GameManager.Instance.IsFinishedFading)
            {
                //What I did:
                //1-I changed 'Negative Button' to 'left' & 'Positive button' to 'right'
                //This means now I can use left and right arrows as keyboard input
                //2-Values are from [-1,1]. when left is pressed value changes according gravity , etc. from 0 to -1. 
                //In the same way when right is pressed and 0 to 1.
                if (keyPressed>0)
                {
                    Rotate(0.1f);
                } else
                {
                    Rotate(0.8f);
                }
            }
        }
    }

    private void Rotate(float x)
    {
        float multiplier = 1.5f; //Used to make rotatin faster
        if (x <= 0.5f) //Touch left
        {
            transform.eulerAngles += Vector3.up * rotatingSpeed * Time.deltaTime * multiplier;
        }
        else //Touch right
        {
            transform.eulerAngles += Vector3.down * rotatingSpeed * Time.deltaTime * multiplier;
        }
    }
}
