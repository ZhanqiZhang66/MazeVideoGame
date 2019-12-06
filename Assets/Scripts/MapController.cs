using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MapController : MonoBehaviour
{
    
    public static MapController game_controller;
    public GameObject Camera;
    public PlayerController player;
    public MazeGenerator maze;
    public GameObject keyPrefab;
    public Navigator MapNav;
    public string[] level_names;
    public string[] menu_names;
    public bool is_game_level;

    public UnityEditor.SceneAsset SuccessScene;
    public UnityEditor.SceneAsset FailureScene;
    
    void Awake()
    {
        if (game_controller != null)
        {
            Destroy(gameObject);
        }
        else
        {
            game_controller = this;
            player = PlayerController.instance;
            MapNav = GameObject.FindWithTag("Navigator").GetComponent<Navigator>();
            Camera = GameObject.FindWithTag("MainCamera");
            maze = GameObject.FindWithTag("MazeGenerator").GetComponent<MazeGenerator>();
        }
    }

    public void FixedUpdate()
    {
        transform.position = new Vector3(Camera.transform.position.x, Camera.transform.position.y, transform.position.z);
    }

    public void Update()
    {
        if (player.oxygen <= 0)
        {
            SoundManager.instance.PlayFailure();
            player.oxygen = 1f;
            SceneManager.LoadScene(FailureScene.name);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(gameObject);
        SceneManager.activeSceneChanged += ChangedActiveScene;
        is_game_level = sceneNameToInt() >= 0;
        if (is_game_level) show_minimap();
        else hide_minimap();
        // Reset Player Parameters
        if (is_menu_scene()) player.GetComponent<Rigidbody2D>().gravityScale = 5;
        else player.GetComponent<Rigidbody2D>().gravityScale = 0;
        player.numKeys = 0;
        player.oxygen = 1f;
        // Generate level if is level
        if (!is_game_level)
        {
            SoundManager.instance.audioSource.volume = 0.2f;
            GameObject p_spawn = GameObject.FindWithTag("PlayerSpawn");
            if (p_spawn != null) player.transform.position = p_spawn.transform.position;
        }
        else
        {
            SoundManager.instance.audioSource.volume = 0.6f;
            maze.generate_map();
            MapNav.load_map();
            GlobeController.instance.reset();
        }
    }

    void ChangedActiveScene(Scene current, Scene next)
    {
        string s = next.name;
        int i = sceneNameToInt();
        is_game_level = i >= 0;
        if (is_game_level) show_minimap();
        else hide_minimap();
        Debug.Log("TRANSITION INTO SCENCE INT: " + i);
        // Reset Player Parameters
        if (is_menu_scene()) player.GetComponent<Rigidbody2D>().gravityScale = 5;
        else player.GetComponent<Rigidbody2D>().gravityScale = 0;
        player.numKeys = 0;
        player.oxygen = 1f;
        // Generate level if is level
        if (!is_game_level)
        {
            SoundManager.instance.audioSource.volume = 0.2f;
            GameObject p_spawn = GameObject.FindWithTag("PlayerSpawn");
            if (p_spawn != null) player.transform.position = p_spawn.transform.position;
        }
        else
        {
            SoundManager.instance.audioSource.volume = 0.6f;
            maze.generate_map();
            MapNav.load_map();
            GlobeController.instance.reset();
        }
    }

    public void hide_minimap()
    {
        foreach (Transform c_obj in transform)
        {
            c_obj.gameObject.SetActive(false);
        }
    }

    public void show_minimap()
    {
        Debug.Log("Initialize Objects");
        foreach (Transform c_obj in transform)
        {
            c_obj.gameObject.SetActive(true);
        }
    }

    public bool is_menu_scene()
    {
        string s = SceneManager.GetActiveScene().name;
        return System.Array.IndexOf(menu_names, s) >= 0;
    }

    public int sceneNameToInt()
    {
        string s = SceneManager.GetActiveScene().name;
        return System.Array.IndexOf(level_names, s);
        //return s == "A" ? 0 : s == "B" ? 1 : s == "C" ? 2 : s == "D" ? 3 : s == "E" ? 4 : 5;
    }

    public void move_player(Vector2 move_loc)
    {
        player.transform.position = move_loc;
    }
}
