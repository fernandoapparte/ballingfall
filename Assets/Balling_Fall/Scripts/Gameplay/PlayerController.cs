using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BallingFallGame;


public enum PlayerState
{
    Prepare,
    Living,
    Pause,
    PassLevel,
    Die,
}

public class PlayerController : MonoBehaviour {

    public LayerMask layerCollision;

    public static PlayerController Instance { private set; get; }
    public static event System.Action<PlayerState> PlayerStateChanged = delegate { };

    public PlayerState PlayerState
    {
        get
        {
            return playerState;
        }

        private set
        {
            if (value != playerState)
            {
				playerState = value;
				if (PlayerStateChanged!=null) {
					PlayerStateChanged(playerState);	
				} else {
					Debug.Log ("ZZZ: No one is suscribed to PlayerController event!!!");
				}
                
            }
        }
    }
    private PlayerState playerState = PlayerState.Living;


    [Header("Player Config")]
    [SerializeField] private float jumpVelocity = 12;
    [SerializeField] private float fallingSpeed = -30;
    [SerializeField] private float minScale = 0.8f;
    [SerializeField] private float maxScale = 1.2f;
    [SerializeField] private float scalingFactor = 2;

    [Header("Player References")]
    [SerializeField] private HelixDetector helixDetector;
    [SerializeField] private ParticleSystem ballExplode;
    [SerializeField] private MeshRenderer meshRender;

    public float TargetY { private set; get; }

    private RaycastHit hit;
    private Vector3 originalScale = Vector3.zero;
    private float currentJumpVelocity = 0;
    private Vector3 pointInitial = Vector3.zero;
    private Vector3 pointTarget = Vector3.zero;


    private void OnEnable()
    {
		//Debug.Log ("ZZZ:PlayerController enabled");
        GameManager.GameStateChanged += GameManager_GameStateChanged;
    }
    private void OnDisable()
    {
		//Debug.Log ("ZZZ:PlayerController disabled");
        GameManager.GameStateChanged -= GameManager_GameStateChanged;
    }
    private void GameManager_GameStateChanged(GameState obj)
    {
        if (obj == GameState.Playing)
        {
            PlayerLiving();
        }
        else if (obj == GameState.Pause)
        {
            PlayerPause();
        }
    }
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
    void OnDestroy()
    {
		//Debug.Log ("ZZZ:PlayerController destroyed");
        if (Instance == this)
        {
			//Debug.Log ("ZZZ:PlayerController instance nulled");
            Instance = null;
        }
    }

    void Start () {

        PlayerState = PlayerState.Prepare; //Fire Event

        //Add another actions here
        currentJumpVelocity = jumpVelocity;
        TargetY = transform.position.y;
        originalScale = transform.localScale;
        ballExplode.gameObject.SetActive(false);
    }

