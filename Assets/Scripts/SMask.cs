using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SMask : MonoBehaviour
{

    public static SMask instance;

	[Range(0.05f, 0.2f)]
	public float flickTime;

	[Range(0.02f, 0.09f)]
	public float addSize;

    private PlayerController player;

    private float min_speed_thres = 0.2f;
    private float max_speed_thres = 3f;
    private float scale_speed = 0.9f;

	float timer = 0;

	private bool bigger = true;
	private Transform target;

    void Awake() {
        if (instance != null) {
            Destroy(gameObject);
        }
        else {
            instance = this;
            //target = PlayerController.instance.transform;
            player = transform.parent.gameObject.GetComponent<MapController>().player;
            target = player.gameObject.transform;
            max_speed_thres = player.max_horizontal_velocity/2;
        }
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;

        //Debug.Log(player.rigidbody.velocity.magnitude);
        if (player.rigidbody.velocity.magnitude > min_speed_thres)
        {
            float move_speed = Mathf.Min(1, player.rigidbody.velocity.magnitude / max_speed_thres);
            float size_scale = 1 + move_speed*move_speed;
            transform.localScale = new Vector3(scale_speed*transform.localScale.x + (1-scale_speed)*size_scale, scale_speed*transform.localScale.y + (1-scale_speed)*size_scale, transform.localScale.z);
        }
        else
        {
            float size_scale = 1;
            transform.localScale = new Vector3(scale_speed * transform.localScale.x + (1 - scale_speed) * size_scale, scale_speed * transform.localScale.y + (1 - scale_speed) * size_scale, transform.localScale.z);
        }

        if (timer > flickTime){

        	if(bigger){
        		transform.localScale = new Vector3(transform.localScale.x + addSize, transform.localScale.y + addSize, transform.localScale.z);
        	}
        	else{
	        	transform.localScale = new Vector3(transform.localScale.x - addSize, transform.localScale.y - addSize, transform.localScale.z);

        	}
        	timer = 0;

        	bigger = !bigger;

        }

        transform.position = Vector3.MoveTowards(transform.position, target.position, 20*Time.deltaTime );
    }
}
