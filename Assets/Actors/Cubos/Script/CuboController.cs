using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

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

	//moving
	[Header( "Movement" )]
	public float unmovingVelocityThreshold;
	public float unmovingTimeCutoff;
	private Vector3 m_moveTarget;
	private float m_lastUnmovedTime;
	private bool m_isUnmoving;

	//working
	public float workTargetDistance;
	private OrderableTarget m_orderableTarget;
	private WorkTarget m_workTarget;

    void Start()
    {
		m_state = CuboState.IDLE;

		m_navAgent = GetComponent<NavMeshAgent>();
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
					SwitchState( CuboState.WORKING );
				}
				break;
			case CuboState.WORKING:
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
				m_navAgent.SetDestination( transform.position );
				m_navAgent.isStopped = false;
				break;
			default:
				throw new UnityException( "Unrecognized cubo state: " + m_state );
		}
	}

	void OnExitState( CuboState oldState )
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
				m_orderableTarget.LeaveWorkTarget( m_workTarget );
				m_orderableTarget = null;
				m_workTarget = null;
				break;
			default:
				throw new UnityException( "Unrecognized cubo state: " + m_state );
		}
	}

	void SwitchState( CuboState newState )
	{
		OnExitState( m_state );
		m_state = newState;
		OnEnterState( newState );
	}

	public override void OrderGround( Vector3 location )
	{
		location.y = 1f;
		m_moveTarget = location;
		SwitchState( CuboState.MOVING_TO_EMPTY );
	}

	public override void OrderOrderableTarget( OrderableTarget target, WorkTarget workTarget )
	{
		m_orderableTarget = target;
		m_orderableTarget.FillWorkTarget( workTarget );
		m_workTarget = workTarget;
		m_moveTarget = workTarget.pos;
		SwitchState( CuboState.MOVING_TO_WORK );
	}
}
