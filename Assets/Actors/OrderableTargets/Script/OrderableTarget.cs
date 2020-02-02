using MEC;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrderableTarget : MonoBehaviour
{
	public Color clearWorkTargetColor;
	public Color claimedWorkTargetColor;
	public Color occupiedWorkTargetColor;

	public List<GameObject> workTargetObjs;
	protected List<WorkTarget> m_workTargets = new List<WorkTarget>();

	public GameObject workFxObj;

	protected void InitWorkTargets()
	{
		int id = 0;
		foreach( GameObject obj in workTargetObjs )
		{
			WorkTarget target = new WorkTarget();
			target.id = id++;
			target.state = WorkTarget.WorkTargetState.CLEAR;
			target.pos = new Vector3( obj.transform.position.x, 1f, obj.transform.position.z );
			target.renderer = obj.GetComponentInChildren<Renderer>();
			target.obj = obj;
			m_workTargets.Add( target );
		}

		//hack lol
		m_startMaterial = GetComponentInChildren<Renderer>().material;
	}

	public virtual List<WorkTarget> GetOpenWorkTargets()
	{
		List<WorkTarget> res = new List<WorkTarget>();
		foreach ( WorkTarget workTarget in m_workTargets )
		{
			if ( workTarget.state == WorkTarget.WorkTargetState.CLEAR )
				res.Add( workTarget );
		}
		return res;
	}

	public virtual void ClaimWorkTarget( WorkTarget workTarget )
	{
		workTarget.state = WorkTarget.WorkTargetState.CLAIMED;
		workTarget.renderer.material.color = claimedWorkTargetColor;
	}

	public virtual void OccupyWorkTarget( WorkTarget workTarget )
	{
		workTarget.state = WorkTarget.WorkTargetState.OCCUPIED;
		workTarget.renderer.material.color = occupiedWorkTargetColor;
	}

	public virtual void LeaveWorkTarget( WorkTarget workTarget )
	{
		workTarget.state = WorkTarget.WorkTargetState.CLEAR;
		workTarget.renderer.material.color = clearWorkTargetColor;
	}

	public virtual void OnWork( CuboController user )
	{
		Instantiate( workFxObj, user.transform.position + Vector3.up * 4f, Quaternion.LookRotation( Vector3.up, Vector3.left ) );
	}

	//hacky death fx stuff
	public Renderer[] renderersToSwapOnDeath;
	public Material deathMaterial;
	private Material m_startMaterial;
	public GameObject smokeFxObj;
	private List<GameObject> m_spawnedSmokeFx = new List<GameObject>();
	public Transform[] smokeFxSpots;
	public int smokeCount;
	public int smokeCountRandAdd;
	public void OnDeath()
	{
		foreach (Renderer r in renderersToSwapOnDeath )
		{
			r.material = deathMaterial;
		}

		int smokeToSpawn = Mathf.Min( smokeCount + Random.Range( 0, smokeCountRandAdd + 1 ), smokeFxSpots.Length );
		List<Transform> prunedSmokeSpots = new List<Transform>( smokeFxSpots );
		for ( int i=0; i<smokeToSpawn; i++ )
		{
			int rand = Random.Range( 0, prunedSmokeSpots.Count );
			Transform smokeSpot = prunedSmokeSpots[rand];
			GameObject smoke = (GameObject)Instantiate( smokeFxObj, smokeSpot.position, Quaternion.LookRotation( Vector3.up ), smokeSpot);
			m_spawnedSmokeFx.Add( smoke );
			//smoke.GetComponent<ParticleSystem>().randomSeed = (uint)Random.Range( 0, int.MaxValue );
		}
	}

	public void OnRevive()
	{
		foreach ( Renderer r in renderersToSwapOnDeath )
		{
			r.material = m_startMaterial;
		}

		List<GameObject> m_spawnedSmokeFxCopy = new List<GameObject>( m_spawnedSmokeFx );
		foreach (GameObject fx in m_spawnedSmokeFxCopy)
		{
			foreach(ParticleSystem part in fx.GetComponentsInChildren<ParticleSystem>())
			{
				part.Stop();
			}

			m_spawnedSmokeFx.Remove( fx );
			//system is set to destroy itself
		}
	}
}

public class WorkTarget
{
	public enum WorkTargetState { CLEAR, CLAIMED, OCCUPIED }
	public WorkTargetState state;
	public int id;
	public GameObject obj;
	public Vector3 pos;
	public Renderer renderer;
}
