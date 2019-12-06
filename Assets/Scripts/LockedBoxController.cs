using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LockedBoxController : MonoBehaviour
{
    // Start is called before the first frame update
    public UnityEditor.SceneAsset ChangeTo;

void Awake()
    {
        gameObject.SetActive(false);
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Something collided with our Goal");
        if (collision.gameObject.GetComponent<PlayerController>())
        {
            SoundManager.instance.PlaySuccess();
            SoundManager.instance.audioSource.volume = 0.2f;
            SceneManager.LoadScene(ChangeTo.name);
        }
    }
}
