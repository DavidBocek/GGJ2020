using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Assertions;

public class PlayerCommands : MonoBehaviour
{
	public LayerMask orderLayerMask;

	[Header("Move")]
	public GameObject moveOrderUIPrefab;
	public float moveOrderFormationOffsetDist;
	public LayerMask moveOrderFormationBlockerMask;
	public float moveDistScoreMultiplier;
	public float moveAngleScoreMultiplier;
	private const bool DEBUG_MOVE_FORMATION = true;

	[Header( "Work" )]
	public float workMoveDistScoreMultiplier;
	public float workMoveAngleScoreMultiplier;

	private void Start()
	{
		
	}

	private void Update()
	{
		UpdateOrders();
	}

	private void UpdateOrders()
	{
		if ( !Input.GetButtonDown( "Order" ) )
			return;

		if ( Selection.GetSelected().Count == 0 )
			return;

		Ray mouseToWorldRay = Camera.main.ScreenPointToRay( Input.mousePosition );
		RaycastHit hitInfo;
		if ( Physics.Raycast( mouseToWorldRay, out hitInfo, 1000f, orderLayerMask, QueryTriggerInteraction.Ignore ) )
		{
			string layer = LayerMask.LayerToName( hitInfo.collider.gameObject.layer );
			switch ( layer )
			{
				case "Ground":
					List<Selectable> moveableUnits = new List<Selectable>();
					foreach ( Selectable selectable in Selection.GetSelected() )
					{
						if ( selectable.canMove )
							moveableUnits.Add( selectable );
					}
					if ( moveableUnits.Count == 0 )
						break;

					Vector3 groundedPoint = hitInfo.point;
					groundedPoint.y = 1f;
					GameObject orderUI = (GameObject)Instantiate( moveOrderUIPrefab, groundedPoint + Vector3.up * 0.01f, Quaternion.LookRotation( Vector3.down ) );

					foreach ( KeyValuePair<Selectable, Vector3> order in CalculateFormationMovementPoints( moveableUnits, groundedPoint ) )
					{
						order.Key.OrderGround( order.Value );
					}

					break;
				case "OrderableCollision":
					List<Selectable> workingUnits = new List<Selectable>();
					foreach ( Selectable selectable in Selection.GetSelected() )
					{
						if ( selectable.canMove )
							workingUnits.Add( selectable );
					}
					if ( workingUnits.Count == 0 )
						break;

					OrderableTarget target = hitInfo.collider.gameObject.GetComponentInChildren<OrderableTarget>();

					List<WorkTarget> workTargets = target.GetOpenWorkTargets();

					foreach ( KeyValuePair<Selectable, WorkTarget> kvp in CalculateMovementPointsForOrderable( workingUnits, workTargets ) )
					{
						kvp.Key.OrderOrderableTarget( target, kvp.Value );
					}
					break;
				default:
					Debug.LogWarning( "Unknown order raycast hit. Layer: " + layer );
					break;
			}
		}
	}

	private struct FormationMovementAtom
	{
		public Selectable selectable;
		public float distFromCenter;
		public Vector2 vectorFromCenter;
	}