    void Update () {

        if (playerState != PlayerState.Living && playerState != PlayerState.PassLevel) {
            //Do nothing while the player isn't living or passed a level
            return;
        }
        
        float deltaTime = Time.deltaTime; //saving delta for avoding changed values

        if(currentJumpVelocity + fallingSpeed * deltaTime < 0)
        {
            pointInitial = transform.position;
        }

        transform.position = transform.position + Vector3.up * (currentJumpVelocity * deltaTime + fallingSpeed * deltaTime * deltaTime / 2);

        if (currentJumpVelocity < fallingSpeed)
            currentJumpVelocity = fallingSpeed;
        else
            currentJumpVelocity = currentJumpVelocity + fallingSpeed * deltaTime;

        if (currentJumpVelocity > 0) //If the player is going up...
        {
            //Change the scale
            Vector3 scale = transform.localScale;
            if (scale.x < maxScale)
            {
                scale.x += scalingFactor * deltaTime;
            }
            else
                scale.x = maxScale;
            transform.localScale = scale;
        }
        else //The player is going down
        {
            Vector3 scale = transform.localScale;
            if (scale.x > minScale)
            {
                scale.x -= scalingFactor * deltaTime;
            }
            else
                scale.x = minScale;
            transform.localScale = scale;

            if (transform.position.y < TargetY)
            {
                TargetY = transform.position.y;
            }

            #region Documentation

            //Check colliding
            //20191004: Some explanations & math here.
            //meshRender commands Mesh Filter to render the proper object on screen. (See Player object you will find both)
            //This will allow you in later code version to define which object to render and switch to another objects (cube, cylinder , etc.)
            //transform.position give us where is the player at this current moment. The problem here is this coordinate is at the very object center.
            //That is not useful to calculate a collision. In order to calculate a collision, we'll use a trick, calculating where is the player's
            //border.

            //The y-size of the object is coming from meshRender.bounds.size.y , but if we think the player is a sphere, the distance from center
            // (transform.position) to the border is half of this size or meshRender.bounds.size.y /2;
            // Pay attention if we are talking about a sphere it won't matter if we use size.y or x or z, it will be the same, but it can change if we 
            // use different objects. 
            // Finally, we need check collision vertically at the bottom of the player. To proper calculate that we need Vector3.up but reversed 
            // So pointTarget give us with very good approximation the bottom border of the player. We add a 0.1f to make sure the collision is visible , otherwise would be too perfect.

            //Summing up: pointTarget is used to find the bottom place of the player. Ok, now we have it we need to find out if there were collision.
            //What is the way?
            //Pyshics.Raycast trace a line to find which objects collides
            //https://docs.unity3d.com/ScriptReference/Physics.Raycast.html
            //We are using it in this way:
            //bool Raycast(Vector3 origin, Vector3 direction, float maxDistance = Mathf.Infinity, int layerMask = DefaultRaycastLayers)
            //First is the origin, second is the OTHER point which traces a line from one to another, in our case this is a perfectly vertical line
            //from the player center to the player bottom , making a segment slightly shorter (remember the 0.1f and it is not adding).
            //Ok, and what about layerCollision? This is one of the most important parts! We are selectively saying:
            //'Let's ignore any collision which is not in the layer that I will specify'
            //In this game, we only check collision with 'solid' layer (this is specified in the Player object, check this, whithout it the player
            //cant' work). 
            //FIRST RULE: EVERY OBJECT where the player will collide needs to be in the solid layer(!!!!!).

            //Ok, I understand how it works, but WHY we are using 3 hits?
            //This is also simple, however it needs to be revisited:
            //I we only check collision to down, the ball will fall if there are no platform and that is fine.
            //But what happen if the ball is 'slightly' in the border? Let's say we are 2 pixel off from the border, in the air.
            //In that case, we won't have a hit, and the ball will fall down. In that case, the ball is going 'passing through' the objects.
            //But this game won't work in this way, since the ball only bounds vertically.
            //In order to 'simulate' the player detects the border, we need to be somewhat tolerant to the border. This is a compromise and makes the
            // the game more or less 'shitty': the ball will bound where is no floor, because it detects the player's border is not far enough.
            //That is the reason we need two additional vectors: check the border to the right & left to provide some sense of better border checking.

            //One thing we could do is some adjustment to be tolerant to the left nd right, just we did when checking the floor.

            //Ok, we need 3 hits to check our collision.
            //Can we improve it?
            //Yes: ther is a Physics.SphereCast to check for any collision inside a sphere. That would cut the calculation to at minimum.
            //It would be interesting to check this out. I don't recall if this was previoulsly implemented, but It should simplify the process a bit.

            //UPDATE: as a second thought, SphereCast won't work and this is why: the current way of detecting is in fact 'proyecting' 
            //circle on the player's bottom to check if is touching a border properly. If we use SphereCast, the effect would be entirely different, no projection but checking collision from some distance from object's center. So it won't work. I hope I understand this properly.

            #endregion
            
            pointTarget = transform.position - (Vector3.up * ((meshRender.bounds.size.y / 2) + 0.1f));
            float dst = Vector3.Distance(pointInitial, pointTarget); //Measure distance between the points
            float length = meshRender.bounds.size.x / 5; //Get a distance compromise to calculate borders left and right (initially was /4)
            //Check if we have a floor under the player
            bool isHit = Physics.Raycast(pointInitial, pointTarget, out hit, dst, layerCollision);
            //Check if we have some floor to the right
            if (!isHit)
            {
                isHit = Physics.Raycast(pointInitial + Vector3.right * length,
                                        pointTarget + Vector3.right * length, out hit, dst, layerCollision);
            }
            //Check if we have some floor to the left
            if (!isHit)
            {
                isHit = Physics.Raycast(pointInitial - Vector3.right * length,
                                        pointTarget - Vector3.right * length, out hit, dst, layerCollision);
            }

            if (isHit)
            {
                currentJumpVelocity = jumpVelocity;
                TargetY = hit.point.y;  //Find the y hit point
                //And make a point slightly above of the hit , so the splat is not inside the collided object.
                Vector3 pos = hit.point + Vector3.up * 0.01f;
                //Make a splat over the collided object.
                GameManager.Instance.CreateBallSplat(pos, meshRender.material.color, hit.collider.transform);
                //Initiate also a particle effect to make it slightly better.
                GameManager.Instance.PlaySplatShatter(pos);

                //Ok we collided, against what?
                //We'll check the tags for the collide object to define what to do next

                //If the player is not alive, return and do nothing else.
                if (playerState != PlayerState.Living) { return; }

                if (hit.collider.CompareTag("Respawn")) //Hit the bottom platform , we won the level
                {
                    Vector3 fadingHelixPos = hit.transform.position + Vector3.down * 0.05f;
                    GameManager.Instance.CreateFadingHelix(fadingHelixPos);
                    PlayerState = PlayerState.PassLevel;  //Fire event
                    // GameManager.Instance.PassLevel();
                }
                else
                {
                    //The ball has fallen several levels, explode the helix
                    if (helixDetector.PassedCount >= GameManager.Instance.PassedCountForBreakHelix)
                    {
                        SoundManager.Instance.PlaySound(SoundManager.Instance.SMC.breakPieces);
                        hit.collider.transform.parent.GetComponent<HelixController>().ShatterAllPieces();
                        CameraController.Instance.Shake();
                    }
                    else
                    {
                        //The player just fell one level.
                        if (hit.collider.CompareTag("Finish")) //Hit dead piece -> game over
                        {
                            PlayerDies();
                        }
                        else
                        {
                            SoundManager.Instance.PlaySound(SoundManager.Instance.SMC.bounce);
                        }
                    }
                }

                helixDetector.ResetPassedCount();
            }
        }
              
	}

