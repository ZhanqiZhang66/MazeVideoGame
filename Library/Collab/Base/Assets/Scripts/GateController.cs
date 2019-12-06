using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;

public class GateController : MonoBehaviour
{

    public Vector2 player_teleport_pos;
    public SceneAsset ChangeTo;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        
        if (collision.gameObject.GetComponent<PlayerController>())
        {
            /*
            Vector2 new_loc = new Vector2(transform.position.x, transform.position.y);
            if (Mathf.Abs(new_loc.x) > 5)
            {
                new_loc.x = -new_loc.x;
            }
            if (Mathf.Abs(new_loc.y) > 5)
            {
                new_loc.y = -new_loc.y;
            }
            */
            MapController.game_controller.move_player(player_teleport_pos);
            SceneManager.LoadScene(ChangeTo.name);
        }
    }
}
