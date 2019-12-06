using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MazeGenerator : MonoBehaviour
{
    public GameObject nav_object;
    public GameObject grid;
    public Tilemap DarkMap;
    public Tilemap BlurredMap;
    public Tilemap GameMap;
    public Tilemap BackgroundMap;
    public Tile DarkTile;
    public Tile BlurredTile;
    public Tile BackgroundTile;
    public Tile[] walls;
    public Tile[] gates;
    public Tile[] items;

    // Instance Variables
    private float timeElapsed;
    private float next_depletion;
    private System.Random rnd;
    private GameObject big_boss;
    private PlayerController player;
    private GameObject end_goal;
    private List<(int, int)> key_locs;
    private HashSet<(int, int)> occ_locs;
    private List<GameObject> oxy_objects;
    private List<GameObject> key_objects;

    // Difficulty settings
    private static int KEY_CLOSE_THRESHOLD = 10;
    private static int SPAWNER_CLOSE_THRESHOLD = 5;
    private static int OXYGEN_CLOSE_THRESHOLD = 2;
    private static int BOSS_DIST_THRESHOLD = 15;

    // Default Params
    private static int NUM_KEYS = 1;
    private static int NUM_SPAWNERS = 5;
    private static int NUM_OXYGENS = 25;
    private static float SPAWN_TIMER = 5;
    private static int SIZE = 10;
    private static float OXY_DEPLETION = 0.005f;
    private static float DEPLETION_RATE = 1f;

    // Map stats
    public int size;
    public int num_keys;
    public int num_spawners;
    public int num_oxygens;
    public float spawn_timer;
    public float oxy_depletion;

    // Enemy Difficulty Levers
    public float boss_mass = 0;
    public float boss_dmg = 0;
    public float boss_speed = 0;
    public int boss_bounceback = 0;
    public float mob_mass = 0;
    public float mob_speed = 0;
    public float mob_lifetime = 0;
    public float mob_dmg = 0;

    // Prefabs
    public GameObject key;
    public GameObject goal;
    public GameObject spawner;
    public GameObject oxygen;
    public GameObject boss;

    // UP | RIGHT | DOWN | LEFT
    private static Vector3[] ADJ_VECS = { new Vector3(0, 1), new Vector3(1, 0), new Vector3(0, -1), new Vector3(-1, 0) };
    private static int[,] ADJ = { { 0, 1 }, { 1, 0 }, { 0, -1 }, { -1, 0 } };

    // Valid Spawn points
    private static (int, int) START_LOC = (1, 1);
    List<(int, int)> valid_spawn_locs;
    List<(int, int)> passable_locs;

    void Awake()
    {
        player = PlayerController.instance;
        rnd = new System.Random();
        valid_spawn_locs = new List<(int, int)>();
        key_locs = new List<(int, int)>();
        occ_locs = new HashSet<(int, int)>();
        passable_locs = new List<(int, int)>();
        oxy_objects = new List<GameObject>();
        key_objects = new List<GameObject>();
    }

    private bool is_valid(int x, int y, int size)
    {
        return x >= 0 && y >= 0 && x < size && y < size;
    }

    public void generate_map()
    {
        // Clear last map
        oxy_objects.Clear();
        key_objects.Clear();
        valid_spawn_locs.Clear();
        passable_locs.Clear();
        key_locs.Clear();
        occ_locs.Clear();
        end_goal = null;
        big_boss = null;
        occ_locs.Add(START_LOC);

        // Try to get game parameters if available
        GameObject param_obj = GameObject.FindWithTag("Level_Params");
        Level_Parameters lvl_params = null;
        if (param_obj != null)
        {
            lvl_params = param_obj.GetComponent<Level_Parameters>();
        }
        if (lvl_params != null)
        {
            size = lvl_params.size;
            num_keys = lvl_params.num_keys;
            num_spawners = lvl_params.num_spawners;
            num_oxygens = lvl_params.num_oxygens;
            spawn_timer = lvl_params.spawn_timer;
            oxy_depletion = lvl_params.oxy_depletion;
            boss_mass = lvl_params.boss_mass;
            boss_dmg = lvl_params.boss_dmg;
            boss_speed = lvl_params.boss_speed;
            boss_bounceback = lvl_params.boss_bounceback;
            mob_mass = lvl_params.mob_mass;
            mob_speed = lvl_params.mob_speed;
            mob_lifetime = lvl_params.mob_lifetime;
            mob_dmg = lvl_params.mob_dmg;
        }
        else
        {
            boss_mass = 0;
            boss_dmg = 0;
            boss_speed = 0;
            boss_bounceback = 0;
            mob_mass = 0;
            mob_speed = 0;
            mob_lifetime = 0;
            mob_dmg = 0;
        }
        if (size < 1) size = SIZE;
        if (num_keys < 1) num_keys = NUM_KEYS;
        if (num_spawners < 1) num_spawners = NUM_SPAWNERS;
        if (num_oxygens < 1) num_oxygens = NUM_OXYGENS;
        if (spawn_timer < 1f) spawn_timer = SPAWN_TIMER;
        if (oxy_depletion < 0.001f) oxy_depletion = OXY_DEPLETION;

        // Get Grid of current scene
        grid = GameObject.Find("Grid");
        foreach (Transform t in grid.transform)
        {
            GameObject map_obj = t.gameObject;
            if (map_obj.name == "Tilemap") GameMap = map_obj.GetComponent<Tilemap>();
            else if (map_obj.name == "Blurredmap") BlurredMap = map_obj.GetComponent<Tilemap>();
            else if (map_obj.name == "Darkmap") DarkMap = map_obj.GetComponent<Tilemap>();
            else if (map_obj.name == "Backgroundmap") BackgroundMap = map_obj.GetComponent<Tilemap>();
        }

        //INIT Background Map
        GameMap.origin = new Vector3Int(0, 0, 0);
        GameMap.size = new Vector3Int(size, size, 1);
        GameMap.ResizeBounds();

        //Actually generate map using randomized Prim's
        bool[,] map = new bool[size, size];
        for (int x = 0; x < size; ++x)
        {
            for (int y = 0; y < size; ++y)
            {
                map[x, y] = false;
            }
        }
        //Assume all tiles are walls first then free up cells as we go along
        int traverse_timer = 0;
        SortedSet<(double, int, int)> pq = new SortedSet<(double, int, int)>();
        HashSet<(int, int)> v = new HashSet<(int, int)>();
        int sx = 1, sy = 1;
        map[sx, sy] = true;
        for (int i = 0; i < 4; ++i)
        {
            int ssx = sx + ADJ[i, 0]*2;
            int ssy = sy + ADJ[i, 1]*2;
            if (is_valid(ssx, ssy, size) && !map[ssx, ssy])
            {
                pq.Add((rnd.NextDouble(), ssx, ssy));
                v.Add((ssx, ssy));
            }
        }

        //Loop through till no more valid walls remain
        while(pq.Count > 0)
        {
            traverse_timer++;
            //if (traverse_timer > 5000) break;
            (double, int, int) cur = pq.Min;
            pq.Remove(pq.Min);
            int cx = cur.Item2;
            int cy = cur.Item3;
            if (map[cx, cy]) continue;
            int rand_start = rnd.Next(0, 4);
            for (int i = rand_start; i < rand_start + 4; ++i)
            {
                int ccx = cx + ADJ[i%4, 0] * 2;
                int ccy = cy + ADJ[i%4, 1] * 2;
                if (is_valid(ccx, ccy, size) && map[ccx, ccy])
                {
                    int nx = cx + ADJ[i%4, 0];
                    int ny = cy + ADJ[i%4, 1];
                    //Connect the in-between cell
                    if (map[nx, ny]) continue;
                    else
                    {
                        //Check if we are able to connect
                        int adj_cells = 0;
                        for (int j = 0; j < 4; ++j)
                        {
                            int ax = nx + ADJ[j, 0];
                            int ay = ny + ADJ[j, 1];
                            if (is_valid(ax, ay, size))
                            {
                                if (map[ax, ay]) adj_cells++;
                            }
                            else adj_cells++;
                        }
                        if (adj_cells > 1) continue;
                        map[nx, ny] = true;
                        //Add neighbors from (nx, ny)
                        for (int j = 0; j < 4; ++j)
                        {
                            int nnx = nx + ADJ[j, 0] * 2;
                            int nny = ny + ADJ[j, 1] * 2;
                            if (is_valid(nnx, nny, size) && !map[nnx, nny] && !v.Contains((nnx, nny)))
                            {
                                pq.Add((rnd.NextDouble(), nnx, nny));
                                v.Add((nnx, nny));
                            }
                        }
                        v.Remove((cx, cy));
                        break;
                    }
                }
            }
        }

        //Clean up 'fake' walls
        for (int x = 0; x < size; ++x)
        {
            for (int y = 0; y < size; ++y)
            {
                bool is_valid_wall = false;
                for (int i = 0; i < 4; ++i)
                {
                    int ax = x + ADJ[i, 0];
                    int ay = y + ADJ[i, 1];
                    if (is_valid(ax, ay, size) && !map[ax, ay])
                    {
                        is_valid_wall = true;
                        break;
                    }
                }
                if (!is_valid_wall) map[x, y] = true;
            }
        }

        //map now stores walls in false locations
        for (int x = 0; x < size; ++x)
        {
            for (int y = 0; y < size; ++y)
            {
                if (map[x, y] == false) GameMap.SetTile(new Vector3Int(x, y, 0), walls[rnd.Next(0, walls.Length)]);
                else
                {
                    int adj_cells = 0;
                    for (int i = 0; i < 4; ++i)
                    {
                        int ax = x + ADJ[i, 0];
                        int ay = y + ADJ[i, 1];
                        if (is_valid(ax, ay, size) && map[ax, ay]) adj_cells++;
                    }
                    if (adj_cells == 1) valid_spawn_locs.Add((x, y));
                    passable_locs.Add((x, y));
                }
            }
        }

        //Set Dark/Blurred maps
        DarkMap.origin = BlurredMap.origin = BackgroundMap.origin = GameMap.origin - new Vector3Int(10, 10, 0);
        DarkMap.size = BlurredMap.size = BackgroundMap.size = GameMap.size + new Vector3Int(20, 20, 0);
        DarkMap.ResizeBounds();
        BlurredMap.ResizeBounds();
        BackgroundMap.ResizeBounds();
        foreach (Vector3Int p in DarkMap.cellBounds.allPositionsWithin) DarkMap.SetTile(p, DarkTile);
        foreach (Vector3Int p in BlurredMap.cellBounds.allPositionsWithin) BlurredMap.SetTile(p, BlurredTile);
        foreach (Vector3Int p in BackgroundMap.cellBounds.allPositionsWithin) BackgroundMap.SetTile(p, BackgroundTile);

        // Generate Objects
        generate_spawners();
        generate_boss();
        generate_oxygens();
        generate_keys();
        generate_endGoal();

        // Finally Move player into position
        PlayerController.instance.transform.position = new Vector3(1, 1);
    }

    private void generate_spawners()
    {
        List<(int, int)> spawn_locs = new List<(int, int)>();
        spawn_locs.Add(START_LOC);
        for (int i = 0; i < num_spawners;)
        {
            int idx = rnd.Next(0, valid_spawn_locs.Count);
            (int, int) loc = valid_spawn_locs[idx];
            if (occ_locs.Contains(loc)) continue;
            int nearest_spawn = 9999999;
            foreach ((int, int) l in spawn_locs)
            {
                int cur_dist = Mathf.Abs(l.Item1 - loc.Item1) + Mathf.Abs(l.Item2 - loc.Item2);
                nearest_spawn = Mathf.Min(nearest_spawn, cur_dist);
            }
            if (nearest_spawn < SPAWNER_CLOSE_THRESHOLD) continue;
            Vector3 position = GameMap.origin + new Vector3(loc.Item1, loc.Item2, 0);
            GameObject en_spawner = Instantiate(spawner, position, Quaternion.identity);
            en_spawner.GetComponent<SpawnerController>().MapNav = nav_object;
            en_spawner.GetComponent<SpawnerController>().set_spawn(spawn_timer);
            en_spawner.GetComponent<SpawnerController>().set_params(mob_mass, mob_speed, mob_lifetime, mob_dmg);
            Debug.Log("Spawner @ Loc: " + loc);
            ++i;
            spawn_locs.Add(loc);
            occ_locs.Add(loc);
        }
    }

    private void generate_boss()
    {
        while (big_boss == null)
        {
            (int, int) loc = passable_locs[rnd.Next(0, passable_locs.Count)];
            if (loc.Item1 + loc.Item2 < BOSS_DIST_THRESHOLD) continue;
            Vector3 position = GameMap.origin + new Vector3(loc.Item1, loc.Item2, 0);
            big_boss = Instantiate(boss, position, Quaternion.identity);
            big_boss.GetComponent<BossController>().navigator = nav_object;
            if (boss_mass > 0) big_boss.GetComponent<Rigidbody2D>().mass = boss_mass;
            if (boss_dmg > 0) big_boss.GetComponent<BossController>().damage = boss_dmg;
            if (boss_speed > 0) big_boss.GetComponent<BossController>().speed = boss_speed;
            if (boss_bounceback > 0) big_boss.GetComponent<BossController>().bounceBack = boss_bounceback;
        }
    }

    private void replenish_oxygen()
    {
        List<(int, int)> spawn_locs = new List<(int, int)>();
        List<GameObject> current_oxy = new List<GameObject>();
        foreach (GameObject o in oxy_objects)
        {
            if (o != null)
            {
                current_oxy.Add(o);
                spawn_locs.Add(((int)o.transform.position.x, (int)o.transform.position.y));
            }
        }
        oxy_objects.Clear();
        for (int i = 0; i < num_oxygens - current_oxy.Count; ++i)
        {
            int idx = rnd.Next(0, passable_locs.Count);
            (int, int) loc = passable_locs[idx];
            if (occ_locs.Contains(loc)) continue;
            int nearest_spawn = 9999999;
            foreach ((int, int) l in spawn_locs)
            {
                int cur_dist = Mathf.Abs(l.Item1 - loc.Item1) + Mathf.Abs(l.Item2 - loc.Item2);
                nearest_spawn = Mathf.Min(nearest_spawn, cur_dist);
            }
            if (nearest_spawn < OXYGEN_CLOSE_THRESHOLD) continue;
            Vector3 position = GameMap.origin + new Vector3(loc.Item1, loc.Item2, 0);
            GameObject oxy_obj = Instantiate(oxygen, position, Quaternion.identity);
            Debug.Log("Oxygen Patch @ Loc: " + loc);
            ++i;
            spawn_locs.Add(loc);
            current_oxy.Add(oxy_obj);
        }
        foreach (GameObject o in current_oxy)
        {
            oxy_objects.Add(o);
        }
    }

    private void generate_oxygens()
    {
        List<(int, int)> spawn_locs = new List<(int, int)>();
        for (int i = 0; i < num_oxygens;)
        {
            int idx = rnd.Next(0, passable_locs.Count);
            (int, int) loc = passable_locs[idx];
            if (occ_locs.Contains(loc)) continue;
            int nearest_spawn = 9999999;
            foreach ((int, int) l in spawn_locs)
            {
                int cur_dist = Mathf.Abs(l.Item1 - loc.Item1) + Mathf.Abs(l.Item2 - loc.Item2);
                nearest_spawn = Mathf.Min(nearest_spawn, cur_dist);
            }
            if (nearest_spawn < OXYGEN_CLOSE_THRESHOLD) continue;
            Vector3 position = GameMap.origin + new Vector3(loc.Item1, loc.Item2, 0);
            GameObject oxy_obj = Instantiate(oxygen, position, Quaternion.identity);
            Debug.Log("Oxygen Patch @ Loc: " + loc);
            ++i;
            spawn_locs.Add(loc);
            oxy_objects.Add(oxy_obj);
        }
    }

    private void generate_keys()
    {
        for (int i = 0; i < num_keys;)
        {
            int idx = rnd.Next(0, valid_spawn_locs.Count);
            (int, int) loc = valid_spawn_locs[idx];
            if (occ_locs.Contains(loc)) continue;
            int nearest_key = 9999999;
            foreach ((int, int) l in key_locs)
            {
                int cur_dist = Mathf.Abs(l.Item1 - loc.Item1) + Mathf.Abs(l.Item2 - loc.Item2);
                nearest_key = Mathf.Min(nearest_key, cur_dist);
            }
            if (nearest_key < KEY_CLOSE_THRESHOLD) continue;
            Vector3 position = GameMap.origin + new Vector3(loc.Item1, loc.Item2, 0);
            GameObject key_obj = Instantiate(key, position, Quaternion.identity);
            Debug.Log("Key @ Loc: " + loc);
            ++i;
            key_locs.Add(loc);
            key_objects.Add(key_obj);
            occ_locs.Add(loc);
        }
    }

    private void generate_endGoal()
    {
        for (int i = valid_spawn_locs.Count - 1; i > 0; --i)
        {
            (int, int) loc = valid_spawn_locs[i];
            if (occ_locs.Contains(loc)) continue;
            Vector3 position = new Vector3(loc.Item1, loc.Item2);
            end_goal = Instantiate(goal, position, Quaternion.identity);
            occ_locs.Add(loc);
            break;
        }
    }

    public int get_num_keys()
    {
        int left_keys = 0;
        foreach (GameObject g_key in key_objects)
        {
            if (g_key != null) left_keys += 1;
        }
        return left_keys;
    }

    public Vector3 find_nearest_goal(Transform pos)
    {
        float min_dist = 99999f;
        Vector3 ret_dir = new Vector3(0, 0, 0);
        foreach (GameObject g_key in key_objects)
        {
            if (g_key == null) continue;
            float dist = (g_key.transform.position - pos.position).magnitude;
            if (dist < min_dist)
            {
                min_dist = dist;
                ret_dir = g_key.transform.position - pos.position;
            }
        }
        if (min_dist == 99999f && end_goal != null)
        {
            // No more keys, towards goal
            ret_dir = end_goal.transform.position - pos.position;
        }
        return ret_dir;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        timeElapsed += Time.deltaTime;
        if (timeElapsed >= next_depletion)
        {
            next_depletion = timeElapsed + DEPLETION_RATE;
            PlayerController.instance.oxygen -= oxy_depletion;
        }
        replenish_oxygen();
        if (player.numKeys >= num_keys)
        {
            end_goal.SetActive(true);
        }
    }
}
