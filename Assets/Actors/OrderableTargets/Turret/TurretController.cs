using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    void Start()
    {
		InitWorkTargets();
		m_health = GetComponent<Health>();
		m_state = TurretState.IDLE;
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
				}

				break;
			case TurretState.ATTACKING:
				float sqrDist = Vector3.ProjectOnPlane( m_target.transform.position - transform.position, Vector3.up ).sqrMagnitude;
				if ( sqrDist > (engageDist +5)*(engageDist +5))
				{
					float closestDist1 = Mathf.Infinity;
					GameObject closestEnemy1 = null;
					foreach ( GameObject enemy in GameObject.FindGameObjectsWithTag( "Enemy" ) )
					{
						float sqrDist2 = Vector3.ProjectOnPlane( enemy.transform.position - transform.position, Vector3.up ).sqrMagnitude;
						if ( sqrDist2 <= engageDist * engageDist )
						{
							if ( sqrDist2 < closestDist1 )
							{
								closestDist = sqrDist;
								closestEnemy = enemy;
							}
						}
					}
					if ( closestEnemy1 != null )
						m_target = closestEnemy1;
					else
					{
						m_target = null;
						m_state = TurretState.IDLE;
					}
				}

				if ( m_target == null )
					break;

				Vector3 targetDir = m_target.transform.position - top.transform.position;
				Vector3 targetDirHoriz = targetDir;
				targetDirHoriz.y = 0f;
				Vector3 newTopDir = Vector3.Slerp( top.transform.forward, targetDirHoriz, 6f * Time.deltaTime );
				top.transform.rotation = Quaternion.LookRotation( newTopDir , Vector3.up );
				float targetAngle = Mathf.Asin( targetDir.y / targetDir.magnitude ) * Mathf.Rad2Deg;
				targetAngle = 90f - targetAngle;
				barrel.transform.localEulerAngles = new Vector3(Mathf.Lerp(barrel.transform.localEulerAngles.x, targetAngle, 4f * Time.deltaTime), 0f, 0f);

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

	public override void OnWork( CuboController user )
	{
		m_health.Heal( healPerWork );
	}

	public void OnDeath()
	{

	}
}
