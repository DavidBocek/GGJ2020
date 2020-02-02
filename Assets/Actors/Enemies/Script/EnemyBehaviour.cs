using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBehaviour : MonoBehaviour
{

    [Header("Gameplay")]
    public float moveSpeed;
    public float angularVelocity;
    public float slerpScale = 30.0f;
    public float timeBetweenAttacks;
    public int attackDamage;
    public float tooCloseThreshold;

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
    private float m_theta;
    private Vector3 m_vecToTarget;

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

        switch ( newState )
        {
            case eAIState.ATTACK:
                m_theta = Mathf.Asin( -1 * m_vecToTarget.z );
                if ( -1 * m_vecToTarget.x < 0 )
                {
                    m_theta = ( Mathf.PI ) - m_theta;
                }
                break;
        }
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
        m_vecToTarget = m_orbitCenterPoint.position - transform.position;
        m_vecToTarget.y = 0;
        m_vecToTarget.Normalize();

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
                CompensateForNearbyUnits();
                break;
            case eAIState.MOVE_TO_TARGET:
                transform.rotation = Quaternion.Slerp( transform.rotation, Quaternion.LookRotation( m_vecToTarget, Vector3.up ), 0.5f * Time.deltaTime * slerpScale );
                transform.position += transform.forward * ( moveSpeed * Time.deltaTime );
                CompensateForNearbyUnits();
                break;
            case eAIState.ATTACK:
                transform.rotation = Quaternion.LookRotation( m_vecToTarget, Vector3.up );
                transform.position = GetNextPositionOnCircle();

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

    private Vector3 GetNextPositionOnCircle()
    {
        m_theta += angularVelocity * Time.deltaTime;

        float x = m_orbitCenterPoint.position.x + m_stoppingDistance * Mathf.Cos( m_theta );
        float z = m_orbitCenterPoint.position.z + m_stoppingDistance * Mathf.Sin( m_theta );

        return new Vector3( x, transform.position.y, z );
    }

    private void GetRandomTarget()
    {
        m_target = m_potentialTargets[Random.Range( 0, m_potentialTargets.Count )];
        m_stoppingDistance = m_target.GetComponent<BuildingController>().GetOrbitRadius();
        m_orbitCenterPoint = m_target.GetComponent<BuildingController>().GetOrbitCenter();
    }

    private void CompensateForNearbyUnits()
    {
        GameObject[] allUnits = GameObject.FindGameObjectsWithTag( "Enemy" );
        Vector3 positionAdjustment =  Vector3.zero;

        foreach ( GameObject unit in allUnits )
        {
            if ( Vector3.Distance( transform.position, unit.transform.position ) < tooCloseThreshold )
            {
                Vector3 vecAwayFromUnit = transform.position - unit.transform.position;
                float dist = vecAwayFromUnit.magnitude;
                vecAwayFromUnit.Normalize();

                positionAdjustment += moveSpeed * ( 1 - dist / tooCloseThreshold ) * Time.deltaTime * vecAwayFromUnit;
            }
        }

        transform.position += positionAdjustment;
    }
}
