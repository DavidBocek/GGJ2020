using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using MEC;

public class NegaCube : OrderableTarget
{
	public int power = 0;
	public int goalPower;
	public float sacrificeTime;
	public float sacrificeDist;



	void Start()
    {
		InitWorkTargets();
		UIStatic.SetInt( UIStatic.MAX_SACRIFICE, goalPower );
		UIStatic.SetInt( UIStatic.CUR_SACRIFICE, power );
	}

	private void Update()
	{	
		UIStatic.SetInt( UIStatic.CUR_SACRIFICE, power );

		if ( Input.GetKeyDown( KeyCode.W ) )
			PlayerCommands.Get().Win();
		else if ( Input.GetKeyDown( KeyCode.L ) )
			PlayerCommands.Get().Lose();
	}

	public override void ClaimWorkTarget( WorkTarget workTarget )
	{	
	}

	public override void OccupyWorkTarget( WorkTarget workTarget )
	{
	}

	public override void LeaveWorkTarget( WorkTarget workTarget )
	{
	}

	public override void OnWork( CuboController user )
	{
		PlayerCommands.Get().OnCuboDestroyed();
		user.GetComponentInChildren<Selectable>().enabled = false;

		GameObject model = user.model;
		model.transform.DOPunchScale( new Vector3( 1.025f, 0.8f, 1.025f ), 0.25f, 0, 2f );

		Sequence seq = DOTween.Sequence();
		seq.Append( model.transform.DOLocalMoveY( 5f, sacrificeTime ).SetEase( Ease.InQuad ) );
		seq.Join( model.transform.DOScale( 0f, sacrificeTime - 0.4f ).SetEase( Ease.InQuad ).SetDelay( 0.4f ));
		seq.Play();

		Timing.CallDelayed( sacrificeTime + 0.1f, delegate
		{
			Instantiate( workFxObj, transform.position + Vector3.up * 17f, Quaternion.LookRotation( Vector3.up, Vector3.left ) );
			power++;
			UIStatic.SetInt( UIStatic.CUR_SACRIFICE, power );
			Destroy( user.gameObject );

			if ( power >= goalPower )
				PlayerCommands.Get().Win();
		} );
	}
}
