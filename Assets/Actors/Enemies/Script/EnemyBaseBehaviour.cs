using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBaseBehaviour : MonoBehaviour
{
    public float timeBetweenWaves;
    public int enemiesPerWave;
    public Transform spawnPos;
    public GameObject EnemyPrefab;

    private float m_nextSpawnTime;
    private bool m_isActive;

    // Start is called before the first frame update
    void Start()
    {
        m_nextSpawnTime = Time.time + timeBetweenWaves;
        m_isActive = true;
    }

    // Update is called once per frame
    void Update()
    {
        if ( m_isActive && Time.time > m_nextSpawnTime )
        {
            m_nextSpawnTime = Time.time + timeBetweenWaves;

            for ( int i = 0; i < enemiesPerWave; i++ )
            {
                Instantiate<GameObject>( EnemyPrefab, spawnPos.position, spawnPos.rotation );
            }
        }

    }
}
