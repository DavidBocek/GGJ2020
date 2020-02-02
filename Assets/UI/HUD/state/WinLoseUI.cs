using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using MEC;
using UnityEngine.SceneManagement;

public class WinLoseUI : MonoBehaviour
{
	public Image winLoseBg;
	public Text winLoseText;

	public float fadeDur;

    void Start()
    {
		Color col = winLoseBg.color;
		col.a = 0f;
		winLoseBg.color = col;
		Color col2 = winLoseText.color;
		col2.a = 0f;
		winLoseText.color = col2;
	}

    void Update()
    {
        
    }

	public void OnWin()
	{
		Timing.RunCoroutineSingleton( _OnWin(), Segment.RealtimeUpdate, "_OnEndUI", SingletonBehavior.Abort );
	}

	private IEnumerator<float> _OnWin()
	{
		winLoseBg.DOFade( 1f, fadeDur ).SetEase( Ease.InQuad ).SetUpdate(true);
		yield return Timing.WaitForSeconds( fadeDur + 1f );
		winLoseText.text = "YOU WIN!";
		winLoseText.DOFade( 1f, 2f ).SetEase( Ease.InQuad ).SetUpdate( true );
	}

	public void OnLose()
	{
		Timing.RunCoroutineSingleton( _OnLose(), Segment.RealtimeUpdate, "_OnEndUI", SingletonBehavior.Abort );
	}

	private IEnumerator<float> _OnLose()
	{
		winLoseBg.DOFade( 1f, fadeDur ).SetEase( Ease.InQuad ).SetUpdate( true );
		yield return Timing.WaitForSeconds( fadeDur );
		winLoseText.text = "TRY AGAIN";
		winLoseText.DOFade( 1f, 2f ).SetEase( Ease.InQuad ).SetUpdate( true );
	}
}
