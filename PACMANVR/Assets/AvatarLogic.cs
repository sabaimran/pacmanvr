using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class AvatarLogic : MonoBehaviour {
    public GameObject bulletPrefab;
    public Transform bulletSpawn;
    public OVRCameraRig cameraRig;
    public Rigidbody rb;
    public Text scoreboard;
    public Text pauseMenu;

    public AudioClip powerSound;
    public AudioClip pelletSound;
    public AudioClip ghostSound;
    public AudioClip bulletSound;
    public AudioSource source;

    private float bulletVelocity = 5;
    private float bulletSpawnDistance = 2;
    private int numAmmo = 10;
    private float yPosForLookingDown = 1.5f;
    private float distanceAwayFromAvatar = 2;
    private int numPelletsCollected = 0;
    private bool wallCollision = false;
    private int speed = 5;
    private int numLives = 3;

    private string pauseText = "PAUSED.\nTo unpause, press A.\nTo quit, press X.";
    private string gameOverText = "GAME OVER. \nTo restart, press A.\nTo quit, press X.";
    private string wonGameText = "Thanks for playing!\nTo replay, press A.\nTo quit, press X";

    private bool isPaused = false;
    private bool isRotating = false;
    private bool gameOver = false;
    private bool wonGame = false;

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
        source = GetComponent<AudioSource>();
        pauseMenu.text = "";
        Time.timeScale = 1;
    }
	
	// Update is called once per frame
	void Update () {
        if (gameOver || wonGame)
        {
            rb.velocity = Vector3.zero;
            Time.timeScale = 0;
            if (Input.GetButtonDown("TouchControllerA"))
            {
                restartGame();
            } else if (Input.GetButtonDown("TouchControllerX"))
            {
                Application.Quit();
            }
            if (gameOver)
            {
                pauseMenu.text = gameOverText;
            } else
            {
                pauseMenu.text = wonGameText;
            }
        }

        if (Input.GetButtonDown("TouchControllerA") && !isPaused && !gameOver)
        {
            Time.timeScale = 0;
            isPaused = true;
            rb.velocity = (transform.forward * 0);
            pauseMenu.text = pauseText;


        } else if (Input.GetButtonDown("TouchControllerA") && isPaused && !gameOver)
        {
            // unpause
            Time.timeScale = 1;
            isPaused = false;
            speed = 5;
            rb.velocity = (transform.forward * speed);
            pauseMenu.text = "";
        }

        if (!isPaused && !gameOver)
        {
            Vector2 currSwipe = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);
            float x = currSwipe.x;
            float y = currSwipe.y;

            float diffX = Mathf.Abs(currSwipe.x - prevSwipe.x);
            float diffY = Mathf.Abs(currSwipe.y - prevSwipe.y);


            if ((diffX > thresholdForSwipes || diffY > thresholdForSwipes) && (x != 0 || y != 0))
            {
                Debug.Log("swiped controler");
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

                }
                else
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
            if (transform.position.y > 3)
            {
                transform.position = new Vector3(transform.position.x, 3, transform.position.z);
            }

            if (Input.GetButtonDown("TouchControllerB"))
            {
                if (numAmmo > 0)
                {
                    fireBullet();
                    source.clip = bulletSound;
                    source.Play();
                    numAmmo--;
                    scoreboard.text = "Score: " + numPelletsCollected.ToString() + "\nNumber of Bullets Left: " + numAmmo;
                }
            }
        } else
        {
            if (Input.GetButtonDown("TouchControllerX"))
            {
                Application.Quit();
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
        } else if (other.gameObject.name.Contains("Ghost"))
        {
            source.clip = ghostSound;
            source.Play();
            if (numLives > 0)
            {
                numLives--;
            } else
            {
                // game over
                gameOver = true;
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // because we're using rb.addforce, must have pellets' onTrigger be CHECKED so that avatar won't stop when it runs into one
        if (other.gameObject.name.Contains("Pellet"))
        {
            source.clip = pelletSound;
            source.Play();
            Destroy(other.gameObject);
            numPelletsCollected++;
            scoreboard.text = "Score: " + numPelletsCollected.ToString();
        } else if (other.gameObject.name.Contains("PowerUp"))
        {
            source.clip = powerSound;
            source.Play();
            Destroy(other.gameObject);
            numAmmo += 10;
            scoreboard.text = "Score: " + numPelletsCollected.ToString() + "\nNumber of Bullets Left: " + numAmmo;
        }

        if (numPelletsCollected == 80) {
            scoreboard.fontSize = 40;
            scoreboard.alignment = TextAnchor.UpperCenter;
            scoreboard.text = "YOU WIN!";
            wonGame = true;
        }
    }

    void restartGame()
    {
        Time.timeScale = 1;
        gameOver = false;
        numLives = 3;
        numAmmo = 10;
        speed = 5;
        rb.velocity = transform.forward * speed;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
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
