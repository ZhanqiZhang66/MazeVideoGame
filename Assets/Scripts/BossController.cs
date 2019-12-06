using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossController : MonoBehaviour
{

    private GameObject target;
    private Vector3 dir;
    private Rigidbody2D rigidbody;
    public float speed;
    public GameObject navigator = null;
    public float damage = 0.10f;
    public int bounceBack = 100;
    //public float health = 100f;
    // Start is called before the first frame update
    void Start()
    {
        rigidbody = GetComponent<Rigidbody2D>();
        target = GameObject.FindWithTag("Player");
    }

    // Update is called once per frame
    void Update()
    {
        //if (health <= 0) Destroy(gameObject);
        if (navigator == null) dir =  target.transform.position - transform.position;
        else dir = navigator.GetComponent<Navigator>().find_dir(transform.position, target.transform.position);
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        //EXPT: Triangle physics thingy, faster pathing
        dir = new Vector2(dir.x, dir.y) - rigidbody.velocity;
        //rigidbody.AddForce(dir * Mathf.Min(speed, speed - rigidbody.velocity.magnitude));
        rigidbody.AddForce(dir * speed);
        rigidbody.MoveRotation(angle - 180);
        rigidbody.velocity = Vector2.ClampMagnitude(rigidbody.velocity, speed);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //if (collision.gameObject.CompareTag("Projectile"))
        //{
        //    health -= 5f;
        //}
        if (collision.gameObject.GetComponent<PlayerController>())
        {
            PlayerController.instance.oxygen -= damage;
            rigidbody.AddForce(-((target.transform.position - transform.position) * Mathf.Min(speed, speed - rigidbody.velocity.magnitude))*bounceBack);
        }
    }
}
