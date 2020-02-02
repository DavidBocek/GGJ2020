using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    public int maxHealth = 10;
    public float healthFracNeededToBeAlive = 0.2f;

    private bool m_isAlive = true;
    private int m_health = 1;
    // Start is called before the first frame update
    void Start()
    {
        m_health = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Heal( int healAmt )
    {
        m_health += healAmt;

        if ( !m_isAlive && m_health > maxHealth * healthFracNeededToBeAlive )
        {
            m_isAlive = true;
        }
    }

    public void TakeDamage( int damageAmt )
    {
        m_health = Mathf.Max( 0, m_health - damageAmt );

        if ( m_health <= 0 )
        {
            m_isAlive = false;
            gameObject.SendMessage( "OnDeath" );
        }

    }

    public bool IsAlive()
    {
        return m_isAlive;
    }
}
