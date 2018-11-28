using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarLogic : MonoBehaviour {

    private float velocity = 2;
    private Vector3 direction;
    int numAmmo = 5;
    public GameObject bulletPrefab;
    public Transform bulletSpawn;
    public OVRCameraRig cameraRig;

	// Use this for initialization
	void Start () {
        direction = Vector3.forward;
	}
	
	// Update is called once per frame
	void Update () {

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
                    cameraRig.transform.localPosition = new Vector3(-2, 0, 0);
                    // must change bullet spawn location to be in front of avatar by a little bit or else the bullets will collide with avatar and become stuck
                    bulletSpawn.transform.localPosition = new Vector3(1, 0, 0);
                  
                }
                else
                {
                    direction = Vector3.right;
                    cameraRig.transform.localPosition = new Vector3(2, 0, 0);
                    bulletSpawn.transform.localPosition = new Vector3(-1, 0, 0);
                }
            }
            else
            {
                // vertical movement over horizontal
                if (y < 0)
                {
                    direction = Vector3.back;
                    cameraRig.transform.localPosition = new Vector3(0, 0, 2);
                    bulletSpawn.transform.localPosition = new Vector3(0, 0, -1);
                }
                else
                {
                    direction = Vector3.forward;
                    cameraRig.transform.localPosition = new Vector3(0, 0, -2);
                    bulletSpawn.transform.localPosition = new Vector3(0, 0, 1);
                }
            }
            cameraRig.transform.LookAt(transform);
        }

        // constant velocity in one direction
        transform.Translate(direction * velocity * Time.deltaTime);


        // shooting
        if (Input.GetButtonDown("LeftTrigger"))
        {
            if (numAmmo > 0)
            {
                fireBullet();
                // TODO: how much ammo should player start off with?  
                numAmmo--;
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
}
