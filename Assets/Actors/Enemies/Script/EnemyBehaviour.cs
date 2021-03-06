﻿using MEC;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class EnemyBehaviour : MonoBehaviour
{

    [Header( "Gameplay" )]
    public float moveSpeed;
    public float angularVelocity;
    public float slerpScale = 30.0f;
    public float timeBetweenAttacks;
    public int attackDamage;
    public float tooCloseThreshold;
	public string[] targetTags;

    [Header( "Laser Visuals" )]
    public Transform muzzlePos;
    public LineRenderer laserFx;
    public float attackForwardRand;
    public float attackRightRand;
    public float attackLaserTime;
    public float attackLaserEndWidth;

	[Header( "Sounds" )]
	public AudioClip laserNoise;
	public AudioClip deathNoise;
	public AudioClip spawnNoise;

    public GameObject visuals;

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
		PlaySound( spawnNoise );
        EnterState( eAIState.MOVE_TO_LINE );
		UpdateTargetList();
		GetRandomTarget();
    }

    private void UpdateTargetList()
    {
		m_potentialTargets = new List<GameObject>();
		foreach ( string tag in targetTags)
		{
			foreach ( GameObject obj in GameObject.FindGameObjectsWithTag( tag ) )
			{
				Health health = obj.GetComponent<Health>();
				if ( health != null && health.IsAlive() )
				{
					m_potentialTargets.Add( obj );
				}
			}
			if ( m_potentialTargets.Count > 0 )
				break;
		}

		if (m_potentialTargets.Count == 0)
		{
			GameObject[] objs = GameObject.FindGameObjectsWithTag( targetTags[0] );
			m_potentialTargets.Add(objs[Random.Range( 0, objs.Length )]);
		}
        /*if ( m_target == null )
        {
            GetRandomTarget();
			return;
        }
		Health targetHealth = m_target.GetComponent<Health>();
		if ( targetHealth == null || !targetHealth.IsAlive() )
			GetRandomTarget();*/
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
				if ( !IsTargetAlive() )
				{
					GetRandomTarget();
				}
				break;
            case eAIState.ATTACK:
                if ( ! IsTargetAlive() )
                {
					GetRandomTarget();
                    EnterState( eAIState.MOVE_TO_TARGET );
                }
                break;
			case eAIState.MOVE_TO_LINE:
				if ( !IsTargetAlive() )
				{
					GetRandomTarget();
				}
				break;
		}

		m_vecToTarget = m_orbitCenterPoint.position - transform.position;
		m_vecToTarget.y = 0;
		m_vecToTarget.Normalize();
	}

    private void Act()
    {
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
                //visuals.transform.localRotation *= Quaternion.Euler( 0, 500 * Time.deltaTime, 0 );

                //transform.Rotate( transform.forward * 50 * Time.deltaTime, Space.Self );
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
            Timing.RunCoroutineSingleton( _PlayAttackFX().CancelWith(gameObject), gameObject, "_PlayAttackFX", SingletonBehavior.Overwrite );
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
        m_stoppingDistance = m_target.GetComponent<OrderableTarget>().GetOrbitRadius() + Random.Range(-3f, 3f);
        m_orbitCenterPoint = m_target.GetComponent<OrderableTarget>().GetOrbitCenter();
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

    public IEnumerator<float> _PlayAttackFX()
    {
        laserFx.gameObject.SetActive( true );
        //muzzleFx.gameObject.SetActive( true );

        Health enemyHealth = m_target.GetComponent<Health>();
        if ( enemyHealth == null )
            yield return 0f;

        Vector3 targetPos = m_orbitCenterPoint.transform.position;
        float startTime = Time.time;
        Vector3 offset = m_orbitCenterPoint.forward * Random.Range( -attackForwardRand, attackForwardRand )
                    + m_orbitCenterPoint.right * Random.Range( -attackRightRand, attackRightRand )
                    + m_orbitCenterPoint.up;

        targetPos += offset;

		PlaySound( laserNoise );

        while ( Time.time < startTime + attackLaserTime )
        {
            if ( m_currentState != eAIState.ATTACK )
                break;

            laserFx.SetPosition( 0, muzzlePos.position );

            laserFx.SetPosition( 1, targetPos );

            //laserFx.widthMultiplier = MathUtil.RemapClamped( Time.time, startTime, startTime + attackLaserTime, 1f, attackLaserEndWidth );

            yield return Timing.WaitForOneFrame;
        }

        laserFx.gameObject.SetActive( false );
        //muzzleFx.gameObject.SetActive( false );
    }

    public void OnDeath()
	{
		PlaySound( deathNoise );
		Destroy( gameObject );
	}

	private void PlaySound(AudioClip clip)
	{
		Camera.main.GetComponent<AudioSource>().PlayOneShot( clip );
	}
}
