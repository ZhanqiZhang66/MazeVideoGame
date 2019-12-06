 using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Healthbar : MonoBehaviour
{
    // Start is called before the first frame update

    private Transform bar;

    private void Awake()
    {
        bar = transform.Find("Bar");
    }

    void Start()
    {

    }

    public void SetSize(float sizeNormalized){
    	bar.localScale = new Vector3(sizeNormalized, 1f);
    }

    public void SetColor(Color color){
        bar.Find("BarSprite").GetComponent<SpriteRenderer>().color = color;
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
