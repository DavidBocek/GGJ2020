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
	

    void Start()
    {
        
    }

    void Update()
    {
        foreach( EnemySpawnerSpawn spawner in spawnerSpawns )
		{
			if ( spawner.completed )
				continue;

			if (Time.time > spawner.time)
			{
				Transform loc = spawnLocs[Random.Range( 0, spawnLocs.Count )];
				Instantiate( spawner.spawnerObj, loc.position, Quaternion.identity );
				spawnLocs.Remove( loc );
			}
		}
    }
}
