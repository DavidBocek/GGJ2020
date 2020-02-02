using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AddTurretButton : MonoBehaviour
{
	public MoneyCounter moneyCounter;

    void Start()
    {
		GetComponent<Button>().onClick.AddListener( OnButtonClick );
	}

    void Update()
    {
		if ( Input.GetButtonDown( "AddTurret" ) )
			TryAddTurret();
    }

	void OnButtonClick()
	{
		TryAddTurret();
	}

	void TryAddTurret()
	{
		if ( PlayerCommands.Get().TryAddTurret() )
		{

		}
		else
		{
			moneyCounter.FlashRed();
		}
	}
}
