using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class OrderGround : MonoBehaviour
{
	public float duration;
	public float size;
	public Ease easeIn;
	public Ease easeOut;

    void Start()
    {
		transform.localScale = Vector3.zero;
		Sequence seq = DOTween.Sequence();
		seq.Append( transform.DOScale( size, duration * 0.25f ).SetEase( easeIn ) );
		seq.Append( transform.DOScale( 0f, duration * 0.75f ).SetEase( easeOut ) );
		seq.Play();

		Destroy( gameObject, duration + 1f );
    }
}
