using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;
    //Outlets
    public AudioSource audioSource;
    public AudioClip getKeySound;
    public AudioClip successSound;
    public AudioClip failureSound;
  
    // Start is called before the first frame update
    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
    }
    void Start()
    {
        DontDestroyOnLoad(gameObject);
        audioSource = GetComponent<AudioSource>();
    }
    public void PlaySoundGetKey()
    {
        audioSource.PlayOneShot(getKeySound);
    }

    public void PlaySuccess()
    {
        audioSource.PlayOneShot(successSound);
    }

    public void PlayFailure()
    {
        audioSource.PlayOneShot(failureSound);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
