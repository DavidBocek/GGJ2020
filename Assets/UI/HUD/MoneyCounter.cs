using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MoneyCounter : MonoBehaviour
{
	private Text m_text;

    // Start is called before the first frame update
    void Start()
    {
		m_text = GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
		if ( UIStatic.HasInt( UIStatic.MONEY ) )
			m_text.text = UIStatic.GetInt( UIStatic.MONEY ).ToString();
    }
}
