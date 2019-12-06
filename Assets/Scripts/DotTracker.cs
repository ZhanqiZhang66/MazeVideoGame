using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DotTracker : MonoBehaviour
{

    private float elapsedTime;
    public float alive_time = 3f;
    private Color emission_col;
    private Color danger_col;
    private Color init_col;

    // Start is called before the first frame update
    void Start()
    {
        elapsedTime = 0f;
        emission_col = gameObject.GetComponent<Renderer>().material.GetColor("_EmissionColor");
        init_col = gameObject.GetComponent<Renderer>().material.GetColor("_EmissionColor");
        danger_col = Color.red;
    }

    // Update is called once per frame
    void Update()
    {
        elapsedTime += Time.deltaTime;
        float decay = ((alive_time - elapsedTime) / alive_time);
        emission_col.r = (decay * init_col.r + (1 - decay) * danger_col.r)*decay;
        emission_col.g = (decay * init_col.g + (1 - decay) * danger_col.g)*decay;
        emission_col.b = (decay * init_col.b + (1 - decay) * danger_col.b)*decay;
        gameObject.GetComponent<Renderer>().material.SetColor("_EmissionColor", emission_col);
        if (elapsedTime > alive_time) Destroy(gameObject);
    }
}