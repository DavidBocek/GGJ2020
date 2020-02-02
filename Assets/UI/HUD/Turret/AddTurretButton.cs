using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AddTurretButton : MonoBehaviour
{
	public MoneyCounter moneyCounter;
	public Text turretCost;

    void Start()
    {
		GetComponent<Button>().onClick.AddListener( OnButtonClick );
		turretCost.text = PlayerCommands.Get().addTurretCost.ToString();
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
