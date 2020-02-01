using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCommands : MonoBehaviour
{
	public LayerMask orderLayerMask;

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

		if ( Selection.selectables.Count == 0 )
			return;

		Ray mouseToWorldRay = Camera.main.ScreenPointToRay( Input.mousePosition );
		RaycastHit hitInfo;
		if ( Physics.Raycast( mouseToWorldRay, out hitInfo, 1000f, orderLayerMask, QueryTriggerInteraction.Ignore ) )
		{
			string layer = LayerMask.LayerToName( hitInfo.collider.gameObject.layer );
			switch ( layer )
			{
				case "Ground":
					foreach ( Selectable selectable in Selection.selectables )
						selectable.OrderGround( hitInfo.point );
					break;
				case "OrderableCollision":
					OrderableTarget target = hitInfo.collider.gameObject.GetComponentInChildren<OrderableTarget>();

					foreach ( Selectable selectable in Selection.selectables )
						selectable.OrderOrderableTarget( target );
					break;
				default:
					Debug.LogWarning( "Unknown order raycast hit. Layer: " + layer );
					break;
			}
		}
	}
}
