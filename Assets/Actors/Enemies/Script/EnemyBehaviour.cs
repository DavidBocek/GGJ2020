using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBehaviour : MonoBehaviour
{

    [Header("Gameplay")]
    public GameObject target;
    public float moveSpeed;

    private enum eAIState
    {
        MOVE_TO_LINE,
        MOVE_TO_TARGET,
        ATTACK
    }

    private eAIState m_currentState;

    // Start is called before the first frame update
    void Start()
    {
        EnterState( eAIState.MOVE_TO_LINE );
    }

    private void EnterState( eAIState newState )
    {
        eAIState oldState = m_currentState;

        /*
        switch ( newState )
        {
            case eAIState.IDLE:
                m_navMeshAgent.isStopped = true;

                break;
            case eAIState.MOVING:
                m_navMeshAgent.isStopped = false;
                break;
        }
        */

        m_currentState = newState;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateState();
        Act();
    }

    private void UpdateState()
    {
        switch ( m_currentState )
        {
            case eAIState.IDLE:
                if ( m_canSeePlayer && Time.time > m_forceIdleUntilTime )
                {
                    EnterState( eMeleeAIState.MOVING );
                }
                //m_animator.SetBool( "IsMoving", false );
                break;
            case eAIState.MOVING:
                if ( m_transitionToIdle )
                {
                    m_transitionToIdle = false;
                    EnterState( eMeleeAIState.IDLE );
                }
                break;
        }
    }
}
