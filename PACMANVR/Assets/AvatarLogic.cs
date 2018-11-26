using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarLogic : MonoBehaviour {

    private float velocity = 10;
    private Vector3 direction;

	// Use this for initialization
	void Start () {
        direction = Vector3.forward;
	}
	
	// Update is called once per frame
	void Update () {
        // constant velocity in one direction
        transform.Translate(direction * velocity * Time.deltaTime);

        float x = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).x;
        float y = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).y;

        // no diagonal movement allowed, movement is in 90 degree increments
        if (Mathf.Abs(x) >= Mathf.Abs(y))
        {
            // horizontal movement over vertical
            if (x < 0)
            {
                direction = Vector3.left;
            }
            else
            {
                direction = Vector3.right;
            }
        } else
        {
            // vertical movement over horizontal
            if (y < 0)
            {
                direction = Vector3.down;
            } else
            {
                direction = Vector3.up;
            }
        }

    }
}
