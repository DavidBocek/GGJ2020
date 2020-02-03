using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBrain : MonoBehaviour
{
	public List<Transform> spawnLocs;

	[System.Serializable]
	public class EnemySpawnerSpawn
	{
		public GameObject spawnerObj;
		public float time;
		[HideInInspector]
		public bool completed = false;
	}
	public List<EnemySpawnerSpawn> spawnerSpawns;
	private float m_startTime;
	

    void Start()
    {
		m_startTime = Time.time;
    }

    void Update()
    {
		if ( spawnerSpawns.Count == 0 )
			return;

		EnemySpawnerSpawn spawner = spawnerSpawns[0];

		if ( spawner.completed )
			return;

		if ( spawner.spawnerObj == null )
		{
			spawnerSpawns.Remove( spawner );
			return;
		}

		if (Time.time > spawner.time + m_startTime)
		{
			Transform loc = spawnLocs[Random.Range( 0, spawnLocs.Count )];
			Instantiate( spawner.spawnerObj, loc.position, Quaternion.identity );
			spawnLocs.Remove( loc );
			spawner.completed = true;
			spawnerSpawns.Remove( spawner );
			m_startTime = Time.time;
		}
    }
}
