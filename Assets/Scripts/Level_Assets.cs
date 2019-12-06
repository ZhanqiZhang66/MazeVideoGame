using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class Level_Assets : MonoBehaviour
{

    CinemachineVirtualCamera cine_cam;
    private GameObject player_obj = null;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        cine_cam = GameObject.Find("CM vcam1").GetComponent<CinemachineVirtualCamera>();
        player_obj = GameObject.FindWithTag("Player");
        if (player_obj != null)
        {
            cine_cam.Follow = player_obj.transform;
            //Debug.Log(cine_cam + " follow: " + player_obj.transform);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (player_obj == null)
        {
            player_obj = GameObject.FindWithTag("Player");
            if (player_obj != null)
            {
                cine_cam.Follow = player_obj.transform;
                //Debug.Log(cine_cam + " follow: " + player_obj.transform);
            }
        }
        /*else
        {
            cine_cam.Follow = player_obj.transform;
            //Debug.Log(cine_cam + " follow: " + player_obj.transform);
        }*/
    }
}
