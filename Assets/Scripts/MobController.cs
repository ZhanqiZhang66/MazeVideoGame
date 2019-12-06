using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobController : MonoBehaviour
{

    private GameObject target;
    private Vector3 dir;
    private Rigidbody2D rigidbody;
    public float speed;
    public GameObject navigator = null;
    public float lifeTime = 10f;
    public float damage = 0.05f;
    private float FACE_THRESHOLD = 3f;
    // Start is called before the first frame update
    void Start()
    {
        rigidbody = GetComponent<Rigidbody2D>();
        target = GameObject.FindWithTag("Player");

        System.Random rnd = new System.Random();
        lifeTime += (float)rnd.NextDouble() * 10f;
        Destroy(gameObject, lifeTime);
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 dir_to_target = target.transform.position - transform.position;
        if (navigator == null) dir = dir_to_target;
        else dir = navigator.GetComponent<Navigator>().find_dir(transform.position, target.transform.position);
        //Debug.Log("YAHAHAHAHHAHAH");
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        if (dir_to_target.magnitude < FACE_THRESHOLD) angle = Mathf.Atan2(dir_to_target.y, dir_to_target.x) * Mathf.Rad2Deg;
        //Debug.Log(angle);
        //EXPT: Triangle physics thingy, faster pathing
        dir = new Vector2(dir.x, dir.y) - rigidbody.velocity;
        //rigidbody.AddForce(dir * Mathf.Min(speed, speed - rigidbody.velocity.magnitude));
        rigidbody.AddForce(dir * speed);
        rigidbody.MoveRotation(angle - 90);
        rigidbody.velocity = Vector2.ClampMagnitude(rigidbody.velocity, speed);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController.instance.oxygen -= damage;
        }
    }
}
