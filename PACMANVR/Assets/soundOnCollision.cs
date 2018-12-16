using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class soundOnCollision : MonoBehaviour {

    public AudioClip sound;

	// Use this for initialization
	void Start () {
        GetComponent<AudioSource>().playOnAwake = false;
        GetComponent<AudioSource>().clip = sound;
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    void OnCollisionEnter(Collision collision)
    {
        GetComponent<AudioSource>().Play();
    }
}
