using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBehaviour : MonoBehaviour
{

    [Header("Gameplay")]
    public float moveSpeed;
    public float slerpScale = 30.0f;
    public float timeBetweenAttacks;
    public int attackDamage;

    private enum eAIState
    {
        MOVE_TO_LINE,
        MOVE_TO_TARGET,
        ATTACK
    }

    private eAIState m_currentState;
    private GameObject m_target;
    private float m_stoppingDistance;
    private Transform m_orbitCenterPoint;
    private List<GameObject> m_potentialTargets = new List<GameObject>();
    private float m_nextGoodAttackTime;

    // Start is called before the first frame update
    void Start()
    {
        EnterState( eAIState.MOVE_TO_LINE );
    }

    private void UpdateTargetList()
    {
        m_potentialTargets = new List<GameObject>( GameObject.FindGameObjectsWithTag( "Building" ) );

        foreach( GameObject target in new List<GameObject>( m_potentialTargets ) )
        {
            if ( ! target.GetComponent<Health>().IsAlive() )
            {
                m_potentialTargets.Remove( target );
            }
        }

        if ( m_target == null )
        {
            GetRandomTarget();
        }
    }
    
    private void EnterState( eAIState newState )
    {
        m_currentState = newState;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateTargetList();
        UpdateState();
        Act();
    }

    private void UpdateState()
    {
        switch ( m_currentState )
        {
            case eAIState.MOVE_TO_TARGET:
                if ( Vector3.Distance( gameObject.transform.position, m_orbitCenterPoint.position) < m_stoppingDistance )
                {
                    EnterState( eAIState.ATTACK );
                }
                break;
            case eAIState.ATTACK:
                if ( ! IsTargetAlive() )
                {
                    EnterState( eAIState.MOVE_TO_TARGET );
                }
                break;
        }
    }

    private void Act()
    {
        if ( ! m_target.GetComponent<Health>().IsAlive() )
        {
            GetRandomTarget();
        }

        switch ( m_currentState )
        {
            case eAIState.MOVE_TO_LINE:
                transform.position += transform.forward * ( moveSpeed * Time.deltaTime );
                break;
            case eAIState.MOVE_TO_TARGET:
                Vector3 vecToTarget = m_orbitCenterPoint.position - transform.position;
                vecToTarget.y = 0;
                transform.rotation = Quaternion.Slerp( transform.rotation, Quaternion.LookRotation( vecToTarget, Vector3.up ), 0.5f * Time.deltaTime * slerpScale );
                transform.position += transform.forward * ( moveSpeed * Time.deltaTime );
                break;
            case eAIState.ATTACK:
                TryAttackTarget();
                break;
        }
    }

    public void OnTriggerEnter( Collider other )
    {
        if ( other.gameObject.name == "TheLine" )
        {
            EnterState( eAIState.MOVE_TO_TARGET );
        }
    }

    private bool IsTargetAlive()
    {
        return m_target.GetComponent<Health>().IsAlive();
    }

    private void DamageTarget()
    {
        m_target.GetComponent<Health>().TakeDamage( attackDamage );
    }

    private void TryAttackTarget()
    {
        if ( Time.time >= m_nextGoodAttackTime )
        {
            m_nextGoodAttackTime = Time.time + timeBetweenAttacks;
            // attack anim stuff
            DamageTarget();
        }
    }

    private void GetRandomTarget()
    {
        m_target = m_potentialTargets[Random.Range( 0, m_potentialTargets.Count )];
        m_stoppingDistance = m_target.GetComponent<BuildingController>().GetOrbitRadius();
        m_orbitCenterPoint = m_target.GetComponent<BuildingController>().GetOrbitCenter();
    }
}
