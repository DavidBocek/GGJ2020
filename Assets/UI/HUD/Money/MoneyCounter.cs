using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MEC;

public class MoneyCounter : MonoBehaviour
{
	public Color flashRedColor;
	public float flashRedDuration;

	private Text m_text;
	private Color m_startColor;

	// Start is called before the first frame update
	void Start()
	{
		m_text = GetComponent<Text>();
		m_startColor = m_text.color;
	}

	// Update is called once per frame
	void Update()
	{
		if ( UIStatic.HasInt( UIStatic.MONEY ) )
			m_text.text = UIStatic.GetInt( UIStatic.MONEY ).ToString();
	}

	public void FlashRed()
	{
		Timing.RunCoroutineSingleton( _FlashRed(), "_FlashRed", SingletonBehavior.Overwrite );
	}

	private IEnumerator<float> _FlashRed()
	{
		m_text.color = flashRedColor;
		yield return Timing.WaitForSeconds( flashRedDuration );
		m_text.color = m_startColor;

	}
}
