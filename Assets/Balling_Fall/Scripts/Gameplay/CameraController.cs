using UnityEngine;

public class CameraController : MonoBehaviour {

    public static CameraController Instance { private set; get; }

    [Header("Camera Config")]
    [SerializeField] private float smoothTime = 0.2f;

    private Vector3 velocity = Vector3.zero;
    private float originalDistance = 0;
	private bool buffer=false;
     
    public float shakeDuration = 0f;     // How long the object should shake for.

    public float shakeAmount = 0.3f;     // Amplitude of the shake. A larger value shakes the camera harder.
    public float decreaseFactor = 1.0f;
    Vector3 originalPos;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            DestroyImmediate(Instance.gameObject);
            Instance = this;
        }
    }

    private void Start()
    {
        originalDistance = transform.position.y - PlayerController.Instance.transform.position.y;
    }

    private void Update()
    {
        if (PlayerController.Instance.PlayerState == PlayerState.Living)
        {
            #region Documentation
            //This is the main camera controller, which is attached to the camera. That's why we use transform object (which it's the camera attached)
            //As you can see, the camera moves according distance between the camera and the player (only y axis)
            //That has some consequences: if we shake the camera, please don't touch the y coordinate.
            #endregion

            float currentDistance = transform.position.y - PlayerController.Instance.TargetY;
            if (currentDistance > originalDistance)
            {
                float distance = currentDistance - originalDistance;
                Vector3 targetPos = transform.position + Vector3.down * distance;
                transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, smoothTime);
            }

            if (shakeDuration > 0)
            {
				if (!buffer) {
					buffer = true;
					originalPos = transform.position; //let's remember where we are before shaking
				}
                Vector3 r = Random.insideUnitSphere;
                r.y = 0; //don't use y axis, because it will be used for CameraController.
				r.z=0;
                transform.position = originalPos + r * shakeAmount;
                shakeDuration -= Time.deltaTime * decreaseFactor;
            } else
            {
				//Shake ended, restore camera to original coordinates except y
				if (buffer) {
					buffer = false;
					//transform.position.Set (originalPos.x, transform.position.y, originalPos.z);
					//Let-s go back but smoothly!
					Vector3 goback = new Vector3 (originalPos.x, transform.position.y, transform.position.z);
					transform.position = Vector3.SmoothDamp(goback, transform.position,  ref velocity, smoothTime);
				}

            }
        }
    }

    //Version 1.0: Currently the camera shale is disabled because it was too much distracting
    public void Shake()
    {
           // shakeDuration = 0.1f; disabled. Is not very convincing, the shake seemt so slow down the game.
    }
}