	private Dictionary<Selectable, Vector3> CalculateFormationMovementPoints( List<Selectable> movers, Vector3 target )
	{
		//build grid of valid offset points
		List<Vector2> offsetGrid = new List<Vector2>();
		if ( DEBUG_MOVE_FORMATION )
			DebugExtension.DebugPoint( target, Color.green, 0.75f, 1f );
		offsetGrid.Add( Vector2.zero );

		int count = 1;
		int dist = 1;
		bool useOffset = true;
		Vector3 attemptPoint;
		RaycastHit hitInfo;
		NavMeshHit navHitInfo;
		while ( count < movers.Count && dist <= 6 )
		{
			int x = 0;
			bool xIncreasing = true;
			int y = dist;
			bool yIncreasing = false;
			for ( int i=0; i<4*dist; i++ )
			{
				useOffset = true;
				attemptPoint = target + Vector3.right * moveOrderFormationOffsetDist * x + Vector3.forward * moveOrderFormationOffsetDist * y;
			
				if ( !NavMesh.SamplePosition( attemptPoint, out navHitInfo, 0.25f, NavMesh.AllAreas ) )
				{
					if ( DEBUG_MOVE_FORMATION )
						DebugExtension.DebugPoint( attemptPoint, Color.red, 2f, 3f );

					useOffset = false;
				}
				if ( useOffset && Physics.Linecast(target + Vector3.up, attemptPoint + Vector3.up, out hitInfo, moveOrderFormationBlockerMask ) )
				{
					if ( DEBUG_MOVE_FORMATION )
						Debug.DrawLine( target + Vector3.up, hitInfo.point, Color.red, 3f );

					useOffset = false;
				}

				if ( useOffset )
				{
					if ( DEBUG_MOVE_FORMATION )
						DebugExtension.DebugPoint( attemptPoint, Color.green, 0.75f, 3f );
					offsetGrid.Add( new Vector2( moveOrderFormationOffsetDist * x, moveOrderFormationOffsetDist * y ) );
					count++;
				}

				if ( xIncreasing )
				{
					x++;
					if ( x == dist )
						xIncreasing = false;
				}
				else
				{
					x--;
					if ( x == -dist )
						xIncreasing = true;
				}

				if ( yIncreasing )
				{
					y++;
					if ( y == dist )
						yIncreasing = false;
				}
				else
				{
					y--;
					if ( y == -dist )
						yIncreasing = true;
				}
			}

			dist++;
		}

		if ( count < movers.Count )
		{
			//couldn't find valid offsets, so just add center points as a fallback
			for ( int i = count; i < movers.Count; i++ )
			{
				offsetGrid.Add( new Vector2( 0f, 0f ) );
			}
		}

		//build offsets of current movers
		List<FormationMovementAtom> moverAtoms = new List<FormationMovementAtom>();
		Vector3 avgPoint = Vector3.zero;
		foreach ( Selectable mover in movers )
		{
			avgPoint += mover.transform.position;
		}
		avgPoint /= movers.Count;
		if ( DEBUG_MOVE_FORMATION )
			DebugExtension.DebugPoint( avgPoint, Color.blue, 3f, 3f );

		Vector3 offsetVector;
		foreach ( Selectable mover in movers )
		{
			FormationMovementAtom atom = new FormationMovementAtom();
			atom.selectable = mover;
			atom.distFromCenter = Vector3.Distance( mover.transform.position, avgPoint );
			offsetVector = mover.transform.position - avgPoint;
			atom.vectorFromCenter = new Vector2( offsetVector.x, offsetVector.z );
			moverAtoms.Add( atom );
		}

		//distribute target points
		Dictionary<Selectable, Vector3> res = new Dictionary<Selectable, Vector3>();
		float score;
		float bestScore;
		FormationMovementAtom bestAtom = moverAtoms[0];
		Vector3 finalPoint;
		count = 0;
		List<FormationMovementAtom> prunedMoverAtoms = new List<FormationMovementAtom>( moverAtoms );
		foreach ( Vector2 targetOffset in offsetGrid )
		{
			bestScore = Mathf.NegativeInfinity;
			foreach ( FormationMovementAtom atom in prunedMoverAtoms )
			{
				score = Mathf.Abs( atom.distFromCenter - targetOffset.magnitude ) * -moveDistScoreMultiplier;
				score += Vector2.Dot( atom.vectorFromCenter, targetOffset ) * moveAngleScoreMultiplier;
				if ( score > bestScore )
				{
					bestScore = score;
					bestAtom = atom;
				}
			}

			finalPoint = target + Vector3.right * targetOffset.x + Vector3.forward * targetOffset.y;
			res.Add( bestAtom.selectable, finalPoint );
			prunedMoverAtoms.Remove( bestAtom );

			if ( DEBUG_MOVE_FORMATION )
				Debug.DrawLine( bestAtom.selectable.transform.position + Vector3.up * 0.1f, finalPoint + Vector3.up * 0.1f, Color.blue, 3f );

			count++;
			if ( count >= movers.Count )
				break;
		}

		return res;
	}


