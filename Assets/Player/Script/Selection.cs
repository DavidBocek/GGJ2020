/*
 * Copyright (c) 2016, Ivo van der Marel
 * Released under MIT License (= free to be used for anything)
 * Enjoy :)
 */

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class Selection : MonoBehaviour
{
	public static List<Selectable> selectables = new List<Selectable>();
	public static List<Selectable> GetSelected()
	{
		return new List<Selectable>( selectables.Where( x => x.isSelected ) );
	}

	public Canvas canvas;
	public Image selectionBox;
	public LayerMask selectionLayerMask;

	private Vector3 startScreenPos;

	private BoxCollider worldCollider;

	private RectTransform rt;

	private bool isSelecting;

	void Awake()
	{
		if ( canvas == null )
			canvas = FindObjectOfType<Canvas>();

		if ( selectionBox != null )
		{
			//We need to reset anchors and pivot to ensure proper positioning
			rt = selectionBox.GetComponent<RectTransform>();
			rt.pivot = Vector2.one * .5f;
			rt.anchorMin = Vector2.one * .5f;
			rt.anchorMax = Vector2.one * .5f;
			selectionBox.gameObject.SetActive( false );
		}
	}

	void Update()
	{
		if ( PlayerCommands.Get().isEnding )
			return;

		if ( PlayerCommands.Get().isAddingTurret || PlayerCommands.Get().lastAddingTurretTime == Time.time )
		{
			ClearSelected();
			isSelecting = false;
			return;
		}

		if ( Input.GetButtonDown( "ClearSelection" ) )
			ClearSelected();

		if ( Input.GetButtonDown( "Select" ) )
		{
			if ( Camera.main.ScreenToViewportPoint( Input.mousePosition ).y < .1026f )
			{
				//ui bar hack
				return;
			}

			Ray mouseToWorldRay = Camera.main.ScreenPointToRay( Input.mousePosition );
			RaycastHit hitInfo;
			if ( Physics.Raycast( mouseToWorldRay, out hitInfo, 1000f, selectionLayerMask, QueryTriggerInteraction.Ignore ) )
			{
				if ( hitInfo.collider.gameObject.layer == LayerMask.NameToLayer( "UI" ) )
				{
					return;
				}

				Selectable s = hitInfo.collider.GetComponentInParent<Selectable>();
				if ( s != null )
				{
					if ( Input.GetButton( "AdditiveModifier" ) )
					{
						UpdateSelection( s, !s.isSelected );
					}
					else
					{
						ClearSelected();
						UpdateSelection( s, true );
					}

					return;
				}
			}
			else if ( !Input.GetButton( "AdditiveModifier" ) )
			{
				foreach ( Selectable selectable in selectables )
				{
					UpdateSelection( selectable, false );
				}
			}

			if ( selectionBox == null )
				return;
			//Storing these variables for the selectionBox
			startScreenPos = Input.mousePosition;
			isSelecting = true;
		}

		//If we never set the selectionBox variable in the inspector, we are simply not able to drag the selectionBox to easily select multiple objects. 'Regular' selection should still work
		if ( selectionBox == null )
			return;

		//We finished our selection box when the key is released
		if ( Input.GetMouseButtonUp( 0 ) )
			isSelecting = false;

		selectionBox.gameObject.SetActive( isSelecting );

		if ( isSelecting )
		{
			Bounds b = new Bounds();
			//The center of the bounds is inbetween startpos and current pos
			b.center = Vector3.Lerp( startScreenPos, Input.mousePosition, 0.5f );
			//We make the size absolute (negative bounds don't contain anything)
			b.size = new Vector3( Mathf.Abs( startScreenPos.x - Input.mousePosition.x ),
				Mathf.Abs( startScreenPos.y - Input.mousePosition.y ),
				0 );

			//To display our selectionbox image in the same place as our bounds
			rt.position = b.center;
			rt.sizeDelta = canvas.transform.InverseTransformVector( b.size );

			//Looping through all the selectables in our world (automatically added/removed through the Selectable OnEnable/OnDisable)
			foreach ( Selectable selectable in selectables )
			{
				//If the screenPosition of the worldobject is within our selection bounds, we can add it to our selection
				Vector3 screenPos = Camera.main.WorldToScreenPoint( selectable.transform.position );
				screenPos.z = 0;
				bool isSelected = b.Contains( screenPos );
				if ( Input.GetButton( "AdditiveModifier" ) && !isSelected )
					continue;

				UpdateSelection( selectable, isSelected );
			}
		}
	}

	void UpdateSelection( Selectable s, bool value )
	{
		if ( s.isSelected != value )
		{
			s.isSelected = value;
			s.OnSelectionUpdate( value );
		}
	}

	void ClearSelected()
	{
		foreach ( Selectable selectable in selectables )
		{
			UpdateSelection( selectable, false );
		}
	}

}