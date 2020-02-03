using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingController : OrderableTarget
{
	private Health m_health;
	public int healPerWork;

    public AudioClip explodeNoise;
    public AudioClip repairNoise;

    // Start is called before the first frame update
    void Start()
    {
		InitWorkTargets();
		m_health = GetComponent<Health>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }



	public override void OnWork( CuboController user )
	{
		if ( m_health.IsFull() )
			return;

		m_health.Heal( healPerWork );
        Camera.main.GetComponent<AudioSource>().PlayOneShot( repairNoise );
        base.OnWork( user );
	}

    public override void OnDeath()
    {
		base.OnDeath();
        Camera.main.GetComponent<AudioSource>().PlayOneShot( explodeNoise );
		bool lose = true;
		foreach ( GameObject obj in GameObject.FindGameObjectsWithTag("Building"))
		{
			if (obj.GetComponent<Health>().IsAlive())
			{
				lose = false;
				break;
			}
		}
		if (lose)
		{
			PlayerCommands.Get().Lose();
		}
    }
}
