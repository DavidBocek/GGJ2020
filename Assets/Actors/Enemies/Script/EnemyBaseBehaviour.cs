using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyBaseBehaviour : MonoBehaviour
{
    public float timeBetweenWaves;
    public int enemiesPerWave;
	public bool bombersAvailable = false;
	public int bomberWavePer;
	public int bombersPerBomberWave;
    public Transform spawnPos;
    public GameObject EnemyPrefab;
    public GameObject BomberPrefab;

	public EnemySpawnTimerUI timerUI;

    private float m_nextSpawnTime;
    private bool m_isActive;
	private int m_wave = 0;
	private int m_rand;

    // Start is called before the first frame update
    void Start()
    {
        m_nextSpawnTime = Time.time + timeBetweenWaves;
        m_isActive = true;
		m_rand = Random.Range( 0, bomberWavePer );
    }

    // Update is called once per frame
    void Update()
    {
		timerUI.SetFrac( Mathf.Clamp01( 1 - ( m_nextSpawnTime - Time.time ) / timeBetweenWaves ) );
        if ( m_isActive && Time.time > m_nextSpawnTime )
        {
            m_nextSpawnTime = Time.time + timeBetweenWaves;

			int enemyCount = enemiesPerWave;
			int bomberCount = 0;
			if ( bombersAvailable && ((m_wave + m_rand) % bomberWavePer) == 0)
			{
				bomberCount = bombersPerBomberWave;
				enemyCount = Mathf.Max( enemyCount - bomberCount, 0 );
			}

            for ( int i = 0; i < enemyCount; i++ )
            {
                Instantiate<GameObject>( EnemyPrefab, spawnPos.position, spawnPos.rotation );
            }

			for ( int i = 0; i < bomberCount; i++ )
            {
                Instantiate<GameObject>( BomberPrefab, spawnPos.position, spawnPos.rotation );
            }


			m_wave++;
        }

    }
}