	private Dictionary<Selectable, WorkTarget> CalculateMovementPointsForOrderable( List<Selectable> movers, List<WorkTarget> targets )
	{
		Dictionary<Selectable, WorkTarget> res = new Dictionary<Selectable, WorkTarget>();
		List<Selectable> prunedMovers = new List<Selectable>( movers );
		int count = 0;
		float score;
		float bestScore;
		Selectable bestMover = movers[0];
		Vector3 target;
		foreach ( WorkTarget workTarget in targets )
		{
			target = workTarget.pos;
			bestScore = Mathf.NegativeInfinity;
			foreach ( Selectable mover in prunedMovers )
			{
				score = -(mover.transform.position - target).sqrMagnitude;
				if ( score > bestScore )
				{
					bestScore = score;
					bestMover = mover;
				}
			}

			res.Add( bestMover, workTarget );
			prunedMovers.Remove( bestMover );

			if ( DEBUG_MOVE_FORMATION )
				Debug.DrawLine( bestMover.transform.position + Vector3.up * 0.1f, target + Vector3.up * 0.1f, Color.blue, 3f );

			count++;
			if ( count >= movers.Count )
				break;
		}

		return res;
	}

	/*
	private Dictionary<Selectable, Vector3> ScoreAndAssignFormationMovement( List<Selectable> movers, Vector3 target, List<Vector2> offsetGrid, float distScoreMultiplier, float angleScoreMultiplier )
	{
		//build offsets of current movers
		List<FormationMovementAtom> moverAtoms = new List<FormationMovementAtom>();
		Vector3 avgPoint = Vector3.zero;
		foreach ( Selectable mover in movers )
		{
			avgPoint += mover.transform.position;
		}
		avgPoint /= movers.Count;
		if ( DEBUG_MOVE_FORMATION )
			DebugExtension.DebugPoint( avgPoint, Color.blue, 3f, 3f );

		Vector3 offsetVector;
		foreach ( Selectable mover in movers )
		{
			FormationMovementAtom atom = new FormationMovementAtom();
			atom.selectable = mover;
			atom.distFromCenter = Vector3.Distance( mover.transform.position, avgPoint );
			offsetVector = mover.transform.position - avgPoint;
			atom.vectorFromCenter = new Vector2( offsetVector.x, offsetVector.z );
			moverAtoms.Add( atom );
		}

		//distribute target points
		Dictionary<Selectable, Vector3> res = new Dictionary<Selectable, Vector3>();
		float score;
		float bestScore;
		FormationMovementAtom bestAtom = moverAtoms[0];
		Vector3 finalPoint;
		int count = 0;
		List<FormationMovementAtom> prunedMoverAtoms = new List<FormationMovementAtom>( moverAtoms );
		foreach ( Vector2 targetOffset in offsetGrid )
		{
			bestScore = Mathf.NegativeInfinity;
			foreach ( FormationMovementAtom atom in prunedMoverAtoms )
			{
				score = Mathf.Abs( atom.distFromCenter - targetOffset.magnitude ) * -distScoreMultiplier;
				score += Vector2.Dot( atom.vectorFromCenter, targetOffset ) * angleScoreMultiplier;
				if ( score > bestScore )
				{
					bestScore = score;
					bestAtom = atom;
				}
			}

			finalPoint = target + Vector3.right * targetOffset.x + Vector3.forward * targetOffset.y;
			res.Add( bestAtom.selectable, finalPoint );
			prunedMoverAtoms.Remove( bestAtom );

			if ( DEBUG_MOVE_FORMATION )
				Debug.DrawLine( bestAtom.selectable.transform.position + Vector3.up * 0.1f, finalPoint + Vector3.up * 0.1f, Color.blue, 3f );

			count++;
			if ( count >= movers.Count )
				break;
		}

		return res;
	}
	*/
}