    private void PlayerLiving()
    {
        PlayerState = PlayerState.Living;         //Fire event

        //Add another actions here
        meshRender.enabled = true;
        if (GameManager.Instance.IsRevived)
        {
		//	Debug.Log ("XYZ:PlayerLiving fired and revived");
            currentJumpVelocity = jumpVelocity;
		} else {
			//Debug.Log ("XYZ:PlayerLiving fired and NOTrevived");
		}
    }



    private void PlayerPause()
    {
        PlayerState = PlayerState.Pause;         //Fire event
    }

    /// <summary>
    /// What's going on when the player dies
    /// </summary>
    public void PlayerDies()
    {
        PlayerState = PlayerState.Die; //Fire event in GameManager, check that out.

        SoundManager.Instance.PlaySound(SoundManager.Instance.SMC.playerExplode);//Play a sound of dying
        meshRender.enabled = false; //Hide player object
        transform.localScale = originalScale; //I don't exactly what is doing
        StartCoroutine(PlayBallExplode()); //Explode the ball.
    }


    //Play ball explode particle
    private IEnumerator PlayBallExplode()
    {
        ballExplode.transform.position = transform.position;
        ballExplode.gameObject.SetActive(true);
        ballExplode.Play();
        yield return new WaitForSeconds(ballExplode.main.startLifetimeMultiplier);
        ballExplode.gameObject.SetActive(false);
    }

    /// <summary>
    /// Set color for this ball (player)
    /// </summary>
    /// <param name="color"></param>
    public void SetBallColor(Color color)
    {
        meshRender.sharedMaterial.color = color;
    }
}
