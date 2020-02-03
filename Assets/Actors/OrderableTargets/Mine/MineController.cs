using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MineController : OrderableTarget
{
	//public Dictionary<CuboController, float> userTimes = new Dictionary<CuboController, float>();
	public List<CuboController> users = new List<CuboController>();
    public AudioClip moneyNoise;

    void Start()
    {
		InitWorkTargets();
    }

	public override void OnWork( CuboController user )
	{
		base.OnWork(user);
		if ( !users.Contains(user))
		{
			users.Add( user );
		}
		PlayerCommands.Get().AddMoney( GetMoneyPerWork( user ) );
        Camera.main.GetComponent<AudioSource>().PlayOneShot( moneyNoise );
    }

	public override void LeaveWorkTarget( WorkTarget workTarget, CuboController user )
	{
		base.LeaveWorkTarget( workTarget, user );
		users.Remove( user );
	}

	private int GetMoneyPerWork( CuboController user )
	{
		int idx = users.IndexOf( user );
		if ( idx < 4 )
			return 7;
		else if ( idx < 8 )
			return 6;
		else
			return 5;

	}
}
