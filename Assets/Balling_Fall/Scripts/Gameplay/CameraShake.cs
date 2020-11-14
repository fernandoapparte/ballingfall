using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    // Transform of the camera to shake. Grabs the gameObject's transform if null.
    public Transform camTransform;
    public float shakeDuration = 0f; // How long the object should shake for.
    public float shakeAmount = 0.7f;     // Amplitude of the shake. A larger value shakes the camera harder.
    public float decreaseFactor = 1.0f;

    Vector3 originalPos;

    void Awake()
    {
        if (camTransform == null)
        {
            camTransform = GetComponent(typeof(Transform)) as Transform;
        }
    }

    void OnEnable()
    {
        originalPos = camTransform.localPosition;
    }

    void Update()
    {
        if (shakeDuration > 0)
        {
            Vector3 r = Random.insideUnitSphere;
            r.y = 0; //don't use y axis, because it will be used for CameraController.

            camTransform.localPosition = originalPos +r * shakeAmount;
            

            shakeDuration -= Time.deltaTime * decreaseFactor;
        }
        else
        {
          //  shakeDuration = 0f;
//            camTransform.localPosition = originalPos;
        }
    }
}