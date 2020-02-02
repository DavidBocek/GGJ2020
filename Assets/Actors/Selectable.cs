using UnityEngine;

public class Selectable : MonoBehaviour
{
	[HideInInspector]
	public bool isSelected;

	public GameObject selectionRing;

	public void OnSelectionUpdate( bool selected )
	{
		if ( selected )
		{
			selectionRing.SetActive( true );
		}
		else
		{
			selectionRing.SetActive( false );
		}
	}

	public bool canMove = false;
	public virtual void OrderGround( Vector3 location ) { }

	public bool canUseOrderableTargets = false;
	public virtual void OrderOrderableTarget( OrderableTarget target, WorkTarget workTarget ) { }

	void OnEnable()
	{
		Selection.selectables.Add( this );
	}

	void OnDisable()
	{
		Selection.selectables.Remove( this );
	}

}