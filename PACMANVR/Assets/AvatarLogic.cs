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
        if (cameraRig != null)
        {
            //cameraRig.transform.position = Vector3.forward * -3;
        }
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
                    //cameraRig.transform.Rotate(new Vector3(0, -90, 0));
                  
                }
                else
                {
                    direction = Vector3.right;
                    cameraRig.transform.localPosition = new Vector3(2, 0, 0);
                }
            }
            else
            {
                // vertical movement over horizontal
                if (y < 0)
                {
                    direction = Vector3.back;
                    cameraRig.transform.localPosition = new Vector3(0, 0, 2);
                }
                else
                {
                    direction = Vector3.forward;
                    cameraRig.transform.localPosition = new Vector3(0, 0, -2);
                }
            }
            cameraRig.transform.LookAt(transform);
        }

        // constant velocity in one direction
        transform.Translate(direction * velocity * Time.deltaTime);



        // shooting
        if (Input.GetButtonDown("LeftTrigger"))
        {
            fireBullet();
        }

    }

    private void fireBullet()
    {
        GameObject bullet = (GameObject)Instantiate(bulletPrefab, bulletSpawn.position, bulletSpawn.rotation);
        Debug.Log("direction " + direction);
        bullet.GetComponent<Rigidbody>().velocity = direction * velocity * 10;
        Destroy(bullet, 3.0f);
    }
}
