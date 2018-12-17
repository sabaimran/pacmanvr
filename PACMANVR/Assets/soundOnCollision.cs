using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class soundOnCollision : MonoBehaviour {

    public AudioSource source;

	// Use this for initialization
	void Start () {
        source = GetComponent<AudioSource>();
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Bullet")
            source.Play();
    }
}
