using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Assertions;
using DG.Tweening;
using MEC;
using UnityEngine.SceneManagement;

public class PlayerCommands : MonoBehaviour
{
	private static PlayerCommands m_instance;
	public static PlayerCommands Get()
	{
		if ( m_instance == null )
			m_instance = GameObject.FindWithTag( "Player" ).GetComponent<PlayerCommands>();
		return m_instance;
	}

	public LayerMask orderLayerMask;

	[Header("Cubo Count")]
	public int maxCubos;
	private int m_curCuboCount;

	[Header( "Money" )]
	public int startingMoney;
	public int maxMoney;
	private int m_curMoney;
	public float uiMoneyPerSecond;
	private int m_curUIMoney;

	[Header("Move")]
	public GameObject moveOrderUIPrefab;
	public float moveOrderFormationOffsetDist;
	public LayerMask moveOrderFormationBlockerMask;
	public float moveDistScoreMultiplier;
	public float moveAngleScoreMultiplier;
	private const bool DEBUG_MOVE_FORMATION = true;

	[Header( "AddCubo" )]
	public GameObject cuboObj;
	public int addCuboCost;
	public GameObject addCuboPos;
	public float addCuboPosRand;

	[Header( "AddTurret" )]
	public GameObject turretGhostObj;
	public GameObject turretObj;
	public int addTurretCost;
	public int turretGhostGridGranularity;
	private List<BoxCollider> m_noTurretZones = new List<BoxCollider>();
	[HideInInspector]
	public bool isAddingTurret = false;
	[HideInInspector]
	public float lastAddingTurretTime = 0f;
	private GameObject m_turretGhost;
	public LayerMask turretGhostLayerMask;
	public Material turretGhostValidMat;
	public Material turretGhostInvalidMat;

	private void Awake()
	{
		UIStatic.Init();
	}

	private void Start()
	{

		m_curMoney = startingMoney;
		m_curUIMoney = m_curMoney;

		foreach ( Selectable selectable in Selection.selectables )
		{
			CuboController cubo = selectable.GetComponent<CuboController>();
			if ( cubo != null )
				m_curCuboCount++;
		}

		foreach ( GameObject obj in GameObject.FindGameObjectsWithTag("NoTurretZone"))
		{
			m_noTurretZones.Add( obj.GetComponent<BoxCollider>() );
		}
	}

	private void Update()
	{
		if ( isEnding )
			return;
		UpdateOrders();
		UpdateMoney();
		UpdateCubos();
		UpdateTurretAdding();
	}

	#region orders
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
			//Debug.DrawLine( Camera.main.transform.position, hitInfo.point, Color.red, 3f );
			//DebugExtension.DebugPoint( hitInfo.point, 3f, 3f );
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
					OrderableTarget target = hitInfo.collider.gameObject.GetComponentInChildren<OrderableTarget>();

					List<Selectable> workingUnits = new List<Selectable>();
					foreach ( Selectable selectable in Selection.GetSelected() )
					{
						CuboController cubo = selectable.GetComponent<CuboController>();
						if ( selectable.canMove && (cubo == null || cubo.GetOrderableTarget() != target) )
							workingUnits.Add( selectable );
					}
					if ( workingUnits.Count == 0 )
						break;

					List<WorkTarget> workTargets = target.GetOpenWorkTargets();

					if ( target.GetComponent<NegaCube>() != null)
					{
						foreach( Selectable unit in workingUnits)
						{
							unit.OrderOrderableTarget( target, target.GetOpenWorkTargets()[0] );
						}
					}

