using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{

    public static PlayerController instance;
    //outlet
    Rigidbody2D rigidbody;
    public Transform aimPivot;
    public GameObject projectilePrefab;
    private float speed = 10f;
    private float jump_force = 10f;
    public int jumpsLeft = 2;
    private float max_horizontal_velocity = 10f;
    private static bool playerExists;
    Animator animator;
    SpriteRenderer sprite;
    private int isJump = 0;
    private int isClimb = 0;
    private int isShoot = 0;

    public int numKeys;
    public int maxKeys = 6;
    public bool[] keysFound = { false, false, false, false, false, false };

    void Awake()
    {
        //This works but we need to attach the one instance to the MapManager
        if (instance != null)
        {
            PlayerPrefs.SetInt("numKeys", instance.numKeys);
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
        rigidbody = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        DontDestroyOnLoad(gameObject);
       
    }

    void FixedUpdate()
    {
        animator.SetFloat("speed", rigidbody.velocity.magnitude);
        animator.SetInteger("isJump", isJump);
        animator.SetInteger("isClimb", isClimb);
        animator.SetInteger("isShoot", isShoot);
    }


    // Update is called once per frame
    void Update()
    {
        //Vector3 cameraCenter 
        GameObject.Find("Main Camera").transform.position = new Vector3(transform.position.x, transform.position.y, -10);
        Vector2 cur_velocity = rigidbody.velocity;

        if (Input.GetKey(KeyCode.A))
        {
            if (cur_velocity.x - speed >= -max_horizontal_velocity)
            {
                rigidbody.AddForce(Vector2.left * speed);
            }
            else if (max_horizontal_velocity + cur_velocity.x > 0)
            {
                rigidbody.AddForce(Vector2.left * (max_horizontal_velocity + cur_velocity.x));
            }
            sprite.flipX = true;
        }
        if (Input.GetKey(KeyCode.D))
        {
            if (speed + cur_velocity.x <= max_horizontal_velocity)
            {
                rigidbody.AddForce(Vector2.right * speed);
            }
            else if (max_horizontal_velocity - cur_velocity.x > 0)
            {
                rigidbody.AddForce(Vector2.right * (max_horizontal_velocity - cur_velocity.x));
            }
            sprite.flipX = false;
        }
        if (Input.GetKey(KeyCode.W))
        {
            isClimb = 1;
            if (cur_velocity.y - speed >= -max_horizontal_velocity)
            {
                rigidbody.AddForce(Vector2.up * speed);
            }
            else if (max_horizontal_velocity + cur_velocity.y > 0)
            {
                rigidbody.AddForce(Vector2.up * (max_horizontal_velocity + cur_velocity.y));
            }

        }
        if (Input.GetKey(KeyCode.S))
        {
            isClimb = 1;
            if (speed + cur_velocity.y <= max_horizontal_velocity)
            {
                rigidbody.AddForce(Vector2.down * speed);
            }
            else if (max_horizontal_velocity - cur_velocity.y > 0)
            {
                rigidbody.AddForce(Vector2.down * (max_horizontal_velocity - cur_velocity.y));
            }

        }
        else
        {
            isClimb = 0;
        }

        // Aim Toward Mouse
        Vector3 mousePosition = Input.mousePosition;
        Vector3 mousePositionInWorld = Camera.main.ScreenToWorldPoint(mousePosition);
        Vector3 directionFromPlayerToMouse = mousePositionInWorld - transform.position;

        float radiansToMouse = Mathf.Atan2(directionFromPlayerToMouse.y, directionFromPlayerToMouse.x);
        float angleToMouse = radiansToMouse * 180f / Mathf.PI;

        aimPivot.rotation = Quaternion.Euler(0, 0, angleToMouse);

        // Shoot Stuff
        if (Input.GetMouseButtonDown(0))
        {
            isShoot = 1;
            GameObject newProjectile = Instantiate(projectilePrefab);
            newProjectile.transform.position = transform.position;
            newProjectile.transform.rotation = aimPivot.rotation;
        }

        // Double Jump
        if (Input.GetKeyDown(KeyCode.Space) && jumpsLeft > 0)
        {
            isJump = 1;
            rigidbody.AddForce(new Vector2(0, jump_force), ForceMode2D.Impulse);
            jumpsLeft--;
        }
        else
        {
            isShoot = 0;
            isJump = 0;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            // Check beneath
            RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, -transform.up, 0.7f);

            for (int i = 0; i < hits.Length; ++i)
            {
                if (hits[i].collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
                {
                    jumpsLeft = 2;
                }
            }
        }
        else if (collision.gameObject.CompareTag("Key"))
        {
            int keyIndex = sceneNameToInt();
            instance.keysFound[keyIndex] = true;
            instance.numKeys += 1;
            Destroy(collision.gameObject);
        }
    }

    private int sceneNameToInt()
    {
        string s = SceneManager.GetActiveScene().name;
        return s == "A" ? 0 : s == "B" ? 1 : s == "C" ? 2 : s == "D" ? 3 : s == "E" ? 4 : 5;
    }
}
