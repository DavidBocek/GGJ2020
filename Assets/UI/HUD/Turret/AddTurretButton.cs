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
		turretCost.text = PlayerCommands.Get().addTurretCostStart.ToString();
	}

    void Update()
    {
		if ( Input.GetButtonDown( "AddTurret" ) )
			TryAddTurret();

		turretCost.text = PlayerCommands.Get().GetCurTurretCost().ToString();
	}

	void OnButtonClick()
	{
		TryAddTurret();
	}

	void TryAddTurret()
	{
		if ( PlayerCommands.Get().isAddingTurret )
			return;

		if ( PlayerCommands.Get().TryAddTurret() )
		{

		}
		else
		{
			moneyCounter.FlashRed();
		}
	}
}
