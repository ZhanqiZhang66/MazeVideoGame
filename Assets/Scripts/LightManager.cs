using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LightManager : MonoBehaviour
{
    public static LightManager instance;

	public Tilemap DarkMap;
	public Tilemap BlurredMap; 
	public Tilemap BackgroundMap;

	public Tile DarkTile;
	public Tile BlurredTile;

    void Awake() {
        if (instance != null) {
            Destroy(gameObject);
        }
        else {
            instance = this;
            
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(gameObject);

        DarkMap.origin = BlurredMap.origin = BackgroundMap.origin;
        DarkMap.size = BlurredMap.size = BackgroundMap.size;



        foreach (Vector3Int p in DarkMap.cellBounds.allPositionsWithin)
        {
        	DarkMap.SetTile(p, DarkTile);
        }

        foreach (Vector3Int p in BlurredMap.cellBounds.allPositionsWithin)
        {
        	BlurredMap.SetTile(p, BlurredTile);
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
