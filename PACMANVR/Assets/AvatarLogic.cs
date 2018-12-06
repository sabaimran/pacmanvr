using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarLogic : MonoBehaviour {
    public const int LEFT = 0;
    public const int RIGHT = 1;
    public const int UP = 2;
    public const int DOWN = 3;
    public const int TOTAL_NUM_OF_TURNS = 4;

    public GameObject bulletPrefab;
    public Transform bulletSpawn;
    public OVRCameraRig cameraRig;
    public Rigidbody rb;

    private float velocity = 2;
    private Vector3 direction;
    private int numAmmo = 5;
    private float yPosForLookingDown = 1.5f;
    private float distanceAwayFromAvatar = 2;

    // needed so that avatar doesn't spaz out since swipes normally last for more than one frame
    private float thresholdForSwipes = 0.2f;
    private Vector2 prevSwipe = Vector2.zero;
    private Quaternion newRot = Quaternion.identity;
    private bool inProgRot = false;
    float time = 0;

    int[] numRepeatedTurns = { 0, 0, 0, 0 };
    int prevTurn = -1;

    private Vector3[] potentialLeftRots = { new Vector3(0, 270, 0), new Vector3(0, 180, 0), new Vector3(0, 0, 0) };

    // MAJOR ISSUE - if bullets and avatar collide, then the avatar will spin around and do undefined stuff.
    /*
     * TO DO:
     * - rotation sort of works 
     * - fix bullets (do i still need direction?)
     * */
    void Start () {
        direction = Vector3.forward;
        cameraRig.transform.localPosition = new Vector3(0, yPosForLookingDown, -2);
        rb = GetComponent<Rigidbody>();

        // should make bullets ignore avatar, therefore preventing undefined behavior (like avatar spinning around)
        Physics.IgnoreCollision(bulletPrefab.GetComponent<Collider>(), GetComponent<Collider>());
    }
	
	// Update is called once per frame
	void Update () {
       
        Vector2 currSwipe = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);
        float x = currSwipe.x;
        float y = currSwipe.y;

        float diffX = Mathf.Abs(currSwipe.x - prevSwipe.x);
        float diffY = Mathf.Abs(currSwipe.y - prevSwipe.y);

        time += Time.deltaTime;

        if ((diffX > thresholdForSwipes || diffY > thresholdForSwipes) && (x != 0 || y != 0))
        {
            time = 0;
            prevSwipe = currSwipe;
            // no diagonal movement allowed, movement is in 90 degree increments

            if (Mathf.Abs(x) >= Mathf.Abs(y))
            {
                if (Mathf.Abs(x) > 0.7)
                {
                    // horizontal movement over vertical
                    if (x < 0)
                    {
                        if (prevTurn != LEFT)
                        {
                            goLeft();
                        } else
                        {
                            numRepeatedTurns[LEFT] += 1;
                            if ((numRepeatedTurns[LEFT] % TOTAL_NUM_OF_TURNS) == 1)
                            {
                                Debug.Log("DOOWN " + numRepeatedTurns[LEFT]);
                                goDown();
                            } else if ((numRepeatedTurns[LEFT] % TOTAL_NUM_OF_TURNS) == 2)
                            {
                                Debug.Log("RIGHT " + numRepeatedTurns[LEFT]);
                                goRight();
                            } else if ((numRepeatedTurns[LEFT] % TOTAL_NUM_OF_TURNS) == 3)
                            {
                                Debug.Log("Up " + numRepeatedTurns[LEFT]);
                                goRight();
                            } else
                            {
                                // 0 or 4
                                Debug.Log("UP " + numRepeatedTurns[LEFT]);
                                goUp();
                            } 
                        }
                        prevTurn = LEFT;
                    }
                    else
                    {
                        //Debug.Log("pressed right");
                        direction = Vector3.right;
                        Debug.Log("pressed right ");
                        if (prevTurn != RIGHT)
                        {
                            goRight();
                        }
                        else
                        {
                            numRepeatedTurns[RIGHT] += 1;
                            if ((numRepeatedTurns[RIGHT] % TOTAL_NUM_OF_TURNS) == 1)
                            {
                                goDown();
                            }
                            else if ((numRepeatedTurns[RIGHT] % TOTAL_NUM_OF_TURNS) == 2)
                            {
                                goLeft();
                            }
                            else if ((numRepeatedTurns[RIGHT] % TOTAL_NUM_OF_TURNS) == 3)
                            {
                                goRight();
                            }
                            else
                            {
                                // 0 or 4
                                goUp();
                            }
                        }
                        prevTurn = RIGHT;
                    }
                }

            } else
            {
                if (Mathf.Abs(y) > 0.7)
                {
                    // vertical movement over horizontal
                    if (y < 0)
                    {
                        direction = Vector3.back;
                        Debug.Log("pressed down ");
                        if (prevTurn != DOWN)
                        {
                            goDown();
                        }
                        else
                        {
                            numRepeatedTurns[DOWN] += 1;
                            if ((numRepeatedTurns[DOWN] % TOTAL_NUM_OF_TURNS) == 1 || (numRepeatedTurns[DOWN] % TOTAL_NUM_OF_TURNS == 3))
                            {
                                goRight();
                            } else
                            {
                                goDown();
                            }
                        }
                        prevTurn = DOWN;
                    }
                }
            
            }
            
            cameraRig.transform.LookAt(transform);
        }

        transform.localRotation = Quaternion.Slerp(transform.localRotation, newRot, 40f * Time.deltaTime);

        // constant velocity in one direction
        direction = Vector3.forward;
        transform.Translate(direction * velocity * Time.deltaTime);

        if (Input.GetButtonDown("LeftTrigger"))
        {
            if (numAmmo > 0)
            {
                fireBullet();
                // TODO: how much ammo should player start off with?  
                //numAmmo--;
            }
        }

    }

    private void fireBullet()
    {
        GameObject bullet = (GameObject)Instantiate(bulletPrefab, bulletSpawn.position, bulletSpawn.rotation);
        if (direction.z == 0)
        {
            // was moving left or right
            bullet.GetComponent<Rigidbody>().velocity = direction * -1 * velocity * 10;
        } else
        {
            bullet.GetComponent<Rigidbody>().velocity = direction * velocity * 10;
        }
        
        Destroy(bullet, 3.0f);
    }

    void OnCollisionEnter(Collision other)
    {
        //Debug.Log("collided");
    }

    void OnTriggerEnter(Collider other)
    {
        // for avatar to not pass through maze walls, avatar needs rigid body (gravity, kinematic, isTrigger all false) and walls must have collider (isTrigger false)
        //Debug.Log("triggered");
    }

    private void goLeft()
    {
        newRot = Quaternion.Euler(new Vector3(0, 270, 0));
        direction = new Vector3(-1, 0, 0);
        // must change bullet spawn location to be in front of avatar by a little bit or else the bullets will collide with avatar and become stuck
        bulletSpawn.transform.localPosition = new Vector3(-1, 0, 0);
    }

    private void goRight()
    {
        newRot = Quaternion.Euler(new Vector3(0, 0, 0));
        direction = new Vector3(1, 0, 0);
        //bulletSpawn.transform.localPosition = new Vector3(-1, 0, 0);
        bulletSpawn.transform.localPosition = new Vector3(1, 0, 0);
    }

    private void goDown()
    {
        newRot = Quaternion.Euler(new Vector3(0, 180, 0));
        //direction = Vector3.back;
        //bulletSpawn.transform.localPosition = new Vector3(0, 0, -1);
        direction = Vector3.back;
        bulletSpawn.transform.localPosition = new Vector3(0, 0, 1);
    }

    private void goUp()
    {
        newRot = Quaternion.Euler(new Vector3(0, 90, 0));
        direction = Vector3.forward;
        //bulletSpawn.transform.localPosition = new Vector3(0, 0, 1);
        bulletSpawn.transform.localPosition = new Vector3(0, 0, -1);
    }
}
