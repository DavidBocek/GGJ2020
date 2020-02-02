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

	public virtual void OnWork( CuboController user ) { }
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
