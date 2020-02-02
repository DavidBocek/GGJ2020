using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MEC;

public class CurCubos : MonoBehaviour
{
	public Image icon;
	public Text label;
	public Text text;
	public Text maxText;
	private Color m_iconStartColor;
	private Color m_textStartColor;

	public Color flashRedColor;
	public float flashRedDuration;

	// Start is called before the first frame update
	void Start()
    {
		m_iconStartColor = icon.color;
		m_textStartColor = text.color;
		maxText.text = "MAX";
    }

    // Update is called once per frame
    void Update()
    {
		if ( UIStatic.HasInt( UIStatic.CUR_CUBOS ) )
		{
			text.text = UIStatic.GetInt( UIStatic.CUR_CUBOS ).ToString();
		}
		if ( !PlayerCommands.Get().EnoughRoomForCubos() )
			maxText.text = "MAX";
		else
			maxText.text = "";
	}

	public void FlashRed()
	{
		Timing.RunCoroutineSingleton( _FlashRed(), "_FlashRed", SingletonBehavior.Overwrite );
	}

	private IEnumerator<float> _FlashRed()
	{
		text.color = flashRedColor;
		label.color = flashRedColor;
		icon.color = flashRedColor;
		yield return Timing.WaitForSeconds( flashRedDuration );
		text.color = m_textStartColor;
		label.color = m_textStartColor;
		icon.color = m_iconStartColor;
	}
}
