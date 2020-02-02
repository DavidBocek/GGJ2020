using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AddCubeButtonController : MonoBehaviour
{
	public MoneyCounter moneyCounter;
	public CurCubos curCubos;

    void Start()
    {
		GetComponent<Button>().onClick.AddListener( OnButtonClick );
    }

	private void Update()
	{
		if ( Input.GetButtonDown( "AddCube" ) && !PlayerCommands.Get().isAddingTurret )
			TryAddCubo();
	}

	void OnButtonClick()
	{
		TryAddCubo();
	}

	public void TryAddCubo()
	{
		if ( !PlayerCommands.Get().EnoughRoomForCubos() )
		{
			curCubos.FlashRed();
			return;
		}

		if ( PlayerCommands.Get().TryAddCubo() )
		{
			//play sound and stuff
		}
		else
		{
			moneyCounter.FlashRed();
		}
	}
}
