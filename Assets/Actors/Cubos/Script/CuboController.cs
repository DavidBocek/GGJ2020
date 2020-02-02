using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CuboController : Selectable
{
	private enum CuboState
	{
		IDLE,
		MOVING,
		REPAIRING,
		MINING,
		BUILDING
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
			case CuboState.MOVING:
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
			case CuboState.REPAIRING:
				break;
			case CuboState.MINING:
				break;
			case CuboState.BUILDING:
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
			case CuboState.MOVING:
				m_navAgent.SetDestination( m_moveTarget );
				m_navAgent.isStopped = false;
				break;
			case CuboState.REPAIRING:
				break;
			case CuboState.MINING:
				break;
			case CuboState.BUILDING:
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
			case CuboState.MOVING:
				m_isUnmoving = false;
				break;
			case CuboState.REPAIRING:
				break;
			case CuboState.MINING:
				break;
			case CuboState.BUILDING:
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
		SwitchState( CuboState.MOVING );
	}

	public override void OrderOrderableTarget( OrderableTarget target )
	{
		
	}
}
