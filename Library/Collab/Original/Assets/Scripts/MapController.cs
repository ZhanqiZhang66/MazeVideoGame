using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapController : MonoBehaviour
{
    
    public static MapController game_controller;
    public PlayerController player;

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
        }
        //player = GetComponent<PlayerController>();
    }

    // Start is called before the first frame update

    void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void move_player(Vector2 move_loc)
    {
        player.transform.position = move_loc;
    }
}
