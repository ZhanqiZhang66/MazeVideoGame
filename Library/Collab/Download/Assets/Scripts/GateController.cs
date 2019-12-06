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
            MapController.game_controller.move_player(player_teleport_pos);
            SceneManager.LoadScene(ChangeTo.name);
        }
    }
}
