using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostAI : MonoBehaviour {
    int speed = 7;
    int rand;
    float t;
    bool left_turned;
    bool ray_activate;
    public GameObject shadow;
    RaycastHit forward_obj;
    RaycastHit left_obj;
    RaycastHit right_obj;
    bool allow_turn;
    float turn_time;

    int[] direction = new int[] { -90, 90, 180 };
    // Use this for initialization
    void Start () {
        ray_activate = true;
        t = Time.time;
        turn_time = Time.time;
        allow_turn = false;

    }

    // Update is called once per frame
    void Update()
    {

        LayerMask mask = LayerMask.GetMask("Default");
        Ray forward_ray = new Ray(transform.position - new Vector3(0, 2.5f, 1.0f), transform.forward);
        Ray left_ray = new Ray(transform.position - new Vector3(0, 2.5f, 1.0f), transform.right * -1);
        Ray right_ray = new Ray(transform.position - new Vector3(0, 2.5f, 1.0f), transform.right);
        
        transform.Translate(speed * Vector3.forward * Time.deltaTime);
       
        Debug.DrawRay(transform.position - new Vector3(0, 2.5f, 1.0f) , transform.right * 8, Color.yellow);

        Debug.DrawRay(transform.position - new Vector3(0, 2.5f, 1.0f), transform.forward * 4, Color.green);
        Debug.DrawRay(transform.position - new Vector3(0, 2.5f, 1.0f), transform.right*-1 * 8, Color.yellow);
        if (Time.time- t > 3.0f)
        {
            ray_activate = true;
            Debug.Log(ray_activate);
        }
        if (Time.time - turn_time > 1.0f)
        {
            allow_turn = true;
        }
        
        if (ray_activate)
        {
            bool turn = (Random.value > 0.25f);

            Debug.Log("activited");
                left_turned = true;
                if (!Physics.Raycast(left_ray, out left_obj, 8, mask) && turn)
                {
                    Debug.Log("left");
                    if (allow_turn)
                    {
                    transform.RotateAround(transform.position, Vector3.up, -90);
                    ray_activate = false;
                    t = Time.time;
                    left_turned = false;
                    }
                }

                if (!Physics.Raycast(right_ray, out right_obj, 8, mask) && left_turned && turn)
                {
                    Debug.Log("right");
                    if (allow_turn)
                    {
                    transform.RotateAround(transform.position, Vector3.up, 90);
                    ray_activate = false;
                    t = Time.time;
                     }
                }
            
            
            
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "MazeWall" || collision.gameObject.tag == "OuterWall")
        {
            rand = Random.Range(0, 2);
            transform.RotateAround(transform.position, Vector3.up, direction[rand]);
        }
        if (collision.gameObject.tag == "Ghost")
        {
            rand = Random.Range(0, 2);
            transform.RotateAround(transform.position, Vector3.up, 180);
        }
    }
}
