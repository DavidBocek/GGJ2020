using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DontDestroyOnLoad : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if ( GameObject.FindGameObjectsWithTag( "MusicPlayer" ).Length > 1 )
        { 
            Destroy( gameObject );
        }
        else
        {
            GameObject.DontDestroyOnLoad( gameObject );
            gameObject.transform.position = Camera.main.transform.position;
        }
        
    }

    // Update is called once per frame
    void Update()
    {
		if ( GameObject.FindGameObjectsWithTag( "MusicPlayer" ).Length > 1 )
		{
			Destroy( gameObject );
		}
	}
}
