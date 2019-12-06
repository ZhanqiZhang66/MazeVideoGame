using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OxygenPatch : MonoBehaviour
{
    public bool persistent = false;
    private float timer = 0f;
    private float expire_timer = 3f;
    public float oxygen_regeneration_rate = 0.0005f;
    // Start is called before the first frame update
    void Start()
    {
        expire_timer += Random.Range(0f, 20f);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.GetComponent<PlayerController>())
        {
            StartCoroutine("AddOxygen");
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.GetComponent<PlayerController>())
        {
            StopCoroutine("AddOxygen");
        }
    }


    private IEnumerator AddOxygen()
    {
        while (true)
        {
            PlayerController.instance.oxygen += PlayerController.instance.oxygen < 1f ? oxygen_regeneration_rate : 0f;
            yield return new WaitForSeconds(0.5f);
        }
    }
    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if (!persistent && timer > expire_timer)
        {
            Destroy(gameObject);
        }
    }
}
