using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretSelectable : Selectable
{
	public TurretController controller;

	private void Start()
	{
	}

	public override void OrderTargetEnemy( GameObject enemy )
	{
		controller.TryForceTarget( enemy );
	}
}
