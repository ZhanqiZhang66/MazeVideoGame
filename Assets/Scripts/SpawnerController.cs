using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerController : MonoBehaviour
{

    private bool[] can_spawn;
    private Vector3[] spawn_dir;
    public GameObject enemy;
    private System.Random rnd = new System.Random();
    public float spawn_time = 2f;
    public GameObject MapNav = null;
    public int num_spawn = 0;
    private float SPAWN_THRESHOLD_DIST = 10f;
    private bool is_init = false;

    private bool invoked = false;

    public float mob_mass;
    public float mob_speed;
    public float mob_lifetime;
    public float mob_dmg;

    bool can_spawn_dir(int idx)
    {
        if (MapNav == null || MapNav.GetComponent<Navigator>().game_board == null) return true;
        return MapNav.GetComponent<Navigator>().is_passable(transform.position + spawn_dir[idx] * 1f);
    }

    void Spawn()
    {
        if (!is_init) return;
        // Spawn only when player is nearby
        Vector3 dir_to_player = PlayerController.instance.transform.position - transform.position;
        if (dir_to_player.magnitude > SPAWN_THRESHOLD_DIST) return;
        // Choose spawn direction
        int rnd_dir = rnd.Next(0, can_spawn.Length);
        int rnd_conut = 0;
        while(!can_spawn[rnd_dir])
        {
            rnd_dir = (rnd_dir + 1) % can_spawn.Length;
            rnd_conut++;
            if (rnd_conut > can_spawn.Length)
            {
                Debug.Log("Error in spawner: No directions possible");
                return;
            }
        }
        GameObject new_en = Instantiate(enemy);
        // Enemy fine-tuning controls
        if (mob_mass > 0) new_en.GetComponent<Rigidbody2D>().mass = mob_mass;
        if (mob_speed > 0) new_en.GetComponent<MobController>().speed = mob_speed;
        if (mob_lifetime > 0) new_en.GetComponent<MobController>().lifeTime = mob_lifetime;
        if (mob_dmg > 0) new_en.GetComponent<MobController>().damage = mob_dmg;
        new_en.transform.position = transform.position + spawn_dir[rnd_dir] * 1f;
        new_en.GetComponent<MobController>().navigator = MapNav;
        num_spawn += 1;
    }

    private void Awake()
    {

    }

    // Start is called before the first frame update
    void Start()
    {
        /*
        if (MapNav == null)
        {
            MapNav = GameObject.FindWithTag("Navigator");
        }
        if (MapNav != null && MapNav.GetComponent<Navigator>().game_board != null) init_spawner();
        */
        init_spawner();
    }

    void init_spawner()
    {
        if (is_init) return;
        is_init = true;
        spawn_dir = new Vector3[] {transform.up, transform.right, -transform.up, -transform.right};
        can_spawn = new bool[spawn_dir.Length];
        for (int i = 0; i < spawn_dir.Length; ++i)
        {
            can_spawn[i] = can_spawn_dir(i);
            //Debug.Log("Spawner: " + transform.position + "| " + i.ToString() + can_spawn[i]);
        }
    }

    public void set_params(float m_mass, float m_speed, float m_life, float m_dmg)
    {
        mob_mass = m_mass;
        mob_speed = m_speed;
        mob_lifetime = m_life;
        mob_dmg = m_dmg;
    }

    public void set_spawn(float s_time)
    {
        invoked = true;
        spawn_time = s_time;
        Debug.Log("Spawner: " + transform.position + "| Spawning every: " + spawn_time + "s");
        InvokeRepeating("Spawn", spawn_time, spawn_time);
    }

    // Update is called once per frame
    void Update()
    {
        /*if (MapNav == null)
        {
            MapNav = GameObject.FindWithTag("Navigator");
        }
        if (MapNav != null && MapNav.GetComponent<Navigator>().game_board != null) init_spawner();
        Debug.Log("Navigator: " + MapNav + " | has_board: " + (MapNav.GetComponent<Navigator>().game_board != null));*/
        if (!invoked) set_spawn(spawn_time);
    }
}
