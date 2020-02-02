using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using MEC;

public class NegaCube : OrderableTarget
{
	public int power = 0;
	public float sacrificeTime;
	public float sacrificeDist;



	void Start()
    {
		InitWorkTargets();
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
			Destroy( user.gameObject );
		} );
	}
}
