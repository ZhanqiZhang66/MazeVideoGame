using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;

public class GlobeController : MonoBehaviour
{

    static private float EPSILON = 0.0001f;
    Quaternion rand;
    private float rotateSpeed = 1f;
    private GameObject player;
    public GameObject playerMapTracker;
    private float timeElapsed;
    private float last_spawn_tracker;
    public float tracker_update_delay = 0.5f;
    public GameObject Tracker;
    private ArrayList trackers;
    public static GlobeController instance;
    private float MapSize_x, MapSize_y;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
            trackers = new ArrayList();
            GameObject game_map_obj = GameObject.FindWithTag("Map");
            Tilemap game_map = null;
            if (game_map_obj != null) game_map = game_map_obj.GetComponent<Tilemap>();
            if (game_map != null)
            {
                MapSize_x = game_map.cellBounds.size.y;
                MapSize_y = game_map.cellBounds.size.x;
                Debug.Log("Init Globe Bounds: " + MapSize_x + "," + MapSize_y);
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        SceneManager.activeSceneChanged += ChangedActiveScene;
        timeElapsed = 0f;
        last_spawn_tracker = timeElapsed + tracker_update_delay;
        rand = Quaternion.Euler(new Vector3(Random.Range(-180.0f, 180.0f), Random.Range(-180.0f, 180.0f), Random.Range(-180.0f, 180.0f)));
        Quaternion init = Quaternion.Euler(new Vector3(0, 0, 0));
        transform.rotation = init;
        player = GameObject.Find("Player");
        if (player == null) return;
        Vector3 playerPos = player.transform.position;
        render_position(playerPos);
    }

    private void ChangedActiveScene(Scene current, Scene next)
    {
        GameObject game_map_obj = GameObject.FindWithTag("Map");
        Tilemap game_map = null;
        if (game_map_obj != null) game_map = game_map_obj.GetComponent<Tilemap>();
        if (game_map != null)
        {
            MapSize_x = game_map.cellBounds.size.y;
            MapSize_y = game_map.cellBounds.size.x;
            Debug.Log("Init Globe Bounds: " + MapSize_x + "," + MapSize_y);
        }
    }

    void Run_Random()
    {
        Vector3 cur = transform.rotation.eulerAngles;
        Vector3 rand_angles = rand.eulerAngles;
        float diff = Vector3.Dot(cur.normalized, rand_angles.normalized);
        if ((1 - diff) > EPSILON)
        {
            //Rotate
            transform.rotation = Quaternion.Slerp(transform.rotation, rand, Time.deltaTime * rotateSpeed);
        }
        else
        {
            //New Rand
            rand = Quaternion.Euler(new Vector3(Random.Range(-180.0f, 180.0f), Random.Range(-180.0f, 180.0f), Random.Range(-180.0f, 180.0f)));
        }
    }

    void render_position(Vector3 pos)
    {
        if (MapSize_x < 1 || MapSize_y < 1) return;
        // Compute rotated quaternion
        //Quaternion rot = Quaternion.Euler(new Vector3(-pos.y/MapSize_y*30, pos.x/MapSize_x*30, 0));
        //Debug.Log(rot.eulerAngles);
        //transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * rotateSpeed);
        playerMapTracker.transform.localPosition = new Vector3(pos.x/MapSize_x, pos.y/MapSize_y, playerMapTracker.transform.localPosition.z);
    }

    void spawn_tracker(Vector3 pos)
    {
        if (MapSize_x < 1 || MapSize_y < 1) return;
        GameObject tracker = Instantiate(Tracker);
        tracker.transform.parent = gameObject.transform;
        tracker.transform.localPosition = new Vector3(pos.x / MapSize_x, pos.y / MapSize_y, playerMapTracker.transform.localPosition.z);
        trackers.Add(tracker);
    }

    public void reset()
    {
        foreach (GameObject t in trackers)
        {
            if (t == null) continue;
            Destroy(t);
        }
        trackers.Clear();
    }

    // Update is called once per frame
    void Update()
    {
        timeElapsed += Time.deltaTime;
        if (player == null)
        {
            player = GameObject.Find("Player");
        }
        if (player == null) return;
        Vector3 playerPos = player.transform.position;
        //Debug.Log(playerPos);
        render_position(playerPos);
        if (timeElapsed > last_spawn_tracker)
        {
            spawn_tracker(playerPos);
            last_spawn_tracker += tracker_update_delay;
        }
    }
}
