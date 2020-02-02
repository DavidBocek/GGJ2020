using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MEC;

public class MoneyCounter : MonoBehaviour
{
	public Color flashRedColor;
	public float flashRedDuration;

	public Text moneyText;
	private Text m_labelText;
	private Color m_startColor;

	// Start is called before the first frame update
	void Start()
	{
		m_labelText = GetComponent<Text>();
		m_startColor = moneyText.color;
	}

	// Update is called once per frame
	void Update()
	{
		if ( UIStatic.HasInt( UIStatic.MONEY ) )
			moneyText.text = UIStatic.GetInt( UIStatic.MONEY ).ToString();
	}

	public void FlashRed()
	{
		Timing.RunCoroutineSingleton( _FlashRed(), "_FlashRed", SingletonBehavior.Overwrite );
	}

	private IEnumerator<float> _FlashRed()
	{
		moneyText.color = flashRedColor;
		m_labelText.color = flashRedColor;
		yield return Timing.WaitForSeconds( flashRedDuration );
		moneyText.color = m_startColor;
		m_labelText.color = m_startColor;

	}
}
