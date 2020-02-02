using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MineController : OrderableTarget
{
	public int moneyPerWork;

    void Start()
    {
		InitWorkTargets();
    }

	public override void OnWork( CuboController user )
	{
		base.OnWork(user);
		PlayerCommands.Get().AddMoney( moneyPerWork );
	}
}
