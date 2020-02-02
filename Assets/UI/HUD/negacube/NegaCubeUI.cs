using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class NegaCubeUI : MonoBehaviour
{
	public Text label1;
	public Text label2;
	public Text currentSacrificeText;
	public Text goalText;

	public Color closeColor;
	private bool isFlashing = false;

	void Start()
    {
        
    }

    void Update()
    {
		if ( UIStatic.HasInt( UIStatic.MAX_SACRIFICE ) )
			goalText.text = UIStatic.GetInt( UIStatic.MAX_SACRIFICE ).ToString();

		if ( UIStatic.HasInt( UIStatic.CUR_SACRIFICE ) )
			currentSacrificeText.text = UIStatic.GetInt( UIStatic.CUR_SACRIFICE ).ToString();

		if ( UIStatic.HasInt( UIStatic.CUR_SACRIFICE ) && UIStatic.HasInt( UIStatic.MAX_SACRIFICE ) )
		{
			if ( UIStatic.GetInt( UIStatic.CUR_SACRIFICE ) >= UIStatic.GetInt( UIStatic.MAX_SACRIFICE ) - 10 )
			{	
				FlashColor();
			}	
		}
    }

	private void FlashColor()
	{
		if ( isFlashing )
			return;

		label1.DOColor( closeColor, 1f ).SetEase( Ease.InOutQuart ).SetLoops( -1, LoopType.Yoyo );
		label2.DOColor( closeColor, 1f ).SetEase( Ease.InOutQuart ).SetLoops( -1, LoopType.Yoyo );
		currentSacrificeText.DOColor( closeColor, 1f ).SetEase( Ease.InOutQuart ).SetLoops( -1, LoopType.Yoyo ); ;
		goalText.DOColor( closeColor, 1f ).SetEase( Ease.InOutQuart ).SetLoops( -1, LoopType.Yoyo ); ;

		isFlashing = true;
	}
}
