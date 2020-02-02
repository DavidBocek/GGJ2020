using MEC;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TurretController : OrderableTarget
{
	private Health m_health;
	public int healPerWork;

	public enum TurretState { IDLE, ATTACKING }
	private TurretState m_state;

	[Header( "Turret Parts" )]
	public GameObject top;
	public GameObject barrel;

	[Header( "Idle" )]
	public Vector3 idleTopTarget;
	public float idleTopSpeed;
	public float idleBarrelAngle;
	public float idleBarrelSpeed;
	public float engageDist;

	[Header( "Attacking" )]
	private GameObject m_target;
	public int attackDamage;
	public float attackCooldown;
	private float m_lastAttackTime;
	public LineRenderer laserFx;
	public float attackForwardRand;
	public float attackRightRand;
	public float attackLaserTime;
	public float attackLaserEndWidth;
	public Transform muzzlePos;
	public GameObject muzzleFx;

	void Start()
    {
		InitWorkTargets();
		m_health = GetComponent<Health>();
		m_state = TurretState.IDLE;
		//Timing.CallDelayed(0.01f, delegate { m_health.TakeDamage( m_health.maxHealth ); } );
		m_health.TakeDamage( m_health.maxHealth );
	}

    void Update()
    {
		if ( !m_health.IsAlive() )
		{
			m_state = TurretState.IDLE;
		}

		switch ( m_state )
		{
			case TurretState.IDLE:
				Vector3 topDir = Vector3.RotateTowards( top.transform.forward, idleTopTarget, idleTopSpeed * Mathf.Deg2Rad * Time.deltaTime, 0f );
				top.transform.rotation = Quaternion.LookRotation( topDir, Vector3.up );

				Vector3 barrelAngles = barrel.transform.localEulerAngles;
				float newAngle = barrelAngles.x;
				if ( barrelAngles.x > idleBarrelAngle )
				{
					newAngle = Mathf.Max( barrelAngles.x - idleBarrelSpeed * Time.deltaTime, idleBarrelAngle );
				}
				else if ( barrelAngles.x < idleBarrelAngle )
				{
					newAngle = Mathf.Min( barrelAngles.x + idleBarrelSpeed * Time.deltaTime, idleBarrelAngle );
				}
				barrelAngles.x = newAngle;
				barrel.transform.localEulerAngles = barrelAngles;

				if ( !m_health.IsAlive() )
					break;

				float closestDist = Mathf.Infinity;
				GameObject closestEnemy = null;
				foreach ( GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy"))
				{
					float sqrDist1 = Vector3.ProjectOnPlane( enemy.transform.position - transform.position, Vector3.up ).sqrMagnitude;
					if ( sqrDist1 <= engageDist * engageDist)
					{
						if ( sqrDist1 < closestDist )
						{
							closestDist = sqrDist1;
							closestEnemy = enemy;
						}
					}
				}
				if ( closestEnemy != null )
				{
					m_target = closestEnemy;
					m_state = TurretState.ATTACKING;
					m_lastAttackTime = Time.time - Random.Range( attackCooldown*.4f, attackCooldown*0.65f);
				}

				break;
			case TurretState.ATTACKING:
				bool needNewTarget = false;
				if ( m_target == null )
					needNewTarget = true;
				else
				{
					float sqrDist = Vector3.ProjectOnPlane( m_target.transform.position - transform.position, Vector3.up ).sqrMagnitude;
					if ( sqrDist > ( engageDist + 5 ) * ( engageDist + 5 ) )
						needNewTarget = true;
				}
				
				if ( needNewTarget )
				{
					float closestDist1 = Mathf.Infinity;
					GameObject closestEnemy1 = null;
					foreach ( GameObject enemy in GameObject.FindGameObjectsWithTag( "Enemy" ) )
					{
						if ( !enemy.GetComponent<Health>().IsAlive() )
							continue;

						float sqrDist2 = Vector3.ProjectOnPlane( enemy.transform.position - transform.position, Vector3.up ).sqrMagnitude;
						if ( sqrDist2 <= engageDist * engageDist )
						{
							if ( sqrDist2 < closestDist1 )
							{
								closestDist = sqrDist2;
								closestEnemy = enemy;
							}
						}
					}
					if ( closestEnemy1 != null )
					{
						m_target = closestEnemy1;
					}
					else
					{
						m_target = null;
						m_state = TurretState.IDLE;
					}
				}

				if ( m_target == null )
					break;

				//
				Vector3 targetDir = m_target.transform.position - top.transform.position;
				Vector3 targetDirHoriz = targetDir;
				targetDirHoriz.y = 0f;
				Vector3 newTopDir = Vector3.Slerp( top.transform.forward, targetDirHoriz, 6f * Time.deltaTime );
				top.transform.rotation = Quaternion.LookRotation( newTopDir , Vector3.up );
				float targetAngle = Mathf.Asin( targetDir.y / targetDir.magnitude ) * Mathf.Rad2Deg;
				targetAngle = 90f - targetAngle;
				barrel.transform.localEulerAngles = new Vector3(Mathf.Lerp(barrel.transform.localEulerAngles.x, targetAngle, 4f * Time.deltaTime), 0f, 0f);

				//
				if ( Time.time > m_lastAttackTime + attackCooldown )
				{
					Timing.RunCoroutineSingleton(_AttackTarget( m_target ), gameObject, "_AttackTarget", SingletonBehavior.Overwrite);
				}

				break;
			default:
				break;
		}
    }

	public void TryForceTarget( GameObject target )
	{
		if ( Vector3.ProjectOnPlane( m_target.transform.position - transform.position, Vector3.up ).sqrMagnitude > engageDist * engageDist )
			return;

		m_target = target;
	}

	public IEnumerator<float> _AttackTarget( GameObject target )
	{
		m_lastAttackTime = Time.time;
		laserFx.gameObject.SetActive( true );
		if ( target == null )
			yield return 0f;
		muzzleFx.gameObject.SetActive( true );

		Health enemyHealth = target.GetComponent<Health>();
		if ( enemyHealth == null )
			yield return 0f;
		enemyHealth.TakeDamage( attackDamage );

		Vector3 targetPos = target.transform.position;
		float startTime = Time.time;
		Vector3 offset = target.transform.forward * Random.Range( -attackForwardRand * 0.5f, attackForwardRand )
					+ target.transform.right * Random.Range( -attackRightRand, attackRightRand )
					+ target.transform.up * -0.2f;
		while (Time.time < startTime + attackLaserTime)
		{
			if ( m_target != null && m_target != target )
				break;

			laserFx.SetPosition( 0, muzzlePos.position );
			if ( target != null && enemyHealth.IsAlive())
				targetPos = target.transform.position + offset;
					
			laserFx.SetPosition( 1, targetPos );

			//laserFx.widthMultiplier = MathUtil.RemapClamped( Time.time, startTime, startTime + attackLaserTime, 1f, attackLaserEndWidth );

			yield return Timing.WaitForOneFrame;
		}

		laserFx.gameObject.SetActive( false );
		muzzleFx.gameObject.SetActive( false );
	}

	public override void OnWork( CuboController user )
	{
		base.OnWork( user );
		m_health.Heal( healPerWork );
	}
}
