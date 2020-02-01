using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCommands : MonoBehaviour
{


	private void Start()
	{
		
	}

	private void Update()
	{
		UpdateOrders();
	}

	private void UpdateOrders()
	{
		if ( !Input.GetButtonDown( "Order" ) )
			return;

		if ( Selection.selectables.Count == 0 )
			return;


	}
}
