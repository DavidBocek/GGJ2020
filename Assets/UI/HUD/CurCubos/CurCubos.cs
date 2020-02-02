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
	private Color m_iconStartColor;
	private Color m_textStartColor;

	public Color flashRedColor;
	public float flashRedDuration;

	// Start is called before the first frame update
	void Start()
    {
		m_iconStartColor = icon.color;
		m_textStartColor = text.color;
    }

    // Update is called once per frame
    void Update()
    {
		if ( UIStatic.HasInt( UIStatic.CUR_CUBOS ) )
		{
			text.text = UIStatic.GetInt( UIStatic.CUR_CUBOS ).ToString();
		}
		if ( !PlayerCommands.Get().EnoughRoomForCubos() )
		{
			label.color = flashRedColor;
			text.color = flashRedColor;
		}
		else
		{
			label.color = m_textStartColor;
			text.color = m_textStartColor;
		}
	}

	public void FlashRed()
	{
		Timing.RunCoroutineSingleton( _FlashRed(), "_FlashRed", SingletonBehavior.Overwrite );
	}

	private IEnumerator<float> _FlashRed()
	{
		//text.color = flashRedColor;
		//label.color = flashRedColor;
		icon.color = flashRedColor;
		yield return Timing.WaitForSeconds( flashRedDuration );
		//text.color = m_textStartColor;
		//label.color = m_textStartColor;
		icon.color = m_iconStartColor;
	}
}
