using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using DG.Tweening;
using MEC;

public class CuboController : Selectable
{
	private enum CuboState
	{
		IDLE,
		MOVING_TO_EMPTY,
		MOVING_TO_WORK,
		WORKING
	}
	private CuboState m_state;

	private NavMeshAgent m_navAgent;

	//visuals
	[Header( "Visuals" )]
	public GameObject model;

	//moving
	[Header( "Movement" )]
	public float unmovingVelocityThreshold;
	public float unmovingTimeCutoff;
	private Vector3 m_moveTarget;
	private float m_lastUnmovedTime;
	private bool m_isUnmoving;

	//working
	[Header("Working")]
	public float workTargetDistance;
	public float workRotSnapLerp;
	public float workCooldown;
	private float m_lastWorkTime;
	private OrderableTarget m_orderableTarget;
	private WorkTarget m_workTarget;
	public OrderableTarget GetOrderableTarget() { return m_orderableTarget; }

    void Start()
    {
		m_state = CuboState.IDLE;

		m_navAgent = GetComponent<NavMeshAgent>();

		SpawnAnim();
    }

	public void SpawnAnim()
	{
		model.transform.localScale = Vector3.zero;
		Sequence seq = DOTween.Sequence();
		seq.Append( model.transform.DOScale( 1.15f, 0.5f ).SetEase( Ease.OutCubic ) );
		seq.Append( model.transform.DOScale( 0.925f, 0.3f ).SetEase( Ease.InCubic ) );
		seq.Append( model.transform.DOScale( 1f, 0.2f ).SetEase( Ease.InCubic ) );
		seq.Play();
		Timing.CallDelayed( 1.25f, delegate { model.transform.localScale = Vector3.one; } );
	}

    void Update()
    {
		UpdateState();
    }

	void UpdateState()
	{
		switch ( m_state )
		{
			case CuboState.IDLE:
				break;
			case CuboState.MOVING_TO_EMPTY:
				if ( m_navAgent.velocity.magnitude < unmovingVelocityThreshold )
				{
					if ( !m_isUnmoving )
					{
						m_lastUnmovedTime = Time.time;
						m_isUnmoving = true;
					}

					if ( Time.time >= m_lastUnmovedTime + unmovingTimeCutoff )
						SwitchState( CuboState.IDLE );
				}
				else
				{
					m_isUnmoving = false;
				}
				break;
			case CuboState.MOVING_TO_WORK:
				if ( Vector3.Distance( m_moveTarget, transform.position ) < workTargetDistance )
				{
					m_orderableTarget.OccupyWorkTarget( m_workTarget );
					SwitchState( CuboState.WORKING );
				}
				break;
			case CuboState.WORKING:
				transform.rotation = Quaternion.Slerp( transform.rotation, m_workTarget.obj.transform.rotation, workRotSnapLerp * Time.deltaTime );
				if ( Time.time > m_lastWorkTime + workCooldown )
				{
					m_orderableTarget.OnWork( this );
					m_lastWorkTime = Time.time;
					OnDoWork();
				}
				break;
			default:
				throw new UnityException( "Unrecognized cubo state: " + m_state );
		}
	}

	void OnEnterState( CuboState newState )
	{
		switch ( m_state )
		{
			case CuboState.IDLE:
				m_navAgent.SetDestination( transform.position );
				m_navAgent.isStopped = true;
				break;
			case CuboState.MOVING_TO_EMPTY:
				m_navAgent.SetDestination( m_moveTarget );
				m_navAgent.isStopped = false;
				break;
			case CuboState.MOVING_TO_WORK:
				m_navAgent.SetDestination( m_moveTarget );
				m_navAgent.isStopped = false;
				break;
			case CuboState.WORKING:
				m_orderableTarget.OccupyWorkTarget( m_workTarget );
				m_navAgent.Move( m_workTarget.pos - transform.position );
				m_navAgent.SetDestination( m_workTarget.pos );
				m_navAgent.isStopped = false;
				m_lastWorkTime = Time.time + Random.Range(0f, 0.3f);
				break;
			default:
				throw new UnityException( "Unrecognized cubo state: " + m_state );
		}
	}

	void OnExitState( CuboState oldState, CuboState nextState )
	{
		switch ( m_state )
		{
			case CuboState.IDLE:
				break;
			case CuboState.MOVING_TO_EMPTY:
				m_isUnmoving = false;
				break;
			case CuboState.MOVING_TO_WORK:
				break;
			case CuboState.WORKING:
				break;
			default:
				throw new UnityException( "Unrecognized cubo state: " + m_state );
		}
	}

	void SwitchState( CuboState newState )
	{
		OnExitState( m_state, newState );
		m_state = newState;
		OnEnterState( newState );
	}

	public override void OrderGround( Vector3 location )
	{
		if ( m_orderableTarget != null )
		{
			m_orderableTarget.LeaveWorkTarget( m_workTarget );
			m_workTarget = null;
			m_orderableTarget = null;
		}

		location.y = 1f;
		m_moveTarget = location;
		SwitchState( CuboState.MOVING_TO_EMPTY );
	}

	public override void OrderOrderableTarget( OrderableTarget target, WorkTarget workTarget )
	{
		if ( m_orderableTarget != null )
			m_orderableTarget.LeaveWorkTarget( m_workTarget );

		m_orderableTarget = target;
		m_orderableTarget.ClaimWorkTarget( workTarget );
		m_workTarget = workTarget;
		m_moveTarget = workTarget.pos;
		SwitchState( CuboState.MOVING_TO_WORK );
	}

	private void OnDoWork()
	{
		model.transform.DOPunchScale( new Vector3( 1.025f, 0.8f, 1.025f ), 0.25f, 0, 2f );

		Sequence seq = DOTween.Sequence();
		seq.Append( model.transform.DOLocalMoveY( 2f, 0.2f ).SetEase( Ease.InQuad ) );
		seq.Append( model.transform.DOLocalMoveY( 0.5f, 0.15f ).SetEase( Ease.OutCubic ) );
		seq.Append( model.transform.DOScale( new Vector3( 1.15f, 0.6f, 1.15f ), 0.1f ).SetLoops( 2, LoopType.Yoyo ) );
		seq.Play();
		//model.transform.DOJump( transform.position+Vector3.up, 2f, 1, 0.5f ).SetDelay(0.2f);
	}
}
