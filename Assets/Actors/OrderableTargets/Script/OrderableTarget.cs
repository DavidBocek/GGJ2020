using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrderableTarget : MonoBehaviour
{
	public Color openWorkTargetColor;
	public Color claimedWorkTargetColor;

	public List<GameObject> workTargetObjs;
	protected List<WorkTarget> m_workTargets;

	protected void InitWorkTargets()
	{
		int id = 0;
		foreach( GameObject obj in workTargetObjs )
		{
			WorkTarget target = new WorkTarget();
			target.id = id++;
			target.isOccupied = false;
			target.pos = new Vector3( obj.transform.position.x, 1f, obj.transform.position.z );
			target.renderer = obj.GetComponentInChildren<Renderer>();
			m_workTargets.Add( target );
		}
	}

	public virtual List<WorkTarget> GetOpenWorkTargets()
	{
		List<WorkTarget> res = new List<WorkTarget>();
		foreach ( WorkTarget workTarget in m_workTargets )
		{
			if ( workTarget.isOccupied )
				res.Add( workTarget );
		}
		return res;
	}

	public virtual void FillWorkTarget( WorkTarget workTarget )
	{
		workTarget.isOccupied = true;
	}

	public virtual void LeaveWorkTarget( WorkTarget workTarget )
	{
		workTarget.isOccupied = false;
	}

	public virtual void OnWork( CuboController user ) { }
}

public class WorkTarget
{
	public int id;
	public bool isOccupied;
	public Vector3 pos;
	public Renderer renderer;
}
