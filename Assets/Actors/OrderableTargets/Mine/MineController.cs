using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MineController : OrderableTarget
{
	public int moneyPerWork;
    public AudioClip moneyNoise;

    void Start()
    {
		InitWorkTargets();
    }

	public override void OnWork( CuboController user )
	{
		base.OnWork(user);
		PlayerCommands.Get().AddMoney( moneyPerWork );
        Camera.main.GetComponent<AudioSource>().PlayOneShot( moneyNoise );
    }
}
