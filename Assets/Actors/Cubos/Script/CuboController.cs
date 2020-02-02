﻿using System.Collections;
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
	[Header("Working")]
	public float workTargetDistance;
	public float workRotSnapLerp;
	public float workCooldown;
	private OrderableTarget m_orderableTarget;
	private WorkTarget m_workTarget;
	public OrderableTarget GetOrderableTarget() { return m_orderableTarget; }

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
					m_orderableTarget.OccupyWorkTarget( m_workTarget );
					SwitchState( CuboState.WORKING );
				}
				break;
			case CuboState.WORKING:
				transform.rotation = Quaternion.Slerp( transform.rotation, m_workTarget.obj.transform.rotation, workRotSnapLerp * Time.deltaTime );
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
				m_navAgent.Move( m_workTarget.pos - transform.position );
				m_navAgent.SetDestination( m_workTarget.pos );
				m_navAgent.isStopped = false;		
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
				if ( nextState != CuboState.WORKING )
				{
					m_orderableTarget.LeaveWorkTarget( m_workTarget );
					m_orderableTarget = null;
					m_workTarget = null;
				}
				break;
			case CuboState.WORKING:
				if ( nextState != CuboState.MOVING_TO_WORK && nextState != CuboState.WORKING )
				{
					m_orderableTarget.LeaveWorkTarget( m_workTarget );
					m_orderableTarget = null;
					m_workTarget = null;
				}
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
}
