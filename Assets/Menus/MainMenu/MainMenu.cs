using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
	public Button playButton;
	public string playScene;

    void Start()
    {
		playButton.onClick.AddListener( OnPlayButtonClick );
	}

    void Update()
    {
        
    }

	void OnPlayButtonClick()
	{
		if ( playScene == "" )
			return;

		SceneManager.LoadScene( playScene );
	}
}
