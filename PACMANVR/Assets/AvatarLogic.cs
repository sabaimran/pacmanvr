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
    private int numPelletsCollected = 0;
    private bool wallCollision = false;

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
       // rb.angularVelocity = new Vector3(0.0f, 0.0f, 0.0f);
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
            if (wallCollision)
            {
//                rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezeRotationY;
//                rb.AddForce(Vector3.forward * 2);
            } else
            {
                velocity = 2;
            }

            RaycastHit hit;
            /*if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity))
            {
                Debug.Log("hit");
            } else
            {
                Debug.Log("did not hit");
            } */

            if (Mathf.Abs(x) >= Mathf.Abs(y))
            {
                if (Mathf.Abs(x) > 0.7)
                {
                    // horizontal movement over vertical
                    if (x < 0)
                    {
                        if (!isRotating)
                        {
                            isRotating = true;
                            StartCoroutine(Rotate(Vector3.up, -90, 1.0f));
                        }
                    }
                    else
                    {
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
 //       rb.AddForce(Vector3.forward * velocity * Time.deltaTime);

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
        // Detects walls, but keeps moving through them.
        if (other.gameObject.name.Contains("Pellet")) {
            Destroy(other.gameObject);
            numPelletsCollected++;
        } else if (other.gameObject.CompareTag("OuterWall") || other.gameObject.CompareTag("MazeWall"))
        {
            Debug.Log("registered");
            wallCollision = true;
            velocity = 0;
        }
    }

    void OnCollisionStay(Collision collision)
    {
        //        Debug.Log("hmmm00");
        //    rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezeRotationY;
        //    rb.AddForce(Vector3.forward * 15, ForceMode.VelocityChange);

        /*     Vector3 heading = collision.transform.position - transform.position;
                    float dot = Vector3.Dot(heading.normalized, transform.forward);
                    if  (dot == 1)
                    {
                        Debug.Log("facing wall00");
                    } else if (dot == -1)
                    {
                        Debug.Log("facing AWY from wall");
                    } else
                    {
                        Debug.Log("facing wall side");
                    } */
        Ray ray = new Ray(transform.position, (collision.transform.position - transform.position).normalized * 10);
        RaycastHit hit;
        int layerMask = 1 << 9;
        layerMask = ~layerMask;

        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity, layerMask))
        {
            Debug.Log("hit " + collision.gameObject.tag);
            if (hit.collider.gameObject.tag == "OuterWall" || hit.collider.gameObject.tag == "MazeWall")
            {
                Debug.Log("HIT THE FUCKING WALL");
            } else
            {
               // Debug.Log("NOT HITTING THE ASJFLSFJFSLFJL WALL");
            }
        }

    }

       private void OnCollisionExit(Collision collision)
       {
           wallCollision = false;
   //        velocity = 2;
   //        transform.Translate(Vector3.forward * velocity * Time.deltaTime);
       } 

    void OnTriggerEnter(Collider other)
    {
        // for avatar to not pass through maze walls, avatar needs rigid body (gravity, kinematic, isTrigger all false) and walls must have collider (isTrigger false)
        Debug.Log("triggered");
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
 //           Debug.Log("transform.rotate is " + from);
  //          Debug.Log("to is " + to);

            float elapsed = 0.0f;
            while (elapsed < duration)
            {
                transform.rotation = Quaternion.Slerp(from, to, (elapsed / duration) * 2);
                elapsed += Time.deltaTime;
 //               Debug.Log("currently rotating ------ " + transform.rotation);
                yield return null;
            }
            transform.rotation = to;
 //           Debug.Log("finished rotating, transform.rotation is " + transform.rotation);
            isRotating = false;
        }
        
    }
}
