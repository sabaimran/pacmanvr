using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarLogic : MonoBehaviour {
      public GameObject bulletPrefab;
    public Transform bulletSpawn;
    public OVRCameraRig cameraRig;
    public Rigidbody rb;

    private float velocity = 2;
    private float bulletVelocity = 5;
    private float bulletSpawnDistance = 2;
    private int numAmmo = 5;
    private float yPosForLookingDown = 1.5f;
    private float distanceAwayFromAvatar = 2;

    private bool isRotating = false;

    // needed so that avatar doesn't spaz out since swipes normally last for more than one frame
    private float thresholdForSwipes = 0.2f;
    private Vector2 prevSwipe = Vector2.zero;
    private Quaternion newRot = Quaternion.identity;
    private Vector3 target;

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


        if ((diffX > thresholdForSwipes || diffY > thresholdForSwipes) && (x != 0 || y != 0))
        {
            prevSwipe = currSwipe;
            // no diagonal movement allowed, movement is in 90 degree increments
            target = transform.rotation.eulerAngles;
            if (Mathf.Abs(x) >= Mathf.Abs(y))
            {
                if (Mathf.Abs(x) > 0.7)
                {
                    // horizontal movement over vertical
                    if (x < 0)
                    {
                        Debug.Log("-------------------------------LEFT SWIPE--------------------------------------");
                        if (!isRotating)
                        {
                            isRotating = true;
                            StartCoroutine(Rotate(Vector3.up, -90, 1.0f));
                        }
                    }
                    else
                    {
                        Debug.Log("-------------------------------RIGHT SWIPE--------------------------------------");
                        if (!isRotating)
                        {
                            isRotating = true;
                            StartCoroutine(Rotate(Vector3.up, 90, 1.0f));
                        }
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
                        if (!isRotating)
                        {
                            isRotating = true;
                            StartCoroutine(Rotate(Vector3.up, 180, 1.0f));
                        }
                    }
                }
            
            }
            cameraRig.transform.LookAt(transform);
        }

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

    /* Code from: https://answers.unity.com/questions/1236494/how-to-rotate-fluentlysmoothly.html#answer-1236502 */
    IEnumerator Rotate(Vector3 axis, float angle, float duration = 1.0f)
    {
        if (isRotating)
        {
            Quaternion from = transform.rotation;
            Quaternion to = transform.rotation;
            to *= Quaternion.Euler(axis * angle);
            Debug.Log("transform.rotate is " + from);
            Debug.Log("to is " + to);

            float elapsed = 0.0f;
            while (elapsed < duration)
            {
                transform.rotation = Quaternion.Slerp(from, to, (elapsed / duration) * 2);
                elapsed += Time.deltaTime;
                Debug.Log("currently rotating ------ " + transform.rotation);
                yield return null;
            }
            transform.rotation = to;
            Debug.Log("finished rotating, transform.rotation is " + transform.rotation);
            isRotating = false;
        }
        
    }
}
