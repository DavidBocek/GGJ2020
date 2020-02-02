using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemySpawnTimerUI : MonoBehaviour
{
	public Image timerFill;

    void Start()
    {
		timerFill.fillAmount = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	public void SetFrac( float frac)
	{
		timerFill.fillAmount = frac;
	}
}
