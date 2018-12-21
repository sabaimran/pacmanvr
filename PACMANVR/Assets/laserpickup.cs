using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class laserpickup : MonoBehaviour
{

    public float deploymentHeight = 5.0f;
    private LineRenderer laserPointer;
    private bool isLaserOn;
    private bool dropped;
    private Transform body;
    private Transform righthand;

    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetButtonDown("TouchControllerA"))
        {
            Debug.Log("pressed A in start room");
            SceneManager.LoadScene("Maze");
        }
        if (Input.GetButtonDown("TouchControllerB")) {
            Application.Quit();
        }

    }
}
