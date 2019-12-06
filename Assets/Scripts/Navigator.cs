using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;

public class Navigator : MonoBehaviour
{
    public Tilemap game_board = null;
    private bool[,] terrain;
    private Vector3[] ADJ_VECS = { new Vector3(0, 1), new Vector3(0, -1), new Vector3(-1, 0), new Vector3(1, 0) };
    private int[,] ADJ = { { 0, 1 }, { 0, -1 }, { -1, 0 }, { 1, 0 } };
    private BoundsInt bounds;
    private Vector3 offset;

    // Start is called before the first frame update
    void Start()
    {

    }

    public void load_map()
    {
        game_board = GameObject.FindWithTag("Map").GetComponent<Tilemap>();
        bounds = game_board.cellBounds;
        TileBase[] allTiles = game_board.GetTilesBlock(bounds);
        terrain = new bool[bounds.size.x, bounds.size.y];

        for (int x = 0; x < bounds.size.x; x++)
        {
            for (int y = 0; y < bounds.size.y; y++)
            {
                TileBase tile = allTiles[x + y * bounds.size.x];
                if (tile != null)
                {
                    terrain[x, y] = false;
                    //Debug.Log("x:" + x + " y:" + y + " tile:" + tile.name);
                }
                else
                {
                    terrain[x, y] = true;
                    //Debug.Log("x:" + x + " y:" + y + " tile: (null)");
                }
            }
        }
        offset = new Vector3(0.5f, 0.5f);
        
        for (int x = 0; x < bounds.size.x; ++x)
        {
            string debug_line = "";
            for (int y = 0; y < bounds.size.y; ++y)
            {
                if (terrain[x, y]) debug_line += "_";
                else debug_line += "X";
            }
            Debug.Log(debug_line);
        }
        
    }

    private void get_cell(out int x, out int y, Vector3 loc)
    {
        //x = Mathf.RoundToInt(loc.x - offset.x);
        //y = Mathf.RoundToInt(loc.y - offset.y);
        Vector3Int tile_loc = game_board.WorldToCell(loc + offset);
        x = tile_loc.x;
        y = tile_loc.y;
    }

    public bool is_passable(Vector3 loc)
    {
        int lx, ly;
        get_cell(out lx, out ly, loc);
        Debug.Log("Testing loc: " + lx + "," + ly);
        return terrain[lx, ly];
    }

    private int bfs(int cx, int cy, int tx, int ty)
    {
        Queue<(int, int, int)> q = new Queue<(int, int, int)>();
        HashSet<(int, int)> v = new HashSet<(int, int)>();
        q.Enqueue((cx, cy, -1));
        v.Add((cx, cy));
        while(q.Count > 0)
        {
            (int, int, int) cur_loc = q.Dequeue();
            int x = cur_loc.Item1;
            int y = cur_loc.Item2;
            int s_dir = cur_loc.Item3;
            if (x == tx && y == ty)
            {
                //Debug.Log("Found path");
                return s_dir;
            }
            for (int i = 0; i < 4; ++i)
            {
                int nx = x + ADJ[i, 0];
                int ny = y + ADJ[i, 1];
                if (!v.Contains((nx, ny)) && nx < bounds.size.x && nx >= 0 && ny < bounds.size.y && ny >= 0 && terrain[nx, ny])
                {
                    if (s_dir == -1)
                    {
                        q.Enqueue((nx, ny, i));
                        v.Add((nx, ny));
                    }
                    else
                    {
                        q.Enqueue((nx, ny, s_dir));
                        v.Add((nx, ny));
                    }
                }
            }
        }
        return -1;
    }

    public Vector3 find_dir(Vector3 cur, Vector3 loc)
    {
        int tx, ty, cx, cy;
        get_cell(out tx, out ty, loc);
        get_cell(out cx, out cy, cur);
        Vector3 dist_to_center = new Vector3(cx, cy, cur.z) - cur;
        int dir_idx = bfs(cx, cy, tx, ty);
        //Debug.Log("Current: " + cx + "," + cy + "| Target: " + tx + "," + ty + " | Direction: " + dir_idx);
        if (dir_idx == -1) return new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
        RaycastHit2D[] hits = Physics2D.RaycastAll(cur, ADJ_VECS[dir_idx], 0.4f);
        Debug.DrawRay(cur, ADJ_VECS[dir_idx], Color.red);
        bool is_wall = false;
        for (int i = 0; i < hits.Length; ++i)
        {
            if (hits[i].collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
            {
                is_wall = true;
                break;
            }
        }
        if (is_wall) return dist_to_center;
        return (ADJ_VECS[dir_idx] + new Vector3(cx, cy, cur.z)) - cur;
        //return ADJ_VECS[dir_idx];
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
