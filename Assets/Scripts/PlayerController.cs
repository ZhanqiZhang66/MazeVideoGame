using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using CodeMonkey.Utils;
public class PlayerController : MonoBehaviour
{

    public static PlayerController instance;
    //outlet
    public Rigidbody2D rigidbody;
    public Transform aimPivot;
    public GameObject projectilePrefab;
    private float speed = 10f;
    private float jump_force = 10f;
    public int jumpsLeft = 2;
    public float max_horizontal_velocity = 10f;
    private static bool playerExists;
    Animator animator;
    SpriteRenderer sprite;
    public int numKeys;
    private GameObject MazeGenObj = null;

    private float health = 0f;
    private float tempTime;
    public float minDistance;
    ////////

    private Healthbar oxygenBar;
    public float oxygen = 1.0f;
    public float oxygenShotCost = 0.02f;
    public int numBubbles = 5;

    public Sprite[] compass;


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
            oxygenBar = transform.Find("OxygenBar").gameObject.GetComponent<Healthbar>();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        rigidbody = GetComponent<Rigidbody2D>();
        oxygenBar.SetSize(oxygen);
        oxygenBar.SetColor(Color.blue);
        animator = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
        MazeGenObj = GameObject.FindWithTag("MazeGenerator");
        DontDestroyOnLoad(gameObject);
    }

    //void FixedUpdate()
    //{
    //    animator.SetFloat("speed", rigidbody.velocity.magnitude);
    //}

    // Update is called once per frame
    void Update()
    {

        //Vector3 cameraCenter 
        //GameObject.Find("Main Camera").transform.position = new Vector3(transform.position.x, transform.position.y, -10);
        MazeGenObj = GameObject.FindWithTag("MazeGenerator");
        Vector2 cur_velocity = rigidbody.velocity;
        float movementSpeed = cur_velocity.magnitude;
        animator.SetFloat("Speed", movementSpeed);
        if (movementSpeed > 0.1f)
        {
            animator.SetFloat("movementX", cur_velocity.x);
            animator.SetFloat("movementY", cur_velocity.y);
        }
        if (oxygen <= 0.0f)
        {
            animator.SetTrigger("Dead");

        }
        if (Input.GetKey(KeyCode.A))
        {
            rigidbody.AddForce(Vector2.left * speed);
        }
        if (Input.GetKey(KeyCode.D))
        {
            rigidbody.AddForce(Vector2.right * speed);
        }
        if (Input.GetKey(KeyCode.W))
        {
            rigidbody.AddForce(Vector2.up * speed);
        }
        if (Input.GetKey(KeyCode.S))
        {

            rigidbody.AddForce(Vector2.down * speed);
        } 

        rigidbody.velocity = Vector2.ClampMagnitude(rigidbody.velocity, max_horizontal_velocity);

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
            shootBubbles(transform.position, radiansToMouse, numBubbles);
        }

        tempTime += Time.deltaTime;
        if (tempTime > 0.1 && health > 0.7f)
        {
            tempTime = 0;

            healthWarning();
        }
        
        if (MazeGenObj != null)
        {
            MazeGenerator MazeScript = MazeGenObj.GetComponent<MazeGenerator>();
            Vector3 goal_dir = MazeScript.find_nearest_goal(transform);
            Debug.Log("PLAYER Goal direction: " + goal_dir);
            int compass_idx = Mathf.Min(3, MazeScript.get_num_keys());
            transform.Find("Compass").GetComponent<SpriteRenderer>().sprite = compass[compass_idx];
            if (goal_dir.x != 0 || goal_dir.y != 0 || goal_dir.z != 0)
            {
                float angle = Mathf.Atan2(goal_dir.y, goal_dir.x) * Mathf.Rad2Deg;
                transform.Find("Compass").eulerAngles = new Vector3(0, 0, angle-90);
                transform.Find("Compass").gameObject.SetActive(true);
            }
            else
            {
                Debug.Log("PLAYER No goal in sight");
                transform.Find("Compass").gameObject.SetActive(false);
            }
        }
        else
        {
            Debug.Log("PLAYER No goal in sight");
            transform.Find("Compass").gameObject.SetActive(false);
        }

        //Make the fear just a UI element that indicates whetehr or not the boss guy is close, not anything with the oxygen
        // Calculate distance to shrimp and update fear meters
        //if (1f / distanceToBoss() >= 1f)
        ////{
        ////    health = 1f / (FindClostestMob());
        ////    healthBar.SetSize(health);
        ////}
        ////else
        //{
        //    health = 1f;
        //    healthBar.SetSize(health);
        //}
        ////Debug.Log(FindClostestMob());

        // else if (health + 1f / FindClostestMob() > 1f)
        //{
        //    health = 1f;
        //}


        //health = health + 1f / (FindClostestMob() * 2);
        //float lobsterDistance = distanceToBoss();
        //health = lobsterDistance < 5f ?  
        //healthBar.SetSize(health);
        oxygenBar.SetSize(oxygen);
        //Debug.Log("distance " + FindClostestMob());
        //Debug.Log("health "+ health);
        // 


    }

    private void shootBubbles(Vector3 position, float radiansToMouse, int numberShots)
    {
        if (oxygen - oxygenShotCost > 0.0f) 
        {

            for (int i = 0; i < numberShots; i++)
            {
                //angles of bubbles in degrees
                float angle = (radiansToMouse + (Random.Range(-Mathf.PI/8f, Mathf.PI/8f)))*180f/Mathf.PI;

                float speed = Random.Range(8f, 12f);
                GameObject newProjectile = Instantiate(projectilePrefab);
                newProjectile.transform.position = position;
                newProjectile.transform.rotation = Quaternion.Euler(0, 0, angle);
                newProjectile.GetComponent<ProjectileController>().speed = speed;
            }
            oxygen -= oxygenShotCost;
        }
    }

    private bool flash = false;
    private void healthWarning()
    {
        //Debug.Log("color change?");
        if (flash)
        {
            flash = false;
            oxygen -= 0.25f;
        }
        else
        {
            flash = true;
        }
    }

    private float distanceToBoss()
    {
        GameObject boss = GameObject.FindWithTag("Boss");
        if (boss != null) return (boss.transform.position - transform.position).sqrMagnitude;
        return 99999f;
    }

    private float FindClostestMob()
    {
        float distanceToClosestMob = 100f;
        MobController closestMob = null;
        MobController[] allMob = GameObject.FindObjectsOfType<MobController>();

        foreach (MobController currentMob in allMob)
        {
            float distanceToMob = (currentMob.transform.position - gameObject.transform.position).sqrMagnitude;
            if (distanceToMob < distanceToClosestMob)
            {
                distanceToClosestMob = distanceToMob;
                closestMob = currentMob;
            }
        }
        return distanceToClosestMob;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        
        //if (collision.gameObject.CompareTag("Mob"))
        //{
        //    oxygen -= 0.05f;
        //}
        if (collision.gameObject.CompareTag("Key"))
        {
            int keyIndex = MapController.game_controller.sceneNameToInt();
            instance.numKeys += 1;
            Destroy(collision.gameObject);
            SoundManager.instance.PlaySoundGetKey();
        }
        
    }
}