					foreach ( KeyValuePair<Selectable, WorkTarget> kvp in CalculateMovementPointsForOrderable( workingUnits, workTargets ) )
					{
						kvp.Key.OrderOrderableTarget( target, kvp.Value );
					}
					break;
				/*case "Enemy":
					GameObject enemy = hitInfo.collider.gameObject.GetComponentInParent<EnemyBehaviour>().gameObject;
					if ( enemy == null )
						break;

					List<Selectable> targetingUnits = new List<Selectable>();
					foreach( Selectable selectable in Selection.GetSelected() )
					{
						if ( selectable.canTargetEnemies )
							selectable.OrderTargetEnemy( enemy );
					}

					break;*/
				default:
					Debug.LogWarning( "Unknown order raycast hit. Layer: " + layer );
					break;
			}
		}
	}
	#endregion

	#region movement logic
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
	#endregion

	#region money
	public void UpdateMoney()
	{
		//TODO REMOVE ME
		if ( Input.GetKeyDown( KeyCode.O ) )
			AddMoney( 50 );
		else if ( Input.GetKeyDown( KeyCode.I ) )
			TakeMoney( 50 );


		if (m_curUIMoney != m_curMoney)
		{
			float moneyRate = uiMoneyPerSecond;

			if ( Mathf.Abs( m_curUIMoney - m_curMoney ) > 250 )
				moneyRate *= 3f;
				
			if (m_curMoney > m_curUIMoney)
			{
				m_curUIMoney = Mathf.Min( (int)(m_curUIMoney + moneyRate * Time.deltaTime), m_curMoney );
			}
			else
			{
				m_curUIMoney = Mathf.Max( (int)(m_curUIMoney - moneyRate * Time.deltaTime), m_curMoney );
			}
		}
		UpdateMoneyUI();
	}

	public void AddMoney( int money )
	{
		m_curMoney = Mathf.Min( m_curMoney + money, maxMoney );
		UpdateMoneyUI();
	}

	public void TakeMoney( int money )
	{
		m_curMoney = Mathf.Max( m_curMoney - money, 0 );
		UpdateMoneyUI();
	}

	public void UpdateMoneyUI()
	{
		UIStatic.SetInt( UIStatic.MONEY, m_curUIMoney );
	}
	#endregion

	#region commands
	private void UpdateCubos()
	{
		UpdateCubosUI();
	}

	public bool EnoughRoomForCubos()
	{
		return m_curCuboCount < maxCubos;
	}

	public bool TryAddCubo()
	{
		if ( m_curMoney < addCuboCost )
		{
			return false;
		}
		else
		{
			TakeMoney( addCuboCost );
			AddCubo();
			return true;
		}
	}

	private void AddCubo()
	{
		Vector2 rand = Random.insideUnitSphere * addCuboPosRand;
		Vector3 spawnPos = addCuboPos.transform.position + Vector3.right * rand.x + Vector3.forward * rand.y;
		spawnPos.y = 1f;
		GameObject newCubo = (GameObject)Instantiate( cuboObj, spawnPos, Quaternion.identity );
		m_curCuboCount++;
		UpdateCubosUI();
	}

	public void OnCuboDestroyed()
	{
		m_curCuboCount--;
		UpdateCubosUI();
	}

	private void UpdateCubosUI()
	{
		UIStatic.SetInt( UIStatic.CUR_CUBOS, m_curCuboCount );
	}

	private void UpdateTurretAdding()
	{
		if ( !isAddingTurret )
			return;

		if ( Input.GetButtonDown("ClearSelection"))
		{
			StopAddingTurret();
			return;
		}

		lastAddingTurretTime = Time.time;

		Ray mouseToWorldRay = Camera.main.ScreenPointToRay( Input.mousePosition );
		RaycastHit hitInfo;
		bool hitSomething = false;
		if ( Physics.Raycast( mouseToWorldRay, out hitInfo, 1000f, turretGhostLayerMask, QueryTriggerInteraction.Ignore ) )
		{
			hitSomething = true;
		}

		Vector3 roundedPosition = hitInfo.point;
		roundedPosition.y = 1f;
		roundedPosition.x = Mathf.Round( roundedPosition.x / turretGhostGridGranularity ) * turretGhostGridGranularity;
		roundedPosition.z = Mathf.Round( roundedPosition.z / turretGhostGridGranularity ) * turretGhostGridGranularity;
		m_turretGhost.transform.position = roundedPosition;

		bool canPlace = true;
		foreach ( BoxCollider box in m_noTurretZones)
		{
			if ( box.bounds.Contains(roundedPosition))
			{
				canPlace = false;
				foreach ( Renderer r in m_turretGhost.GetComponentsInChildren<Renderer>())
				{
					r.material = turretGhostInvalidMat;
				}
				break;
			}
		}
		if ( canPlace )
		{
			foreach ( Renderer r in m_turretGhost.GetComponentsInChildren<Renderer>() )
			{
				r.material = turretGhostValidMat;
			}
		}

		if ( Input.GetButtonDown("Select") && hitSomething )
		{
			if ( hitInfo.collider.gameObject.layer == LayerMask.NameToLayer( "UI" ) )
			{
				StopAddingTurret();
				return;
			}

			if ( Camera.main.WorldToViewportPoint( roundedPosition).y < .1026f)
			{
				//ui bar hack
				StopAddingTurret();
				return;
			}

			if ( canPlace )
			{
				PlaceTurret(roundedPosition);
			}
		}
	}

	public bool TryAddTurret()
	{
		if ( m_curMoney < addTurretCost )
		{
			return false;
		}
		else
		{
			BeginAddTurret();
			return true;
		}
	}

	private void BeginAddTurret()
	{
		isAddingTurret = true;
		GameObject turretGhost = (GameObject)Instantiate( turretGhostObj, Vector3.zero, Quaternion.identity );
		m_turretGhost = turretGhost;
	}

	private void StopAddingTurret()
	{
		Destroy( m_turretGhost );
		isAddingTurret = false;
		m_turretGhost = null;
	}

	private void PlaceTurret( Vector3 pos )
	{
		if ( m_curMoney >= addTurretCost )
			Instantiate( turretObj, pos, Quaternion.identity );
		StopAddingTurret();
		TakeMoney( addTurretCost );
	}

	#endregion

	#region game state
	public WinLoseUI winLoseUI;
	public bool isEnding = false;
	public void Lose()
	{
		if ( isEnding )
			return;
		isEnding = true;

		winLoseUI.OnLose();
		Timing.RunCoroutineSingleton( _End(false),Segment.RealtimeUpdate, "_End", SingletonBehavior.Abort );
	}

	private IEnumerator<float> _End(bool win)
	{
		for (float t=0f; t<=1f; t+=(Time.unscaledDeltaTime/ winLoseUI.fadeDur ) )
		{
			Time.timeScale = Mathf.Lerp( 1f, 0.2f, t );
			yield return Timing.WaitForOneFrame;
		}
		Time.timeScale = 0.2f;
		yield return Timing.WaitForSeconds( 4f );
		if ( win )
			SceneManager.LoadScene( "MainMenu" );
		else
			SceneManager.LoadScene( "BananaTest" );
	}

	public void Win()
	{
		if ( isEnding )
			return;
		isEnding = true;

		winLoseUI.OnWin();
		Timing.RunCoroutineSingleton( _End(true),Segment.RealtimeUpdate, "_End", SingletonBehavior.Abort );
	}


	private void Reset()
	{
		
	}
	#endregion
}
