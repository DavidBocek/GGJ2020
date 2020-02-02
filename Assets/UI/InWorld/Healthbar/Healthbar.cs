using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Healthbar : MonoBehaviour
{
	public GameObject restoreBar;
	private Slider m_bar;
	private RectTransform m_rect;

    void Awake()
    {
		m_bar = GetComponentInChildren<Slider>();
		m_rect = GetComponent<RectTransform>();
    }

    public void SetHealthFrac( float frac )
	{
		m_bar.value = frac;
	}

	public void ShowRestoreBar( bool show, float frac = 0f )
	{
		if ( !show )
		{
			restoreBar.SetActive( false );
			return;
		}

		restoreBar.SetActive( true );

		if ( m_rect == null )
			m_rect = GetComponent<RectTransform>();

		float newPoint = m_rect.sizeDelta.x * frac;
		RectTransform restoreRect = restoreBar.GetComponent<RectTransform>();
		restoreRect.anchoredPosition = new Vector2( newPoint, restoreRect.anchoredPosition.y );
	}
}
