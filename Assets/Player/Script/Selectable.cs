using UnityEngine;

public class Selectable : MonoBehaviour
{
	public GameObject selectionRing;

	public bool isSelected;
	

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

	void OnEnable()
	{
		Selection.selectables.Add( this );
	}

	void OnDisable()
	{
		Selection.selectables.Remove( this );
	}

}