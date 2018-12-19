using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarLogic : MonoBehaviour {
    public GameObject bulletPrefab;
    public Transform bulletSpawn;
    public OVRCameraRig cameraRig;
    public Rigidbody rb;

    private float bulletVelocity = 5;
    private float bulletSpawnDistance = 2;
    private int numAmmo = 5;
    private float yPosForLookingDown = 1.5f;
    private float distanceAwayFromAvatar = 2;
    private int numPelletsCollected = 0;
    private bool wallCollision = false;
    private int speed = 5;

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
        // prevent sphere from rolling
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezeRotationY;
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

        // make sphere move + (probably) can't be in start() bc you need to account for change in directions
        rb.velocity = (transform.forward * speed);

        // make sure it doesn't start moving up walls
        if (transform.position.y > 1)
        {
            transform.position = new Vector3(transform.position.x, 1, transform.position.z);
        }
        
        if (Input.GetButtonDown("TouchControllerA"))
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
        // for avatar to not pass through maze walls, avatar needs rigid body (gravity, kinematic, isTrigger all false) and walls must have collider (isTrigger false)
        if (other.gameObject.tag == "Floor")
        {
            Physics.IgnoreCollision(other.collider, GetComponent<Collider>());
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // because we're using rb.addforce, must have pellets' onTrigger be CHECKED so that avatar won't stop when it runs into one
        if (other.gameObject.name.Contains("Pellet"))
        {
            Destroy(other.gameObject);
            numPelletsCollected++;
        }
    }

    /* Code from: https://answers.unity.com/questions/1236494/how-to-rotate-fluentlysmoothly.html#answer-1236502 */
    IEnumerator Rotate(Vector3 axis, float angle, float duration = 1.0f)
    {
        if (isRotating)
        {
            Quaternion from = transform.rotation;
            Quaternion to = transform.rotation;
            to *= Quaternion.Euler(axis * angle);

            float elapsed = 0.0f;
            while (elapsed < duration)
            {
                transform.rotation = Quaternion.Slerp(from, to, (elapsed / duration) * 2);
                elapsed += Time.deltaTime;
                yield return null;
            }
            transform.rotation = to;
            isRotating = false;
        }
        
    }
}
