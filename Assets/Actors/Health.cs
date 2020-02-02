using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    public int maxHealth = 10;
    public float healthFracNeededToBeAlive = 0.2f;

	public Healthbar healthbar;

    private bool m_isAlive = true;
    private int m_health = 1;
    void OnEnable()
    {
        m_health = maxHealth;
		if ( healthbar != null )
			healthbar.ShowRestoreBar( false );
    }

    // Update is called once per frame
    void Update()
    {
		if ( healthbar != null )
			healthbar.SetHealthFrac( m_health / (float)maxHealth );
    }

    public void Heal( int healAmt )
    {
        m_health = Mathf.Min( m_health + healAmt, maxHealth );

		if ( healthbar != null )
			healthbar.SetHealthFrac( m_health / (float)maxHealth );

		if ( !m_isAlive && m_health > maxHealth * healthFracNeededToBeAlive )
        {
            m_isAlive = true;
			gameObject.SendMessage( "OnRevive", SendMessageOptions.DontRequireReceiver );

			if ( healthbar != null )
				healthbar.ShowRestoreBar( false );
        }
    }

    public void TakeDamage( int damageAmt )
    {
        m_health = Mathf.Max( 0, m_health - damageAmt );

		if ( healthbar != null )
			healthbar.SetHealthFrac( m_health / (float)maxHealth );

		if ( m_health <= 0 )
        {
            m_isAlive = false;
            gameObject.SendMessage( "OnDeath", SendMessageOptions.DontRequireReceiver );

			if (healthbar != null)
				healthbar.ShowRestoreBar( true, healthFracNeededToBeAlive );
        }
	}

    public bool IsAlive()
    {
        return m_isAlive;
    }
}
