using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class laserpickup : MonoBehaviour
{
    /*
     * TODO
     * Figure out laser placement
    */
    public float deploymentHeight = 5.0f;
    private LineRenderer laserPointer;
    private bool isLaserOn;
    private bool dropped;
    private Transform body;
    private Transform righthand;
/*    public AudioClip laserSound;
    private AudioSource source;
    */
    /*
    private void Awake()
    {
        source = gameObject.GetComponent<AudioSource>();
        source.clip = laserSound;
        source.volume = 0.2f;
        source.Stop();
    }*/

    // Use this for initialization
    void Start()
    {
        dropped = true;
        laserPointer = GetComponent<LineRenderer>();
        laserPointer.startWidth = 0.05f;
        laserPointer.endWidth = 0.05f;
        isLaserOn = false;
        righthand = transform.parent;
        transform.parent = null;

        Debug.Log(laserPointer.transform.position);
        //transform.position = new Vector3(0, 1, 1);
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit hit;
        Vector3 orientation = new Vector3(transform.localRotation.x, transform.localRotation.y, transform.localRotation.z);
        Ray landingRay = new Ray(transform.position, transform.TransformDirection(Vector3.forward));
        //Debug.Log(laserPointer.transform.position);

        if (Input.GetButtonDown("TouchControllerA"))
        {
            Debug.Log("pressed A");
        }

        if (Input.GetButtonDown("TouchControllerA") && !isLaserOn)
        {
            Debug.Log("pressed A and holding something");
            transform.parent = righthand;
            transform.localPosition = Vector3.zero;
            laserPointer.SetPosition(0, transform.position);
            isLaserOn = true;
            dropped = false;
            laserPointer.enabled = true;
            //source.Play();
        }
        else if (isLaserOn)
        {
            laserPointer.SetPosition(0, transform.position);
            if (Input.GetButtonDown("TouchControllerA") && !dropped)
            {
                Debug.Log("pressed A with laser on");
                transform.position = righthand.position;
                transform.parent = null;
                isLaserOn = false;
                dropped = true;
                laserPointer.enabled = false;
                //source.Stop();
            }
            else if (Physics.Raycast(landingRay, out hit))
            {
                laserPointer.SetPosition(1, hit.point);
                if (hit.collider.tag == "quit")
                {
                    Application.Quit();
                }
                else if (hit.collider.tag == "start")
                {
                    Debug.Log("start game");
                    dropped = true;
                    isLaserOn = false;
                    SceneManager.LoadScene("CS498HW4");
                }
            }
        }
    }

    private void OnTriggerStay(Collider collision)
    {
        dropped = false;
    }
}
