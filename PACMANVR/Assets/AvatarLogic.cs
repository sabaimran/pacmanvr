using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarLogic : MonoBehaviour {
    public GameObject bulletPrefab;
    public Transform bulletSpawn;
    public OVRCameraRig cameraRig;
    public Rigidbody rb;

    private float velocity = 2;
    private Vector3 direction;
    private int numAmmo = 5;
    private float yPosForLookingDown = 1.5f;
    private float distanceAwayFromAvatar = 2;

	// Use this for initialization
	void Start () {
        direction = Vector3.forward;
        cameraRig.transform.localPosition = new Vector3(0, yPosForLookingDown, -2);
        rb = GetComponent<Rigidbody>();
    }
	
	// Update is called once per frame
	void Update () {
        // left controller thumbstick/dpad position
        float x = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).x;
        float y = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).y;

        if (x != 0 && y != 0)
        {
            // no diagonal movement allowed, movement is in 90 degree increments
            if (Mathf.Abs(x) >= Mathf.Abs(y))
            {
                // horizontal movement over vertical
                if (x < 0)
                {
                    direction = Vector3.left;
                    cameraRig.transform.localPosition = new Vector3(distanceAwayFromAvatar, yPosForLookingDown, 0);
                    // must change bullet spawn location to be in front of avatar by a little bit or else the bullets will collide with avatar and become stuck
                    bulletSpawn.transform.localPosition = new Vector3(1, 0, 0);
                  
                }
                else
                {
                    direction = Vector3.right;
                    cameraRig.transform.localPosition = new Vector3(-distanceAwayFromAvatar, yPosForLookingDown, 0);
                    bulletSpawn.transform.localPosition = new Vector3(-1, 0, 0);
                }
            }
            else
            {
                // vertical movement over horizontal
                if (y < 0)
                {
                    direction = Vector3.back;
                    cameraRig.transform.localPosition = new Vector3(0, yPosForLookingDown, distanceAwayFromAvatar);
                    bulletSpawn.transform.localPosition = new Vector3(0, 0, -1);
                }
                else
                {
                    direction = Vector3.forward;
                    cameraRig.transform.localPosition = new Vector3(0, yPosForLookingDown, -distanceAwayFromAvatar);
                    bulletSpawn.transform.localPosition = new Vector3(0, 0, 1);
                }
            }
            cameraRig.transform.LookAt(transform);
        }

        // constant velocity in one direction
        transform.Translate(direction * velocity * Time.deltaTime);
//        rb.AddForce(direction * velocity);


        if (Input.GetButtonDown("LeftTrigger"))
        {
            Debug.Log("Left trigger pressed");
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
        Debug.Log("collided");
    }

    void OnTriggerEnter(Collider other)
    {
        // for avatar to not pass through maze walls, avatar needs rigid body (gravity, kinematic, isTrigger all false) and walls must have collider (isTrigger false)
        Debug.Log("triggered");
    }
}
