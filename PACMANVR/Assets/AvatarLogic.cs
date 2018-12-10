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

    private float velocity = 0;
    private float bulletVelocity = 5;
    private float bulletSpawnDistance = 2;
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
        cameraRig.transform.localPosition = new Vector3(0, yPosForLookingDown, -2);
        rb = GetComponent<Rigidbody>();
        bulletSpawn.transform.localPosition = new Vector3(0, 0, bulletSpawnDistance);


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
                        Debug.Log("-------------------------------LEFT SWIPE--------------------------------------");
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
                                goUp();
                            } else
                            {
                                // 0 or 4
                                Debug.Log("LEFT " + numRepeatedTurns[LEFT]);
                                goLeft();
                            } 
                        }
                        prevTurn = LEFT;
                    }
                    else
                    {
                        Debug.Log("-------------------------------RIGHT SWIPE--------------------------------------");
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
                        Debug.Log("-------------------------------DOWN SWIPE--------------------------------------");
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
        transform.Translate(Vector3.forward * velocity * Time.deltaTime);

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
        // should make bullets ignore avatar, therefore preventing undefined behavior (like avatar spinning around)
        Physics.IgnoreCollision(bullet.GetComponent<Collider>(), GetComponent<Collider>());
        bullet.GetComponent<Rigidbody>().velocity = bullet.transform.forward * bulletVelocity * 10;
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
    }

    private void goRight()
    {
        newRot = Quaternion.Euler(new Vector3(0, 0, 0));
    }

    private void goDown()
    {
        newRot = Quaternion.Euler(new Vector3(0, 180, 0));
    }

    private void goUp()
    {
        newRot = Quaternion.Euler(new Vector3(0, 90, 0));
    }
}
