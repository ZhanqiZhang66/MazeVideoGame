using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameInit : MonoBehaviour
{

    public GameObject level_assets;
    public GameObject map_manager;
    public GameObject player_obj;
    public bool is_tutorial = false;
    public GameObject goal_obj;

private void Awake()
    {
        if (GameObject.FindWithTag("Player") == null)
        {
            Instantiate(player_obj, new Vector3(0, 0, 0), Quaternion.identity);
        }
        if (GameObject.FindWithTag("Level_Assets") == null)
        {
            Instantiate(level_assets, new Vector3(0, 0, 0), Quaternion.identity);
        }
        if (GameObject.FindWithTag("MapManager") == null)
        {
            Instantiate(map_manager, new Vector3(0, 0, 0), Quaternion.identity);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (is_tutorial)
        {
            Debug.Log("Player keys: " + PlayerController.instance.numKeys);
            if (PlayerController.instance.numKeys >= 1)
            {
                goal_obj.SetActive(true);
            }
        }
    }
}
