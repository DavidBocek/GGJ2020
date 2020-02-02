using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AddCubeButtonController : MonoBehaviour
{
	public MoneyCounter moneyCounter;

    void Start()
    {
		GetComponent<Button>().onClick.AddListener( OnButtonClick );
    }

	void OnButtonClick()
	{
		if ( PlayerCommands.Get().TryAddCubo() )
		{

		}
		else
		{
			moneyCounter.FlashRed();
		}
	}
}
